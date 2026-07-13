# Architecture Backlog

This document tracks architectural improvements that have been intentionally deferred.

These are not bugs. They are engineering improvements that should be completed before the platform reaches production maturity.

---

## AB-001 - Consolidate EF Core DbContexts

Status: Planned

Priority: High

Description

Currently each capability owns its own DbContext.

- ProductDbContext
- RegulatoryApplicationDbContext

Before introducing additional capabilities (Submission, Authority, Organization, Country, etc.) these should be consolidated into a single:

RegOSDbContext

inside

RegOS.Persistence

Reason

- Single migration history
- Single database model
- Simpler schema evolution
- Easier transaction management

Target Sprint

Sprint 10

---

## AB-002 - Global Exception Handling

Status: Planned

Priority: Medium

Description

Introduce a consistent exception handling middleware.

Current state

Unhandled exceptions are returned directly from ASP.NET.

Desired state

Map domain/application exceptions to ProblemDetails responses.

Target Sprint

TBD

---

## AB-003 - Replace Temporary Reference Data

Status: Planned

Priority: High

Description

Regulatory Applications require:

- Country
- Authority
- Applicant Organization

These capabilities should replace temporary assumptions in the Application workflow.

Target Sprint

Sprint 11-13
