import React from "react";
import { Navigate, useLocation } from "react-router-dom";

interface PrivateRouteProps {
  element: React.ComponentType;
  path?: string; // Optional since we don't use it directly here
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ element: Element }) => {
  const isAuthenticated = Boolean(localStorage.getItem("token")); // Replace with your auth logic
  const location = useLocation();

  return isAuthenticated ? (
    <Element />
  ) : (
    <Navigate to="/login" state={{ from: location }} />
  );
};

export default PrivateRoute;
