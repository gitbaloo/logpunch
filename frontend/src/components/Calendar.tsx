import React from "react";
import useCalendar from "../hooks/useCalendar";

const Calendar: React.FC = () => {
  const { selectedDate, weekNumber, selectDate } = useCalendar(new Date());

  return (
    <div>
      <h1>Calendar</h1>
      <p>Selected Date: {selectedDate.toDateString()}</p>
      <p>Week Number: {weekNumber}</p>
      <input
        type="date"
        value={selectedDate.toISOString().substr(0, 10)}
        onChange={(e) => selectDate(new Date(e.target.value))}
      />
    </div>
  );
};

export default Calendar;
