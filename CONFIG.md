Configuration and secrets — best practices

Use environment variables or the .NET user-secrets store for secrets (do NOT check them into source control).

Recommended keys (binds to SupabaseOptions and ConnectionStrings):
- Supabase:Url
- Supabase:Audience
- ConnectionStrings:DefaultConnection

Local development options

1) dotnet user-secrets (per-project, development only)

In project directory run:

```powershell
# initialize user-secrets (only once per project)
dotnet user-secrets init

# set values
dotnet user-secrets set "Supabase:Url" "https://your-project.supabase.co"
dotnet user-secrets set "Supabase:Audience" "your-audience"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Username=...;Password=...;Database=...;"

If you want to set the exact Supabase Postgres URL you provided for local development, you can run (RECOMMENDED: keep this secret out of source control):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://postgres:rahasia@db.arvwkskumhbnognlkqms.supabase.co:5432/postgres"
```
```

2) Environment variables (useful for containers or CI)

PowerShell example (temporary for session):

```powershell
$env:Supabase__Url = "https://your-project.supabase.co"
$env:Supabase__Audience = "your-audience"
$env:ConnectionStrings__DefaultConnection = "postgresql://postgres:rahasia@db.arvwkskumhbnognlkqms.supabase.co:5432/postgres"
```

Note: .NET binds double-underscore __ to nested configuration keys.

Production: use your cloud provider secrets manager or environment variables configured in the host (Azure Key Vault, AWS Secrets Manager, etc.).

Security note
- Use the least-privileged DB credentials. Prefer RLS (Row Level Security) and JWT-based Postgres roles for per-user access rather than service_role keys in production.

Testing the API
- Start the app: `dotnet run`
- Call GET /api/threads with an Authorization: Bearer <jwt> header.

Contact me if you want me to add a sample `appsettings.Development.json` (excluded from source control) or a small script to set local env variables.
