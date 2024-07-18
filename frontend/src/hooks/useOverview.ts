// src/hooks/useOverview.ts
import { useState, useEffect } from "react";
import axios from "axios";

export const useOverview = () => {
  const [overviewType, setOverviewType] = useState("work");
  const [timePeriod, setTimePeriod] = useState("year");
  const [timeMode, setTimeMode] = useState("last");
  const [groupBy, setGroupBy] = useState("year");
  const [thenBy, setThenBy] = useState("year");
  const [custom, setCustom] = useState(false);
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  useEffect(() => {
    const fetchDefaultParameters = async () => {
      try {
        const response = await axios.get("/api/overview/get-default-overview");
        const defaultQueryString = response.data.queryString;

        const params = new URLSearchParams(defaultQueryString);

        setOverviewType(params.get("overviewType") || "work");
        setTimePeriod(params.get("timePeriod") || "year");
        setTimeMode(params.get("timeMode") || "last");
        setGroupBy(params.get("groupBy") || "year");
        setThenBy(params.get("thenBy") || "year");

        if (params.get("timePeriod") === "custom") {
          setCustom(true);
          setStartDate(params.get("startDate") || "");
          setEndDate(params.get("endDate") || "");
        }
      } catch (error) {
        console.error("Failed to fetch default parameters", error);
      }
    };

    fetchDefaultParameters();
  }, []);

  return {
    overviewType,
    setOverviewType,
    timePeriod,
    setTimePeriod,
    timeMode,
    setTimeMode,
    groupBy,
    setGroupBy,
    thenBy,
    setThenBy,
    custom,
    setCustom,
    startDate,
    setStartDate,
    endDate,
    setEndDate,
  };
};
