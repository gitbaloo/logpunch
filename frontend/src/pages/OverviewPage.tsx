import React, { useEffect, useState } from "react";
import TopBar from "../components/TopBar";
import ParameterBox from "../components/ParameterBox";
import {
  WorkQueryParam,
  TransportQueryParam,
  AbsenceQueryParam,
} from "../types/queryParams";
import { OverviewResponse } from "../types/overviewResponse";
import { fetchDefaultParams, fetchOverviewData } from "../services/apiService";

const timePeriodOptions = ["Year", "Month", "Week", "Day", "Custom"];
const timeModeOptions = ["Last", "Current", "Rolling", "Custom"];
const groupByOptions = ["Year", "Month", "Week", "Day", "Client"];
const thenByOptions = ["Year", "Month", "Week", "Day", "Client", "None"];

const overviewActions: { id: string; label: string }[] = [
  { id: "work", label: "Work" },
  { id: "transport", label: "Transport" },
  { id: "vacation", label: "Vacation" },
  { id: "sickness", label: "Sickness" },
  { id: "leave", label: "Leave" },
];

const OverviewPage: React.FC = () => {
  const [workParams, setWorkParams] = useState<WorkQueryParam | null>(null);
  const [transportParams, setTransportParams] =
    useState<TransportQueryParam | null>(null);
  const [absenceParams, setAbsenceParams] = useState<AbsenceQueryParam | null>(
    null
  );
  const [data, setData] = useState<OverviewResponse | null>(null);
  const [activeParamBox, setActiveParamBox] = useState<string | null>(null);
  const [selectedAction, setSelectedAction] = useState<string>("");
  const [shouldFetchData, setShouldFetchData] = useState<boolean>(false);

  useEffect(() => {
    const loadDefaultParams = async () => {
      try {
        const defaultParams = await fetchDefaultParams();
        setWorkParams(defaultParams as WorkQueryParam);
        setTransportParams(defaultParams as TransportQueryParam);
        setAbsenceParams(defaultParams as AbsenceQueryParam);
      } catch (error) {
        console.error("Failed to load default params", error);
      }
    };
    loadDefaultParams();
  }, []);

  useEffect(() => {
    if (!shouldFetchData) return;

    const getParams = () => {
      switch (selectedAction) {
        case "work":
          return workParams || ({} as WorkQueryParam);
        case "transport":
          return transportParams || ({} as TransportQueryParam);
        case "vacation":
        case "sickness":
        case "leave":
          return absenceParams || ({} as AbsenceQueryParam);
        default:
          return null;
      }
    };

    const params = getParams();

    if (params) {
      const loadData = async () => {
        try {
          const action =
            selectedAction === "vacation" ||
            selectedAction === "sickness" ||
            selectedAction === "leave"
              ? "absence"
              : selectedAction;

          const stringParams: Record<string, string> = {
            sortAsc: params.sortAsc?.toString() || "false",
            showUnitsWithNoRecords:
              params.showUnitsWithNoRecords?.toString() || "false",
            ...(selectedAction === "work" && {
              setDefault:
                (params as WorkQueryParam).setDefault?.toString() || "false",
            }),
            startDate: params.startDate || "",
            endDate: params.endDate || "",
            timePeriod: params.timePeriod?.toLowerCase() || "",
            timeMode: params.timeMode?.toLowerCase() || "",
            groupBy: params.groupBy?.toLowerCase() || "",
            thenBy: params.thenBy?.toLowerCase() || "", // Ensure thenBy is included even if empty
            ...(selectedAction === "vacation" && { absenceType: "Vacation" }),
            ...(selectedAction === "sickness" && { absenceType: "Sickness" }),
            ...(selectedAction === "leave" && { absenceType: "Leave" }),
          };

          console.log("Final stringParams before API call:", stringParams);

          const filteredParams = Object.fromEntries(
            Object.entries(stringParams).filter(
              ([key, value]) => key === "thenBy" || value !== ""
            )
          );

          const overviewData = await fetchOverviewData(action, filteredParams);
          setData(overviewData);
        } catch (error) {
          console.error("Failed to load overview data", error);
        }
      };
      loadData();
    }
    setShouldFetchData(false); // Reset the flag after fetching data
  }, [
    shouldFetchData,
    selectedAction,
    workParams,
    transportParams,
    absenceParams,
  ]);

  const handleSaveParams = () => {
    setShouldFetchData(true);
    setActiveParamBox(null);
  };

  const handleCancel = () => {
    setActiveParamBox(null);
  };

  const handleButtonClick = (type: string) => {
    setActiveParamBox(type === activeParamBox ? null : type);
    setSelectedAction(type);

    if (type === "vacation" || type === "sickness" || type === "leave") {
      setAbsenceParams((prevParams) => ({
        ...(prevParams as AbsenceQueryParam),
        absenceType: type.charAt(0).toUpperCase() + type.slice(1),
        sortAsc: prevParams?.sortAsc ?? false,
        showUnitsWithNoRecords: prevParams?.showUnitsWithNoRecords ?? false,
      }));
    } else if (type === "work") {
      setWorkParams((prevParams) => ({
        ...(prevParams as WorkQueryParam),
        setDefault: prevParams?.setDefault ?? false,
        sortAsc: prevParams?.sortAsc ?? false,
        showUnitsWithNoRecords: prevParams?.showUnitsWithNoRecords ?? false,
      }));
    } else if (type === "transport") {
      setTransportParams((prevParams) => ({
        ...(prevParams as TransportQueryParam),
        sortAsc: prevParams?.sortAsc ?? false,
        showUnitsWithNoRecords: prevParams?.showUnitsWithNoRecords ?? false,
      }));
    }
  };

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;

    console.log(`Input change detected: ${name} = ${value}`);

    if (selectedAction === "work") {
      setWorkParams((prevParams) => ({
        ...(prevParams as WorkQueryParam),
        [name]: type === "checkbox" ? checked : value,
      }));
      console.log("Updated workParams:", { ...workParams });
    } else if (selectedAction === "transport") {
      setTransportParams((prevParams) => ({
        ...(prevParams as TransportQueryParam),
        [name]: type === "checkbox" ? checked : value,
      }));
      console.log("Updated transportParams:", { ...transportParams });
    } else if (
      selectedAction === "vacation" ||
      selectedAction === "sickness" ||
      selectedAction === "leave"
    ) {
      setAbsenceParams((prevParams) => ({
        ...(prevParams as AbsenceQueryParam),
        [name]: type === "checkbox" ? checked : value,
      }));
      console.log("Updated absenceParams:", { ...absenceParams });
    }
  };

  const filteredGroupByOptions = groupByOptions;

  const filteredThenByOptions = thenByOptions.filter((option) => {
    const groupByIndex = groupByOptions.indexOf(
      selectedAction === "work"
        ? workParams?.groupBy || ""
        : selectedAction === "transport"
        ? transportParams?.groupBy || ""
        : absenceParams?.groupBy || ""
    );
    const thenByIndex = thenByOptions.indexOf(option);

    return thenByIndex > groupByIndex;
  });

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">
      <TopBar />
      <div className="flex justify-center space-x-4 bg-gray-100 p-4 rounded-md shadow-md mt-4">
        {overviewActions.map((action) => (
          <button
            key={action.id}
            onClick={() => handleButtonClick(action.id)}
            className={`px-4 py-2 rounded-md ${
              selectedAction === action.id
                ? "bg-blue-600 text-white"
                : "bg-gray-300 text-gray-800"
            } hover:bg-blue-500 hover:text-white`}
          >
            {action.label}
          </button>
        ))}
      </div>
      <div className="overview-content flex-grow">
        {data ? (
          <div>
            <h1>{data.type}</h1>
            <pre>{JSON.stringify(data, null, 2)}</pre>
          </div>
        ) : (
          <p className="flex justify-center items-center h-96 text-lg">
            Loading...
          </p>
        )}
      </div>
      {activeParamBox && (
        <div className="fixed inset-0 flex items-center justify-center z-50">
          <div className="absolute inset-0 bg-black opacity-50 z-40"></div>
          <div className="relative z-50">
            <ParameterBox
              title={
                selectedAction.charAt(0).toUpperCase() + selectedAction.slice(1)
              }
            >
              <form className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Start Date
                  </label>
                  <input
                    type="date"
                    name="startDate"
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm"
                    value={
                      selectedAction === "work"
                        ? workParams?.startDate || ""
                        : selectedAction === "transport"
                        ? transportParams?.startDate || ""
                        : absenceParams?.startDate || ""
                    }
                    onChange={handleInputChange}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    End Date
                  </label>
                  <input
                    type="date"
                    name="endDate"
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm"
                    value={
                      selectedAction === "work"
                        ? workParams?.endDate || ""
                        : selectedAction === "transport"
                        ? transportParams?.endDate || ""
                        : absenceParams?.endDate || ""
                    }
                    onChange={handleInputChange}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Time Period
                  </label>
                  <select
                    name="timePeriod"
                    value={
                      selectedAction === "work"
                        ? workParams?.timePeriod || ""
                        : selectedAction === "transport"
                        ? transportParams?.timePeriod || ""
                        : absenceParams?.timePeriod || ""
                    }
                    onChange={handleInputChange}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm"
                  >
                    {timePeriodOptions.map((option) => (
                      <option key={option} value={option}>
                        {option}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Time Mode
                  </label>
                  <select
                    name="timeMode"
                    value={
                      selectedAction === "work"
                        ? workParams?.timeMode || ""
                        : selectedAction === "transport"
                        ? transportParams?.timeMode || ""
                        : absenceParams?.timeMode || ""
                    }
                    onChange={handleInputChange}
                    disabled={
                      (selectedAction === "work"
                        ? workParams?.timePeriod || ""
                        : selectedAction === "transport"
                        ? transportParams?.timePeriod || ""
                        : absenceParams?.timePeriod || "") === "Custom"
                    }
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm"
                  >
                    {timeModeOptions.map((option) => (
                      <option key={option} value={option}>
                        {option}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Group By
                  </label>
                  <select
                    name="groupBy"
                    value={
                      selectedAction === "work"
                        ? workParams?.groupBy || ""
                        : selectedAction === "transport"
                        ? transportParams?.groupBy || ""
                        : absenceParams?.groupBy || ""
                    }
                    onChange={handleInputChange}
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm"
                  >
                    {filteredGroupByOptions.map((option) => (
                      <option key={option} value={option}>
                        {option}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Then By
                  </label>
                  <select
                    name="thenBy" // This name must be correct
                    value={
                      selectedAction === "work"
                        ? workParams?.thenBy || ""
                        : selectedAction === "transport"
                        ? transportParams?.thenBy || ""
                        : absenceParams?.thenBy || ""
                    }
                    onChange={handleInputChange} // This function should be triggered
                    className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm"
                  >
                    {filteredThenByOptions.map((option) => (
                      <option key={option} value={option}>
                        {option}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    name="sortAsc"
                    className="mr-2"
                    checked={
                      selectedAction === "work"
                        ? workParams?.sortAsc || false
                        : selectedAction === "transport"
                        ? transportParams?.sortAsc || false
                        : absenceParams?.sortAsc || false
                    }
                    onChange={handleInputChange}
                  />
                  <label className="text-sm font-medium text-gray-700">
                    Sort Ascending
                  </label>
                </div>
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    name="showUnitsWithNoRecords"
                    className="mr-2"
                    checked={
                      selectedAction === "work"
                        ? workParams?.showUnitsWithNoRecords || false
                        : selectedAction === "transport"
                        ? transportParams?.showUnitsWithNoRecords || false
                        : absenceParams?.showUnitsWithNoRecords || false
                    }
                    onChange={handleInputChange}
                  />
                  <label className="text-sm font-medium text-gray-700">
                    Show Units with No Records
                  </label>
                </div>
                {selectedAction === "work" &&
                  workParams &&
                  "setDefault" in workParams && (
                    <div className="flex items-center">
                      <input
                        type="checkbox"
                        name="setDefault"
                        className="mr-2"
                        checked={workParams.setDefault}
                        onChange={handleInputChange}
                      />
                      <label className="text-sm font-medium text-gray-700">
                        Set as Default
                      </label>
                    </div>
                  )}
                <div className="flex justify-end space-x-2">
                  <button
                    type="button"
                    onClick={handleSaveParams}
                    className="mt-4 px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-500"
                  >
                    OK
                  </button>
                  <button
                    type="button"
                    onClick={handleCancel}
                    className="mt-4 px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-500"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </ParameterBox>
          </div>
        </div>
      )}
    </div>
  );
};

export default OverviewPage;
