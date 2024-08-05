import { useState, useEffect } from "react";
import { authorizeLogin, authenticateUser } from "../services/apiService";
import { useNavigate } from "react-router-dom";
import { User } from "../types/genericTypes";

export const useAuth = () => {
  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem("token")
  );
  const [error, setError] = useState<string | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState<boolean | null>(null);
  const [user, setUser] = useState<User | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const checkAuthentication = async () => {
      if (token) {
        try {
          const user = await authenticateUser(token);
          setUser(user);
          setIsAuthenticated(true);
        } catch (error) {
          setIsAuthenticated(false);
          localStorage.removeItem("token");
        }
      } else {
        setIsAuthenticated(false);
      }
    };

    checkAuthentication();
  }, [token]);

  const login = async (email: string, password: string) => {
    try {
      const token = await authorizeLogin(email, password);
      setToken(token);
      localStorage.setItem("token", token);
      setError(null);
      setIsAuthenticated(true);
      navigate("/dashboard");
    } catch (error) {
      setError("Login failed: Invalid email or password");
    }
  };

  const logout = () => {
    setToken(null);
    localStorage.removeItem("token");
    setIsAuthenticated(false);
    setUser(null);
    navigate("/login");
  };

  return { token, error, login, logout, isAuthenticated, user };
};
