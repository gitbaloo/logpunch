import axios from "axios";

const API_URL = "http://localhost:7206/api";

// Identity management services
export const authorizeLogin = async (email: string, password: string) => {
  const response = await axios.post(
    `${API_URL}/login/authorize`,
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

export const authenticateUser = async (token: string) => {
  const response = await axios.get(`${API_URL}/login/authenticate`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });
  return response.data;
};

// Overview services
export const fetchDefaultParams = async () => {
  const token = localStorage.getItem("token");
  const response = await axios.get(
    `${API_URL}/overview/work/get-default-overview`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  if (response.status !== 200)
    throw new Error("Failed to fetch default parameters");
  return response.data;
};

export const fetchOverviewData = async (params: Record<string, string>) => {
  const token = localStorage.getItem("token");
  const queryParams = new URLSearchParams(params).toString();
  const response = await axios.get(
    `${API_URL}/overview/work/get-overview?${queryParams}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  if (response.status !== 200) throw new Error("Failed to fetch overview data");
  return response.data;
};
