import React, { useState, useEffect } from "react";
import useCalendar from "../hooks/useCalendar";

const Calendar: React.FC = () => {
  const { currentDate, weekNumber } = useCalendar();
  const [currentTime, setCurrentTime] = useState(new Date());

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);
    return () => clearInterval(interval); // Cleanup on component unmount
  }, []);

  const formattedDate = currentDate.toLocaleDateString("en-US", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  const formatTime = (date: Date) => {
    const hours = date.getHours().toString().padStart(2, "0");
    const minutes = date.getMinutes().toString().padStart(2, "0");
    return `${hours}:${minutes}`;
  };

  const formattedTime = formatTime(currentTime);

  return (
    <div>
      <h2 className="text-left sm:block hidden">Time: {formattedTime}</h2>
      <h2 className="text-left sm:block hidden">Date: {formattedDate}</h2>
      <h2 className="text-left sm:block hidden">Week: {weekNumber}</h2>
    </div>
  );
};

export default Calendar;
