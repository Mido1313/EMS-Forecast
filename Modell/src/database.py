"""
PostgreSQL-Verbindung und Schreiblogik für Forecast-Ergebnisse.
Connection String IMMER aus Umgebungsvariable DATABASE_URL lesen – nie hardcoden!
"""

import os
from datetime import datetime, timezone

from sqlalchemy import create_engine, Column, Integer, Float, String, DateTime
from sqlalchemy.orm import DeclarativeBase, Session, sessionmaker
from dotenv import load_dotenv

load_dotenv()

DATABASE_URL = os.environ.get("DATABASE_URL")

# Engine und SessionFactory werden nur erstellt wenn DATABASE_URL gesetzt ist.
# Im Notebook-Betrieb (Phase 1–3) wird dieses Modul nicht importiert.
if DATABASE_URL:
    engine = create_engine(DATABASE_URL, pool_pre_ping=True)
    SessionLocal = sessionmaker(bind=engine, autocommit=False, autoflush=False)
else:
    engine = None
    SessionLocal = None


class Base(DeclarativeBase):
    pass


class Forecast(Base):
    """ORM-Mapping für die forecasts-Tabelle (Schema mit Merjem abgestimmt)."""
    __tablename__ = "forecasts"

    id              = Column(Integer, primary_key=True, index=True)
    gebiet_id       = Column(Integer, nullable=False)
    calculated_at   = Column(DateTime(timezone=True), nullable=False)
    horizon_hours   = Column(Integer, nullable=False)
    predicted_count = Column(Float, nullable=False)
    risk_level      = Column(Integer, nullable=False)
    model_version   = Column(String(50), nullable=False)


def get_db():
    """FastAPI-Dependency: liefert eine DB-Session und schließt sie danach."""
    if SessionLocal is None:
        raise RuntimeError("DATABASE_URL ist nicht gesetzt.")
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


def write_forecast(
    db: Session,
    gebiet_id: int,
    horizon_h: int,
    predicted_count: float,
    risk_level: int,
    model_version: str,
) -> None:
    """Schreibt einen einzelnen Forecast-Eintrag in die DB."""
    entry = Forecast(
        gebiet_id=gebiet_id,
        calculated_at=datetime.now(timezone.utc),
        horizon_hours=horizon_h,
        predicted_count=predicted_count,
        risk_level=risk_level,
        model_version=model_version,
    )
    db.add(entry)
    db.commit()
