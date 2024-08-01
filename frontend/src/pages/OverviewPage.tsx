// src/pages/OverviewPage.tsx

import React, { useEffect, useState } from "react";
import Calendar from "../components/Calendar";
import TopBar from "../components/TopBar";
import QueryParamWindow from "../components/QueryParamWindow";
import { QueryParam } from "../types/QueryParams";
import { OverviewQueryResponse } from "../types/OverviewResponses";
import { fetchDefaultParams, fetchOverviewData } from "../services/apiService";

const OverviewPage: React.FC = () => {
  const [params, setParams] = useState<QueryParam | null>(null);
  const [data, setData] = useState<OverviewQueryResponse | null>(null);
  const [showParamWindow, setShowParamWindow] = useState(false);

  useEffect(() => {
    const loadDefaultParams = async () => {
      try {
        const defaultParams = await fetchDefaultParams();
        setParams(defaultParams);
      } catch (error) {
        console.error("Failed to load default params", error);
      }
    };
    loadDefaultParams();
  }, []);

  useEffect(() => {
    if (params) {
      const loadData = async () => {
        try {
          // Convert params to Record<string, string>
          const stringParams: Record<string, string> = {
            sortAsc: params.sortAsc.toString(),
            showUnitsWithNoRecords: params.showUnitsWithNoRecords.toString(),
            setDefault: params.setDefault.toString(),
            startDate: params.startDate,
            endDate: params.endDate || "",
            timePeriod: params.timePeriod,
            timeMode: params.timeMode,
            groupBy: params.groupBy,
            thenBy: params.thenBy,
          };

          const overviewData = await fetchOverviewData(stringParams);
          setData(overviewData);
        } catch (error) {
          console.error("Failed to load overview data", error);
        }
      };
      loadData();
    }
  }, [params]);

  const handleSaveParams = (newParams: QueryParam) => {
    setParams(newParams);
  };

  return (
    <div>
      <TopBar />
      <Calendar />
      <div className="overview-content">
        {/* Render the overview data */}
        {data ? (
          <div>
            <h1>{data.type}</h1>
            <pre>{JSON.stringify(data, null, 2)}</pre>
          </div>
        ) : (
          <p>Loading...</p>
        )}
        <button onClick={() => setShowParamWindow(true)}>
          Change Parameters
        </button>
      </div>
      {showParamWindow && params && (
        <QueryParamWindow
          currentParams={params}
          onSave={handleSaveParams}
          onClose={() => setShowParamWindow(false)}
        />
      )}
    </div>
  );
};

export default OverviewPage;
