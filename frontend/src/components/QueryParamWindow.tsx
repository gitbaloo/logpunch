import React, { useState, useEffect, useCallback } from "react";
import { QueryParam } from "../types/queryParams";
import useCalendar from "../hooks/useCalendar";

interface QueryParamWindowProps {
  currentParams: QueryParam;
  onSave: (newParams: QueryParam) => void;
  onClose: () => void;
}

const QueryParamWindow: React.FC<QueryParamWindowProps> = ({
  currentParams,
  onSave,
  onClose,
}) => {
  const [params, setParams] = useState(currentParams);
  const [showCustomDates, setShowCustomDates] = useState(false);
  const [groupByOptions, setGroupByOptions] = useState<string[]>([]);
  const { getIsoWeekNumber } = useCalendar(new Date()); // Initialize useCalendar with a date

  useEffect(() => {
    if (params.timePeriod === "custom") {
      setParams((prevParams) => ({
        ...prevParams,
        timeMode: "custom",
      }));
      setShowCustomDates(true);
    } else {
      setShowCustomDates(false);
    }
  }, [params.timePeriod]);

  const handleSave = () => {
    onSave(params);
    onClose();
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    const checked =
      type === "checkbox" ? (e.target as HTMLInputElement).checked : undefined;
    setParams((prevParams) => ({
      ...prevParams,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const updateGroupByOptions = useCallback(() => {
    const options = ["client"];
    const today = new Date();

    switch (params.timePeriod) {
      case "year":
        options.push("year");
        if (params.timeMode === "last" || params.timeMode === "rolling") {
          options.push("month", "week", "day");
        } else if (params.timeMode === "current") {
          options.push("month", "week", "day");
        }
        break;
      case "month":
        options.push("month");
        if (params.timeMode === "last" || params.timeMode === "rolling") {
          options.push("week", "day");
        } else if (params.timeMode === "current") {
          options.push("week", "day");
        }
        break;
      case "week":
        options.push("week");
        if (params.timeMode === "last" || params.timeMode === "rolling") {
          options.push("day");
        } else if (params.timeMode === "current") {
          options.push("day");
        }
        break;
      case "day":
        options.push("day");
        break;
      case "custom":
        options.push("year", "month", "week", "day");
        break;
      default:
        break;
    }

    setGroupByOptions(options);
  }, [params.timePeriod, params.timeMode]);

  useEffect(() => {
    updateGroupByOptions();
  }, [params.timePeriod, params.timeMode, updateGroupByOptions]);

  return (
    <div className="query-param-window">
      <div>
        <label>
          Sort Ascending:
          <input
            type="checkbox"
            name="sortAsc"
            checked={params.sortAsc}
            onChange={handleChange}
          />
        </label>
      </div>
      <div>
        <label>
          Show Units With No Records:
          <input
            type="checkbox"
            name="showUnitsWithNoRecords"
            checked={params.showUnitsWithNoRecords}
            onChange={handleChange}
          />
        </label>
      </div>
      <div>
        <label>
          Set Default:
          <input
            type="checkbox"
            name="setDefault"
            checked={params.setDefault}
            onChange={handleChange}
          />
        </label>
      </div>
      <div>
        <label>
          Time Period:
          <select
            name="timePeriod"
            value={params.timePeriod}
            onChange={handleChange}
          >
            <option value="year">Year</option>
            <option value="month">Month</option>
            <option value="week">Week</option>
            <option value="day">Day</option>
            <option value="custom">Custom</option>
          </select>
        </label>
      </div>
      {!showCustomDates && (
        <div>
          <label>
            Time Mode:
            <select
              name="timeMode"
              value={params.timeMode}
              onChange={handleChange}
            >
              <option value="last">Last</option>
              <option value="current">Current</option>
              <option value="rolling">Rolling</option>
            </select>
          </label>
        </div>
      )}
      {showCustomDates && (
        <>
          <div>
            <label>
              Start Date:
              <input
                type="date"
                name="startDate"
                value={params.startDate}
                onChange={handleChange}
              />
            </label>
          </div>
          <div>
            <label>
              End Date:
              <input
                type="date"
                name="endDate"
                value={params.endDate}
                onChange={handleChange}
              />
            </label>
          </div>
        </>
      )}
      <div>
        <label>
          Group By:
          <select name="groupBy" value={params.groupBy} onChange={handleChange}>
            {groupByOptions.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </label>
      </div>
      <div>
        <label>
          Then By:
          <select name="thenBy" value={params.thenBy} onChange={handleChange}>
            <option value="year">Year</option>
            <option value="month">Month</option>
            <option value="week">Week</option>
            <option value="day">Day</option>
            <option value="client">Client</option>
            <option value="none">None</option>
          </select>
        </label>
      </div>
      <button onClick={handleSave}>Save</button>
      <button onClick={onClose}>Cancel</button>
    </div>
  );
};

export default QueryParamWindow;
