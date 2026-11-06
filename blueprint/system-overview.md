# System Overview

## Problem Statement

A mid-sized water treatment plant depends on hundreds of connected assets, including pumps, motors, compressors, sensors and network gateways. The plant's information is currently split across spreadsheets, separate sensor records and paper maintenance documents. This makes it difficult for staff to find a reliable asset history, compare readings over time or apply consistent thresholds for equipment conditions. Important information can be delayed or missed when teams use different records and processes.

## Stakeholders

| Stakeholder | What they need from the platform |
|---|---|
| Plant operators | A dependable view of plant assets and the telemetry associated with equipment in operation. |
| Maintenance technicians | Accurate equipment details and maintenance history to support inspection and repair work. |
| Reliability and maintenance engineers | Consistent asset and measurement information to review equipment condition and define alert thresholds. |
| Plant management | A consolidated view of asset information and maintenance activity to support planning and operational decisions. |
| IT/OT administrators | A structured system of record with consistent data and a maintainable technical foundation. |

## Platform Scope

The Industrial IoT Asset & Telemetry Platform provides one system of record for four connected areas: **assets**, the **telemetry** recorded from those assets, the **alert rules** that describe measurement thresholds, and **maintenance history**. Keeping these areas together gives the plant a consistent foundation for understanding equipment, its reported measurements, the conditions that matter to staff and the work performed on each asset.

## Phase 1 Delivery and Later Phases

Phase 1 establishes the platform's foundation. It delivers a validated domain model containing four entities, two value objects and lifecycle rules; a SQL Server database built through Entity Framework Core migrations; JSON sample data representing eight water-treatment-plant devices; and unit tests for the domain and supporting components. These elements establish the core structure and demonstrate that the main domain rules can be checked consistently.

Later phases build on this foundation with a Web API, telemetry ingestion, live alert evaluation and dashboards. Those capabilities extend the system beyond its Phase 1 data model and sample data, allowing operational applications and real-time workflows to use the same underlying asset, telemetry, alert-rule and maintenance information.
