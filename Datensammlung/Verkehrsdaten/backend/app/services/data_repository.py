from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

import pandas as pd

from app.config import Settings


@dataclass
class DataBundle:
    areas: pd.DataFrame
    hotspots: pd.DataFrame
    traffic: pd.DataFrame
    constructions: pd.DataFrame
    accidents: pd.DataFrame
    weather: pd.DataFrame


class SeedDataRepository:
    def __init__(self, settings: Settings) -> None:
        self._seed_dir = settings.data_seed_dir

    def _csv_path(self, filename: str) -> Path:
        path = self._seed_dir / filename
        if not path.exists():
            raise FileNotFoundError(f"Seed data file not found: {path}")
        return path

    @staticmethod
    def _require_columns(df: pd.DataFrame, expected: list[str], file_label: str) -> pd.DataFrame:
        missing = [col for col in expected if col not in df.columns]
        if missing:
            raise ValueError(f"{file_label} is missing columns: {missing}")
        return df

    def load_areas(self) -> pd.DataFrame:
        df = pd.read_csv(self._csv_path("areas.csv"))
        self._require_columns(df, ["areaId", "areaName"], "areas.csv")
        df["areaId"] = df["areaId"].astype(int)
        return df.sort_values("areaId").reset_index(drop=True)

    def load_hotspots(self) -> pd.DataFrame:
        df = pd.read_csv(self._csv_path("hotspots.csv"))
        self._require_columns(
            df,
            [
                "segmentId",
                "linkId",
                "areaId",
                "hotspotName",
                "roadType",
                "criticalityWeight",
                "freeFlowSpeedKph",
                "lengthKm",
                "isTouristic",
                "isCommuter",
            ],
            "hotspots.csv",
        )
        df["areaId"] = df["areaId"].astype(int)
        return df

    def load_traffic(self) -> pd.DataFrame:
        df = pd.read_csv(self._csv_path("traffic.csv"), parse_dates=["timestamp"])
        self._require_columns(
            df,
            ["segmentId", "linkId", "areaId", "timestamp", "averageVehicleSpeed", "travelTime", "trafficStatus"],
            "traffic.csv",
        )
        df["areaId"] = df["areaId"].astype(int)
        df["timestamp"] = pd.to_datetime(df["timestamp"], utc=True)
        return df

    def load_constructions(self) -> pd.DataFrame:
        df = pd.read_csv(self._csv_path("constructions.csv"), parse_dates=["startTime", "endTime"])
        self._require_columns(df, ["constructionId", "areaId", "startTime", "endTime", "severity", "segmentId"], "constructions.csv")
        df["areaId"] = df["areaId"].astype(int)
        df["startTime"] = pd.to_datetime(df["startTime"], utc=True)
        df["endTime"] = pd.to_datetime(df["endTime"], utc=True)
        return df

    def load_accidents(self) -> pd.DataFrame:
        df = pd.read_csv(self._csv_path("accidents.csv"), parse_dates=["timestamp"])
        self._require_columns(df, ["accidentId", "areaId", "timestamp", "severity", "segmentId"], "accidents.csv")
        df["areaId"] = df["areaId"].astype(int)
        df["timestamp"] = pd.to_datetime(df["timestamp"], utc=True)
        return df

    def load_weather(self) -> pd.DataFrame:
        df = pd.read_csv(self._csv_path("weather.csv"), parse_dates=["timestamp"])
        self._require_columns(
            df,
            ["areaId", "timestamp", "temperatureC", "precipitationMm", "windKph", "icyFlag"],
            "weather.csv",
        )
        df["areaId"] = df["areaId"].astype(int)
        df["timestamp"] = pd.to_datetime(df["timestamp"], utc=True)
        return df

    def load_bundle(self) -> DataBundle:
        return DataBundle(
            areas=self.load_areas(),
            hotspots=self.load_hotspots(),
            traffic=self.load_traffic(),
            constructions=self.load_constructions(),
            accidents=self.load_accidents(),
            weather=self.load_weather(),
        )
