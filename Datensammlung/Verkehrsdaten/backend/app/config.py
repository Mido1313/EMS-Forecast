from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class Settings:
    project_root: Path
    data_seed_dir: Path
    geojson_path: Path
    frontend_dir: Path


def get_settings() -> Settings:
    project_root = Path(__file__).resolve().parents[2]
    data_seed_dir = Path(os.getenv("SEED_DATA_DIR", project_root / "data" / "seed"))
    geojson_path = Path(os.getenv("AREA_GEOJSON_PATH", project_root / "data" / "geo" / "oo_areas_mock.geojson"))
    frontend_dir = Path(os.getenv("FRONTEND_DIR", project_root / "frontend"))
    return Settings(
        project_root=project_root,
        data_seed_dir=data_seed_dir,
        geojson_path=geojson_path,
        frontend_dir=frontend_dir,
    )
