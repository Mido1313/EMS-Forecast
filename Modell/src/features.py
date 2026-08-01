"""
Feature Engineering Konstanten und Hilfsfunktionen für das EMS-Forecast-Modell.
Dieses Modul wird von Notebooks UND dem FastAPI-Service verwendet.
"""

from datetime import datetime

# ---------------------------------------------------------------------------
# PLZ → Gebiet-Mapping (OÖ, 15 Gebiete) – Quelle: Datensammlung/PLZ_Liste.xlsx
# ---------------------------------------------------------------------------
PLZ_TO_GEBIET: dict[int, int] = {
    # Gebiet 1 – Linz (Kernstadt)
    4020: 1, 4030: 1, 4040: 1, 4048: 1, 4221: 1,
    # Gebiet 2 – Wels Stadt
    4600: 2, 4621: 2, 4622: 2, 4623: 2, 4631: 2, 4633: 2,
    4642: 2,
    # Gebiet 3 – Rohrbach
    4113: 3, 4114: 3, 4115: 3, 4116: 3, 4120: 3, 4121: 3,
    4122: 3, 4131: 3, 4132: 3, 4133: 3, 4134: 3, 4141: 3,
    4142: 3, 4143: 3, 4144: 3, 4150: 3, 4151: 3, 4152: 3,
    4153: 3, 4154: 3, 4155: 3, 4160: 3, 4161: 3, 4162: 3,
    4163: 3, 4164: 3, 4170: 3, 4171: 3, 4172: 3, 4173: 3,
    4174: 3, 4184: 3,
    # Gebiet 4 – Freistadt
    3925: 4, 4192: 4, 4193: 4, 4212: 4, 4213: 4, 4230: 4,
    4240: 4, 4242: 4, 4251: 4, 4254: 4, 4261: 4, 4262: 4,
    4263: 4, 4264: 4, 4271: 4, 4272: 4, 4273: 4, 4274: 4,
    4280: 4, 4281: 4, 4282: 4, 4283: 4, 4284: 4, 4291: 4,
    4292: 4, 4293: 4, 4294: 4,
    # Gebiet 5 – Perg
    4222: 5, 4223: 5, 4224: 5, 4225: 5, 4310: 5, 4311: 5,
    4312: 5, 4320: 5, 4322: 5, 4323: 5, 4324: 5, 4331: 5,
    4332: 5, 4341: 5, 4342: 5, 4343: 5, 4351: 5, 4352: 5,
    4360: 5, 4362: 5, 4363: 5, 4364: 5, 4371: 5, 4372: 5,
    4381: 5, 4382: 5, 4391: 5, 4392: 5,
    # Gebiet 6 – Linz-Land
    4050: 6, 4052: 6, 4053: 6, 4054: 6, 4055: 6, 4060: 6,
    4061: 6, 4062: 6, 4063: 6, 4064: 6, 4073: 6, 4470: 6,
    4481: 6, 4483: 6, 4490: 6, 4491: 6, 4492: 6, 4501: 6,
    4502: 6, 4511: 6, 4521: 6, 4531: 6, 4611: 6, 4612: 6,
    4613: 6, 4614: 6, 4615: 6, 4616: 6,
    # Gebiet 7 – Steyr
    4400: 7, 4407: 7, 4421: 7, 4442: 7, 4451: 7, 4484: 7,
    4493: 7, 4522: 7, 4523: 7, 4532: 7, 4533: 7, 4540: 7,
    4541: 7, 4542: 7, 4550: 7, 4551: 7, 4552: 7, 4553: 7,
    4554: 7,
    # Gebiet 8 – Kirchdorf + Pyhrn
    3334: 8, 3335: 8, 4443: 8, 4452: 8, 4453: 8, 4460: 8,
    4461: 8, 4462: 8, 4463: 8, 4464: 8, 4560: 8, 4562: 8,
    4563: 8, 4564: 8, 4565: 8, 4571: 8, 4572: 8, 4573: 8,
    4574: 8, 4575: 8, 4580: 8, 4581: 8, 4582: 8, 4591: 8,
    4592: 8, 4593: 8, 4594: 8, 4595: 8, 4596: 8,
    # Gebiet 9 – Vöcklabruck + Gmunden Nord
    4624: 9, 4625: 9, 4641: 9, 4643: 9, 4644: 9, 4645: 9,
    4650: 9, 4651: 9, 4652: 9, 4653: 9, 4654: 9, 4655: 9,
    4656: 9, 4661: 9, 4662: 9, 4663: 9, 4664: 9, 4671: 9,
    4672: 9, 4690: 9, 4691: 9, 4692: 9, 4693: 9, 4694: 9,
    4800: 9, 4810: 9, 4812: 9, 4813: 9, 4814: 9, 4816: 9,
    4817: 9, 4840: 9, 4841: 9, 4842: 9, 4844: 9, 4845: 9,
    4846: 9, 4849: 9, 4850: 9, 4851: 9, 4852: 9, 4853: 9,
    4854: 9, 4860: 9, 4861: 9, 4863: 9, 4864: 9, 4865: 9,
    4866: 9, 4870: 9, 4872: 9, 4880: 9, 4881: 9, 4882: 9,
    4893: 9, 4894: 9, 5310: 9, 5311: 9,
    # Gebiet 10 – Salzkammergut
    4801: 10, 4802: 10, 4820: 10, 4821: 10, 4822: 10, 4823: 10,
    4824: 10, 4825: 10, 4830: 10, 4831: 10,
    # Gebiet 11 – Ried im Innkreis
    4743: 11, 4751: 11, 4752: 11, 4753: 11, 4754: 11, 4843: 11,
    4871: 11, 4873: 11, 4890: 11, 4891: 11, 4892: 11, 4906: 11,
    4910: 11, 4911: 11, 4912: 11, 4920: 11, 4921: 11, 4922: 11,
    4923: 11, 4924: 11, 4925: 11, 4926: 11, 4931: 11, 4932: 11,
    4933: 11, 4941: 11, 4942: 11, 4943: 11, 4970: 11, 4971: 11,
    4972: 11, 4973: 11, 4974: 11, 4981: 11, 4982: 11, 4983: 11,
    4984: 11,
    # Gebiet 12 – Braunau + Mattighofen
    4950: 12, 4951: 12, 4952: 12, 4961: 12, 4962: 12, 4963: 12,
    5120: 12, 5121: 12, 5122: 12, 5123: 12, 5124: 12, 5131: 12,
    5132: 12, 5133: 12, 5134: 12, 5142: 12, 5143: 12, 5144: 12,
    5145: 12, 5163: 12, 5166: 12, 5202: 12, 5204: 12, 5211: 12,
    5212: 12, 5221: 12, 5222: 12, 5223: 12, 5224: 12, 5225: 12,
    5230: 12, 5231: 12, 5232: 12, 5233: 12, 5241: 12, 5242: 12,
    5251: 12, 5252: 12, 5261: 12, 5270: 12, 5271: 12, 5272: 12,
    5273: 12, 5274: 12, 5280: 12, 5282: 12,
    # Gebiet 13 – Schärding
    4090: 13, 4091: 13, 4092: 13, 4724: 13, 4725: 13, 4755: 13,
    4760: 13, 4761: 13, 4762: 13, 4770: 13, 4771: 13, 4772: 13,
    4773: 13, 4774: 13, 4775: 13, 4776: 13, 4777: 13, 4780: 13,
    4782: 13, 4783: 13, 4784: 13, 4785: 13, 4786: 13, 4791: 13,
    4792: 13, 4793: 13, 4794: 13, 4975: 13, 4980: 13,
    # Gebiet 14 – Grieskirchen + Eferding
    4070: 14, 4072: 14, 4074: 14, 4075: 14, 4076: 14, 4081: 14,
    4082: 14, 4083: 14, 4084: 14, 4085: 14, 4632: 14, 4673: 14,
    4674: 14, 4675: 14, 4676: 14, 4680: 14, 4681: 14, 4682: 14,
    4701: 14, 4702: 14, 4707: 14, 4710: 14, 4712: 14, 4713: 14,
    4714: 14, 4715: 14, 4716: 14, 4720: 14, 4721: 14, 4722: 14,
    4723: 14, 4730: 14, 4731: 14, 4732: 14, 4733: 14, 4741: 14,
    4742: 14, 4902: 14, 4903: 14, 4904: 14,
    # Gebiet 15 – Urfahr-Umgebung
    4100: 15, 4101: 15, 4111: 15, 4112: 15, 4175: 15, 4180: 15,
    4181: 15, 4182: 15, 4183: 15, 4190: 15, 4201: 15, 4202: 15,
    4203: 15, 4204: 15, 4209: 15, 4210: 15, 4211: 15,
}

GEBIET_NAMES: dict[int, str] = {
    1:  "Linz",
    2:  "Wels Stadt",
    3:  "Rohrbach",
    4:  "Freistadt",
    5:  "Perg",
    6:  "Linz-Land",
    7:  "Steyr",
    8:  "Kirchdorf + Pyhrn",
    9:  "Vöcklabruck + Gmunden Nord",
    10: "Salzkammergut",
    11: "Ried im Innkreis",
    12: "Braunau + Mattighofen",
    13: "Schärding",
    14: "Grieskirchen + Eferding",
    15: "Urfahr-Umgebung",
}

FORECAST_HORIZONS_H: list[int] = [1, 12, 24, 168, 720]  # 1h, 12h, 24h, 7d, 30d
N_GEBIETE: int = 15

FEATURE_COLUMNS: list[str] = [
    "gebiet_id", "hour", "weekday", "month", "quarter", "is_weekend", "season"
]

FEATURES_V2: list[str] = [
    "gebiet_id", "hour", "weekday", "month", "quarter", "is_weekend", "season",
    "is_holiday", "is_school_holiday",
    "population", "elderly_ratio", "nursing_home_beds",
    "has_major_event", "accident_rate",
    "temperature", "precipitation", "snowfall", "windspeed", "is_extreme_weather",
]

# season: 0=Winter, 1=Frühling, 2=Sommer, 3=Herbst
_MONTH_TO_SEASON: dict[int, int] = {
    12: 0, 1: 0, 2: 0,
    3: 1, 4: 1, 5: 1,
    6: 2, 7: 2, 8: 2,
    9: 3, 10: 3, 11: 3,
}

# Koordinaten der Gebiet-Messstationen (für Open-Meteo API)
GEBIET_COORDS: dict[int, tuple[float, float]] = {
    1:  (48.3069, 14.2858),
    2:  (48.1672, 14.0234),
    3:  (48.5706, 13.9980),
    4:  (48.5108, 14.5042),
    5:  (48.2494, 14.6369),
    6:  (48.2667, 14.2500),
    7:  (48.0432, 14.4211),
    8:  (47.9000, 14.1167),
    9:  (48.0000, 13.6500),
    10: (47.7167, 13.6333),
    11: (48.2167, 13.4833),
    12: (48.2289, 13.0356),
    13: (48.4500, 13.4333),
    14: (48.3167, 13.9833),
    15: (48.4500, 14.1667),
}

# Statische Features pro Gebiet (aus Phase 5.3–5.6 berechnet, siehe notebooks/05_feature_engineering_v2.ipynb)
GEBIET_STATIC: dict[int, dict] = {
    1:  {"population": 693794,  "elderly_ratio": 20.70, "nursing_home_beds": 1900, "accident_rate": 401},
    2:  {"population": 117684,  "elderly_ratio": 18.21, "nursing_home_beds":  740, "accident_rate": 410},
    3:  {"population": 129786,  "elderly_ratio": 19.44, "nursing_home_beds":  534, "accident_rate": 413},
    4:  {"population": 199669,  "elderly_ratio": 19.61, "nursing_home_beds":  482, "accident_rate": 406},
    5:  {"population": 232826,  "elderly_ratio": 18.53, "nursing_home_beds":  612, "accident_rate": 411},
    6:  {"population": 551613,  "elderly_ratio": 18.68, "nursing_home_beds": 1235, "accident_rate": 411},
    7:  {"population": 249026,  "elderly_ratio": 19.49, "nursing_home_beds": 1013, "accident_rate": 407},
    8:  {"population":  95394,  "elderly_ratio": 22.10, "nursing_home_beds":  546, "accident_rate": 412},
    9:  {"population": 950488,  "elderly_ratio": 19.00, "nursing_home_beds": 1952, "accident_rate": 413},
    10: {"population":  61202,  "elderly_ratio": 25.31, "nursing_home_beds":  356, "accident_rate": 407},
    11: {"population": 169552,  "elderly_ratio": 18.70, "nursing_home_beds":  679, "accident_rate": 412},
    12: {"population": 235369,  "elderly_ratio": 18.64, "nursing_home_beds":  731, "accident_rate": 406},
    13: {"population": 167644,  "elderly_ratio": 19.66, "nursing_home_beds":  499, "accident_rate": 415},
    14: {"population": 372553,  "elderly_ratio": 19.12, "nursing_home_beds":  717, "accident_rate": 406},
    15: {"population": 151318,  "elderly_ratio": 19.48, "nursing_home_beds":  625, "accident_rate": 416},
}


def map_plz_to_gebiet(plz: int) -> int:
    """Gibt die Gebiets-ID für eine PLZ zurück; -1 wenn nicht gemappt."""
    return PLZ_TO_GEBIET.get(int(plz), -1)


def build_feature_row(gebiet_id: int, dt: datetime) -> dict:
    """Feature-Vektor v1 (7 Zeit-Features) für Rückwärtskompatibilität."""
    season = _MONTH_TO_SEASON[dt.month]
    return {
        "gebiet_id":  gebiet_id,
        "hour":       dt.hour,
        "weekday":    dt.weekday(),
        "month":      dt.month,
        "quarter":    (dt.month - 1) // 3 + 1,
        "is_weekend": int(dt.weekday() >= 5),
        "season":     season,
    }


def build_feature_row_v2(
    gebiet_id: int,
    dt: datetime,
    weather_data: dict,
    is_holiday: int = 0,
    is_school_holiday: int = 0,
    has_major_event: int = 0,
) -> dict:
    """
    Feature-Vektor v2 (19 Features) inkl. Wetter und statische Gebiet-Daten.
    weather_data muss enthalten: temperature, precipitation, snowfall, windspeed.
    is_extreme_weather wird aus den Wetterwerten abgeleitet.
    """
    season = _MONTH_TO_SEASON[dt.month]
    static = GEBIET_STATIC[gebiet_id]
    temp = weather_data.get("temperature", 10.0)
    snow = weather_data.get("snowfall", 0.0)
    wind = weather_data.get("windspeed", 0.0)
    is_extreme = int(temp > 30 or temp < -5 or snow > 2 or wind > 50)
    return {
        "gebiet_id":          gebiet_id,
        "hour":               dt.hour,
        "weekday":            dt.weekday(),
        "month":              dt.month,
        "quarter":            (dt.month - 1) // 3 + 1,
        "is_weekend":         int(dt.weekday() >= 5),
        "season":             season,
        "is_holiday":         is_holiday,
        "is_school_holiday":  is_school_holiday,
        "population":         static["population"],
        "elderly_ratio":      static["elderly_ratio"],
        "nursing_home_beds":  static["nursing_home_beds"],
        "has_major_event":    has_major_event,
        "accident_rate":      static["accident_rate"],
        "temperature":        temp,
        "precipitation":      weather_data.get("precipitation", 0.0),
        "snowfall":           snow,
        "windspeed":          wind,
        "is_extreme_weather": is_extreme,
    }


def count_to_risk_level(predicted: float, max_per_gebiet: float) -> int:
    """
    Rechnet eine vorhergesagte Einsatzzahl auf eine Risikostufe 1–5 um.
    max_per_gebiet ist der historische Maximalwert des jeweiligen Gebiets.
    """
    if max_per_gebiet <= 0:
        return 1
    ratio = predicted / max_per_gebiet
    if ratio < 0.2:
        return 1
    elif ratio < 0.4:
        return 2
    elif ratio < 0.6:
        return 3
    elif ratio < 0.8:
        return 4
    return 5
