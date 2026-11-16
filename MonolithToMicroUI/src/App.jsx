import { useState } from "react";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { InteractionRequiredAuthError } from "@azure/msal-browser";
import {
  signInRequest,
  tokenRequest,
  writeTokenRequest,
} from "./auth";

const defaultApiUrl = import.meta.env.VITE_CATALOG_API_URL || "";

function readableError(error) {
  return error?.message || String(error);
}

function getCatalogUrl(value) {
  const url = new URL(value.trim());

  if (
    !["http:", "https:"].includes(url.protocol) ||
    !["localhost", "127.0.0.1"].includes(url.hostname)
  ) {
    throw new Error("Only localhost URLs are allowed for this lab.");
  }

  return `${url.origin}/api/catalog`;
}

function hasScope(scopes, scopeName) {
  const expected = scopeName.toLowerCase();

  return scopes.some((scope) => {
    const value = scope.toLowerCase();
    return value === expected || value.endsWith(`/${expected}`);
  });
}

async function readResponse(response) {
  const text = await response.text();

  if (!text) return null;

  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

export default function App() {
  const { instance, accounts } = useMsal();
  const isAuthenticated = useIsAuthenticated();

  const [apiUrl, setApiUrl] = useState(defaultApiUrl);
  const [responseBody, setResponseBody] = useState(null);
  const [status, setStatus] = useState(null);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");

  const activeAccount = instance.getActiveAccount() || accounts[0] || null;

  function clearResult() {
    setStatus(null);
    setResponseBody(null);
    setMessage("");
  }

  async function acquireToken(request) {
    if (!activeAccount) {
      throw new Error("Sign in with Microsoft first.");
    }

    try {
      return await instance.acquireTokenSilent({
        ...request,
        account: activeAccount,
      });
    } catch (error) {
      if (!(error instanceof InteractionRequiredAuthError)) {
        throw error;
      }

      return instance.acquireTokenPopup({
        ...request,
        account: activeAccount,
      });
    }
  }

  async function signIn() {
    setBusy(true);
    clearResult();

    try {
      const result = await instance.loginPopup(signInRequest);

      if (result.account) {
        instance.setActiveAccount(result.account);
      }

      setMessage("Signed in. You can now call the Catalog API.");
    } catch (error) {
      setMessage(`Sign-in failed: ${readableError(error)}`);
    } finally {
      setBusy(false);
    }
  }

  async function signOut() {
    setBusy(true);
    clearResult();

    try {
      await instance.logoutPopup({ account: activeAccount });
    } catch (error) {
      setMessage(`Sign-out failed: ${readableError(error)}`);
    } finally {
      setBusy(false);
    }
  }

   async function executeTest(testName) {
    setBusy(true);
    clearResult();

    try {
      const needsWrite =
        testName === "invalidName" || testName === "createItem";

      const readOnlyClientId = "7b2f3eda-779a-4f66-b646-b4de983348a2";

      const currentClientId =
        instance.getConfiguration().auth.clientId.toLowerCase();

      if (needsWrite && currentClientId === readOnlyClientId) {
        setMessage(
          "This test requires Catalog.Write. Switch to the Read/Write test client before continuing."
        );
        return;
      }

      const requestUrl = getCatalogUrl(apiUrl);

      const headers = {
        Accept: "application/json",
      };

      let method = "GET";
      let body;

      if (testName !== "withoutToken") {
        const tokenResponse = await acquireToken(
          needsWrite ? writeTokenRequest : tokenRequest
        );

        if (
          testName === "readOnlyPost" &&
          hasScope(tokenResponse.scopes, "Catalog.Write")
        ) {
          setMessage(
            "Test stopped: the returned token includes Catalog.Write. " +
              "Switch to the Read-only test client before retrying."
          );
          return;
        }

        if (
          testName === "readOnlyPost" &&
          !hasScope(tokenResponse.scopes, "Catalog.Read")
        ) {
          setMessage(
            "Test stopped: Catalog.Read was not reported in the returned scopes."
          );
          return;
        }

        if (
          needsWrite &&
          !hasScope(tokenResponse.scopes, "Catalog.Write")
        ) {
          setMessage(
            "Test stopped: Catalog.Write was not reported in the returned scopes. " +
              "Switch to the Read/Write test client before retrying."
          );
          return;
        }

        // Do not log or display the access token.
        headers.Authorization = `Bearer ${tokenResponse.accessToken}`;
      }

      if (
        testName === "readOnlyPost" ||
        testName === "invalidName" ||
        testName === "createItem"
      ) {
        method = "POST";
        headers["Content-Type"] = "application/json";

        const item = {
          name: testName === "invalidName" ? "" : "Day 05 Test Item",
          description: "MonolithToMicro Day 05 lab",
          category: "Learning",
          price: 100,
          isActive: true,
        };

        body = JSON.stringify(item);
      }

      const response = await fetch(requestUrl, {
        method,
        headers,
        ...(body ? { body } : {}),
      });

      const data = await readResponse(response);

      setStatus(response.status);
      setResponseBody(data);

      if (testName === "withoutToken") {
        setMessage(
          response.status === 401
            ? "PASS: HTTP 401 — the API requires authentication."
            : `Unexpected HTTP ${response.status}; expected 401.`
        );
        return;
      }

      if (testName === "readCatalog") {
        setMessage(
          response.status === 200
            ? "PASS: HTTP 200 — Catalog data returned."
            : `Unexpected HTTP ${response.status}; expected 200.`
        );
        return;
      }

      if (testName === "readOnlyPost") {
        setMessage(
          response.status === 403
            ? "PASS: HTTP 403 — the Read-only token cannot create items."
            : `Unexpected HTTP ${response.status}; expected 403.`
        );
        return;
      }

      if (testName === "invalidName") {
        const nameErrors = data?.errors?.Name ?? data?.errors?.name;

        setMessage(
          response.status === 400 &&
            Array.isArray(nameErrors) &&
            nameErrors.length > 0
            ? "PASS: HTTP 400 — the API rejected the empty Name."
            : `Unexpected HTTP ${response.status}; expected 400 with a Name error.`
        );
        return;
      }

      if (testName === "createItem") {
        setMessage(
          response.status === 201 && data?.id
            ? "PASS: HTTP 201 — test item created. Verify it using GET."
            : `Unexpected HTTP ${response.status}; expected 201 with an ID.`
        );
      }
    } catch (error) {
      setMessage(`Request failed: ${readableError(error)}`);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="app">
      <header className="topbar">
        <div className="brand">
          <span className="brand-mark">
            M<span>→</span>M
          </span>
          <span>
            MonolithToMicro <small>Learning Platform</small>
          </span>
        </div>
        <span className="environment">DAY 05 · CATALOG API LAB</span>
      </header>

      <main>
        <div className="eyebrow">AZURE · .NET 10 · REACT · MSAL</div>

        <h1>
          Secure Catalog <em>Explorer</em>
        </h1>

        <p className="intro">
          Sign in with Microsoft Entra ID and test Catalog API authentication,
          authorization and request validation.
        </p>

        <div className="columns">
          <section className="panel">
            <div className="section-heading">
              <span className="step">01</span>
              <h2>Microsoft identity</h2>
            </div>

            <p className="muted">
              Single-tenant login using Authorization Code + PKCE.
            </p>

            <div className="account">
              <div className="avatar">
                {isAuthenticated
                  ? (activeAccount?.name || "U")[0].toUpperCase()
                  : "◎"}
              </div>

              <div>
                <strong>
                  {isAuthenticated
                    ? activeAccount?.name || "Signed in"
                    : "Not signed in"}
                </strong>

                <div className="muted small">
                  {isAuthenticated
                    ? activeAccount?.username
                    : "Microsoft Entra ID Free tenant"}
                </div>
              </div>
            </div>

            <button
              type="button"
              className="primary"
              disabled={busy}
              onClick={isAuthenticated ? signOut : signIn}
            >
              {isAuthenticated ? "Sign out" : "Sign in with Microsoft"}
            </button>

            <div className="detail">
              <span>Client configuration</span>
              <span>Selected through .env.local</span>
            </div>

            <div className="detail">
              <span>Token acquisition</span>
              <span>MSAL · Silent / Popup</span>
            </div>
          </section>

          <section className="panel">
            <div className="section-heading">
              <span className="step">02</span>
              <h2>Protected API</h2>
            </div>

            <p className="muted">
              Use port 5101 for Visual Studio or port 8085 for Docker.
            </p>

            <label htmlFor="apiUrl">Catalog API base URL</label>
            <input
              id="apiUrl"
              value={apiUrl}
              onChange={(event) => setApiUrl(event.target.value)}
              placeholder="http://localhost:8085/api/catalog"
              spellCheck="false"
            />

            <div className="actions">
              <button
                type="button"
                className="secondary"
                disabled={busy}
                onClick={() => executeTest("withoutToken")}
              >
                Test without token
              </button>

              <button
                type="button"
                className="primary"
                disabled={busy || !isAuthenticated}
                onClick={() => executeTest("readCatalog")}
              >
                Fetch with JWT
              </button>

              <button
                type="button"
                className="secondary"
                disabled={busy || !activeAccount}
                onClick={() => executeTest("readOnlyPost")}
              >
                Test Read-only POST (403)
              </button>

              <button
                type="button"
                className="secondary"
                disabled={busy || !activeAccount}
                onClick={() => executeTest("invalidName")}
              >
                Test invalid Name (400)
              </button>

              <button
                type="button"
                className="secondary"
                disabled={busy || !activeAccount}
                onClick={() => executeTest("createItem")}
              >
                Create Day 05 test item (201)
              </button>
            </div>

            <div className="statusline">
              The Create button adds a test item to your local SQL database.
              Each click can create another item.
            </div>
          </section>
        </div>

        <section className="results panel">
          <div className="result-head">
            <div>
              <span className="step">03</span>
              <h2>API response</h2>
            </div>

            <span
              className={
                "status " +
                (status === 200 || status === 201
                  ? "ok"
                  : status === 401
                    ? "warn"
                    : status === 403
                      ? "bad"
                      : "")
              }
              aria-live="polite"
            >
              {busy
                ? "Working…"
                : status === null
                  ? "Not tested"
                  : `HTTP ${status}`}
            </span>
          </div>

          {message && (
            <p role="status" className="notice">
              {message}
            </p>
          )}

          {responseBody !== null ? (
            <div>
              <p className="muted">
                {Array.isArray(responseBody)
                  ? `${responseBody.length} record(s) returned`
                  : "API response"}
              </p>
              <pre>{JSON.stringify(responseBody, null, 2)}</pre>
            </div>
          ) : (
            <div className="empty">
              {busy
                ? "Waiting for the operation to finish."
                : "No response body to display."}
            </div>
          )}
        </section>

        <footer>
          <span>
            Backend: Catalog → Service → Repository → EF Core → SQL Server
          </span>
          <span>Local learning only · No paid Azure resources</span>
        </footer>
      </main>
    </div>
  );
}