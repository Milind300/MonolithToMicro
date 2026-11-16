import {
  BrowserCacheLocation,
  PublicClientApplication,
} from "@azure/msal-browser";

const tenantId =
  import.meta.env.VITE_ENTRA_TENANT_ID ||
  "43ed8cd9-5f65-4dbe-b58f-67aa7022542e";

const clientId =
  import.meta.env.VITE_ENTRA_CLIENT_ID ||
  "5c1dae0e-4d93-4413-8c90-4385a0a7e428";

export const catalogScope =
  import.meta.env.VITE_CATALOG_SCOPE ||
  "api://e3c9f03f-b3ed-4f57-85c6-21accee06687/Catalog.Read";

export const catalogWriteScope =
  "api://e3c9f03f-b3ed-4f57-85c6-21accee06687/Catalog.Write";

export const msalInstance = new PublicClientApplication({
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: "http://localhost:5173",
    postLogoutRedirectUri: "http://localhost:5173",
  },
  cache: {
    cacheLocation: BrowserCacheLocation.SessionStorage,
  },
});

export const signInRequest = {
  scopes: ["openid", "profile", catalogScope],
};

export const tokenRequest = {
  scopes: [catalogScope],
};

export const writeTokenRequest = {
  scopes: [catalogWriteScope],
};
