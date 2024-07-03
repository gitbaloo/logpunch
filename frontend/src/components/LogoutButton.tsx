import React from "react";
import AuthService from "../services/AuthService";
import { useNavigate } from "react-router-dom";

const LogoutButton: React.FC = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    AuthService.logout();
    navigate("/");
    window.location.reload();
  };

  return <button className="bg-cyan-800 hover:bg-cyan-700 text-white font-bold py-2 px-4 border border-gray-400 rounded shadow" onClick={handleLogout}>Logout</button>;
};

export default LogoutButton;
