import { useState, useEffect } from "react";
import axios from "axios";

const useOverview = () => {
  const [overviewType, setOverviewType] = useState<string>();
  const [timePeriod, setTimePeriod] = useState<string>();
  const [timeMode, setTimeMode] = useState<string>();
  const [groupBy, setGroupBy] = useState<string>();
  const [thenBy, setThenBy] = useState<string>();
  const [custom, setCustom] = useState<boolean>(false);
  const [startDate, setStartDate] = useState<string>();
  const [endDate, setEndDate] = useState<string>();

  useEffect(() => {
    const fetchDefaultParams = async () => {
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

    fetchDefaultParams();
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

export default useOverview;
