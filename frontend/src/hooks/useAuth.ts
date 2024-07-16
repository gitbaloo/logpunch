import { useState, useEffect } from "react";
import { authorizeLogin } from "../services/api";

export const useAuth = () => {
  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem("token")
  );
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (token) {
      localStorage.setItem("token", token);
    } else {
      localStorage.removeItem("token");
    }
  }, [token]);

  const login = async (email: string, password: string) => {
    try {
      const token = await authorizeLogin(email, password);
      setToken(token);
      setError(null);
    } catch (error) {
      setError("Login failed: Invalid email or password");
    }
  };

  const logout = () => {
    setToken(null);
  };

  return { token, error, login, logout };
};
