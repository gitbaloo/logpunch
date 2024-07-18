import React, { useEffect } from "react";
import { useOverview } from "../hooks/useOverview";
import { isAfter, isBefore, parseISO, formatISO } from "date-fns";
import useWeekNumber from "../hooks/useWeekNumber";

interface OverviewParameterSelectorProps {
  overviewType: string;
  setOverviewType: (value: string) => void;
  timePeriod: string;
  setTimePeriod: (value: string) => void;
  startDate: Date;
  setStartDate: (value: Date) => void;
  endDate: Date;
  setEndDate: (value: Date) => void;
  timeMode: string;
  setTimeMode: (value: string) => void;
  groupBy: string;
  setGroupBy: (value: string) => void;
  thenBy: string;
  setThenBy: (value: string) => void;
  sortAscending: boolean;
  setSortAscending: (value: boolean) => void;
  noRecords: boolean;
  setNoRecords: (value: boolean) => void;
  setDefaultQuery: boolean;
  setSetDefaultQuery: (value: boolean) => void;
  custom: (value: boolean) => void;
  setCustom: (value: boolean) => void;
  groupByOptions: string[];
  thenByOptions: string[];
}

const OverviewParameterSelector: React.FC<OverviewParameterSelectorProps> = ({
  overviewType,
  setOverviewType,
  timePeriod,
  setTimePeriod,
  startDate,
  setStartDate,
  endDate,
  setEndDate,
  timeMode,
  setTimeMode,
  groupBy,
  setGroupBy,
  thenBy,
  setThenBy,
  sortAscending,
  setSortAscending,
  noRecords,
  setNoRecords,
  setDefaultQuery: defaultQuery,
  setSetDefaultQuery: setDefaultQuery,
  custom,
  setCustom,
}) = useOverview();

  const today = new Date();
  const startWeekNumber = useWeekNumber(new Date(startDate));
  const endWeekNumber = useWeekNumber(new Date(endDate));

  const handleOverviewTypeChange = (
    e: React.ChangeEvent<HTMLSelectElement>
  ) => {
    setOverviewType(e.target.value);
  };

  const handleTimePeriodChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setTimePeriod(e.target.value);
    if (e.target.value === "custom") {
      setCustom(true);
    } else {
      setCustom(false);
    }
  };

  const handleTimeModeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setTimeMode(e.target.value);
  };

  const handleGroupByChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setGroupBy(e.target.value);
  };

  const handleThenByChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setThenBy(e.target.value);
  };

  const handleStartDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newStartDate = e.target.value;
    const todayISO = formatISO(today, { representation: "date" });

    if (isAfter(parseISO(newStartDate), parseISO(endDate))) {
      setEndDate(newStartDate);
    }

    if (isAfter(parseISO(newStartDate), parseISO(todayISO))) {
      setStartDate(todayISO);
    } else {
      setStartDate(newStartDate);
    }
  };

  const handleEndDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newEndDate = e.target.value;
    const todayISO = formatISO(today, { representation: "date" });
    if (isAfter(parseISO(newEndDate), new Date())) {
      setEndDate(new Date().toISOString().split("T")[0]);
    } else if (isBefore(parseISO(newEndDate), parseISO(startDate))) {
      setStartDate(newEndDate);
    }

    if (isAfter(parseISO(newEndDate), parseISO(todayISO))) {
      setEndDate(todayISO);
    } else {
      setEndDate(newEndDate);
    }
  };

  const getGroupByOptions = () => {
    let options: string[] = [];
    if (timePeriod == "custom") {
      const start = new Date(startDate);
      const end = new Date(endDate);

      if (start.getFullYear() != end.getFullYear()) {
        options.push("year");
      }
      if (start.getMonth() != end.getMonth()) {
        options.push("month");
      }
      if (startWeekNumber != endWeekNumber) {
        options.push("week");
      }
      options.push("day", "client");
    } else {
      if (timePeriod === "year") {
        options = ["year", "month", "week", "day", "client"];
      } else if (timePeriod === "month") {
        options = ["month", "week", "day", "client"];
      } else if (timePeriod === "week") {
        options = ["week", "day", "client"];
      } else if (timePeriod === "day") {
        options = ["day", "client"];
      }
    }
    return options;
  };

  const getThenByOptions = () => {
    let options: string[] = [];
    if (groupBy === "year") {
      options = ["month", "week", "day", "client", "none"];
    } else if (groupBy === "month") {
      options = ["week", "day", "client", "none"];
    } else if (groupBy === "week") {
      options = ["day", "client", "none"];
    } else if (groupBy === "day") {
      options = ["client", "none"];
    } else if (groupBy === "client") {
      options = ["year", "month", "week", "day"];
      if (timePeriod === "year") {
        options = options.filter((option) => option !== "year");
      } else if (timePeriod === "month") {
        options = options.filter(
          (option) => option !== "year" && option !== "month"
        );
      } else if (timePeriod === "week") {
        options = options.filter(
          (option) =>
            option !== "year" && option !== "month" && option !== "week"
        );
      } else if (timePeriod === "day") {
        options = ["none"];
      }
    }
    return options;
  };

  const groupByOptions = getGroupByOptions();
  const thenByOptions = getThenByOptions();

  useEffect(() => {
    // Reset `Group By` if the current value is no longer valid
    if (!groupByOptions.includes(groupBy)) {
      setGroupBy(groupByOptions[0]);
    }

    // Reset `Then By` if the current value is no longer valid
    if (!thenByOptions.includes(thenBy)) {
      setThenBy(thenByOptions[0]);
    }
  }, [
    timePeriod,
    groupBy,
    startDate,
    endDate,
    groupByOptions,
    thenByOptions,
    thenBy,
    setGroupBy,
    setThenBy,
  ]);

  const capitalize = (s: string) => {
    if (typeof s !== "string" || s.length === 0) return "";
    return s.charAt(0).toUpperCase() + s.slice(1);
  };

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col items-center">
      <div className="min-h-screen bg-gray-100 flex flex-col items-center justify-center p-8">
        <h1 className="text-4xl font-bold mb-8">Overview</h1>
        <form className="bg-white shadow-md rounded-lg p-8 w-full max-w-2xl">
          <div className="mb-4">
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Overview Type
            </label>
            <select
              value={overviewType}
              onChange={handleOverviewTypeChange}
              className="w-full px-3 py-2 border rounded"
            >
              <option value="work">Work</option>
              <option value="transport">Transport</option>
              <option value="absence">Absence</option>
            </select>
          </div>
          <div className="mb-4">
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Time Period
            </label>
            <select
              value={timePeriod}
              onChange={handleTimePeriodChange}
              className="w-full px-3 py-2 border rounded"
            >
              <option value="year">Year</option>
              <option value="month">Month</option>
              <option value="week">Week</option>
              <option value="day">Day</option>
              <option value="custom">Custom</option>
            </select>
          </div>
          {custom && (
            <div className="mb-4">
              <label className="block text-gray-700 text-sm font-bold mb-2">
                Start Date
              </label>
              <input
                type="date"
                value={startDate}
                onChange={handleStartDateChange}
                className="w-full px-3 py-2 border rounded"
              />
              <label className="block text-gray-700 text-sm font-bold mb-2 mt-2">
                End Date
              </label>
              <input
                type="date"
                value={endDate}
                onChange={handleEndDateChange}
                className="w-full px-3 py-2 border rounded"
                max={new Date().toISOString().split("T")[0]}
              />
            </div>
          )}
          {!custom && (
            <div className="mb-4">
              <label className="block text-gray-700 text-sm font-bold mb-2">
                Time Mode
              </label>
              <select
                value={timeMode}
                onChange={handleTimeModeChange}
                className="w-full px-3 py-2 border rounded"
              >
                <option value="last">Last</option>
                <option value="current">Current</option>
                <option value="rolling">Rolling</option>
              </select>
            </div>
          )}
          <div className="mb-4">
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Group By
            </label>
            <select
              value={groupBy}
              onChange={handleGroupByChange}
              className="w-full px-3 py-2 border rounded"
            >
              {groupByOptions.map((option) => (
                <option key={option} value={option}>
                  {option.charAt(0).toUpperCase() + option.slice(1)}
                </option>
              ))}
            </select>
          </div>
          <div className="mb-4">
            <label className="block text-gray-700 text-sm font-bold mb-2">
              Then By
            </label>
            <select
              value={thenBy}
              onChange={handleThenByChange}
              className="w-full px-3 py-2 border rounded"
              disabled={thenByOptions.length === 0}
            >
              {thenByOptions.map((option) => (
                <option key={option} value={option}>
                  {option.charAt(0).toUpperCase() + option.slice(1)}
                </option>
              ))}
            </select>
          </div>
          <div style={{ textAlign: "center", marginTop: "20px" }}>
            <div style={{ display: "inline-block", textAlign: "left" }}>
              <label>
                <input
                  type="checkbox"
                  checked={sortAscending}
                  onChange={(e) => setSortAscending(e.target.checked)}
                />
                Sort ascending
              </label>
              <br />
              <label>
                <input
                  type="checkbox"
                  checked={noRecords}
                  onChange={(e) => setNoRecords(e.target.checked)}
                />
                {capitalize(groupBy)}s with no records
              </label>
              <br />
              <label>
                <input
                  type="checkbox"
                  checked={defaultQuery}
                  onChange={(e) => setDefaultQuery(e.target.checked)}
                />
                Set new default query
              </label>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
};

export default OverviewParameterSelector;
