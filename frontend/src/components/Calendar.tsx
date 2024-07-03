import React, { useState, useEffect } from "react";

interface CalendarProps {
  render: (weekday: string, date: string) => React.ReactNode;
}

const Calendar: React.FC<CalendarProps> = ({ render }) => {
  const [weekday, setWeekday] = useState<string>("");
  const [date, setDate] = useState<string>("");

  useEffect(() => {
    const getCurrentDate = () => {
      const days = [
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
        "Sunday",
      ];
      const today = new Date();
      const currentWeekday = days[today.getDay() - 1];
      const currentDate = today.toDateString().substring(4);

      setWeekday(currentWeekday);
      setDate(currentDate);
    };

    getCurrentDate();
  }, []);

  return render(weekday, date);
};

export default Calendar;
