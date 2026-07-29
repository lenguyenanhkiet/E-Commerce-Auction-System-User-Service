# Zero-Warning Cleanup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the solution compile in Release mode with zero warnings without changing intended behavior.

**Architecture:** Align nullable annotations at API, application, and domain boundaries; make mechanical framework-compatibility corrections separately. Verify each warning category with focused compilation/tests, then verify the whole solution and EF model.

**Tech Stack:** C# 14, .NET 10, ASP.NET Core, Entity Framework Core 10, xUnit, FluentValidation

## Global Constraints

- Preserve the existing `DateTimeOffset` database and API semantics.
- Do not suppress warnings with pragmas, `NoWarn`, or unjustified null-forgiving operators.
- Preserve unrelated working-tree changes.
- Do not change global developer tooling.

---

### Task 1: Fix EF materialization constructors

**Files:**
- Modify: `src/ECommerceAuction.UserService.Domain/IdentityVerifications/BuyerVerificationProfile.cs:38`
- Modify: `src/ECommerceAuction.UserService.Domain/Reputation/Buyer/BuyerReputationProfile.cs:44`
- Modify: `src/ECommerceAuction.UserService.Domain/Reputation/Ledger/ReputationLedgerEntry.cs:73`

**Interfaces:**
- Consumes: EF Core constructor binding for entity materialization.
- Produces: Parameterless private constructors with unchanged factory APIs.

- [ ] **Step 1: Capture the current warning**

Run:

```powershell
dotnet build .\src\ECommerceAuction.UserService.Domain\ECommerceAuction.UserService.Domain.csproj --configuration Release --no-restore
```

Expected: build succeeds and reports three `CS0628` warnings.

- [ ] **Step 2: Apply the minimal implementation**

Change each parameterless constructor from:

```csharp
protected EntityName()
{
}
```

to:

```csharp
private EntityName()
{
}
```

- [ ] **Step 3: Verify the warning category is gone**

Run the same Domain build. Expected: success with zero `CS0628` warnings.

### Task 2: Align address response nullability

**Files:**
- Modify response records under:
  - `src/ECommerceAuction.UserService.Application/Features/Users/GetProfile`
  - `src/ECommerceAuction.UserService.Application/Features/Users/UserAddress/CreateAddress`
  - `src/ECommerceAuction.UserService.Application/Features/Users/UserAddress/GetAddressById`
  - `src/ECommerceAuction.UserService.Application/Features/Users/UserAddress/GetListAddresses`
  - `src/ECommerceAuction.UserService.Application/Features/Users/UserAddress/UpdateAddress`
- Test: relevant application unit-test files if a serialization or mapping contract test already exists.

**Interfaces:**
- Consumes: `Address.Province`, `Address.Ward`, and `Address.Type` as `string?`.
- Produces: Response properties with matching `string?` contracts; required address fields remain `string`.

- [ ] **Step 1: Add or update a focused mapping test**

Create an address whose optional fields are absent and assert the mapped response preserves `null` for `Province`, `Ward`, and `Type`.

- [ ] **Step 2: Run the focused test**

Expected before the contract correction: compilation fails where nullable domain properties are passed to non-null response parameters, or the new contract assertion fails.

- [ ] **Step 3: Correct response annotations**

For each response record, change only optional address parameters:

```csharp
string? Province,
string? Ward,
string? Type
```

Keep `RecipientName`, `RecipientPhone`, and `Street` non-nullable.

- [ ] **Step 4: Run focused tests and Application build**

Run:

```powershell
dotnet test .\tests\ECommerceAuction.UserService.Application.UnitTests\ECommerceAuction.UserService.Application.UnitTests.csproj --configuration Release --no-restore
dotnet build .\src\ECommerceAuction.UserService.Application\ECommerceAuction.UserService.Application.csproj --configuration Release --no-restore
```

Expected: tests pass and the address-related `CS8604` warnings are absent.

### Task 3: Align seller input contracts

**Files:**
- Modify: `src/ECommerceAuction.UserService.Application/Features/Sellers/ResubmitSeller/ResubmitSellerCommandHandler.cs`
- Modify: seller command/request/validator files referenced by `SellersController`.
- Modify: `src/ECommerceAuction.UserService.Api/Controllers/SellersController.cs`
- Test: relevant seller handler/validator unit tests.

**Interfaces:**
- Consumes: Nullable HTTP request fields and seller-type-specific validation.
- Produces: A validated command path that passes non-null required business values into `SellerProfile.Resubmit`.

- [ ] **Step 1: Add focused validator tests**

Test that business seller submissions reject missing business-only fields and that individual seller submissions preserve the existing optional-field behavior.

- [ ] **Step 2: Run focused tests and confirm the intended failure**

Expected: the new test fails because current nullable annotations and validation do not establish the required contract consistently.

- [ ] **Step 3: Implement the minimal contract correction**

Use nullable types at the HTTP boundary for conditionally optional fields. After validation, explicitly guard required business values before calling the domain method; throw the existing validation/business exception type rather than using `!`.

- [ ] **Step 4: Verify seller tests and builds**

Run the focused tests, then build Application and API in Release. Expected: no seller-related `CS8604` warnings.

### Task 4: Replace the obsolete forwarded-header API

**Files:**
- Modify: `src/ECommerceAuction.UserService.Api/Program.cs:113`

**Interfaces:**
- Consumes: ASP.NET Core `ForwardedHeadersOptions`.
- Produces: Equivalent trusted-network configuration using `KnownIPNetworks`.

- [ ] **Step 1: Capture the current warning**

Build the API in Release and confirm `ASPDEPR005` is present.

- [ ] **Step 2: Apply the framework-supported replacement**

Replace access to `KnownNetworks` with `KnownIPNetworks` while retaining the same clear/add behavior and network values.

- [ ] **Step 3: Verify the API build**

Build the API in Release. Expected: success without `ASPDEPR005`.

### Task 5: Repository tool alignment and final verification

**Files:**
- Inspect: `.config/dotnet-tools.json` if present.
- Modify only if present and pinned to EF 9.x.

**Interfaces:**
- Consumes: EF Core runtime version `10.0.8`.
- Produces: Repository-local EF tool metadata aligned to the 10.x runtime, or a documented external-tool warning if no local manifest exists.

- [ ] **Step 1: Inspect local tool metadata**

If `.config/dotnet-tools.json` exists and pins `dotnet-ef` 9.x, update it to the matching available 10.x patch. Do not install or alter a global tool.

- [ ] **Step 2: Run full verification**

```powershell
dotnet test .\ECommerceAuction.UserService.sln --configuration Release --no-restore
dotnet build .\ECommerceAuction.UserService.sln --configuration Release --no-restore
dotnet ef migrations has-pending-model-changes --project .\src\ECommerceAuction.UserService.Persistence\ECommerceAuction.UserService.Persistence.csproj --startup-project .\src\ECommerceAuction.UserService.Api\ECommerceAuction.UserService.Api.csproj --configuration Release --no-build
```

Expected:

- All tests pass with zero failures.
- Build succeeds with zero compiler warnings.
- EF reports: `No changes have been made to the model since the last migration.`

- [ ] **Step 3: Review the final diff**

Confirm only warning-related source/test/tool-manifest changes and these planning documents were added; all unrelated user changes remain untouched.
