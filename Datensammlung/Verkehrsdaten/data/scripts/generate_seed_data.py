from __future__ import annotations

import csv
import json
import random
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SEED_DIR = ROOT / "data" / "seed"
GEO_DIR = ROOT / "data" / "geo"
NOW = datetime(2026, 3, 23, 12, 0, tzinfo=timezone.utc)
RNG = random.Random(42)


@dataclass(frozen=True)
class AreaProfile:
    name: str
    profile_type: str
    traffic_pressure: float
    accident_intensity: float
    construction_intensity: float
    commuter_peak: float
    tourism_factor: float
    base_temp_shift: float


AREA_PROFILES: dict[int, AreaProfile] = {
    1: AreaProfile("Linz + Urfahr", "urban_commuter", 0.72, 1.30, 1.20, 1.35, 0.20, 0.8),
    2: AreaProfile("Wels", "urban_commuter", 0.70, 1.20, 1.10, 1.30, 0.15, 0.6),
    3: AreaProfile("Rohrbach", "rural", 0.40, 0.70, 0.60, 0.75, 0.30, -0.2),
    4: AreaProfile("Freistadt", "rural_commuter", 0.50, 0.85, 0.70, 0.90, 0.25, -0.1),
    5: AreaProfile("Perg", "commuter_rural", 0.58, 0.95, 0.85, 1.05, 0.30, 0.1),
    6: AreaProfile("Wels-Land Nord + Linz-Land", "commuter_axis", 0.66, 1.10, 1.10, 1.25, 0.20, 0.5),
    7: AreaProfile("Steyr + Steyr-Land Nord + Kirchdorf Nord", "industrial_commuter", 0.63, 1.05, 1.00, 1.15, 0.25, 0.3),
    8: AreaProfile("Kirchdorf Süd + Steyr-Land Süd", "rural_mountain", 0.52, 0.90, 0.80, 0.85, 0.45, -0.3),
    9: AreaProfile("Gmunden Nord + Vöcklabruck Süd + Wels-Land Süd", "tourism_pendler", 0.68, 1.25, 1.00, 1.10, 1.35, 0.4),
    10: AreaProfile("Gmunden Süd", "tourism", 0.62, 1.20, 0.90, 0.95, 1.45, 0.2),
    11: AreaProfile("Ried + Vöcklabruck Nord", "mixed", 0.56, 0.95, 0.85, 1.00, 0.55, 0.2),
    12: AreaProfile("Braunau", "border_industrial", 0.57, 1.00, 0.90, 1.05, 0.35, 0.3),
    13: AreaProfile("Schärding", "rural_border", 0.48, 0.80, 0.75, 0.90, 0.30, 0.1),
    14: AreaProfile("Grieskirchen + Eferding", "rural_commuter", 0.54, 0.90, 0.80, 1.00, 0.40, 0.2),
    15: AreaProfile("Urfahr-Umgebung", "suburban_commuter", 0.60, 0.95, 0.85, 1.15, 0.35, 0.4),
}

ROAD_TEMPLATES = [
    ("Autobahn", "motorway", 110, 0.95, 8.0),
    ("Schnellstraße", "schnellstrasse", 100, 0.85, 6.5),
    ("Pendlerachse", "pendlerachse", 80, 0.75, 4.5),
    ("Regionalachse", "regional", 70, 0.65, 3.8),
]


def _round(value: float) -> float:
    return round(float(value), 2)


def _write_csv(path: Path, rows: list[dict], fieldnames: list[str]) -> None:
    with path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def generate_areas() -> list[dict]:
    return [{"areaId": area_id, "areaName": profile.name} for area_id, profile in AREA_PROFILES.items()]


def generate_hotspots() -> list[dict]:
    rows: list[dict] = []
    for area_id, profile in AREA_PROFILES.items():
        for idx, (label, road_type, free_speed, criticality, length_km) in enumerate(ROAD_TEMPLATES, start=1):
            segment_id = f"A{area_id:02d}-S{idx:02d}"
            if area_id in {9, 10} and idx == 4:
                label = "B145 Tourismusachse"
                road_type = "tourism_b145"
                free_speed = 75
                criticality = 0.92 if area_id == 10 else 0.88
                length_km = 5.5

            criticality_adj = min(1.0, criticality + (profile.traffic_pressure - 0.5) * 0.2)
            rows.append(
                {
                    "segmentId": segment_id,
                    "linkId": f"L-{segment_id}",
                    "areaId": area_id,
                    "hotspotName": f"{label} {area_id}-{idx}",
                    "roadType": road_type,
                    "criticalityWeight": round(criticality_adj, 3),
                    "freeFlowSpeedKph": free_speed,
                    "lengthKm": _round(length_km + RNG.gauss(0, 0.35)),
                    "isTouristic": int("tour" in road_type or profile.tourism_factor > 1.0),
                    "isCommuter": int("pendler" in road_type or profile.commuter_peak > 1.0),
                }
            )
    return rows


def generate_weather(hours_back: int = 72) -> list[dict]:
    timestamps = [NOW - timedelta(hours=h) for h in range(hours_back - 1, -1, -1)]
    rows: list[dict] = []

    for area_id, profile in AREA_PROFILES.items():
        for ts in timestamps:
            precipitation = max(0.0, RNG.gammavariate(1.4, 0.7) - 0.3)
            if profile.tourism_factor > 1.1 and ts.weekday() >= 4:
                precipitation *= 1.1

            diurnal = 4 * __import__("math").sin((ts.hour - 6) / 24 * 2 * __import__("math").pi)
            temperature = 8.5 + profile.base_temp_shift + diurnal + RNG.gauss(0, 1.6)
            wind = max(0.0, RNG.gauss(15, 6))
            icy = int(temperature <= 1.5 and precipitation >= 0.6)

            rows.append(
                {
                    "areaId": area_id,
                    "timestamp": ts.isoformat(),
                    "temperatureC": _round(temperature),
                    "precipitationMm": _round(precipitation),
                    "windKph": _round(wind),
                    "icyFlag": icy,
                }
            )

    return rows


def _rush_hour_multiplier(hour: int, commuter_peak: float) -> float:
    if 6 <= hour <= 9 or 15 <= hour <= 18:
        return 1.0 - 0.28 * commuter_peak
    return 1.0


def _tourism_multiplier(ts: datetime, tourism_factor: float, road_type: str) -> float:
    if "tour" not in road_type and tourism_factor < 1.0:
        return 1.0
    if ts.weekday() >= 4 and 9 <= ts.hour <= 19:
        return 1.0 - 0.24 * min(1.6, tourism_factor)
    return 1.0


def generate_traffic(hotspots: list[dict], weather: list[dict]) -> list[dict]:
    weather_lookup: dict[tuple[int, str], dict] = {(int(row["areaId"]), row["timestamp"]): row for row in weather}
    timestamps = sorted({row["timestamp"] for row in weather})
    rows: list[dict] = []

    for hotspot in hotspots:
        area_id = int(hotspot["areaId"])
        profile = AREA_PROFILES[area_id]

        for ts in timestamps:
            weather_row = weather_lookup[(area_id, ts)]
            ts_dt = datetime.fromisoformat(ts)

            speed = float(hotspot["freeFlowSpeedKph"])
            speed *= 1.0 - 0.18 * profile.traffic_pressure
            speed *= _rush_hour_multiplier(ts_dt.hour, profile.commuter_peak)
            speed *= _tourism_multiplier(ts_dt, profile.tourism_factor, str(hotspot["roadType"]))

            precipitation_factor = max(0.65, 1.0 - float(weather_row["precipitationMm"]) * 0.05)
            speed *= precipitation_factor
            if int(weather_row["icyFlag"]) == 1:
                speed *= 0.7

            speed *= RNG.gauss(1.0, 0.07)
            speed = max(12.0, speed)

            ratio = speed / float(hotspot["freeFlowSpeedKph"])
            unknown_prob = min(0.15, 0.03 + float(weather_row["precipitationMm"]) * 0.015)
            if RNG.random() < unknown_prob:
                traffic_status = "unknown"
            elif ratio >= 0.82:
                traffic_status = "free_flow"
            elif ratio >= 0.58:
                traffic_status = "slow"
            else:
                traffic_status = "congested"

            length_km = max(1.2, float(hotspot["lengthKm"]))
            travel_time = (length_km / speed) * 60 * RNG.gauss(1.0, 0.05)

            rows.append(
                {
                    "segmentId": hotspot["segmentId"],
                    "linkId": hotspot["linkId"],
                    "areaId": area_id,
                    "timestamp": ts,
                    "averageVehicleSpeed": _round(speed),
                    "travelTime": _round(max(1.0, travel_time)),
                    "trafficStatus": traffic_status,
                }
            )

    return rows


def generate_constructions(hotspots: list[dict]) -> list[dict]:
    rows: list[dict] = []
    hotspots_by_area: dict[int, list[str]] = {}
    for hotspot in hotspots:
        hotspots_by_area.setdefault(int(hotspot["areaId"]), []).append(str(hotspot["segmentId"]))

    for area_id, profile in AREA_PROFILES.items():
        n_items = max(1, int(RNG.gammavariate(2.0, 1.2) * profile.construction_intensity))
        for idx in range(1, n_items + 1):
            start_offset_days = RNG.randint(-20, 7)
            duration_days = RNG.randint(2, 20)
            start = NOW + timedelta(days=start_offset_days)
            end = start + timedelta(days=duration_days)
            severity = int(min(4, max(1, round(RNG.gauss(2.1 * profile.construction_intensity, 0.8)))))
            segment = RNG.choice(hotspots_by_area[area_id]) if RNG.random() < 0.75 else ""

            rows.append(
                {
                    "constructionId": f"C-{area_id:02d}-{idx:03d}",
                    "areaId": area_id,
                    "startTime": start.isoformat(),
                    "endTime": end.isoformat(),
                    "severity": severity,
                    "segmentId": segment,
                }
            )

    return rows


def generate_accidents(hotspots: list[dict]) -> list[dict]:
    rows: list[dict] = []
    hotspots_by_area: dict[int, list[str]] = {}
    for hotspot in hotspots:
        hotspots_by_area.setdefault(int(hotspot["areaId"]), []).append(str(hotspot["segmentId"]))

    for area_id, profile in AREA_PROFILES.items():
        n_items = max(4, int(RNG.gammavariate(2.5, 2.4) * profile.accident_intensity))
        for idx in range(1, n_items + 1):
            if area_id in {1, 2, 9, 10} and idx <= 2:
                hours_back = RNG.randint(1, 20)
            else:
                hours_back = RNG.randint(1, 24 * 10)
            ts = NOW - timedelta(hours=hours_back)

            severity = int(min(5, max(1, round(RNG.gauss(2.0 * profile.accident_intensity, 0.9)))))
            segment = RNG.choice(hotspots_by_area[area_id]) if RNG.random() < 0.82 else ""

            rows.append(
                {
                    "accidentId": f"A-{area_id:02d}-{idx:03d}",
                    "areaId": area_id,
                    "timestamp": ts.isoformat(),
                    "severity": severity,
                    "segmentId": segment,
                }
            )

    return rows


def generate_mock_geojson(areas: list[dict]) -> dict:
    min_lon, min_lat = 13.35, 47.75
    cell_w, cell_h = 0.33, 0.26
    cols = 5

    features: list[dict] = []
    for idx, area in enumerate(sorted(areas, key=lambda x: int(x["areaId"]))):
        row = idx // cols
        col = idx % cols

        lon0 = min_lon + col * cell_w
        lat0 = min_lat + (2 - row) * cell_h
        lon1 = lon0 + cell_w * 0.9
        lat1 = lat0 + cell_h * 0.9

        polygon = [
            [round(lon0, 5), round(lat0, 5)],
            [round(lon1, 5), round(lat0, 5)],
            [round(lon1, 5), round(lat1, 5)],
            [round(lon0, 5), round(lat1, 5)],
            [round(lon0, 5), round(lat0, 5)],
        ]

        features.append(
            {
                "type": "Feature",
                "properties": {
                    "areaId": int(area["areaId"]),
                    "areaName": area["areaName"],
                    "geometryType": "MOCK",
                    "note": "Mock geometry for prototype only. Replace with real OOE boundaries.",
                },
                "geometry": {"type": "Polygon", "coordinates": [polygon]},
            }
        )

    return {"type": "FeatureCollection", "features": features}


def main() -> None:
    SEED_DIR.mkdir(parents=True, exist_ok=True)
    GEO_DIR.mkdir(parents=True, exist_ok=True)

    areas = generate_areas()
    hotspots = generate_hotspots()
    weather = generate_weather(hours_back=72)
    traffic = generate_traffic(hotspots, weather)
    constructions = generate_constructions(hotspots)
    accidents = generate_accidents(hotspots)

    _write_csv(SEED_DIR / "areas.csv", areas, ["areaId", "areaName"])
    _write_csv(
        SEED_DIR / "hotspots.csv",
        hotspots,
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
    )
    _write_csv(
        SEED_DIR / "weather.csv",
        weather,
        ["areaId", "timestamp", "temperatureC", "precipitationMm", "windKph", "icyFlag"],
    )
    _write_csv(
        SEED_DIR / "traffic.csv",
        traffic,
        [
            "segmentId",
            "linkId",
            "areaId",
            "timestamp",
            "averageVehicleSpeed",
            "travelTime",
            "trafficStatus",
        ],
    )
    _write_csv(
        SEED_DIR / "constructions.csv",
        constructions,
        ["constructionId", "areaId", "startTime", "endTime", "severity", "segmentId"],
    )
    _write_csv(
        SEED_DIR / "accidents.csv",
        accidents,
        ["accidentId", "areaId", "timestamp", "severity", "segmentId"],
    )

    geojson = generate_mock_geojson(areas)
    with (GEO_DIR / "oo_areas_mock.geojson").open("w", encoding="utf-8") as f:
        json.dump(geojson, f, ensure_ascii=False, indent=2)


if __name__ == "__main__":
    main()
