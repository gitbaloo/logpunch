import { useEffect, useState } from "react";
import AuthService from "../services/AuthService";
import Consultant from "../components/Consultant";
import { useNavigate } from "react-router-dom";
import { API_BASE_URL } from "../config";

export function useAuthenticatedConsultant() {
  const navigate = useNavigate();
  const [loggedInConsultant, setLoggedInConsultant] =
    useState<Consultant | null>(null);

    useEffect(() => {
        const token = localStorage.getItem('token');
        const requestUrl = `${API_BASE_URL}/login/authenticate?token=${encodeURIComponent(token)}`;

        fetch(requestUrl, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
          },
        })
        .then(response => {
          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }
          return response.json();
        })
        .then(data => {
          // Assuming the response contains the consultant's ID and email
          if (data && "id" in data && "email" in data) {
            const consultant = new Consultant(data.id, data.email);
            setLoggedInConsultant(consultant);
          } else {
            console.log("Consultant data is not valid!");
          }
        })
        .catch(error => {
          console.error('Error fetching consultant data:', error);
          AuthService.logout(); // Consider logging out the user if token validation fails
          navigate("/login");
        });
    }, []);

  return loggedInConsultant;
}
