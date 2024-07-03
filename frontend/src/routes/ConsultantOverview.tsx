import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthenticatedConsultant } from "../hooks/useAuthenticatedConsultant";
import SearchBar from "../components/SearchBar";
import TimeEntry from "../components/TimeEntry";
import { format, parseISO, isValid, startOfWeek, endOfYear } from "date-fns";
import { useLocation } from "react-router-dom";
import defaultParams from "../components/QueryParam";
import { QueryParams } from "../components/QueryParam";
import { API_BASE_URL } from "../config";

const ConsultantOverview: React.FC = () => {
  const navigate = useNavigate();
  const loggedInConsultant = useAuthenticatedConsultant();
  const [data, setData] = useState<ApiResponse | null>(null);
  const [calculateTotalHours, setTotalHours] = useState(0);
  const [timespan, setTimespan] = useState("");
  const [timeperiod, setTimeperiod] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const location = useLocation();

  useEffect(() => {
    setIsLoading(true);
    const searchParams = new URLSearchParams(location.search);
    const params: QueryParams = {
      sortAsc:
        searchParams.get("sort_asc") === "true" ? true : defaultParams.sortAsc,
      showDaysNoRecords:
        searchParams.get("show_days_no_records") === "true"
          ? true
          : defaultParams.showDaysNoRecords,
      setDefault:
        searchParams.get("set_default") === "true"
          ? true
          : defaultParams.setDefault,
      startDate:
        searchParams.get("start_date") === "null"
          ? format(new Date(), "yyyy-MM-dd")
          : defaultParams.startDate,
      endDate:
        searchParams.get("end_date") === "null"
          ? null
          : searchParams.get("end_date") ?? defaultParams.endDate,
      timePeriod: searchParams.get("time_period") ?? defaultParams.timePeriod,
      timeMode: searchParams.get("time_mode") ?? defaultParams.timeMode,
      groupBy: searchParams.get("group_by") ?? defaultParams.groupBy,
      thenBy: searchParams.get("then_by") ?? defaultParams.thenBy,
    };

    const urlSearchParams = new URLSearchParams();
    Object.keys(params).forEach((key) => {
      if (params[key] !== undefined) {
        if (params[key] !== null) {
          urlSearchParams.append(key, params[key].toString());
        }
      }
    });

    const requestUrl = `${API_BASE_URL}/consultant-overview/get-overview/?${urlSearchParams.toString()}`;

    var token = localStorage.getItem("token");

    fetch(requestUrl, {
      method: "GET",
      headers: {
        Accept: "application/json",
        Authorization: `Bearer ${token}`,
      },
    })
      .then((response) => response.json())
      .then((responseData: ApiResponse) => {
        console.log(responseData);
        setData(responseData);

        setTotalHours(responseData.timeModePeriodObject.total);
        setTimespan(responseData.timeModePeriodObject.timespan);
        setTimeperiod(responseData.timeModePeriodObject.name);
        setIsLoading(false);
      })
      .catch((error) => {
        console.error("Error fetching data:", error);
        setIsLoading(false);
      });
  }, [location.search]);

  const handleSearch = (query: string) => {};

  const query = () => {
    navigate("/overview/query");
  };

  return (
    <>
      <div>
        <div className="grid grid-cols-2 bg-cyan-800 h-20 align-items-center pl-2 text-white">
          <div className="text-left col-start-1">
            <span className="text-2xl sm:text-4xl">
              {loggedInConsultant?.email}
            </span>
          </div>
          <div className="text-end pr-8"></div>
        </div>

        <div className="pl-2 pr-2 pt-2">
          {isLoading ? (
            <div role="status" className="text-center">
              <svg
                aria-hidden="true"
                className="inline w-14  h-14 text-gray-200 animate-spin dark:text-gray-600 fill-blue-600"
                viewBox="0 0 100 101"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  d="M100 50.5908C100 78.2051 77.6142 100.591 50 100.591C22.3858 100.591 0 78.2051 0 50.5908C0 22.9766 22.3858 0.59082 50 0.59082C77.6142 0.59082 100 22.9766 100 50.5908ZM9.08144 50.5908C9.08144 73.1895 27.4013 91.5094 50 91.5094C72.5987 91.5094 90.9186 73.1895 90.9186 50.5908C90.9186 27.9921 72.5987 9.67226 50 9.67226C27.4013 9.67226 9.08144 27.9921 9.08144 50.5908Z"
                  fill="currentColor"
                />
                <path
                  d="M93.9676 39.0409C96.393 38.4038 97.8624 35.9116 97.0079 33.5539C95.2932 28.8227 92.871 24.3692 89.8167 20.348C85.8452 15.1192 80.8826 10.7238 75.2124 7.41289C69.5422 4.10194 63.2754 1.94025 56.7698 1.05124C51.7666 0.367541 46.6976 0.446843 41.7345 1.27873C39.2613 1.69328 37.813 4.19778 38.4501 6.62326C39.0873 9.04874 41.5694 10.4717 44.0505 10.1071C47.8511 9.54855 51.7191 9.52689 55.5402 10.0491C60.8642 10.7766 65.9928 12.5457 70.6331 15.2552C75.2735 17.9648 79.3347 21.5619 82.5849 25.841C84.9175 28.9121 86.7997 32.2913 88.1811 35.8758C89.083 38.2158 91.5421 39.6781 93.9676 39.0409Z"
                  fill="currentFill"
                />
              </svg>
              <span className="sr-only">Loading...</span>
            </div>
          ) : data ? (
            <>
              <div className="grid grid-cols-3 gap-2">
                <div className="flex col-span-2">
                  <div className="grid-rows-2 ">
                    <div className="text-base font-bold">{timeperiod}</div>
                    <div className="text-sm sm:text-lg">{timespan}</div>
                  </div>
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth="1.5"
                    stroke="currentColor"
                    className="hidden lg:block w-10 h-10 pl-2 sm:pl-3"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 0 1 2.25-2.25h13.5A2.25 2.25 0 0 1 21 7.5v11.25m-18 0A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75m-18 0v-7.5A2.25 2.25 0 0 1 5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v7.5m-9-6h.008v.008H12v-.008ZM12 15h.008v.008H12V15Zm0 2.25h.008v.008H12v-.008ZM9.75 15h.008v.008H9.75V15Zm0 2.25h.008v.008H9.75v-.008ZM7.5 15h.008v.008H7.5V15Zm0 2.25h.008v.008H7.5v-.008Zm6.75-4.5h.008v.008h-.008v-.008Zm0 2.25h.008v.008h-.008V15Zm0 2.25h.008v.008h-.008v-.008Zm2.25-4.5h.008v.008H16.5v-.008Zm0 2.25h.008v.008H16.5V15Z"
                    />
                  </svg>
                </div>
                <div className=" text-end sm:text-center">
                  <div className="grid-rows-2">
                    <button
                      className="bg-white pointer-events-auto hover:bg-gray-200 text-black font-bold py-2 px-4"
                      onClick={query}
                    >
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                        strokeWidth="1.5"
                        stroke="currentColor"
                        className="w-10 h-10"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          d="M10.5 6h9.75M10.5 6a1.5 1.5 0 1 1-3 0m3 0a1.5 1.5 0 1 0-3 0M3.75 6H7.5m3 12h9.75m-9.75 0a1.5 1.5 0 0 1-3 0m3 0a1.5 1.5 0 0 0-3 0m-3.75 0H7.5m9-6h3.75m-3.75 0a1.5 1.5 0 0 1-3 0m3 0a1.5 1.5 0 0 0-3 0m-9.75 0h9.75"
                        />
                      </svg>
                    </button>
                  </div>
                </div>
              </div>
              <div className="pb-2">
                <SearchBar onSearch={handleSearch}></SearchBar>
              </div>
              <div className="grid font-bold grid-cols-2 bg-gray-200 rounded-xl border shadow-black p-1">
                <div className="pl-2">Total hours:</div>
                <div className="pr-14 sm:pr-18 text-end">
                  {calculateTotalHours} Hrs.
                </div>
              </div>
              <div className="p-2">
                {data.timePeriodObject.groupByObjects.length > 0 ? (
                  data.timePeriodObject.groupByObjects.map((groupObject) => (
                    <div
                      key={groupObject.name}
                      className="bg-gray-100 p-2 rounded-lg shadow-md mb-2"
                    >
                      <div className="flex justify-between align-items-center mb-2">
                        <div className="text-base font-bold pr-16 sm:pr-18">
                          {groupObject.name}
                        </div>
                        <div className="font-bold pr-16 sm:pr-18">
                          {groupObject.total} hrs
                        </div>
                      </div>
                      {groupObject.thenByObjects &&
                        groupObject.thenByObjects.length > 0 && (
                          <table className="w-full">
                            <tbody>
                              {groupObject.thenByObjects.map(
                                (thenByObject, index) => (
                                  <tr key={index} className="bg-white border-b">
                                    <td className="p-2">{thenByObject.name}</td>
                                    <td className="p-2 text-end">
                                      {thenByObject.total} hrs
                                    </td>
                                  </tr>
                                )
                              )}
                            </tbody>
                          </table>
                        )}
                    </div>
                  ))
                ) : (
                  <div className="text-center py-4 text-2xl">
                    No data available for the selected period.
                  </div>
                )}
              </div>
            </>
          ) : (
            <div className="text-center py-4 text-2xl">No data available.</div>
          )}
        </div>
      </div>
    </>
  );
};

export default ConsultantOverview;
