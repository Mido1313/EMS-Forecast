from __future__ import annotations

from datetime import datetime
from typing import Any

from pydantic import BaseModel, Field


class Area(BaseModel):
    areaId: int
    areaName: str


class TrafficRecord(BaseModel):
    segmentId: str
    linkId: str
    areaId: int
    timestamp: datetime
    averageVehicleSpeed: float
    travelTime: float
    trafficStatus: str


class ConstructionRecord(BaseModel):
    constructionId: str
    areaId: int
    startTime: datetime
    endTime: datetime
    severity: int
    segmentId: str | None = None


class AccidentRecord(BaseModel):
    accidentId: str
    areaId: int
    timestamp: datetime
    severity: int
    segmentId: str | None = None


class WeatherRecord(BaseModel):
    areaId: int
    timestamp: datetime
    temperatureC: float
    precipitationMm: float
    windKph: float
    icyFlag: int


class AreaFeatures(BaseModel):
    areaId: int
    areaName: str
    generatedAt: datetime
    metrics: dict[str, float]


class AreaRiskResult(BaseModel):
    areaId: int
    areaName: str
    riskScore: float = Field(ge=0.0, le=1.0)
    riskCategory: int = Field(ge=1, le=5)
    colorHex: str
    explanation: str
    components: dict[str, float]
    metrics: dict[str, Any]


class RecalculationMetadata(BaseModel):
    recalculatedAt: datetime
    areasProcessed: int
    source: str
