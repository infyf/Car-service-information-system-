import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"; 
import App from "./App";
import { AuthProvider } from "./context/AuthContext"; 
import "./index.css";

import 'bootstrap/dist/css/bootstrap.min.css';


import 'bootstrap/dist/js/bootstrap.bundle.min.js';


const queryClient = new QueryClient();

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>

        <QueryClientProvider client={queryClient}>
          <App />
        </QueryClientProvider>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);