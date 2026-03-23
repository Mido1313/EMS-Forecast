from __future__ import annotations

from dataclasses import dataclass

import numpy as np
import pandas as pd
from sklearn.preprocessing import MinMaxScaler

from app.constants import RISK_LEVELS, RISK_WEIGHTS


@dataclass(frozen=True)
class RiskClassification:
    category: int
    label: str
    color_hex: str


class TransparentRiskModel:
    """Rule-based, transparent scoring model with explicit weighted components."""

    def __init__(self) -> None:
        self._weights = RISK_WEIGHTS

    @staticmethod
    def _clip(series: pd.Series, min_value: float = 0.0, max_value: float = 1.0) -> pd.Series:
        return series.clip(lower=min_value, upper=max_value)

    @staticmethod
    def _normalize(df: pd.DataFrame, columns: list[str]) -> pd.DataFrame:
        normalized = pd.DataFrame(index=df.index)
        if df.empty:
            for col in columns:
                normalized[f"norm_{col}"] = 0.0
            return normalized

        scaler = MinMaxScaler()
        values = df[columns].fillna(0.0).astype(float)

        if len(values) == 1:
            for col in columns:
                normalized[f"norm_{col}"] = 0.0
            return normalized

        scaled = scaler.fit_transform(values)
        for idx, col in enumerate(columns):
            normalized[f"norm_{col}"] = scaled[:, idx]
        return normalized

    @staticmethod
    def _classify_risk(score: float) -> RiskClassification:
        score = float(np.clip(score, 0.0, 1.0))
        for level in RISK_LEVELS:
            if score <= level.max_score:
                return RiskClassification(level.category, level.label, level.color_hex)
        highest = RISK_LEVELS[-1]
        return RiskClassification(highest.category, highest.label, highest.color_hex)

    def score(self, feature_df: pd.DataFrame) -> pd.DataFrame:
        df = feature_df.copy()

        normalize_cols = [
            "activeConstructionCount",
            "accidentCount24h",
            "accidentCount7d",
            "precipitationMm",
            "windKph",
            "avgDelayIndex",
        ]
        normalized = self._normalize(df, normalize_cols)
        df = pd.concat([df, normalized], axis=1)

        speed_penalty = self._clip((90.0 - df["avgSpeed"]) / 90.0)
        min_speed_penalty = self._clip((55.0 - df["minSpeed"]) / 55.0)
        delay_penalty = self._clip((df["avgDelayIndex"] - 1.0) / 1.2)
        unknown_penalty = self._clip(df["unknownRatioTrafficStatus"])

        traffic_component = self._clip(
            0.35 * speed_penalty + 0.20 * min_speed_penalty + 0.30 * delay_penalty + 0.15 * unknown_penalty
        )

        construction_component = self._clip(
            0.65 * df["norm_activeConstructionCount"] + 0.35 * self._clip(df["constructionSeverityIndex"] / 4.0)
        )

        accident_component = self._clip(
            0.55 * df["norm_accidentCount24h"]
            + 0.30 * df["norm_accidentCount7d"]
            + 0.15 * self._clip(df["accidentCount24h"] / 6.0)
        )

        low_temp_penalty = self._clip((2.0 - df["temperatureC"]) / 10.0)
        weather_component = self._clip(
            0.40 * df["norm_precipitationMm"]
            + 0.20 * df["norm_windKph"]
            + 0.25 * self._clip(df["icyFlag"])
            + 0.15 * low_temp_penalty
        )

        rush_hour = ((df["hour"].between(6, 9)) | (df["hour"].between(15, 18))).astype(float)
        tourist_window = ((df["isWeekend"] == 1) & (df["hour"].between(10, 19))).astype(float)
        time_component = self._clip(
            0.70 * rush_hour * df["commuterHotspotRatio"]
            + 0.30 * tourist_window * df["touristicHotspotRatio"]
            + 0.10 * df["isHoliday"]
        )

        hotspot_component = self._clip(df["hotspotCriticalityScore"])

        component_df = pd.DataFrame(
            {
                "traffic": traffic_component,
                "construction": construction_component,
                "accident": accident_component,
                "weather": weather_component,
                "time": time_component,
                "hotspot": hotspot_component,
            }
        )

        weighted_contrib = pd.DataFrame(index=component_df.index)
        for key, weight in self._weights.items():
            weighted_contrib[key] = component_df[key] * weight

        risk_score = weighted_contrib.sum(axis=1).clip(0.0, 1.0)

        categories = []
        labels = []
        colors = []
        for value in risk_score:
            cls = self._classify_risk(float(value))
            categories.append(cls.category)
            labels.append(cls.label)
            colors.append(cls.color_hex)

        result = df[["areaId", "areaName"]].copy()
        result["riskScore"] = risk_score.round(4)
        result["riskCategory"] = categories
        result["riskLabel"] = labels
        result["colorHex"] = colors

        for col in component_df.columns:
            result[f"component_{col}"] = component_df[col].round(4)
            result[f"contrib_{col}"] = weighted_contrib[col].round(4)

        return result

    @staticmethod
    def build_explanation(row: pd.Series) -> str:
        contribution_labels: dict[str, str] = {
            "traffic": f"niedrigem Verkehrsfluss (Ø {row['avgSpeed']:.1f} km/h, Minimum {row['minSpeed']:.1f} km/h)",
            "construction": f"{int(row['activeConstructionCount'])} aktiven Baustellen",
            "accident": f"erhöhter Unfallzahl (24h: {int(row['accidentCount24h'])}, 7d: {int(row['accidentCount7d'])})",
            "weather": f"Wetterbelastung ({row['precipitationMm']:.1f} mm Niederschlag, Wind {row['windKph']:.0f} km/h)",
            "time": f"Zeitfaktor (Stunde {int(row['hour'])}, Wochentag {int(row['weekday'])})",
            "hotspot": f"kritischer Hotspot-Struktur (Score {row['hotspotCriticalityScore']:.2f})",
        }

        ranked = sorted(
            [
                ("traffic", float(row["contrib_traffic"])),
                ("construction", float(row["contrib_construction"])),
                ("accident", float(row["contrib_accident"])),
                ("weather", float(row["contrib_weather"])),
                ("time", float(row["contrib_time"])),
                ("hotspot", float(row["contrib_hotspot"])),
            ],
            key=lambda x: x[1],
            reverse=True,
        )

        top = [name for name, value in ranked if value >= 0.02][:3]
        if not top:
            top = [ranked[0][0]]

        parts = [contribution_labels[name] for name in top]
        if len(parts) == 1:
            because = parts[0]
        elif len(parts) == 2:
            because = f"{parts[0]} und {parts[1]}"
        else:
            because = f"{parts[0]}, {parts[1]} und {parts[2]}"

        return (
            f"Gebiet {int(row['areaId'])} ist aktuell {row['riskLabel']} eingestuft wegen "
            f"{because}."
        )
