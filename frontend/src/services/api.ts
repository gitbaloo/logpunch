import axios from "axios";

const API_URL = "http://localhost:7206/api/login"; // Update with your actual backend URL

export const authorizeLogin = async (email: string, password: string) => {
  const response = await axios.post(
    `${API_URL}/authorize`,
    new URLSearchParams({
      email,
      password,
    }),
    {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
      },
    }
  );
  return response.data;
};
