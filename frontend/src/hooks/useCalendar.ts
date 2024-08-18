import { useState, useEffect } from "react";
import { DateTime } from "luxon";

const getISOWeekNumber = (date: Date): number => {
  const tempDate = new Date(date);
  tempDate.setHours(0, 0, 0, 0);
  tempDate.setDate(tempDate.getDate() + 3 - ((tempDate.getDay() + 6) % 7));
  const week1 = new Date(tempDate.getFullYear(), 0, 4);
  return (
    1 +
    Math.round(
      ((tempDate.getTime() - week1.getTime()) / 86400000 -
        3 +
        ((week1.getDay() + 6) % 7)) /
        7
    )
  );
};

const useCalendar = () => {
  const [currentDate, setCurrentDate] = useState(new Date());
  const [weekNumber, setWeekNumber] = useState(getISOWeekNumber(currentDate));
  const [dateTimeOffset, setDateTimeOffset] = useState<string>("");

  useEffect(() => {
    const now = new Date();
    setCurrentDate(now);
    setWeekNumber(getISOWeekNumber(now));
    const isoString = DateTime.fromJSDate(now).toISO();
    setDateTimeOffset(isoString || "");
  }, []);

  const isDifferentYear = (startDate: Date, endDate: Date): boolean => {
    return startDate.getFullYear() !== endDate.getFullYear();
  };

  const isDifferentMonth = (startDate: Date, endDate: Date): boolean => {
    return (
      isDifferentYear(startDate, endDate) ||
      startDate.getMonth() !== endDate.getMonth()
    );
  };

  const isDifferentWeek = (startDate: Date, endDate: Date): boolean => {
    return (
      isDifferentMonth(startDate, endDate) ||
      getISOWeekNumber(startDate) !== getISOWeekNumber(endDate)
    );
  };

  const isDifferentDay = (startDate: Date, endDate: Date): boolean => {
    return (
      isDifferentWeek(startDate, endDate) ||
      startDate.getDate() !== endDate.getDate()
    );
  };

  return {
    currentDate,
    weekNumber,
    dateTimeOffset,
    isDifferentYear,
    isDifferentMonth,
    isDifferentWeek,
    isDifferentDay,
  };
};

export default useCalendar;
