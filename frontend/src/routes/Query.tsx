import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "react-datepicker/dist/react-datepicker.css";
import DatePicker from 'react-datepicker';
import { format, startOfDay } from 'date-fns';

const Query = () => {
    const navigate = useNavigate();
    const [selectedPeriod, setSelectedPeriod] = useState("current");
    const [selectedTimePeriod, setSelectedTimePeriod] = useState("week");
    const [dateRange, setDateRange] = useState([null, null]);
    const [startDate, endDate] = dateRange;
    const [selectedGroup, setSelectedGroup] = useState("client");

    const [selectedSubPeriod, setSelectedSubPeriod] = useState("day")
    const [selectedThenByGroup, setSelectedThenByGroup] = useState()

    const [isSortedAsc, setSortByAsc] = useState(false)
    const [includeDaysWithoutRecords, setDaysWithoutRecords] = useState(false)
    const [isDefault, setDefault] = useState(false)

    const goBack = () => {
        navigate("/overview");
    };

    const togglePeriod = (value) => {
        if (selectedTimePeriod === "custom") {
            setSelectedTimePeriod("week")
        }
        setSelectedPeriod(value);
    };

    const toggleGroup = (value) => {
        setSelectedGroup(value);
        setSelectedSubPeriod("none");
    };

    const toggleTimePeriod = (value) => {
        if (value === "custom") {
            setSelectedPeriod(value);
        }
        else if (selectedPeriod === "custom" && value != "custom") {
            setSelectedPeriod("current")
        }
        setSelectedSubPeriod("none");
        setSelectedTimePeriod(value);
    };

    const toggleSubPeriod = (value) => {
        setSelectedSubPeriod(value);
    };

    const toggleThenByGroup = (value) => {
        setSelectedThenByGroup(prevValue => prevValue === value ? "none" : value);
    };

    const sendDataToBackend = () => {
        const today = new Date();


        if (selectedTimePeriod === "custom") {
            if (startDate === null || endDate === null) {

                alert("Please specify both start and end dates.");

                document.getElementById('dateError').textContent = "Please specify both start and end dates.";

                return
            }
        }

        if (selectedThenByGroup === "sub-period") {
            if (selectedSubPeriod === "none") {

                alert("Please specify sub-period");
                return;
            }

        }


        const effectiveStartDate = selectedTimePeriod === 'custom' && startDate ? startDate : new Date();
        const effectiveEndDate = selectedTimePeriod === 'custom' && endDate ? endDate : new Date();

        const formattedStartDate = format(startOfDay(effectiveStartDate), 'yyyy-MM-dd');
        const formattedEndDate = format(startOfDay(effectiveEndDate), 'yyyy-MM-dd');

        const payload = {
            start_date: formattedStartDate === 'custom' ? formattedStartDate : null,
            end_date: selectedTimePeriod === 'custom' ? formattedEndDate : null,
            time_period: selectedTimePeriod,
            time_mode: selectedPeriod,
            group_by: selectedGroup,
            then_by: determineThenBy(),
            sort_asc: isSortedAsc.toString(),
            show_days_no_records: includeDaysWithoutRecords.toString(),
            set_default: isDefault.toString(),
        };

        const queryParams = new URLSearchParams(payload).toString();


        navigate(`/overview?${queryParams}`);

    };

    function determineThenBy() {

        if (selectedThenByGroup === "sub-period") {
            return selectedSubPeriod;
        }

        if (selectedThenByGroup === "consultant" && selectedGroup === "client") {
            return "consultant";
        } else if (selectedThenByGroup === "client" && selectedGroup === "period") {
            return "client";
        } else if (selectedThenByGroup === "registration") {
            return "allregistrations";
        }

        return "none";
    }
    return (

        <div className="text-center">
            <div className="grid grid-cols-2 w-full bg-cyan-800 h-20 align-items-center pl-4 text-white">
                <div className="xl:text-lg text-left sm:text-sm col-start-1">
                    <h1 className="text-4xl font-bold">Filter by: </h1>
                </div>
                <div className="text-end pr-8 ">
                    <button className="bg-white hover:bg-gray-200 text-black font-bold py-2 px-3 border border-gray-400 rounded-full shadow" onClick={goBack}>X</button>
                </div>
            </div>
            <div className="p-2">
                <div className="font-bold">
                    <h2>Period:</h2>
                </div>
                <div className="pt-2">
                    <button
                        type="button"
                        onClick={() => togglePeriod("last")}
                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ${selectedPeriod === "last" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                        disabled={selectedPeriod === "last"}
                    >
                        Last
                    </button>
                    <button
                        type="button"
                        onClick={() => togglePeriod("current")}
                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedPeriod === "current" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                        disabled={selectedPeriod === "current"}
                    >
                        Current
                    </button>
                    <button
                        type="button"
                        onClick={() => togglePeriod("rolling")}
                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedPeriod === "rolling" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                        disabled={selectedPeriod === "rolling"}
                    >
                        Rolling
                    </button>
                </div>
                <div className="pt-6">
                    <h2>Time period:</h2>
                    <button
                        type="button"
                        onClick={() => toggleTimePeriod("day")}
                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ${selectedTimePeriod === "day" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                        disabled={selectedTimePeriod === "day"}
                    >
                        Day
                    </button>
                    <button
                        type="button"
                        onClick={() => toggleTimePeriod("week")}
                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedTimePeriod === "week" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                        disabled={selectedTimePeriod === "week"}
                    >
                        Week
                    </button>
                    <button
                        type="button"
                        onClick={() => toggleTimePeriod("month")}
                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedTimePeriod === "month" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                        disabled={selectedTimePeriod === "month"}
                    >
                        Month
                    </button>

                    <button
                        type="button"
                        onClick={() => toggleTimePeriod("year")}
                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedTimePeriod === "year" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                        disabled={selectedTimePeriod === "year"}
                    >
                        Year
                    </button>
                    <div >

                        <button
                            type="button"
                            onClick={() => toggleTimePeriod("custom")}
                            className={`inline-block pt rounded px-6 pb-2 pt-2.5 text-xs uppercase leading-normal transition duration-150 ease-in-out mt-2 mr-2  ${selectedTimePeriod === "custom" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                        >
                            Custom

                        </button>
                        {selectedTimePeriod === "custom" && (
                            <DatePicker
                                selectsRange={true}
                                startDate={startDate}
                                endDate={endDate}
                                onChange={(update) => setDateRange(update)}
                                isClearable={true}
                                dateFormat="dd/MM/yy"
                                placeholderText="Select dates"
                                className="bg-gray-bg-white border border-gray-300 text-gray-900 mr-12 block w-full p-2"
                            />
                        )}
                    </div>

                </div>
                <div>
                    <div className="pt-4 font-bold">
                        <h2>Group By:</h2>
                        <div>
                            <div className="pt-2">
                                <button
                                    type="button"
                                    onClick={() => toggleGroup("client")}
                                    className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ${selectedGroup === "client" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                                    disabled={selectedGroup === "client"}
                                >
                                    Client
                                </button>
                                {(selectedTimePeriod === "week" || selectedTimePeriod === "month") && (
                                    <button
                                        type="button"
                                        onClick={() => toggleGroup("day")}
                                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedGroup === "day" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                                        disabled={selectedGroup === "day"}
                                    >
                                        Day
                                    </button>
                                )}
                                {(selectedTimePeriod === "month" || selectedTimePeriod === "year") && (
                                    <button
                                        type="button"
                                        onClick={() => toggleGroup("week")}
                                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedGroup === "week" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                                        disabled={selectedGroup === "week"}
                                    >
                                        Week
                                    </button>
                                )}
                                {(selectedTimePeriod === "year") && (
                                    <button
                                        type="button"
                                        onClick={() => toggleGroup("month")}
                                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedGroup === "month" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                                        disabled={selectedGroup === "month"}
                                    >
                                        Month
                                    </button>
                                )}
                                {(selectedTimePeriod === "year") && (
                                    <button
                                        type="button"
                                        onClick={() => toggleGroup("year")}
                                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedGroup === "year" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}
                                        disabled={selectedGroup === "year"}
                                    >
                                        Year
                                    </button>
                                )}
                            </div>
                        </div>

                    </div>
                    <div className="pt-4 font-bold">
                        <h2>Then by:</h2>
                        {selectedGroup === "client" && (
                            <div className="pt-2">

                                <button
                                    type="button"
                                    onClick={() => toggleThenByGroup("registration")}
                                    className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedThenByGroup === "registration" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}

                                >
                                    All Registrations
                                </button>
                                <div className="pt-2">
                                    <button
                                        type="button"
                                        onClick={() => toggleThenByGroup("consultant")}
                                        className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ${selectedThenByGroup === "consultant" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}

                                    >
                                        All Consultants
                                    </button>

                                    <div className="pt-2">
                                        {selectedTimePeriod !== "day" && (
                                            <button
                                                type="button"
                                                onClick={() => toggleThenByGroup("sub-period")}
                                                className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ${selectedThenByGroup === "sub-period" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}

                                            >
                                                Sub-Period
                                            </button>
                                        )}

                                    </div>
                                </div>

                            </div>
                        )}

                        {selectedGroup !== "client" && (
                            <div className="pt-2">
                                <button
                                    type="button"
                                    onClick={() => toggleThenByGroup("client")}
                                    className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ${selectedThenByGroup === "client" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}

                                >
                                    All Client
                                </button>
                                <button
                                    type="button"
                                    onClick={() => toggleThenByGroup("registration")}
                                    className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ml-2 ${selectedThenByGroup === "registration" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}

                                >
                                    All Registrations
                                </button>
                                <div className="pt-2">
                                    {selectedTimePeriod !== "day" && (
                                        <button
                                            type="button"
                                            onClick={() => toggleThenByGroup("sub-period")}
                                            className={`inline-block rounded px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal transition duration-150 ease-in-out ${selectedThenByGroup === "sub-period" ? "bg-cyan-600 text-white" : "bg-gray-200 text-black"}`}

                                        >
                                            Sub-Period
                                        </button>
                                    )}

                                </div>

                            </div>
                        )}
                        {selectedThenByGroup === "sub-period" && (
                            <div className="pt-2">
                                {(selectedGroup === "month" || selectedGroup === "week") && (
                                    <button
                                        type="button"
                                        onClick={() => toggleSubPeriod("day")}
                                        className={`inline-block rounded-l  px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal  transition duration-150 ease-in-out ${selectedSubPeriod === "day" ? "bg-cyan-800 text-white" : "bg-gray-300  text-black"}`}
                                        disabled={selectedSubPeriod === "day"}
                                    >
                                        Day
                                    </button>
                                )}
                                {(selectedGroup === "month" || selectedGroup === "year") && (
                                    <button
                                        type="button"
                                        onClick={() => toggleSubPeriod("week")}
                                        className={`inline-block   px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal  transition duration-150 ease-in-out ${selectedSubPeriod === "week" ? "bg-cyan-800 text-white" : "bg-gray-300  text-black"}`}
                                        disabled={selectedSubPeriod === "week"}
                                    >
                                        Week
                                    </button>
                                )}
                                {(selectedGroup === "year") && (
                                    <button
                                        type="button"
                                        onClick={() => toggleSubPeriod("month")}
                                        className={`inline-block runded  px-6 pb-2 pt-2.5 text-xs font-medium uppercase leading-normal  transition duration-150 ease-in-out ${selectedSubPeriod === "month" ? "bg-cyan-800 text-white" : "bg-gray-300  text-black"}`}
                                        disabled={selectedSubPeriod === "month"}
                                    >
                                        Month
                                    </button>
                                )}

                            </div>
                        )}

                    </div>

                </div>
                <div className="pt-6">
                    <label className="text-sm pr-20 mr-2 text-black">Sort Ascending</label>
                    <input
                        type="checkbox"
                        className="border-gray-200 rounded accent-cyan-600 w-4 h-4"
                        checked={isSortedAsc}
                        onChange={(e) => setSortByAsc(e.target.checked)}
                    />
                </div>
                <div>
                    <label className="text-sm pr-3 mr-0.5 text-black">Show days without records</label>
                    <input
                        type="checkbox"
                        className="border-gray-200 rounded accent-cyan-600 w-4 h-4"
                        checked={includeDaysWithoutRecords}
                        onChange={(e) => setDaysWithoutRecords(e.target.checked)}
                    />
                </div>
                <div>
                    <label className="text-sm pr-28 ml-1 text-black">Set Default</label>
                    <input
                        type="checkbox"
                        className="border-gray-200 rounded w-4 h-4 accent-cyan-600"
                        checked={isDefault}
                        onChange={(e) => setDefault(e.target.checked)}
                    />
                </div>



            </div>
            <div className="text-center pt-2">
                <button className="bg-cyan-700 hover:bg-cyan-700 text-white font-bold py-2 px-4 border border-gray-400 rounded shadow" onClick={sendDataToBackend}>Apply</button>
            </div>
        </div>
    );
};

export default Query;
