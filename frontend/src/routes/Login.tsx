import React from "react";
import { useNavigate } from "react-router-dom";
import LoginBox from "../components/LoginBox";

const Login: React.FC = () => {
  const navigate = useNavigate();

  const handleLoginSuccess = () => {
    navigate("/");
  };

  return <LoginBox onLoginSuccess={handleLoginSuccess} />;
};

export default Login;
