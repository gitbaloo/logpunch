import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import DashboardPage from "./pages/DashboardPage";
import OverviewPage from "./pages/OverviewPage";
import RegistrationPage from "./pages/RegistrationPage";
import PrivateRoute from "./components/PrivateRoute";
import { useAuth } from "./hooks/useAuth";

const App: React.FC = () => {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/dashboard"
        element={<PrivateRoute element={DashboardPage} />}
      />
      <Route
        path="/overview"
        element={<PrivateRoute element={OverviewPage} />}
      />
      <Route
        path="/registration"
        element={<PrivateRoute element={RegistrationPage} />}
      />
      <Route
        path="/"
        element={
          isAuthenticated === null ? (
            <div>Loading...</div>
          ) : isAuthenticated ? (
            <Navigate to="/dashboard" />
          ) : (
            <Navigate to="/login" />
          )
        }
      />
    </Routes>
  );
};

export default App;
