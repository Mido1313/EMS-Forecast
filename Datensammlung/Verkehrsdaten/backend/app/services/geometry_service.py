from __future__ import annotations

import csv
import json
import re
from pathlib import Path
from typing import Any


def _normalize_name(value: str) -> str:
    value = (value or "").strip().lower()
    replacements = {
        "ä": "ae",
        "ö": "oe",
        "ü": "ue",
        "ß": "ss",
    }
    for source, target in replacements.items():
        value = value.replace(source, target)
    value = re.sub(r"[^a-z0-9]+", "", value)
    return value


class GeometryService:
    def __init__(self, geojson_path: Path, mapping_path: Path | None = None, areas_csv_path: Path | None = None) -> None:
        self._geojson_path = geojson_path
        self._mapping_path = mapping_path
        self._areas_csv_path = areas_csv_path

    @staticmethod
    def _load_json_documents(raw_text: str) -> list[dict[str, Any]]:
        raw_text = raw_text.strip()
        if not raw_text:
            return []

        try:
            loaded = json.loads(raw_text)
            return [loaded] if isinstance(loaded, dict) else []
        except json.JSONDecodeError:
            pass

        # Fallback for concatenated JSON objects in one file.
        decoder = json.JSONDecoder()
        docs: list[dict[str, Any]] = []
        idx = 0
        size = len(raw_text)

        while idx < size:
            while idx < size and raw_text[idx].isspace():
                idx += 1
            if idx >= size:
                break

            obj, end = decoder.raw_decode(raw_text, idx)
            if isinstance(obj, dict):
                docs.append(obj)
            idx = end

        return docs

    def _load_area_names(self) -> dict[int, str]:
        if not self._areas_csv_path or not self._areas_csv_path.exists():
            return {}

        area_names: dict[int, str] = {}
        with self._areas_csv_path.open("r", encoding="utf-8") as f:
            reader = csv.DictReader(f)
            for row in reader:
                try:
                    area_id = int(row.get("areaId", ""))
                except ValueError:
                    continue
                area_name = (row.get("areaName") or "").strip()
                if area_name:
                    area_names[area_id] = area_name
        return area_names

    def _load_mapping(self) -> tuple[dict[str, int], dict[str, int], dict[int, str]]:
        if not self._mapping_path or not self._mapping_path.exists():
            return {}, {}, self._load_area_names()

        docs = self._load_json_documents(self._mapping_path.read_text(encoding="utf-8"))

        iso_to_area: dict[str, int] = {}
        name_to_area: dict[str, int] = {}
        area_names = self._load_area_names()

        for doc in docs:
            for raw_key, raw_value in doc.items():
                key = str(raw_key).strip()

                # Support structured mappings like {"isoToArea": {...}, "nameToArea": {...}}.
                if isinstance(raw_value, dict):
                    target_norm = _normalize_name(key)
                    for sub_key, sub_value in raw_value.items():
                        try:
                            area_id = int(sub_value)
                        except (ValueError, TypeError):
                            continue

                        sub_key_str = str(sub_key).strip()
                        if "iso" in target_norm:
                            iso_to_area[sub_key_str] = area_id
                        elif "name" in target_norm or "gebiet" in target_norm:
                            normalized = _normalize_name(sub_key_str)
                            if normalized:
                                name_to_area[normalized] = area_id
                            if area_id not in area_names:
                                area_names[area_id] = sub_key_str
                    continue

                try:
                    area_id = int(raw_value)
                except (ValueError, TypeError):
                    continue

                if key.isdigit():
                    iso_to_area[key] = area_id
                else:
                    normalized = _normalize_name(key)
                    if normalized:
                        name_to_area[normalized] = area_id
                    if area_id not in area_names:
                        area_names[area_id] = key

        return iso_to_area, name_to_area, area_names

    def _map_features_to_areas(self, geojson: dict[str, Any]) -> dict[str, Any]:
        features = geojson.get("features")
        if not isinstance(features, list):
            return geojson

        iso_to_area, name_to_area, area_names = self._load_mapping()
        if not iso_to_area and not name_to_area:
            return geojson

        mapped_features: list[dict[str, Any]] = []
        missing_count = 0

        for feature in features:
            if not isinstance(feature, dict):
                continue

            properties = feature.get("properties") or {}
            if not isinstance(properties, dict):
                properties = {}

            iso = str(properties.get("iso") or "").strip()
            name = str(properties.get("name") or "").strip()

            area_id = iso_to_area.get(iso)
            if area_id is None:
                area_id = name_to_area.get(_normalize_name(name))

            if area_id is None:
                missing_count += 1
                continue

            updated_properties = dict(properties)
            updated_properties["areaId"] = int(area_id)
            if area_id in area_names:
                updated_properties["areaName"] = area_names[area_id]

            mapped_features.append(
                {
                    "type": "Feature",
                    "properties": updated_properties,
                    "geometry": feature.get("geometry"),
                }
            )

        if not mapped_features:
            return geojson

        result = dict(geojson)
        result["features"] = mapped_features
        result["mappingSummary"] = {
            "totalFeaturesInput": len(features),
            "totalFeaturesMapped": len(mapped_features),
            "totalFeaturesMissing": missing_count,
            "mappedAreas": sorted({f.get("properties", {}).get("areaId") for f in mapped_features}),
        }
        return result

    def load_geojson(self) -> dict | None:
        if not self._geojson_path.exists():
            return None

        with self._geojson_path.open("r", encoding="utf-8") as f:
            geojson = json.load(f)

        return self._map_features_to_areas(geojson)
