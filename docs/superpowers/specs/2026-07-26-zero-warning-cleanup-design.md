# Zero-Warning Cleanup Design

## Goal

Make the current `DateTimeOffset` migration branch compile in Release mode with zero compiler warnings while preserving existing API and domain behavior.

## Scope

- Correct nullable contracts for optional address and seller fields instead of suppressing warnings with `!`.
- Make Entity Framework materialization constructors valid for sealed entities.
- Replace the obsolete forwarded-header network API with its .NET 10 replacement.
- Keep the EF model and the latest migration synchronized.
- Do not refactor unrelated code or alter database column semantics.
- Do not modify the machine-wide EF CLI installation. Repository-local tool metadata may be aligned only if it already exists.

## Design

Address fields `Province`, `Ward`, and `Type` are nullable in the domain model, so response records that expose those fields will declare them as nullable. Required fields such as `Street` remain non-nullable; command validation and handler flow must establish that invariant before mapping.

Seller commands will model business-only fields consistently with seller type. Values that are optional at the HTTP boundary remain nullable until validation establishes the relevant seller-type requirements. Domain methods will continue to receive non-null values only on paths where they are required.

The parameterless constructors used by EF Core in sealed entities will be `private`, because `protected` has no useful inheritance semantics on a sealed type and produces `CS0628`.

Forwarded headers will use `KnownIPNetworks`, the direct .NET 10 replacement for obsolete `KnownNetworks`.

## Testing

- Add or adjust focused unit tests only where null-contract behavior changes.
- Run the focused test first and confirm it fails for the intended contract mismatch.
- Run the full Release test suite and require 67 or more passing tests with zero failures.
- Run a Release build and require zero compiler warnings.
- Run `dotnet ef migrations has-pending-model-changes` and require no pending model changes.

## Acceptance Criteria

- Release build succeeds with zero warnings.
- All tests pass.
- No warning is hidden through pragmas, `NoWarn`, nullable suppression, or unjustified `!`.
- EF model matches the latest migration.
- Existing unrelated user changes remain intact.
