import React from "react";
import { useAuth } from "../hooks/useAuth";
import TopBar from "../components/TopBar";
import logo from "../assets/logpunch-logo.svg";

const DashboardPage: React.FC = () => {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">
      <TopBar />
      <div className="flex-grow flex flex-col items-center justify-center p-4 w-full">
        <h1 className="text-4xl sm:text-6xl font-bold mb-4 sm:mb-12 text-center">
          Greetings {user?.firstName}!
        </h1>
        <img
          src={logo}
          alt="Logo"
          className="w-32 h-32 sm:w-64 sm:h-64 mb-4 sm:mb-2"
        />
      </div>
    </div>
  );
};

export default DashboardPage;
