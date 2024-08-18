import { useEffect, useState } from "react";
import TopBar from "../components/TopBar";
import { useAuth } from "../hooks/useAuth";
import { useClients } from "../hooks/useClients";
import { useRegistrations } from "../hooks/useRegistrations";
import SearchableDropdown from "../components/SearchableDropdown";
import ParameterBox from "../components/ParameterBox";
import {
  createWorkRegistration,
  startWorkRegistration,
  endWorkRegistration,
  createTransportationRegistration,
  startTransportationRegistration,
  endTransportationRegistration,
  employeeConfirmationRegistration,
  employeeCorrectionRegistration,
  fetchOngoingRegistration,
} from "../services/apiService";
import {
  CreateRegistrationRequest,
  StartRegistrationRequest,
  EndRegistrationRequest,
  EmployeeConfirmationRegistrationRequest,
  EmployeeCorrectionRegistrationRequest,
} from "../types/registrationRequests";
import { Registration, Client } from "../types/genericTypes";
import { DateTime } from "luxon";

type FormData = Partial<
  CreateRegistrationRequest &
    StartRegistrationRequest &
    EndRegistrationRequest &
    EmployeeConfirmationRegistrationRequest &
    EmployeeCorrectionRegistrationRequest
> & {
  ClientId?: string | null;
};

type Action = {
  id: string;
  label: string;
};

const actions: Action[] = [
  { id: "start", label: "Start" },
  { id: "end", label: "End" },
  { id: "create", label: "Create" },
  { id: "confirm", label: "Confirm" },
  { id: "correct", label: "Correct" },
];

const RegistrationPage: React.FC = () => {
  const { user } = useAuth();
  const {
    workRegistrations,
    transportRegistrations,
    absenceRegistrations,
    refetch,
  } = useRegistrations();
  const { clients } = useClients();
  const [formData, setFormData] = useState<FormData>({});
  const [selectedAction, setSelectedAction] = useState<string>("");
  const [subAction, setSubAction] = useState<string>("work");
  const [hasOngoingRegistration, setHasOngoingRegistration] =
    useState<boolean>(false);
  const [ongoingRegistration, setOngoingRegistration] =
    useState<Registration | null>(null);
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [selectedRegistration, setSelectedRegistration] =
    useState<Registration | null>(null);
  const [selectedCorrectionRegistration, setSelectedCorrectionRegistration] =
    useState<Registration | null>(null);
  const [date, setDate] = useState<string>("");
  const [startTime, setStartTime] = useState<string>("");
  const [endTime, setEndTime] = useState<string>("");

  useEffect(() => {
    const checkOngoingRegistration = async () => {
      try {
        const response = await fetchOngoingRegistration(user?.id);
        setHasOngoingRegistration(response ? true : false);
        setOngoingRegistration(response || null);
      } catch (error) {
        setHasOngoingRegistration(false);
        setOngoingRegistration(null);
      }
    };

    checkOngoingRegistration();
  }, [user?.id]);

  useEffect(() => {
    setSelectedClient(null);
    setSelectedRegistration(null);
    setSelectedCorrectionRegistration(null);
  }, [selectedAction]);

  useEffect(() => {
    setSelectedRegistration(null);
    setSelectedCorrectionRegistration(null);
  }, [subAction]);

  useEffect(() => {
    setFormData((prevFormData) => ({
      ...prevFormData,
      ClientId:
        selectedClient && selectedClient.id ? selectedClient.id : undefined,
    }));
  }, [selectedClient]);

  const handleChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement
    >
  ) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setDate(e.target.value);
  };

  const handleStartTimeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newStartTime = e.target.value;
    setStartTime(newStartTime);

    const startDateTime = DateTime.fromISO(`${date}T${newStartTime}`);
    const endDateTime = DateTime.fromISO(`${date}T${endTime}`);

    if (endDateTime <= startDateTime) {
      const adjustedEndDateTime = startDateTime.plus({ minutes: 1 });
      setEndTime(adjustedEndDateTime.toFormat("HH:mm"));
    }
  };

  const handleEndTimeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newEndTime = e.target.value;
    setEndTime(newEndTime);

    const startDateTime = DateTime.fromISO(`${date}T${startTime}`);
    const endDateTime = DateTime.fromISO(`${date}T${newEndTime}`);

    if (endDateTime <= startDateTime) {
      const adjustedEndDateTime = startDateTime.plus({ minutes: 1 });
      setEndTime(adjustedEndDateTime.toFormat("HH:mm"));
    }
  };

  // Create
  const handleCreateSubmit = async () => {
    try {
      const startDateTime = DateTime.fromISO(`${date}T${startTime}`).toUTC();
      const endDateTime = DateTime.fromISO(`${date}T${endTime}`).toUTC();

      const createData: CreateRegistrationRequest = {
        employeeId: formData.employeeId,
        clientId: formData.clientId || "",
        start: startDateTime,
        end: endDateTime,
        firstComment: formData.firstComment || "",
        secondComment: formData.secondComment || "",
      };

      if (subAction === "work") {
        await createWorkRegistration(createData);
      } else if (subAction === "transport") {
        await createTransportationRegistration(createData);
      }

      alert("Request was successful");
      resetForm();
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Error:", error.message);
        alert("There was an error processing your request.");
      } else {
        console.error("Unexpected error:", error);
        alert("An unexpected error occurred.");
      }
    }
  };

  // Start
  const handleStartSubmit = async () => {
    try {
      const startData: StartRegistrationRequest = {
        employeeId: formData.employeeId,
        clientId: formData.clientId || "",
        firstComment: formData.firstComment || "",
      };

      if (subAction === "work") {
        await startWorkRegistration(startData);
      } else if (subAction === "transport") {
        await startTransportationRegistration(startData);
      }

      await updateRegistrationState();
      alert("Start registration successful");
      resetForm();
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Error:", error.message);
        alert("There was an error processing your request.");
      } else {
        console.error("Unexpected error:", error);
        alert("An unexpected error occurred.");
      }
    }
  };

  // End
  const handleEndSubmit = async () => {
    try {
      if (ongoingRegistration && ongoingRegistration.id) {
        const endRegistrationData: EndRegistrationRequest = {
          employeeId: ongoingRegistration.employeeId,
          registrationId: ongoingRegistration.id,
          secondComment: formData.secondComment || "",
        };

        if (subAction === "work") {
          await endWorkRegistration(endRegistrationData);
        } else if (subAction === "transport") {
          await endTransportationRegistration(endRegistrationData);
        }

        await updateRegistrationState();
        alert("End registration successful");
        resetForm();
      } else {
        alert("No ongoing registration found.");
      }
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Error:", error.message);
        alert("There was an error processing your request.");
      } else {
        console.error("Unexpected error:", error);
        alert("An unexpected error occurred.");
      }
    }
  };

  // Confirm
  const handleConfirmSubmit = async () => {
    try {
      if (selectedRegistration) {
        const confirmData: EmployeeConfirmationRegistrationRequest = {
          registrationId: selectedRegistration.id,
        };

        await employeeConfirmationRegistration(confirmData);
        refetch();
        alert("Confirmation successful");
        resetForm();
      } else {
        alert("No registration selected for confirmation.");
      }
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Error:", error.message);
        alert("There was an error processing your request.");
      } else {
        console.error("Unexpected error:", error);
        alert("An unexpected error occurred.");
      }
    }
  };

  // Correct
  const handleCorrectSubmit = async () => {
    try {
      if (selectedCorrectionRegistration) {
        const startDateTime = DateTime.fromISO(`${date}T${startTime}`).toUTC();
        const endDateTime = DateTime.fromISO(`${date}T${endTime}`).toUTC();

        const correctData: EmployeeCorrectionRegistrationRequest = {
          clientId: formData.ClientId || "",
          type: selectedCorrectionRegistration.type || "Unknown",
          start: startDateTime,
          end: endDateTime,
          firstComment: formData.firstComment || "",
          secondComment: formData.secondComment || "",
          correctionOfId: selectedCorrectionRegistration.id || "",
        };

        if (subAction === "work") {
          await employeeCorrectionRegistration(correctData);
        } else if (subAction === "transport") {
          await employeeCorrectionRegistration(correctData);
        }

        refetch();
        alert("Correction successful");
        resetForm();
      } else {
        alert("No registration selected for correction.");
      }
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Error:", error.message);
        alert("There was an error processing your request.");
      } else {
        console.error("Unexpected error:", error);
        alert("An unexpected error occurred.");
      }
    }
  };

  const updateRegistrationState = async () => {
    try {
      const response = await fetchOngoingRegistration(user?.id);
      setHasOngoingRegistration(response ? true : false);
      setOngoingRegistration(response || null);
    } catch (error) {
      setHasOngoingRegistration(false);
      setOngoingRegistration(null);
    }
  };

  const resetForm = () => {
    setSelectedAction("");
    setSubAction("work");
    setFormData({});
    setSelectedClient(null);
    setSelectedRegistration(null);
    setSelectedCorrectionRegistration(null);
    setDate("");
    setStartTime("");
    setEndTime("");
  };

  const renderFormFields = () => {
    if (selectedAction === "create") {
      return (
        <>
          {user?.role === "Admin" && (
            <input
              type="text"
              name="EmployeeId"
              placeholder="Employee ID"
              onChange={handleChange}
              className="input w-full mb-2"
            />
          )}
          <SearchableDropdown<Client>
            items={clients}
            selectedItem={selectedClient}
            setSelectedItem={setSelectedClient}
            placeholder="Select Client"
            renderItem={(client) => client.name}
          />
          <input
            type="date"
            name="Date"
            value={date}
            onChange={handleDateChange}
            className="input w-full mb-2"
          />
          <input
            type="time"
            name="StartTime"
            value={startTime}
            onChange={handleStartTimeChange}
            className="input w-full mb-2"
          />
          <input
            type="time"
            name="EndTime"
            value={endTime}
            onChange={handleEndTimeChange}
            className="input w-full mb-2"
          />
          <textarea
            name="FirstComment"
            placeholder="First Comment"
            onChange={handleChange}
            className="textarea w-full mb-2"
          ></textarea>
          <textarea
            name="SecondComment"
            placeholder="Second Comment"
            onChange={handleChange}
            className="textarea w-full mb-2"
          ></textarea>
        </>
      );
    } else if (selectedAction === "start") {
      return (
        <>
          {user?.role === "Admin" && (
            <input
              type="text"
              name="EmployeeId"
              placeholder="Employee ID"
              onChange={handleChange}
              className="input w-full mb-2"
            />
          )}
          <SearchableDropdown<Client>
            items={clients}
            selectedItem={selectedClient}
            setSelectedItem={setSelectedClient}
            placeholder="Select Client"
            renderItem={(client) => client.name}
          />
          <textarea
            name="FirstComment"
            placeholder="First Comment"
            onChange={handleChange}
            className="textarea w-full mb-2"
          ></textarea>
        </>
      );
    } else if (selectedAction === "end") {
      if (hasOngoingRegistration) {
        return (
          <>
            {user?.role === "Admin" && (
              <input
                type="text"
                name="EmployeeId"
                value={ongoingRegistration?.employeeId || ""}
                readOnly
                className="input w-full mb-2"
              />
            )}
            <input
              type="text"
              name="ClientId"
              value={ongoingRegistration?.clientId || ""}
              readOnly
              className="input w-full mb-2"
            />
            <input
              type="datetime-local"
              name="Start"
              value={
                ongoingRegistration?.start
                  ? new Date(
                      new Date(ongoingRegistration.start).getTime() -
                        new Date(
                          ongoingRegistration.start
                        ).getTimezoneOffset() *
                          60000
                    )
                      .toISOString()
                      .slice(0, 16)
                  : ""
              }
              readOnly
              className="input w-full mb-2"
            />

            <input
              type="text"
              name="RegistrationId"
              value={ongoingRegistration?.id || ""}
              readOnly
              className="input w-full mb-2"
            />
            <textarea
              name="FirstComment"
              value={
                typeof ongoingRegistration?.firstComment === "string"
                  ? ongoingRegistration.firstComment
                  : ""
              }
              readOnly
              className="textarea w-full mb-2"
            ></textarea>
            <textarea
              name="SecondComment"
              placeholder="Second Comment"
              onChange={handleChange}
              className="textarea w-full mb-2"
            ></textarea>
          </>
        );
      }
    } else if (selectedAction === "confirm") {
      return (
        <>
          <SearchableDropdown<Registration>
            items={
              subAction === "work"
                ? workRegistrations
                : subAction === "transport"
                ? transportRegistrations
                : absenceRegistrations
            }
            selectedItem={selectedRegistration}
            setSelectedItem={setSelectedRegistration}
            placeholder="Select Registration"
            renderItem={(registration) =>
              `${DateTime.fromISO(registration.start).toFormat(
                "yyyy-MM-dd"
              )} - ${registration.amount ?? "N/A"}`
            }
          />

          {selectedRegistration && (
            <div className="mt-4 p-4 bg-gray-100 rounded-md shadow-md">
              <h3 className="text-lg font-bold mb-2">Registration Details</h3>
              <p>
                <strong>Start:</strong>{" "}
                {DateTime.fromISO(selectedRegistration.start).toFormat(
                  "yyyy-MM-dd HH:mm"
                )}
              </p>
              <p>
                <strong>End:</strong>{" "}
                {selectedRegistration.end
                  ? DateTime.fromISO(selectedRegistration.end).toFormat(
                      "yyyy-MM-dd HH:mm"
                    )
                  : "N/A"}
              </p>
              <p>
                <strong>Amount:</strong> {selectedRegistration.amount ?? "N/A"}
              </p>
              <p>
                <strong>First Comment:</strong>{" "}
                {typeof selectedRegistration.firstComment === "string"
                  ? selectedRegistration.firstComment
                  : "N/A"}
              </p>
              <p>
                <strong>Second Comment:</strong>{" "}
                {typeof selectedRegistration.secondComment === "string"
                  ? selectedRegistration.secondComment
                  : "N/A"}
              </p>
            </div>
          )}
        </>
      );
    } else if (selectedAction === "correct") {
      return (
        <>
          <SearchableDropdown<Client>
            items={clients}
            selectedItem={selectedClient}
            setSelectedItem={setSelectedClient}
            placeholder="Select Client"
            renderItem={(client) => client.name}
          />
          <input
            type="date"
            name="Date"
            value={date}
            onChange={handleDateChange}
            className="input w-full mb-2"
          />
          <input
            type="time"
            name="StartTime"
            value={startTime}
            onChange={handleStartTimeChange}
            className="input w-full mb-2"
          />
          <input
            type="time"
            name="EndTime"
            value={endTime}
            onChange={handleEndTimeChange}
            className="input w-full mb-2"
          />
          <textarea
            name="FirstComment"
            placeholder="First Comment"
            onChange={handleChange}
            className="textarea w-full mb-2"
          ></textarea>
          <textarea
            name="SecondComment"
            placeholder="Second Comment"
            onChange={handleChange}
            className="textarea w-full mb-2"
          ></textarea>
          Correction of:
          <SearchableDropdown<Registration>
            items={
              subAction === "work"
                ? workRegistrations.filter((reg) =>
                    ["Open", "Rejected"].includes(reg.status)
                  )
                : transportRegistrations.filter((reg) =>
                    ["Open", "Rejected"].includes(reg.status)
                  )
            }
            selectedItem={selectedCorrectionRegistration}
            setSelectedItem={setSelectedCorrectionRegistration}
            placeholder="Select Registration to Correct"
            renderItem={(registration) =>
              `${DateTime.fromISO(registration.start).toFormat(
                "yyyy-MM-dd"
              )} - ${registration.amount ?? "N/A"}`
            }
          />
          {selectedCorrectionRegistration && (
            <div className="mt-4 p-4 bg-gray-100 rounded-md shadow-md">
              <h3 className="text-lg font-bold mb-2">Registration Details</h3>
              <p>
                <strong>Start:</strong>{" "}
                {DateTime.fromISO(
                  selectedCorrectionRegistration.start
                ).toFormat("yyyy-MM-dd HH:mm")}
              </p>
              <p>
                <strong>End:</strong>{" "}
                {selectedCorrectionRegistration.end
                  ? DateTime.fromISO(
                      selectedCorrectionRegistration.end
                    ).toFormat("yyyy-MM-dd HH:mm")
                  : "N/A"}
              </p>
              <p>
                <strong>Amount:</strong>{" "}
                {selectedCorrectionRegistration.amount ?? "N/A"}
              </p>
              <p>
                <strong>First Comment:</strong>{" "}
                {typeof selectedCorrectionRegistration.firstComment === "string"
                  ? selectedCorrectionRegistration.firstComment
                  : "N/A"}
              </p>
              <p>
                <strong>Second Comment:</strong>{" "}
                {typeof selectedCorrectionRegistration.secondComment ===
                "string"
                  ? selectedCorrectionRegistration.secondComment
                  : "N/A"}
              </p>
            </div>
          )}
        </>
      );
    }

    return null;
  };

  const availableActions = hasOngoingRegistration
    ? actions.filter((action) => action.id !== "start")
    : actions.filter((action) => action.id !== "end");

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">
      <TopBar />
      <div className="flex justify-center space-x-4 bg-gray-100 p-4 rounded-md shadow-md mt-4">
        {availableActions.map((action: Action) => (
          <button
            key={action.id}
            onClick={() => {
              setSelectedAction(action.id);
              setSubAction("work");
            }}
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
      {selectedAction && (
        <ParameterBox
          title={
            selectedAction.charAt(0).toUpperCase() + selectedAction.slice(1)
          }
        >
          <div className="flex justify-center space-x-4 mb-4">
            {(selectedAction === "create" ||
              selectedAction === "start" ||
              selectedAction === "end" ||
              selectedAction === "confirm" ||
              selectedAction === "correct") && (
              <>
                <button
                  onClick={() => setSubAction("work")}
                  className={`px-4 py-2 rounded-md ${
                    selectedAction === "end"
                      ? subAction === "work" ||
                        ongoingRegistration?.type.toLowerCase() === "work"
                        ? "bg-blue-600 text-white"
                        : "bg-gray-300 text-gray-800 cursor-not-allowed opacity-50"
                      : subAction === "work"
                      ? "bg-blue-600 text-white"
                      : "bg-gray-300 text-gray-800"
                  } hover:bg-blue-500 hover:text-white`}
                  disabled={
                    selectedAction === "end" &&
                    ongoingRegistration?.type.toLowerCase() === "transport"
                  }
                >
                  Work
                </button>
                <button
                  onClick={() => setSubAction("transport")}
                  className={`px-4 py-2 rounded-md ${
                    selectedAction === "end"
                      ? subAction === "transport" ||
                        ongoingRegistration?.type.toLowerCase() === "transport"
                        ? "bg-blue-600 text-white"
                        : "bg-gray-300 text-gray-800 cursor-not-allowed opacity-50"
                      : subAction === "transport"
                      ? "bg-blue-600 text-white"
                      : "bg-gray-300 text-gray-800"
                  } hover:bg-blue-500 hover:text-white`}
                  disabled={
                    selectedAction === "end" &&
                    ongoingRegistration?.type.toLowerCase() === "work"
                  }
                >
                  Transport
                </button>
                {selectedAction === "confirm" && (
                  <button
                    onClick={() => setSubAction("absence")}
                    className={`px-4 py-2 rounded-md ${
                      subAction === "absence"
                        ? "bg-blue-600 text-white"
                        : "bg-gray-300 text-gray-800"
                    } hover:bg-blue-500 hover:text-white`}
                  >
                    Absence
                  </button>
                )}
              </>
            )}
          </div>
          <form className="space-y-4">
            {renderFormFields()}
            {subAction && (
              <button
                type="button"
                onClick={
                  selectedAction === "create"
                    ? handleCreateSubmit
                    : selectedAction === "start"
                    ? handleStartSubmit
                    : selectedAction === "end"
                    ? handleEndSubmit
                    : selectedAction === "confirm"
                    ? handleConfirmSubmit
                    : selectedAction === "correct"
                    ? handleCorrectSubmit
                    : () => {}
                }
                className={`px-4 py-2 rounded-md bg-green-600 text-white hover:bg-green-500`}
              >
                Submit
              </button>
            )}
          </form>
        </ParameterBox>
      )}
    </div>
  );
};

export default RegistrationPage;
