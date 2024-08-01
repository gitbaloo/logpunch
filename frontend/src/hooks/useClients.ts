import { useEffect, useState } from "react";
import { fetchClients } from "../services/apiService";
import { Client } from "../types/GenericTypes";

export const useClients = () => {
  const [clients, setClients] = useState<Client[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const data = await fetchClients();
        setClients([{ id: null, name: "< none >" }, ...data]);
        setLoading(false);
      } catch (err) {
        setError("Failed to fetch clients");
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  return { clients, loading, error };
};
