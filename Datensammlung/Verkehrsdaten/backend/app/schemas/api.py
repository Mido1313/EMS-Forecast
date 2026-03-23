from __future__ import annotations

from datetime import datetime
from typing import Any

from pydantic import BaseModel, Field


class AreaDto(BaseModel):
    areaId: int
    areaName: str


class CurrentRiskDto(BaseModel):
    areaId: int
    areaName: str
    riskScore: float = Field(ge=0.0, le=1.0)
    riskCategory: int = Field(ge=1, le=5)
    colorHex: str
    explanation: str


class AreaRiskDetailDto(CurrentRiskDto):
    components: dict[str, float]
    metrics: dict[str, Any]


class RecalculateResponseDto(BaseModel):
    recalculatedAt: datetime
    areasProcessed: int
    source: str


class HotspotDto(BaseModel):
    segmentId: str
    areaId: int
    hotspotName: str
    roadType: str
    criticalityWeight: float
    freeFlowSpeedKph: float
    avgSpeed24h: float
    latestSpeed: float
    avgTravelTime24h: float
    unknownStatusRatio: float
    congestionIndex: float
    hotspotRiskHint: float
