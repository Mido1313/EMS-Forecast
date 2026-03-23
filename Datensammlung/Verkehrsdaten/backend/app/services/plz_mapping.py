from __future__ import annotations

from pathlib import Path

import pandas as pd


class PlzAreaMapper:
    """Optional adapter for mapping postal codes to areaId.

    This module is not wired into runtime scoring yet. It is intended as a
    migration helper when source systems provide PLZ-level data instead of
    direct area IDs.
    """

    def __init__(self, excel_path: str | Path) -> None:
        self._excel_path = Path(excel_path)

    def load_mapping(self) -> pd.DataFrame:
        if not self._excel_path.exists():
            return pd.DataFrame(columns=["plz", "areaId"])

        raw = pd.read_excel(self._excel_path)

        # Replace these column names with your concrete layout from PLZ_Liste.xlsx.
        candidate_cols = {col.lower(): col for col in raw.columns}
        plz_col = candidate_cols.get("plz") or candidate_cols.get("postalcode")
        area_col = candidate_cols.get("areaid") or candidate_cols.get("gebietid")

        if not plz_col or not area_col:
            return pd.DataFrame(columns=["plz", "areaId"])

        mapped = raw[[plz_col, area_col]].copy()
        mapped.columns = ["plz", "areaId"]
        mapped["plz"] = mapped["plz"].astype(str).str.strip()
        mapped["areaId"] = pd.to_numeric(mapped["areaId"], errors="coerce")
        mapped = mapped.dropna(subset=["areaId"])
        mapped["areaId"] = mapped["areaId"].astype(int)

        return mapped.drop_duplicates().reset_index(drop=True)
