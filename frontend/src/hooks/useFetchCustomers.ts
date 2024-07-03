import { useState, useEffect } from "react";
import { API_BASE_URL } from "../config";

const useFetchCustomers = (consultantId: number) => {
  const [customers, setCustomers] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchCustomers = async () => {
      setIsLoading(true);
      try {
        const response = await fetch(
          `${API_BASE_URL}/customers/${consultantId}`
        );
        if (!response.ok) {
          throw new Error("Failed to fetch customers");
        }
        const data = await response.json();
        setCustomers(data);
      } catch (error) {
        console.error("Error fetching customers:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchCustomers();
  }, [consultantId]);

  return { customers, isLoading };
};

export default useFetchCustomers;
