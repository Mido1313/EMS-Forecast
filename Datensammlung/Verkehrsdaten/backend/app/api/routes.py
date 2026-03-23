from __future__ import annotations

from fastapi import APIRouter, HTTPException, Request, status

from app.schemas.api import AreaDto, AreaRiskDetailDto, CurrentRiskDto, HotspotDto, RecalculateResponseDto
from app.services.geometry_service import GeometryService
from app.services.risk_service import RiskService

router = APIRouter(prefix="/api", tags=["risk"])


def _risk_service(request: Request) -> RiskService:
    return request.app.state.risk_service


def _geometry_service(request: Request) -> GeometryService:
    return request.app.state.geometry_service


@router.get("/areas", response_model=list[AreaDto])
def get_areas(request: Request) -> list[AreaDto]:
    return _risk_service(request).get_areas()


@router.get("/risk/current", response_model=list[CurrentRiskDto])
def get_current_risk(request: Request) -> list[CurrentRiskDto]:
    return _risk_service(request).get_current_risk()


@router.get("/risk/{area_id}", response_model=AreaRiskDetailDto)
def get_area_risk(area_id: int, request: Request) -> AreaRiskDetailDto:
    detail = _risk_service(request).get_area_detail(area_id)
    if not detail:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail=f"Area {area_id} not found")
    return detail


@router.post("/recalculate", response_model=RecalculateResponseDto)
def recalculate(request: Request) -> RecalculateResponseDto:
    return _risk_service(request).recalculate()


@router.get("/hotspots", response_model=list[HotspotDto])
def get_hotspots(request: Request) -> list[HotspotDto]:
    hotspots = _risk_service(request).get_hotspots()
    return hotspots


@router.get("/geometry")
def get_geometry(request: Request) -> dict:
    geometry = _geometry_service(request).load_geojson()
    if geometry is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="No geometry available. Frontend should switch to chart fallback.",
        )
    return geometry
