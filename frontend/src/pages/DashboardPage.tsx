import React from "react";
import { useAuth } from "../hooks/useAuth";
import TopBar from "../components/TopBar";

const DashboardPage: React.FC = () => {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">
      <TopBar />
      <div className="flex-grow flex flex-col items-center justify-center p-4 w-full">
        <h1 className="text-4xl sm:text-6xl font-bold mb-4 sm:mb-12 text-center">
          Greetings {user?.firstName}!
        </h1>
      </div>
    </div>
  );
};

export default DashboardPage;
