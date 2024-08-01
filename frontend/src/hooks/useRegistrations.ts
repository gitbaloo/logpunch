import { useEffect, useState } from "react";
import { fetchUnsettledRegistrations } from "../services/apiService";
import { Registration } from "../types/GenericTypes";

export const useRegistrations = () => {
  const [workRegistrations, setWorkRegistrations] = useState<Registration[]>(
    []
  );
  const [transportRegistrations, setTransportRegistrations] = useState<
    Registration[]
  >([]);
  const [absenceRegistrations, setAbsenceRegistrations] = useState<
    Registration[]
  >([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchRegistrations = async () => {
    setLoading(true);
    try {
      // Fetch registrations for each category
      const workData = await fetchUnsettledRegistrations("Work");
      const transportData = await fetchUnsettledRegistrations("Transportation");
      const absenceData = await fetchUnsettledRegistrations("Absence");

      // Filter and categorize
      const filterByStatus = (
        registrations: Registration[],
        statuses: string[]
      ) =>
        registrations.filter((reg: Registration) =>
          statuses.includes(reg.status)
        );

      setWorkRegistrations(
        filterByStatus(workData || [], ["Open", "Rejected"])
      );
      setTransportRegistrations(
        filterByStatus(transportData || [], ["Open", "Rejected"])
      );
      setAbsenceRegistrations(
        filterByStatus(absenceData || [], ["Open", "Rejected"])
      );

      setLoading(false);
    } catch (err: unknown) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError("An unknown error occurred.");
      }
      setWorkRegistrations([]);
      setTransportRegistrations([]);
      setAbsenceRegistrations([]);
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRegistrations();
  }, []);

  return {
    workRegistrations,
    transportRegistrations,
    absenceRegistrations,
    loading,
    error,
    refetch: fetchRegistrations, // Export the function to call it externally
  };
};
