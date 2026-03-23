from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True)
class RiskLevel:
    category: int
    label: str
    color_hex: str
    min_score: float
    max_score: float


RISK_LEVELS: list[RiskLevel] = [
    RiskLevel(category=1, label="sehr gering", color_hex="#1b5e20", min_score=0.00, max_score=0.20),
    RiskLevel(category=2, label="gering", color_hex="#7cb342", min_score=0.20, max_score=0.40),
    RiskLevel(category=3, label="mittel", color_hex="#fdd835", min_score=0.40, max_score=0.60),
    RiskLevel(category=4, label="hoch", color_hex="#fb8c00", min_score=0.60, max_score=0.80),
    RiskLevel(category=5, label="kritisch", color_hex="#d32f2f", min_score=0.80, max_score=1.00),
]

AREA_ORDER: list[int] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]

RISK_WEIGHTS: dict[str, float] = {
    "traffic": 0.38,
    "construction": 0.12,
    "accident": 0.22,
    "weather": 0.14,
    "time": 0.09,
    "hotspot": 0.05,
}

FEATURE_COLUMNS: list[str] = [
    "avgSpeed",
    "minSpeed",
    "avgTravelTime",
    "freeFlowTravelTime",
    "avgDelayIndex",
    "unknownRatioTrafficStatus",
    "activeConstructionCount",
    "constructionSeverityIndex",
    "accidentCount24h",
    "accidentCount7d",
    "hotspotCriticalityScore",
    "temperatureC",
    "precipitationMm",
    "windKph",
    "icyFlag",
    "hour",
    "weekday",
    "month",
    "isWeekend",
    "isHoliday",
    "commuterHotspotRatio",
    "touristicHotspotRatio",
]
