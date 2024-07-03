import React from "react";
import { useNavigate } from "react-router-dom";
import { useAuthenticatedConsultant } from "../hooks/useAuthenticatedConsultant";
import { LOGO } from "../config";
import LogoutButton from "../components/LogoutButton";

const Home: React.FC = () => {
  const navigate = useNavigate();
  const loggedInConsultant = useAuthenticatedConsultant();

  const handleLogin = () => {
    navigate("/login");
  };

  const handleRegisterTime = () => {
    navigate("/customer-selector");
  };

  const goToOverview = () => {
    navigate("/overview");
  };

  return (
    <div className="container text-center sm:p-20 xl:px-96">
      <div>
      <h1 className="relative w-full flex-none mb-2 text-4xl font-semibold text-teal">
        {loggedInConsultant ? `Welcome back ${loggedInConsultant.email}!` : "Welcome to Punchlog.io"}
      </h1>
      </div>
      <div>
      <img src={LOGO} alt="" className="object-contain h-48 w-100 py-4" loading="lazy" />
      </div>
      <div className="flex flex-row p-2 justify-center">
      {
        loggedInConsultant ? (
          <>
          <div className="">
            <LogoutButton />
            </div>
            <div className=" pl-5">
            <button className="bg-cyan-800 hover:bg-cyan-700 text-white font-bold py-2 px-4 border border-gray-400 rounded shadow" onClick={goToOverview}>Overview</button>
            </div>
            <div className="pl-5">
            <button className="bg-cyan-800 hover:bg-cyan-700 text-white font-bold py-2 px-4 border border-gray-400 rounded shadow" onClick={handleRegisterTime}>Register Time</button>
            </div>
          </>
        ) : (
          <button className="bg-cyan-800 hover:bg-cyan-700 text-white font-bold py-2 px-4 border border-gray-400 rounded shadow" onClick={handleLogin}>Login</button>
        )
      }
      </div>
    </div>
  );
};

export default Home;
