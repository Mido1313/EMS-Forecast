from __future__ import annotations

from fastapi import FastAPI
from fastapi.responses import FileResponse, JSONResponse
from fastapi.staticfiles import StaticFiles

from app.api.routes import router as api_router
from app.config import get_settings
from app.services.data_repository import SeedDataRepository
from app.services.feature_engineering import FeatureEngineeringService
from app.services.geometry_service import GeometryService
from app.services.risk_model import TransparentRiskModel
from app.services.risk_service import RiskService


settings = get_settings()

repository = SeedDataRepository(settings)
feature_engineering = FeatureEngineeringService()
risk_model = TransparentRiskModel()
risk_service = RiskService(repository=repository, feature_engineering=feature_engineering, model=risk_model)
geometry_service = GeometryService(settings.geojson_path)

app = FastAPI(
    title="Rettungsdienst-Risiko-Prototyp OOE",
    version="0.1.0",
    description="Transparentes Vorhersagesystem fuer Einsatzwahrscheinlichkeiten in Oberoesterreich.",
)

app.state.risk_service = risk_service
app.state.geometry_service = geometry_service

app.include_router(api_router)

frontend_dir = settings.frontend_dir
if frontend_dir.exists():
    app.mount("/static", StaticFiles(directory=str(frontend_dir)), name="static")


@app.on_event("startup")
def startup_calculation() -> None:
    risk_service.recalculate()


@app.get("/", include_in_schema=False, response_model=None)
def root():
    index_path = frontend_dir / "index.html"
    if index_path.exists():
        return FileResponse(index_path)
    return JSONResponse({"status": "ok", "message": "Frontend not found."})


@app.get("/health", include_in_schema=False)
def health() -> dict:
    metadata = risk_service.get_metadata()
    return {
        "status": "ok",
        "recalculatedAt": metadata.recalculatedAt.isoformat() if metadata else None,
    }
