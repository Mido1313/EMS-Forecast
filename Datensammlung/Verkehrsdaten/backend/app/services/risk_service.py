from __future__ import annotations

from datetime import datetime, timezone
from threading import Lock
from typing import Any

from app.constants import AREA_ORDER
from app.domain.models import AreaRiskResult, RecalculationMetadata
from app.services.data_repository import SeedDataRepository
from app.services.feature_engineering import FeatureEngineeringService
from app.services.risk_model import TransparentRiskModel


class RiskService:
    def __init__(
        self,
        repository: SeedDataRepository,
        feature_engineering: FeatureEngineeringService,
        model: TransparentRiskModel,
    ) -> None:
        self._repository = repository
        self._feature_engineering = feature_engineering
        self._model = model

        self._lock = Lock()
        self._current_risk: list[AreaRiskResult] = []
        self._details: dict[int, dict[str, Any]] = {}
        self._last_metadata: RecalculationMetadata | None = None
        self._hotspot_cache: list[dict[str, Any]] = []

    def _sort_results(self, records: list[AreaRiskResult]) -> list[AreaRiskResult]:
        order = {area_id: idx for idx, area_id in enumerate(AREA_ORDER)}
        return sorted(records, key=lambda x: order.get(x.areaId, 999))

    def recalculate(self) -> RecalculationMetadata:
        bundle = self._repository.load_bundle()
        feature_df, as_of = self._feature_engineering.build_area_features(bundle)
        scored_df = self._model.score(feature_df)

        merged = scored_df.merge(feature_df, on=["areaId", "areaName"], how="left")

        results: list[AreaRiskResult] = []
        details: dict[int, dict[str, Any]] = {}

        for _, row in merged.iterrows():
            explanation = self._model.build_explanation(row)
            result = AreaRiskResult(
                areaId=int(row["areaId"]),
                areaName=str(row["areaName"]),
                riskScore=float(row["riskScore"]),
                riskCategory=int(row["riskCategory"]),
                colorHex=str(row["colorHex"]),
                explanation=explanation,
                components={
                    "traffic": round(float(row["component_traffic"]), 4),
                    "construction": round(float(row["component_construction"]), 4),
                    "accident": round(float(row["component_accident"]), 4),
                    "weather": round(float(row["component_weather"]), 4),
                    "time": round(float(row["component_time"]), 4),
                    "hotspot": round(float(row["component_hotspot"]), 4),
                },
                metrics={
                    "avgSpeed": round(float(row["avgSpeed"]), 2),
                    "minSpeed": round(float(row["minSpeed"]), 2),
                    "avgTravelTime": round(float(row["avgTravelTime"]), 2),
                    "freeFlowTravelTime": round(float(row["freeFlowTravelTime"]), 2),
                    "avgDelayIndex": round(float(row["avgDelayIndex"]), 2),
                    "unknownRatioTrafficStatus": round(float(row["unknownRatioTrafficStatus"]), 3),
                    "activeConstructionCount": int(row["activeConstructionCount"]),
                    "constructionSeverityIndex": round(float(row["constructionSeverityIndex"]), 2),
                    "accidentCount24h": int(row["accidentCount24h"]),
                    "accidentCount7d": int(row["accidentCount7d"]),
                    "hotspotCriticalityScore": round(float(row["hotspotCriticalityScore"]), 3),
                    "temperatureC": round(float(row["temperatureC"]), 2),
                    "precipitationMm": round(float(row["precipitationMm"]), 2),
                    "windKph": round(float(row["windKph"]), 2),
                    "icyFlag": int(row["icyFlag"]),
                    "hour": int(row["hour"]),
                    "weekday": int(row["weekday"]),
                    "month": int(row["month"]),
                    "isWeekend": int(row["isWeekend"]),
                    "isHoliday": int(row["isHoliday"]),
                    "commuterHotspotRatio": round(float(row["commuterHotspotRatio"]), 3),
                    "touristicHotspotRatio": round(float(row["touristicHotspotRatio"]), 3),
                    "calculationTimestamp": as_of.isoformat(),
                },
            )
            results.append(result)

            details[result.areaId] = {
                "areaId": result.areaId,
                "areaName": result.areaName,
                "riskScore": result.riskScore,
                "riskCategory": result.riskCategory,
                "colorHex": result.colorHex,
                "explanation": result.explanation,
                "components": result.components,
                "metrics": result.metrics,
            }

        hotspots_df = self._feature_engineering.build_hotspot_metrics(bundle)
        hotspots_df = hotspots_df.sort_values(["areaId", "hotspotRiskHint"], ascending=[True, False])

        metadata = RecalculationMetadata(
            recalculatedAt=datetime.now(timezone.utc),
            areasProcessed=len(results),
            source="seed_csv",
        )

        with self._lock:
            self._current_risk = self._sort_results(results)
            self._details = details
            self._hotspot_cache = hotspots_df.to_dict(orient="records")
            self._last_metadata = metadata

        return metadata

    def get_areas(self) -> list[dict[str, Any]]:
        with self._lock:
            if not self._current_risk:
                return []
            return [{"areaId": record.areaId, "areaName": record.areaName} for record in self._current_risk]

    def get_current_risk(self) -> list[dict[str, Any]]:
        with self._lock:
            return [
                {
                    "areaId": record.areaId,
                    "areaName": record.areaName,
                    "riskScore": record.riskScore,
                    "riskCategory": record.riskCategory,
                    "colorHex": record.colorHex,
                    "explanation": record.explanation,
                }
                for record in self._current_risk
            ]

    def get_area_detail(self, area_id: int) -> dict[str, Any] | None:
        with self._lock:
            return self._details.get(area_id)

    def get_hotspots(self) -> list[dict[str, Any]]:
        with self._lock:
            return list(self._hotspot_cache)

    def get_metadata(self) -> RecalculationMetadata | None:
        with self._lock:
            return self._last_metadata
