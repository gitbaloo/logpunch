import React, { useState, useEffect } from "react";
import { toast } from "react-toastify";
import { useNavigate } from "react-router-dom";
import "react-toastify/dist/ReactToastify.css";
import { API_BASE_URL } from "../config";
import { useParams, useLocation } from "react-router-dom";
import { useAuthenticatedConsultant } from "../hooks/useAuthenticatedConsultant";

interface TimeRegistrationProps { }

const TimeRegistration: React.FC<TimeRegistrationProps> = () => {
  const [hours, setHours] = useState<number>(0);
  const [minutes, setMinutes] = useState<number>(0);
  const [totalHours, setTotalHours] = useState<number>(0);
  const loggedInConsultant = useAuthenticatedConsultant();
  const { customerId } = useParams<{ customerId: string }>();
  const location = useLocation();
  const { customerName } = location.state;
  const selectedCustomer = { id: customerId, name: customerName };
  const navigate = useNavigate();
  const [message, setMessage] = useState({ visible: false, type: '', text: '' });
  const [isButtonDisabled, setIsButtonDisabled] = useState(false);

  useEffect(() => {
    let timer;
    if (message.visible) {
      setIsButtonDisabled(true);
      timer = setTimeout(() => {
        setMessage({ ...message, visible: false });
        setIsButtonDisabled(false);
      }, 5000);
    }

    return () => clearTimeout(timer);
  }, [message]);

  //Save button function - Edit when backend is functional
  const handleSave = async () => {
    try {
      if (!selectedCustomer || !loggedInConsultant) {
        toast.error("Error: No selected customer or logged in consultant");
        return;
      }

      if (totalHours === 0) {
        const errorMessage = "Error: Cannot register 0 hours";
        toast.error(errorMessage);
        console.log(errorMessage);
        return;
      }

      if (totalHours > 10) {
        const confirmMessage =
          "You are about to register more than 10 hours. Are you sure you want to continue?";
        if (!window.confirm(confirmMessage)) {
          return;
        }
      }

      //API call to the backend CHANGE ADDRESS WHEN GOING LIVE
      const response = await fetch(`${API_BASE_URL}/time-registration`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          consultantId: loggedInConsultant.id,
          customerId: selectedCustomer.id,
          hours: totalHours,
        }),
      });

      if (!response.ok) {
        throw new Error(`Failed to register time: ${response.statusText}`);
      }

      const timeRegistered = await response.json();
      console.log("Time registered:", timeRegistered);

      setMessage({
        visible: true,
        type: 'success',
        text: `Time successfully registered for ${totalHours} hours with ${selectedCustomer.name}`,
      });
      
    } catch (error) {
      toast.error(`An error occurred: ${error}`);
      console.error(`An error occurred: ${error}`);
      setMessage({
        visible: true,
        type: 'error',
        text: `An error occurred: ${error.message}`,
      });
    }
  };

  const handleBack = () => {
    navigate("/customer-selector");
    //Needs navigation
  };

  //Rounding the minutes to next half hour
  useEffect(() => {
    let newTotalHours = hours;

    if (minutes > 0 && minutes <= 30) {
      newTotalHours += 0.5;
    }
    if (minutes > 30 && minutes <= 60) {
      newTotalHours += 1;
    }

    setTotalHours(newTotalHours);
  }, [hours, minutes]);

  //Effect to ensure that minutes cant be set once 24 hours (max) is set
  useEffect(() => {
    if (hours >= 24) {
      setMinutes(0);
    }
  });

  return (
    <div className="text-center p-2">
      {message.visible && (
        <div role="alert">
          <div className={`font-bold rounded-t px-4 py-2 ${message.type === 'error' ? 'bg-red-500 text-white' : 'bg-green-500 text-white' }`}>
            {message.type === 'error' ? 'Error' : 'Success'}
          </div>
          <div className={`border rounded-b px-4 py-3 ${message.type === 'error' ? 'border-red-400 bg-red-100 text-red-700' : 'border-green-400 bg-green-100 text-green-700'}`}>
            <p>{message.text}</p>
          </div>
        </div>
      )}
      <div>
        {selectedCustomer && <h1>Customer: {selectedCustomer.name}</h1>}
      </div>
      <div className="text-center p-3">
        <label className="p-1">Hours: </label>
        <input className="shadow-md appearance-none border w-16 rounded py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
          type="number"
          placeholder="Hours"
          value={hours}
          onChange={(e) => {
            const value = +e.target.value;
            if (value >= 0 && value <= 24) {
              setHours(value);
            }
          }}
        />
        <label className="pl-6 pr-1"> Minutes:</label>
        <input className="shadow-md appearance-none border rounded w-15 py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
          type="number"
          placeholder="Minutes"
          value={minutes}
          onChange={(e) => {
            const value = +e.target.value;
            if (value >= 0 && value <= 59) {
              setMinutes(value);
            }
          }}
          min={0}
          max={59}
        />
      </div>
      <div className="pt-3">
        <button className="h-10 px-7 font-semibold rounded-md bg-cyan-800 text-white" onClick={handleBack}>Back</button>
        <span className="pr-9"></span>
        <button
      className={`h-10 px-7 font-semibold rounded-md text-white ${isButtonDisabled ? 'bg-gray-400' : 'bg-green-600'}`}
      onClick={handleSave}
      disabled={isButtonDisabled} // Use the isButtonDisabled state to control the disabled attribute
    >
      Save
    </button>
      </div>
    </div>
  );
};

export default TimeRegistration;
