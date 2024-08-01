import React from "react";
import TopBar from "../components/TopBar";
import Clients from "../components/Clients";

const ClientsPage: React.FC = () => {
  return (
    <div>
      <div className="min-h-screen bg-gray-100 flex flex-col">
        <TopBar />
        <div className="flex-grow flex flex-col items-center justify-center p-4 w-full">
          <Clients />
        </div>
      </div>
    </div>
  );
};

export default ClientsPage;
