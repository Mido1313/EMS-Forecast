from __future__ import annotations

from dataclasses import dataclass
from datetime import datetime
from zoneinfo import ZoneInfo

import numpy as np
import pandas as pd

from app.services.data_repository import DataBundle


@dataclass
class FeatureEngineeringConfig:
    traffic_window_hours: int = 6
    traffic_background_window_hours: int = 24


class FeatureEngineeringService:
    def __init__(self, config: FeatureEngineeringConfig | None = None) -> None:
        self._config = config or FeatureEngineeringConfig()

    @staticmethod
    def _holiday_set() -> set[str]:
        # Core Austrian holidays for prototype mode (extendable per year).
        return {
            "2026-01-01",
            "2026-01-06",
            "2026-04-06",
            "2026-05-01",
            "2026-05-14",
            "2026-05-25",
            "2026-06-04",
            "2026-08-15",
            "2026-10-26",
            "2026-11-01",
            "2026-12-08",
            "2026-12-25",
            "2026-12-26",
        }

    def _effective_timestamp(self, bundle: DataBundle) -> pd.Timestamp:
        if bundle.traffic.empty:
            raise ValueError("Traffic dataset is empty. Cannot calculate risk.")
        return pd.to_datetime(bundle.traffic["timestamp"].max(), utc=True)

    def build_area_features(self, bundle: DataBundle) -> tuple[pd.DataFrame, pd.Timestamp]:
        as_of = self._effective_timestamp(bundle)
        traffic_recent_from = as_of - pd.Timedelta(hours=self._config.traffic_window_hours)
        traffic_background_from = as_of - pd.Timedelta(hours=self._config.traffic_background_window_hours)

        traffic_recent = bundle.traffic.loc[bundle.traffic["timestamp"] >= traffic_recent_from].copy()
        traffic_background = bundle.traffic.loc[bundle.traffic["timestamp"] >= traffic_background_from].copy()

        if traffic_recent.empty:
            traffic_recent = bundle.traffic.copy()
        if traffic_background.empty:
            traffic_background = bundle.traffic.copy()

        area_features = bundle.areas.copy()

        traffic_agg = (
            traffic_recent.groupby("areaId", as_index=False)
            .agg(
                avgSpeed=("averageVehicleSpeed", "mean"),
                minSpeed=("averageVehicleSpeed", "min"),
                avgTravelTime=("travelTime", "mean"),
                unknownRatioTrafficStatus=("trafficStatus", lambda x: float((x == "unknown").mean())),
            )
            .fillna(0)
        )

        # Free-flow baseline from configured hotspot lengths and free-flow speeds.
        free_flow_by_area = (
            bundle.hotspots.assign(freeFlowTravelTime=lambda df: (df["lengthKm"] / df["freeFlowSpeedKph"]) * 60)
            .groupby("areaId", as_index=False)
            .agg(
                freeFlowTravelTime=("freeFlowTravelTime", "mean"),
                commuterHotspotRatio=("isCommuter", "mean"),
                touristicHotspotRatio=("isTouristic", "mean"),
            )
        )

        traffic_with_hotspots = traffic_background.merge(
            bundle.hotspots[["segmentId", "areaId", "freeFlowSpeedKph", "criticalityWeight"]],
            on=["segmentId", "areaId"],
            how="left",
        )
        traffic_with_hotspots["congestion"] = np.clip(
            (traffic_with_hotspots["freeFlowSpeedKph"] - traffic_with_hotspots["averageVehicleSpeed"])
            / traffic_with_hotspots["freeFlowSpeedKph"].replace(0, np.nan),
            0,
            1,
        ).fillna(0)
        traffic_with_hotspots["segmentHotspotRisk"] = (
            traffic_with_hotspots["criticalityWeight"].fillna(0)
            * (0.4 + 0.6 * traffic_with_hotspots["congestion"])
        )

        hotspot_score = (
            traffic_with_hotspots.groupby("areaId", as_index=False)
            .agg(hotspotCriticalityScore=("segmentHotspotRisk", "mean"))
            .fillna(0)
        )

        active_constructions = bundle.constructions.loc[
            (bundle.constructions["startTime"] <= as_of) & (bundle.constructions["endTime"] >= as_of)
        ]
        construction_agg = (
            active_constructions.groupby("areaId", as_index=False)
            .agg(
                activeConstructionCount=("constructionId", "count"),
                constructionSeverityIndex=("severity", "mean"),
            )
            .fillna(0)
        )

        accident_24h_from = as_of - pd.Timedelta(hours=24)
        accident_7d_from = as_of - pd.Timedelta(days=7)
        accidents_24h = bundle.accidents.loc[bundle.accidents["timestamp"] >= accident_24h_from]
        accidents_7d = bundle.accidents.loc[bundle.accidents["timestamp"] >= accident_7d_from]

        accident_24h_agg = accidents_24h.groupby("areaId", as_index=False).agg(accidentCount24h=("accidentId", "count"))
        accident_7d_agg = accidents_7d.groupby("areaId", as_index=False).agg(accidentCount7d=("accidentId", "count"))

        weather_sorted = bundle.weather.loc[bundle.weather["timestamp"] <= as_of].sort_values("timestamp")
        latest_weather = weather_sorted.groupby("areaId", as_index=False).tail(1)
        latest_weather = latest_weather[["areaId", "temperatureC", "precipitationMm", "windKph", "icyFlag"]]

        area_features = area_features.merge(traffic_agg, on="areaId", how="left")
        area_features = area_features.merge(free_flow_by_area, on="areaId", how="left")
        area_features = area_features.merge(hotspot_score, on="areaId", how="left")
        area_features = area_features.merge(construction_agg, on="areaId", how="left")
        area_features = area_features.merge(accident_24h_agg, on="areaId", how="left")
        area_features = area_features.merge(accident_7d_agg, on="areaId", how="left")
        area_features = area_features.merge(latest_weather, on="areaId", how="left")

        area_features = area_features.fillna(0)

        area_features["avgDelayIndex"] = (
            area_features["avgTravelTime"] / area_features["freeFlowTravelTime"].replace(0, np.nan)
        ).fillna(1.0)

        local_now = as_of.tz_convert(ZoneInfo("Europe/Vienna"))
        hour = int(local_now.hour)
        weekday = int(local_now.weekday())
        month = int(local_now.month)
        is_weekend = int(weekday >= 5)
        is_holiday = int(local_now.date().isoformat() in self._holiday_set())

        area_features["hour"] = hour
        area_features["weekday"] = weekday
        area_features["month"] = month
        area_features["isWeekend"] = is_weekend
        area_features["isHoliday"] = is_holiday

        numeric_cols = [
            "avgSpeed",
            "minSpeed",
            "avgTravelTime",
            "unknownRatioTrafficStatus",
            "freeFlowTravelTime",
            "commuterHotspotRatio",
            "touristicHotspotRatio",
            "hotspotCriticalityScore",
            "activeConstructionCount",
            "constructionSeverityIndex",
            "accidentCount24h",
            "accidentCount7d",
            "temperatureC",
            "precipitationMm",
            "windKph",
            "icyFlag",
            "avgDelayIndex",
        ]
        for col in numeric_cols:
            area_features[col] = pd.to_numeric(area_features[col], errors="coerce").fillna(0)

        return area_features, as_of

    def build_hotspot_metrics(self, bundle: DataBundle) -> pd.DataFrame:
        as_of = self._effective_timestamp(bundle)
        latest_24h = bundle.traffic.loc[bundle.traffic["timestamp"] >= (as_of - pd.Timedelta(hours=24))].copy()
        if latest_24h.empty:
            latest_24h = bundle.traffic.copy()

        hotspot_df = latest_24h.merge(
            bundle.hotspots[["segmentId", "areaId", "hotspotName", "roadType", "criticalityWeight", "freeFlowSpeedKph"]],
            on=["segmentId", "areaId"],
            how="left",
        )

        summary = (
            hotspot_df.groupby(["segmentId", "areaId", "hotspotName", "roadType", "criticalityWeight", "freeFlowSpeedKph"], as_index=False)
            .agg(
                avgSpeed24h=("averageVehicleSpeed", "mean"),
                latestSpeed=("averageVehicleSpeed", "last"),
                avgTravelTime24h=("travelTime", "mean"),
                unknownStatusRatio=("trafficStatus", lambda x: float((x == "unknown").mean())),
            )
            .fillna(0)
        )

        summary["congestionIndex"] = np.clip(
            (summary["freeFlowSpeedKph"] - summary["avgSpeed24h"]) / summary["freeFlowSpeedKph"].replace(0, np.nan),
            0,
            1,
        ).fillna(0)
        summary["hotspotRiskHint"] = np.clip(
            summary["criticalityWeight"] * (0.5 + 0.5 * summary["congestionIndex"]),
            0,
            1,
        )

        return summary
