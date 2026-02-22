# Data Model: ASP.NET Core Controller Refactor

**Branch**: `002-aspnet-core-refactor` | **Date**: 2026-02-21

## Overview

No new entities are introduced by this refactor. The existing
data model is unchanged. This document records the single DTO
that is being relocated as part of the migration.

## Existing DTO (relocated, not modified)

### HealthResponse

An immutable record representing the health check response.
Moved from `Endpoints/HealthEndpoint.cs` to
`Models/HealthResponse.cs`. No changes to shape or behavior.

| Field           | Type              | Required | Description                          |
|-----------------|-------------------|----------|--------------------------------------|
| Status          | string            | Yes      | "healthy" or "unhealthy"             |
| DatabaseVersion | string?           | No       | PostgreSQL version string            |
| ServerTime      | DateTimeOffset?   | No       | Server timestamp from PostgreSQL     |
| Error           | string?           | No       | Error message when unhealthy         |

**Namespace change**: `RentalForge.Api.Endpoints` →
`RentalForge.Api.Models`

**Impact**: Test assertions reference JSON property names
(`status`, `databaseVersion`, `serverTime`, `error`) not the C#
type directly, so the namespace change has zero test impact.

## Existing Entities (unchanged)

The 16 entities in `Data/Entities/` and the `DvdrentalContext`
are completely unaffected by this refactor. No schema changes,
no relationship changes, no EF Core configuration changes.
