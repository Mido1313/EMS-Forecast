const state = {
  risks: [],
  riskByArea: new Map(),
  map: null,
  mapLayer: null,
  chart: null,
};

const mapContainer = document.getElementById("mapContainer");
const chartContainer = document.getElementById("chartContainer");
const detailPanel = document.getElementById("detailPanel");
const recalcButton = document.getElementById("recalculateButton");
const lastUpdateEl = document.getElementById("lastUpdate");
const viewModeHint = document.getElementById("viewModeHint");

const CATEGORY_LABELS = {
  1: "Sehr gering",
  2: "Gering",
  3: "Mittel",
  4: "Hoch",
  5: "Kritisch",
};

async function getJson(url, options = {}) {
  const response = await fetch(url, options);
  if (!response.ok) {
    throw new Error(`${url} -> ${response.status}`);
  }
  return response.json();
}

async function loadCurrentRisk() {
  const [areas, risks] = await Promise.all([getJson("/api/areas"), getJson("/api/risk/current")]);
  state.risks = risks
    .map((risk) => ({ ...risk, areaName: areas.find((a) => a.areaId === risk.areaId)?.areaName ?? risk.areaName }))
    .sort((a, b) => a.areaId - b.areaId);
  state.riskByArea = new Map(state.risks.map((x) => [x.areaId, x]));
}

function updateLastUpdate() {
  const now = new Date();
  lastUpdateEl.textContent = `Stand: ${now.toLocaleString("de-AT")}`;
}

function renderDetail(detail) {
  const categoryLabel = CATEGORY_LABELS[detail.riskCategory] || detail.riskCategory;
  const metrics = detail.metrics;

  const topMetrics = [
    ["Ø Geschwindigkeit", `${metrics.avgSpeed} km/h`],
    ["Min Geschwindigkeit", `${metrics.minSpeed} km/h`],
    ["Ø Fahrzeit", `${metrics.avgTravelTime} min`],
    ["Baustellen aktiv", `${metrics.activeConstructionCount}`],
    ["Unfälle 24h", `${metrics.accidentCount24h}`],
    ["Unfälle 7d", `${metrics.accidentCount7d}`],
    ["Niederschlag", `${metrics.precipitationMm} mm`],
    ["Hotspot-Score", `${metrics.hotspotCriticalityScore}`],
  ];

  const components = Object.entries(detail.components)
    .sort((a, b) => b[1] - a[1])
    .map(
      ([k, v]) =>
        `<div class="component-item"><span>${k}</span><span>${(v * 100).toFixed(1)}%</span></div>`,
    )
    .join("");

  detailPanel.innerHTML = `
    <div class="detail-card">
      <h3>${detail.areaId} · ${detail.areaName}</h3>
      <div class="score-row">
        Score: <strong>${detail.riskScore.toFixed(3)}</strong>
        <span class="badge" style="background:${detail.colorHex}">${categoryLabel}</span>
      </div>
      <p>${detail.explanation}</p>
      <div class="metrics-grid">
        ${topMetrics
          .map(
            ([key, value]) =>
              `<div class="metric"><span class="k">${key}</span><span class="v">${value}</span></div>`,
          )
          .join("")}
      </div>
      <div class="component-list">
        <h4>Komponentenbeitrag</h4>
        ${components}
      </div>
    </div>
  `;
}

async function openDetail(areaId) {
  const detail = await getJson(`/api/risk/${areaId}`);
  renderDetail(detail);
}

function styleForFeature(feature) {
  const areaId = feature?.properties?.areaId;
  const risk = state.riskByArea.get(areaId);
  return {
    color: "#334f5f",
    weight: 1,
    fillColor: risk?.colorHex || "#999",
    fillOpacity: 0.65,
  };
}

function tooltipHtml(areaId) {
  const risk = state.riskByArea.get(areaId);
  if (!risk) {
    return `Gebiet ${areaId}`;
  }
  return `
    <strong>${risk.areaName}</strong><br/>
    Score: ${risk.riskScore.toFixed(3)}<br/>
    Stufe: ${risk.riskCategory} (${CATEGORY_LABELS[risk.riskCategory]})
  `;
}

function renderMap(geojson) {
  mapContainer.classList.remove("hidden");
  chartContainer.classList.add("hidden");
  viewModeHint.textContent = "Darstellung: Karte (Leaflet)";

  if (state.map) {
    state.map.remove();
    state.map = null;
  }

  state.map = L.map("mapContainer", { zoomControl: true }).setView([48.2, 13.95], 8.35);
  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 18,
    attribution: "&copy; OpenStreetMap",
  }).addTo(state.map);

  state.mapLayer = L.geoJSON(geojson, {
    style: styleForFeature,
    onEachFeature: (feature, layer) => {
      const areaId = Number(feature?.properties?.areaId);
      layer.bindTooltip(tooltipHtml(areaId));
      layer.on({
        click: () => openDetail(areaId),
        mouseover: () => layer.setStyle({ weight: 2, fillOpacity: 0.82 }),
        mouseout: () => state.mapLayer.resetStyle(layer),
      });
    },
  }).addTo(state.map);

  state.map.fitBounds(state.mapLayer.getBounds(), { padding: [16, 16] });
}

function renderChart() {
  mapContainer.classList.add("hidden");
  chartContainer.classList.remove("hidden");
  viewModeHint.textContent = "Darstellung: Fallback-Diagramm (keine Geometrie verfügbar)";

  const sorted = [...state.risks].sort((a, b) => b.riskScore - a.riskScore);
  const labels = sorted.map((x) => `${x.areaId} ${x.areaName}`);
  const values = sorted.map((x) => Number(x.riskScore.toFixed(3)));
  const colors = sorted.map((x) => x.colorHex);

  if (state.chart) {
    state.chart.destroy();
    state.chart = null;
  }

  const ctx = document.getElementById("riskChart").getContext("2d");
  state.chart = new Chart(ctx, {
    type: "bar",
    data: {
      labels,
      datasets: [
        {
          label: "RiskScore",
          data: values,
          borderRadius: 8,
          borderSkipped: false,
          backgroundColor: colors,
        },
      ],
    },
    options: {
      indexAxis: "y",
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (context) => {
              const risk = sorted[context.dataIndex];
              return `Score ${risk.riskScore.toFixed(3)} | Stufe ${risk.riskCategory}`;
            },
          },
        },
      },
      scales: {
        x: {
          min: 0,
          max: 1,
          title: { display: true, text: "RiskScore (0-1)" },
        },
      },
      onClick: async (_, elements) => {
        if (!elements.length) {
          return;
        }
        const idx = elements[0].index;
        const area = sorted[idx];
        await openDetail(area.areaId);
      },
    },
  });
}

async function drawVisualization() {
  try {
    const geojson = await getJson("/api/geometry");
    if (!geojson?.features?.length) {
      throw new Error("geometry empty");
    }
    renderMap(geojson);
  } catch (error) {
    console.warn("Switching to chart fallback:", error.message);
    renderChart();
  }
}

async function recalculate() {
  recalcButton.disabled = true;
  recalcButton.textContent = "Berechnung...";
  try {
    await getJson("/api/recalculate", { method: "POST" });
    await loadCurrentRisk();
    await drawVisualization();
    updateLastUpdate();
  } finally {
    recalcButton.disabled = false;
    recalcButton.textContent = "Neu berechnen";
  }
}

async function init() {
  await loadCurrentRisk();
  await drawVisualization();
  updateLastUpdate();

  if (state.risks.length) {
    await openDetail(state.risks[0].areaId);
  }

  recalcButton.addEventListener("click", recalculate);
}

init().catch((error) => {
  detailPanel.innerHTML = `<p class="detail-placeholder">Fehler beim Laden: ${error.message}</p>`;
  console.error(error);
});
