import React, { useState } from "react";
import AuthService from "../services/AuthService";
import { API_BASE_URL } from "../config";

interface LoginBoxProps {
  onLoginSuccess: () => void;
}

const LoginBox: React.FC<LoginBoxProps> = ({ onLoginSuccess }) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    // Basic validation
    if (!email.trim() || !password.trim()) {
      setError("Please enter both email and password.");
      return;
    }

    try {
      const formData = new FormData();
      formData.set("email", email);
      formData.set("password", password);

      const response = await fetch(`${API_BASE_URL}/login/authorize`, {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        throw new Error("Failed to log in.");
      }

      const token = await response.text();
      console.log(token)
      // Store the token in localStorage
      // AuthService.login(token);
      localStorage.setItem("token",token)

      // Redirect here if needed
      onLoginSuccess();
    } catch (error) {
      setError("An error occurred while logging in.");
      console.error(error);
    }
  };

  return (
    <div className="flex justify-center items-center pt-20">
      <form className="bg-white shadow-md rounded px-8 pt-6 pb-8 mb-4" onSubmit={handleSubmit}>
      <div className="mb-4">
        <label className="block text-gray-700 text-sm font-bold font-mono mb-2">
          <p>Username</p>
          </label>
          <input className="shadow-md appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            type="text"
            placeholder="Email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            />
            </div>
            <div className="mb-6">
        <label className="block text-gray-700 text-sm font-bold font-mono mb-2">
          <p>Password</p>
          </label>
          <input className="shadow-md appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            type="password"
            placeholder="***********"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          </div>
        <div className="text-red-500 text-xs italic">
          {error && <p className="error-message">{error}</p>}
          <button className="h-10 px-6 font-semibold rounded-md bg-cyan-800 text-white" type="submit">Login</button>
        </div>
      </form>
    </div>
  );
};

export default LoginBox;
