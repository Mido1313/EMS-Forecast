"""
Stündlicher Forecast-Job via APScheduler.
Berechnet Vorhersagen für alle 15 Gebiete × 5 Horizonte und schreibt sie in PostgreSQL.
"""

import logging
from datetime import datetime, timezone
from pathlib import Path

from apscheduler.schedulers.background import BackgroundScheduler

from src.features import FORECAST_HORIZONS_H, N_GEBIETE, count_to_risk_level
from src.model import load_model, predict_v2, fetch_live_weather
from src.database import SessionLocal, write_forecast

logger = logging.getLogger(__name__)

MODEL_PATH = Path(__file__).parent.parent / "models" / "ems_forecast_rf_v2.pkl"
MODEL_VERSION = "rf_v2"

_MAX_PER_GEBIET: dict[int, float] = {g: 10.0 for g in range(1, N_GEBIETE + 1)}


def run_forecast_cycle() -> None:
    """
    Kern-Job: für jede Kombination gebiet_id × horizon_h einen Forecast berechnen
    und in die DB schreiben.
    Wetterdaten werden einmalig pro Cycle für alle Gebiete vorgeholt.
    """
    logger.info("Forecast-Cycle gestartet: %s", datetime.now(timezone.utc).isoformat())

    try:
        model = load_model(MODEL_PATH)
    except FileNotFoundError:
        logger.error("Modelldatei nicht gefunden: %s – Cycle abgebrochen.", MODEL_PATH)
        return

    if SessionLocal is None:
        logger.error("DATABASE_URL nicht gesetzt – Cycle abgebrochen.")
        return

    # Wetterdaten für alle Gebiete vorab holen (1 API-Call pro Gebiet)
    weather_cache: dict[int, dict] = {}
    for gid in range(1, N_GEBIETE + 1):
        weather_cache[gid] = fetch_live_weather(gid)

    now = datetime.now(timezone.utc)
    db = SessionLocal()
    try:
        for gebiet_id in range(1, N_GEBIETE + 1):
            weather = weather_cache[gebiet_id]
            for horizon_h in FORECAST_HORIZONS_H:
                predicted = predict_v2(model, gebiet_id, now, weather_data=weather)
                risk = count_to_risk_level(predicted, _MAX_PER_GEBIET[gebiet_id])
                write_forecast(
                    db=db,
                    gebiet_id=gebiet_id,
                    horizon_h=horizon_h,
                    predicted_count=predicted,
                    risk_level=risk,
                    model_version=MODEL_VERSION,
                )
        logger.info(
            "Forecast-Cycle abgeschlossen: %d Gebiete × %d Horizonte geschrieben.",
            N_GEBIETE, len(FORECAST_HORIZONS_H),
        )
    except Exception:
        logger.exception("Fehler im Forecast-Cycle:")
        db.rollback()
    finally:
        db.close()


def create_scheduler() -> BackgroundScheduler:
    """Erstellt und konfiguriert den APScheduler (noch nicht gestartet)."""
    scheduler = BackgroundScheduler(timezone="UTC")
    scheduler.add_job(
        run_forecast_cycle,
        trigger="interval",
        minutes=60,
        id="forecast_cycle",
        replace_existing=True,
    )
    return scheduler
