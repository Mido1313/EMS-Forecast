from __future__ import annotations

import json
from pathlib import Path


class GeometryService:
    def __init__(self, geojson_path: Path) -> None:
        self._geojson_path = geojson_path

    def load_geojson(self) -> dict | None:
        if not self._geojson_path.exists():
            return None
        with self._geojson_path.open("r", encoding="utf-8") as f:
            return json.load(f)
