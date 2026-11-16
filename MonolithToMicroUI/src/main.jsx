import React from 'react';
import { createRoot } from 'react-dom/client';
import { MsalProvider } from '@azure/msal-react';
import { msalInstance } from './auth';
import App from './App';
import './styles.css';

async function start() {
  await msalInstance.initialize();
  // Popup login is used: no redirect processing is necessary for this lab's normal flow.
  createRoot(document.getElementById('root')).render(
    <React.StrictMode><MsalProvider instance={msalInstance}><App /></MsalProvider></React.StrictMode>
  );
}

start().catch(err => {
  console.error('MSAL initialization failed', err);
  document.getElementById('root').textContent = 'Authentication initialization failed. See browser console.';
});
