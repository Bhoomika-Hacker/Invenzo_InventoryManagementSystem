# Invenzo xUnit Tests

This is a separate xUnit test project. It contains 5 tests each for Product, Sale, Purchase, Billing and Payment controllers (25 total). No UI tests are included.

Important: the main web project explicitly excludes `Tests/**` from its compile items, so xUnit source is not compiled into the application.

Run:
```powershell
dotnet restore .\Tests\Invenzo.Tests\Invenzo.Tests.csproj
dotnet test .\Tests\Invenzo.Tests\Invenzo.Tests.csproj
```
