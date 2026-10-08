# OrderFlow

> A .NET 8 order-management backend built with **Clean Architecture**, **Vertical Slices** and **CQRS**, and instrumented end to end with metrics, structured logs, distributed traces and health checks.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-metrics%20%26%20traces-425CC7?logo=opentelemetry&logoColor=white)
![Grafana](https://img.shields.io/badge/Grafana-dashboard-F46800?logo=grafana&logoColor=white)
![Prometheus](https://img.shields.io/badge/Prometheus-metrics-E6522C?logo=prometheus&logoColor=white)
![k6](https://img.shields.io/badge/k6-load%20testing-7D64FF?logo=k6&logoColor=white)

## Overview

OrderFlow lets clients create, retrieve, complete and cancel orders. The business domain is deliberately small. The goal of the project is to show **how a backend is structured** and **how well it can be observed** when it is under load or when a dependency fails.

The repository ships a complete local observability stack (Prometheus, Grafana, Loki, Jaeger) and a k6 load test that was used to validate the dashboards, including a deliberate SQL Server outage.

## Highlights

- **Clean Architecture** with Domain, Application, Infrastructure and API layers. Dependencies point inward.
- **Vertical Slice** organisation inside the Application layer: every use case owns its command or query, handler and validation.
- **CQRS**: commands change state, queries only read it.
- **Metrics**: OpenTelemetry instrumentation exposed to Prometheus (HTTP, health and business metrics).
- **Logs**: Serilog structured logs shipped to Loki, enriched with `TraceId` and `SpanId` so a log line can be tied to a trace.
- **Traces**: OpenTelemetry spans for ASP.NET Core, outgoing HTTP calls and EF Core (including SQL text), exported to Jaeger over OTLP.
- **Health checks**: a SQL Server probe, published as a metric every 10 seconds so Grafana can chart it.
- **Grafana dashboard** and a **k6** scenario to exercise it.

## Architecture

| Layer | Responsibility |
|---|---|
| **Domain** | Entities, enums and business rules. No dependencies. |
| **Application** | Use cases organised as vertical slices (commands, queries, handlers, validation). Depends on Domain only. |
| **Infrastructure** | EF Core, SQL Server and other external concerns. Depends on Application. |
| **API** (`OrderFlowII`) | Controllers, `Program.cs`, dependency injection and observability wiring. |

Each feature lives in its own folder instead of being spread across technical folders:

```text
Application/
└── Features/
    └── Orders/
        ├── CreateOrder/      # Command + Handler + Validator
        ├── GetOrders/        # Query + Handler
        ├── GetOrderById/     # Query + Handler
        ├── CompleteOrder/    # Command + Handler
        └── CancelOrder/      # Command + Handler
```

## API

| Method | Endpoint | Purpose |
|---|---|---|
| `POST` | `/api/Order` | Create an order |
| `GET` | `/api/Order` | List orders |
| `GET` | `/api/Order/{id}` | Get an order by ID |
| `PUT` | `/api/Order/{id}/complete` | Mark an order as completed (`204 No Content`) |
| `PUT` | `/api/Order/{id}/cancel` | Cancel an order |

Example request body for `POST /api/Order`:

```json
{
  "customerName": "Ahmed",
  "items": [
    { "name": "Keyboard", "price": 500, "quantity": 1 },
    { "name": "Mouse", "price": 150, "quantity": 2 }
  ]
}
```

Interactive documentation is available through Swagger at `/swagger`.

## Observability

### What is collected

| Pillar | Tooling | Details |
|---|---|---|
| **Metrics** | OpenTelemetry → Prometheus | ASP.NET Core and HttpClient instrumentation, business metrics (orders created and completed), and a health-check metric. Scraped from `/metrics`. |
| **Logs** | Serilog → Loki | Structured JSON logs with request logging, correlated with traces through `TraceId` and `SpanId`. |
| **Traces** | OpenTelemetry → Jaeger (OTLP) | ASP.NET Core, HttpClient and EF Core spans, with the SQL statement attached to database spans. |
| **Health** | ASP.NET Core Health Checks | SQL Server probe at `/health`, re-evaluated every 10 seconds and published as a metric. |

### Dashboard

The Grafana dashboard combines all three signals on one screen:

- **SQL Server Health Status**
- **Total Orders Created** and **Total Completed Orders**
- **HTTP Requests / Sec**
- **HTTP Request Duration** (p95 and p99)
- **Live Structured Logs** (Loki)

| Healthy | SQL Server down |
|---|---|
| ![Dashboard while healthy](docs/images/dashboard-healthy.png) | ![Dashboard while SQL Server is down](docs/images/dashboard-sql-down.png) |

### Ports

| Service | URL |
|---|---|
| API and Swagger | http://localhost:5195/swagger |
| Health | http://localhost:5195/health |
| Metrics | http://localhost:5195/metrics |
| Grafana | http://localhost:3000 (`admin` / `admin`, change it outside local use) |
| Prometheus | http://localhost:9090 |
| Jaeger UI | http://localhost:16686 |
| Loki | http://localhost:3100 (API only) |
| OTLP receiver (Jaeger) | `localhost:5317`, mapped to container port `4317` |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- SQL Server running on the host machine

### 1. Start the observability stack

```bash
docker compose up -d
```

This starts Prometheus, Loki, Jaeger and Grafana. Prometheus scrapes the API on the host through `host.docker.internal:5195`.

### 2. Configure the database

Set the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=OrderFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Then apply the migrations:

```bash
dotnet ef database update --project Infrastructure --startup-project OrderFlowII
```

### 3. Run the API

```bash
dotnet run --project OrderFlowII
```

### 4. Set up Grafana

1. Open http://localhost:3000 and sign in.
2. Add the data sources:
   - **Prometheus**: `http://prometheus:9090`
   - **Loki**: `http://loki:3100`
3. Import the dashboard from `grafana/orderflow-dashboard.json`.

## Load and failure testing

`script.js` is a [k6](https://k6.io/) scenario that ramps up to 30 virtual users. Each iteration creates an order, fetches it, then completes it, with a check on every step.

```bash
# CMD (use ${PWD} in PowerShell)
docker run --rm -i -v %cd%:/scripts grafana/k6 run /scripts/script.js
```

To test a dependency failure, stop SQL Server while the test is running and start it again afterwards:

```bash
net stop MSSQLSERVER
net start MSSQLSERVER
```

### Results of one failure run

Single run on a local machine, about 2 minutes, up to 30 virtual users. Treat the numbers as illustrative.

| Metric | Result |
|---|---|
| Total requests | 852 (about 7.1 req/s) |
| Failed requests | 109 (12.79%) |
| Create order | 253 passed, 93 failed |
| Fetch order | 248 passed, 5 failed |
| Complete order | 242 passed, 11 failed |
| p95 latency, all requests | 16.68 s |
| p95 latency, successful requests only | 1.27 s |
| Slowest request | 18.78 s |

![k6 summary](docs/images/k6-failure-summary.png)

### What the run showed

- The **health panel** turned red while SQL Server was down and returned to green after the restart.
- **Failures were slow, not fast.** Successful requests had a p95 of 1.27 s, but the overall p95 was 16.68 s. The failing requests most likely waited on database connection timeouts before giving up.
- The **latency panel flattens at 10 s**, which is the top histogram bucket. The real worst case measured by k6 was 18.78 s, so read the flat line as "10 s or more".

## Roadmap

- Shorter database timeouts and a circuit breaker (for example with Polly) so failures are fast.
- Pagination for `GET /api/Order`, which currently returns every order.
- Grafana alert rules for high error rate, high latency and an unhealthy dependency.
- Additional health checks for any further dependencies (for example a cache).
- Provision the Grafana data sources and dashboard automatically from files.

## Author

Built by **Ahmed**. [LinkedIn](https://www.linkedin.com/in/your-profile) · [GitHub](https://github.com/your-username)
