import React from "react";
import AuthService from "../services/AuthService";

const LoginStatus: React.FC = () => {
  const isLoggedIn = AuthService.isAuthenticated();

  return (
    <div>
      {isLoggedIn ? <p>User is logged in</p> : <p>User is not logged in</p>}
    </div>
  );
};

export default LoginStatus;
