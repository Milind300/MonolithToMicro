# MonolithToMicroUI — React + Vite + Microsoft Entra ID

Day 3 frontend for the `MonolithToMicro` e-commerce learning project. **This is a working authentication/Catalog-read laboratory UI**, not the full store frontend yet.

## Folder location

Unzip the **`MonolithToMicroUI` folder** into `D:\InterviewPrep\MonolithToMicro\`, resulting in:

`D:\InterviewPrep\MonolithToMicro\MonolithToMicroUI\package.json`

## Run (VS Code Terminal on Windows, not Package Manager Console)

```powershell
cd D:\InterviewPrep\MonolithToMicro\MonolithToMicroUI
npm install
npm run dev
```

Open **http://localhost:5173**. Vite uses a strict port, so it will error instead of silently changing the Entra redirect URL.

## Backend connection

1. Run `MonolithToMicro.Catalog.Api` in **Visual Studio** using its local HTTP launch profile.
2. Note the actual `http://localhost:<port>` URL displayed in Visual Studio/Swagger. Do **not** guess the API port.
3. Paste that **base URL without `/swagger`** into the UI's *Catalog API base URL* field.
4. Test **without a token** -> expected `401 Unauthorized`.
5. Sign in with your Microsoft Entra workforce-tenant account, then **Fetch with JWT** -> expected `200 OK` and Catalog records.
6. If you get `403`, verify delegated consent and the `scp` claim contains `Catalog.Read`.

The Catalog API must have `UseCors("ReactClient")` configured with `http://localhost:5173` **before** authorization, and `UseAuthentication` and `UseAuthorization` enabled. If a request fails due to CORS, inspect browser DevTools/Network. HTTP `401` is a valid API test result, not a UI error.

### Identity configuration

Defaults in `src/auth.js` use the **existing** registrations:

- Tenant ID: `43ed8cd9-5f65-4dbe-b58f-67aa7022542e`
- **SPA Client ID** (`MonolithToMicro-Test-Client`): `5c1dae0e-4d93-4413-8c90-4385a0a7e428`
- API scope: `api://e3c9f03f-b3ed-4f57-85c6-21accee06687/Catalog.Read`
- Registered SPA redirect URI: `http://localhost:5173`

You can override the public identifiers via a `.env.local` file, using `.env.example` as a guide. Restart Vite after changing `.env.local`. These IDs are not secrets.

**Never** put a client secret, SQL password, JWT access token or connection string into React files or any `VITE_` variable. Browser-based apps cannot keep secrets. MSAL keeps token data in browser session storage; never paste access tokens into chat or Git.

## What this starter does / does not do

✅ React/Vite on 5173, MSAL popup login, access token acquisition (silent then interactive fallback), Catalog GET with Bearer token, HTTP response/error viewer, unauthenticated GET for 401 verification.

⏳ Full ecommerce frontend, add/edit/delete product, `Catalog.Write`, other services, Entra External ID customer sign-in, Dockerized JWT validation, and Azure deployment are **future labs**.

**No Azure hosting provisioned; no paid services required for this frontend.**

## Troubleshooting

- **Redirect URI mismatch**: ensure the browser URL and registered SPA redirect match `http://localhost:5173` exactly.
- **User can't sign in**: ensure you use the single-tenant workforce identity for this learning registration; a personal Microsoft account alone may not work as a tenant user.
- **Consent prompt or consent error**: `Catalog.Read` is delegated; admin-approval requirements depend on your tenant's consent policies.
- **401 with token**: verify the token's `aud`, `iss`, expiry and the Catalog API's `AzureAd` client/tenant IDs.
- **403 with token**: verify `scp` contains `Catalog.Read`.
- **CORS error**: allow `http://localhost:5173` in Catalog API and verify the backend HTTP URL/port.
- **API 500**: first verify SQL Server availability; the React UI doesn't connect to SQL Server directly.
