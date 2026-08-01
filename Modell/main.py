"""
FastAPI-Service für EMS-Forecasts.
Startet beim Hochfahren den Scheduler und führt sofort einen ersten Forecast-Cycle aus.
"""

import logging
from contextlib import asynccontextmanager
from pathlib import Path

from fastapi import FastAPI
from fastapi.responses import JSONResponse

from src.model import load_model
from src.scheduler import create_scheduler, run_forecast_cycle

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s  %(levelname)-8s  %(name)s – %(message)s",
)
logger = logging.getLogger(__name__)

MODEL_PATH = Path(__file__).parent / "models" / "ems_forecast_rf_v2.pkl"
MODEL_VERSION = "rf_v2"

_scheduler = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    global _scheduler

    # Modell vorladen und cachen
    try:
        load_model(MODEL_PATH)
        logger.info("Modell erfolgreich geladen: %s", MODEL_PATH)
    except FileNotFoundError:
        logger.warning("Modelldatei nicht gefunden – Service startet trotzdem.")

    # Scheduler starten
    _scheduler = create_scheduler()
    _scheduler.start()
    logger.info("Scheduler gestartet.")

    # Ersten Cycle sofort ausführen
    run_forecast_cycle()

    yield  # App läuft

    # Shutdown
    if _scheduler and _scheduler.running:
        _scheduler.shutdown(wait=False)
        logger.info("Scheduler gestoppt.")


app = FastAPI(
    title="EMS Forecast API",
    description="Stündliche Einsatz-Vorhersagen für das Rote Kreuz OÖ",
    version="1.0.0",
    lifespan=lifespan,
)


@app.get("/health")
def health_check():
    """Gibt den Status des Services und die aktive Modellversion zurück."""
    return {"status": "ok", "model_version": MODEL_VERSION}


@app.get("/forecast/trigger")
def manual_trigger():
    """Löst manuell einen Forecast-Cycle aus – für Tests ohne auf den Scheduler zu warten."""
    run_forecast_cycle()
    return JSONResponse({"status": "cycle triggered"})
