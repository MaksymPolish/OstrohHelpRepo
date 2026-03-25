import React from 'react';
import ReactDOM from 'react-dom/client';
import { GoogleOAuthProvider } from '@react-oauth/google';
import './index.css';
import App from './App';
import { GOOGLE_CLIENT_ID, USE_MOCK_AUTH } from './config/env';
import { validateEnv } from './config/validateEnv';

validateEnv();

const root = ReactDOM.createRoot(document.getElementById('root'));
const appTree = (
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

root.render(
  USE_MOCK_AUTH ? appTree : <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>{appTree}</GoogleOAuthProvider>
);
 