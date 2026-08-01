"""
Modell laden und Vorhersagen berechnen.
Das geladene Modell wird gecacht, damit es nur einmal von Disk gelesen wird.
"""

import logging
import joblib
import pandas as pd
from datetime import datetime
from pathlib import Path

from src.features import (
    FEATURE_COLUMNS, FEATURES_V2, GEBIET_COORDS,
    build_feature_row, build_feature_row_v2,
)

logger = logging.getLogger(__name__)

_model_cache: dict = {}


def load_model(path: str | Path):
    """Lädt ein .pkl-Modell und cached es im Prozess-Speicher."""
    key = str(path)
    if key not in _model_cache:
        _model_cache[key] = joblib.load(path)
    return _model_cache[key]


def predict(model, gebiet_id: int, dt: datetime) -> float:
    """Vorhersage mit v1-Feature-Vektor (7 Zeit-Features)."""
    row = build_feature_row(gebiet_id, dt)
    X = pd.DataFrame([row])[FEATURE_COLUMNS]
    return max(0.0, float(model.predict(X)[0]))


def fetch_live_weather(gebiet_id: int) -> dict:
    """
    Ruft aktuelle Stundenwetterdaten für ein Gebiet von der Open-Meteo Forecast-API ab.
    Gibt temperature, precipitation, snowfall, windspeed zurück.
    Fallback auf Nullwerte bei Fehler.
    """
    try:
        import requests
        lat, lon = GEBIET_COORDS[gebiet_id]
        url = (
            f"https://api.open-meteo.com/v1/forecast"
            f"?latitude={lat}&longitude={lon}"
            f"&hourly=temperature_2m,precipitation,snowfall,windspeed_10m"
            f"&forecast_days=2&timezone=Europe/Vienna"
        )
        resp = requests.get(url, timeout=10)
        resp.raise_for_status()
        data = resp.json()["hourly"]

        # Aktuelle Stunde finden
        now_str = datetime.now().strftime("%Y-%m-%dT%H:00")
        times = data["time"]
        idx = next((i for i, t in enumerate(times) if t == now_str), 0)

        return {
            "temperature":  data["temperature_2m"][idx],
            "precipitation": data["precipitation"][idx],
            "snowfall":     data["snowfall"][idx],
            "windspeed":    data["windspeed_10m"][idx],
        }
    except Exception as e:
        logger.warning("Wetter-API Fehler (Gebiet %d): %s – verwende Fallback 0.", gebiet_id, e)
        return {"temperature": 10.0, "precipitation": 0.0, "snowfall": 0.0, "windspeed": 0.0}


def predict_v2(
    model,
    gebiet_id: int,
    dt: datetime,
    weather_data: dict | None = None,
) -> float:
    """
    Vorhersage mit v2-Feature-Vektor (19 Features inkl. Wetter + statische Gebiet-Daten).
    weather_data optional vorberechnet – wenn None, wird Live-API abgerufen.
    """
    if weather_data is None:
        weather_data = fetch_live_weather(gebiet_id)
    row = build_feature_row_v2(gebiet_id, dt, weather_data)
    X = pd.DataFrame([row])[FEATURES_V2]
    return max(0.0, float(model.predict(X)[0]))
