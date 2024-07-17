// src/pages/DashboardPage.tsx
import React from "react";
import { useAuth } from "../hooks/useAuth";
import logo from "../assets/logpunch-logo.svg"; // Import your SVG logo

const DashboardPage: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col items-center">
      <div className="flex justify-between items-center p-4 bg-white shadow-md w-full">
        <h1 className="text-2xl font-bold">Logpunch</h1>
        <button
          onClick={logout}
          className="px-4 py-2 text-white bg-red-500 rounded hover:bg-red-600"
        >
          Logout
        </button>
      </div>
      <div className="flex-grow flex flex-col items-center justify-center p-8 w-full">
        <h1 className="text-xl mb-4">Greetings {user?.firstName}!</h1>
        <img src={logo} alt="Logo" className="w-512 h-512 mb-5 -mt-12" />{" "}
        {/* Adjust the size as needed */}
        <div className="w-full max-w-4xl bg-white shadow-md rounded-lg p-8">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 w-full">
            <button className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
              Create Registration
            </button>
            <button className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
              Clients
            </button>
            <button className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
              Overview
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
