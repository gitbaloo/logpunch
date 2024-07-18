import React, { useState } from "react";
import { useAuth } from "../hooks/useAuth";
import { useNavigate, useLocation } from "react-router-dom";

const TopBar: React.FC = () => {
  const { logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const handleNavigate = (path: string) => {
    navigate(path);
    setIsMenuOpen(false); // Close menu on navigate
  };

  return (
    <div className="flex justify-between items-center p-4 bg-white shadow-md w-full">
      <h1 className="text-2xl font-bold sm:block hidden">Logpunch</h1>
      <div className="sm:flex hidden space-x-4">
        <button
          onClick={() => handleNavigate("/dashboard")}
          className={`px-4 py-2 rounded ${
            location.pathname === "/dashboard"
              ? "bg-blue-600 text-white"
              : "bg-blue-500 text-white hover:bg-blue-600"
          }`}
        >
          Dashboard
        </button>
        <button
          onClick={() => handleNavigate("/overview")}
          className={`px-4 py-2 rounded ${
            location.pathname === "/overview"
              ? "bg-blue-600 text-white"
              : "bg-blue-500 text-white hover:bg-blue-600"
          }`}
        >
          Overview
        </button>
        <button
          onClick={() => handleNavigate("/registration")}
          className={`px-4 py-2 rounded ${
            location.pathname === "/registration"
              ? "bg-blue-600 text-white"
              : "bg-blue-500 text-white hover:bg-blue-600"
          }`}
        >
          Create Registration
        </button>
        <button
          onClick={() => handleNavigate("/clients")}
          className={`px-4 py-2 rounded ${
            location.pathname === "/clients"
              ? "bg-blue-600 text-white"
              : "bg-blue-500 text-white hover:bg-blue-600"
          }`}
        >
          Clients
        </button>
      </div>
      <button
        onClick={logout}
        className="px-4 py-2 text-white bg-red-500 rounded hover:bg-red-600 sm:block hidden"
      >
        Logout
      </button>

      {/* Mobile menu */}
      <div className="sm:hidden flex items-center">
        <button
          onClick={() => setIsMenuOpen(!isMenuOpen)}
          className="text-gray-600 hover:text-gray-800 focus:outline-none focus:text-gray-800"
        >
          <svg
            className="h-6 w-6"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            {isMenuOpen ? (
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M6 18L18 6M6 6l12 12"
              />
            ) : (
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M4 6h16M4 12h16m-7 6h7"
              />
            )}
          </svg>
        </button>
      </div>
      {isMenuOpen && (
        <div className="sm:hidden absolute top-16 left-0 right-0 bg-white shadow-md z-10">
          <button
            onClick={() => handleNavigate("/dashboard")}
            className={`block px-4 py-2 w-full text-left ${
              location.pathname === "/dashboard"
                ? "bg-blue-600 text-white"
                : "bg-blue-500 text-white hover:bg-blue-600"
            }`}
          >
            Dashboard
          </button>
          <button
            onClick={() => handleNavigate("/overview")}
            className={`block px-4 py-2 w-full text-left ${
              location.pathname === "/overview"
                ? "bg-blue-600 text-white"
                : "bg-blue-500 text-white hover:bg-blue-600"
            }`}
          >
            Overview
          </button>
          <button
            onClick={() => handleNavigate("/registration")}
            className={`block px-4 py-2 w-full text-left ${
              location.pathname === "/registration"
                ? "bg-blue-600 text-white"
                : "bg-blue-500 text-white hover:bg-blue-600"
            }`}
          >
            Create Registration
          </button>
          <button
            onClick={() => handleNavigate("/clients")}
            className={`block px-4 py-2 w-full text-left ${
              location.pathname === "/clients"
                ? "bg-blue-600 text-white"
                : "bg-blue-500 text-white hover:bg-blue-600"
            }`}
          >
            Clients
          </button>
          <button
            onClick={logout}
            className="block px-4 py-2 w-full text-left text-white bg-red-500 hover:bg-red-600"
          >
            Logout
          </button>
        </div>
      )}
    </div>
  );
};

export default TopBar;
