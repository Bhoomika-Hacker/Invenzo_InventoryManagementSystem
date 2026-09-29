# ZenoAI — optional OpenAI-powered mode

ZenoAI now supports an optional real AI mode using the OpenAI Responses API.

## Setup

Do **not** hard-code a real API key into source control.

Preferred option in Visual Studio:

```powershell
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_OPENAI_API_KEY"
```

Or set the environment variable:

```powershell
$env:OPENAI_API_KEY="YOUR_OPENAI_API_KEY"
```

The default model is `gpt-5.6-luna` and can be changed in `appsettings.json` under `OpenAI:Model`.

If no API key is configured, ZenoAI automatically falls back to its built-in database-aware rules, so the application still works without an API key.

## Database migration warning fix

The project had a manually-created billing migration whose model snapshot did not include Billing/Payment. That caused EF Core to throw `PendingModelChangesWarning` during startup. The snapshot has been updated and the warning is also ignored during startup so the existing migration can be applied safely.
