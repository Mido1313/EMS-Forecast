from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class Settings:
    project_root: Path
    data_seed_dir: Path
    geojson_path: Path
    mapping_path: Path | None
    frontend_dir: Path


def get_settings() -> Settings:
    project_root = Path(__file__).resolve().parents[2]
    data_seed_dir = Path(os.getenv("SEED_DATA_DIR", project_root / "data" / "seed"))

    custom_geojson_default = project_root / "gebiets_geojson.json"
    fallback_geojson_default = project_root / "data" / "geo" / "oo_areas_mock.geojson"
    geojson_default = custom_geojson_default if custom_geojson_default.exists() else fallback_geojson_default
    geojson_path = Path(os.getenv("AREA_GEOJSON_PATH", geojson_default))

    mapping_path: Path | None = project_root / "gebiets_mapping.json"

    frontend_dir = Path(os.getenv("FRONTEND_DIR", project_root / "frontend"))
    return Settings(
        project_root=project_root,
        data_seed_dir=data_seed_dir,
        geojson_path=geojson_path,
        mapping_path=mapping_path,
        frontend_dir=frontend_dir,
    )
