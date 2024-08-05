import axios from "axios";
import {
  CreateRegistrationRequest,
  StartRegistrationRequest,
  EndRegistrationRequest,
  EmployeeConfirmationRegistrationRequest,
  EmployeeCorrectionRegistrationRequest,
  CreateAbsenceRegistrationRequest,
  UpdateStatusRequest,
  ChangeTypeRequest,
  AdminCorrectionRegistrationRequest,
} from "../types/registrationRequests";

const API_URL = "http://localhost:7206/api";

type UUID = string;

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

export const fetchUnsettledRegistrations = async (
  type: string,
  employeeId?: UUID
) => {
  const token = localStorage.getItem("token");
  if (type === "Work") {
    try {
      const response = await axios.get(
        `${API_URL}/overview/work/get-unsettled`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
          params: {
            employeeId,
          },
        }
      );
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null;
      } else {
        throw new Error("Failed to fetch unsettled work registrations");
      }
    }
  } else if (type === "Transportation") {
    try {
      const response = await axios.get(
        `${API_URL}/overview/transportation/get-unsettled`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
          params: {
            employeeId,
          },
        }
      );
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null;
      } else {
        throw new Error(
          "Failed to fetch unsettled transportation registrations"
        );
      }
    }
  } else if (type === "Absence") {
    try {
      const response = await axios.get(
        `${API_URL}/overview/absence/get-unsettled`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
          params: {
            employeeId,
          },
        }
      );
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null;
      } else {
        throw new Error("Failed to fetch unsettled absence registrations");
      }
    }
  } else {
    throw new Error("Error in registration type");
  }
};

export const fetchOngoingRegistration = async (employeeId?: UUID) => {
  const token = localStorage.getItem("token");
  try {
    const response = await axios.get(`${API_URL}/overview/get-ongoing`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      params: {
        employeeId,
      },
    });
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return null;
    } else {
      throw new Error("Failed to fetch ongoing data");
    }
  }
};

// Client services
export const fetchClients = async (employeeId?: UUID) => {
  const token = localStorage.getItem("token");
  const response = await axios.get(`${API_URL}/clients/get-all`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
    params: {
      employeeId,
    },
  });
  if (response.status !== 200) throw new Error("Failed to fetch clients");
  return response.data;
};

// Registration services
export const createWorkRegistration = async (
  data: CreateRegistrationRequest
) => {
  try {
    const response = await axios.post(
      `${API_URL}/registration/work/create`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error creating work registration:", error);
    throw error;
  }
};

export const startWorkRegistration = async (data: StartRegistrationRequest) => {
  try {
    const response = await axios.post(
      `${API_URL}/registration/work/start`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error starting work registration:", error);
    throw error;
  }
};

export const endWorkRegistration = async (data: EndRegistrationRequest) => {
  try {
    const response = await axios.patch(
      `${API_URL}/registration/work/end`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error ending work registration:", error);
    throw error;
  }
};

export const createTransportationRegistration = async (
  data: CreateRegistrationRequest
) => {
  try {
    const response = await axios.post(
      `${API_URL}/registration/transportation/create`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error creating transportation registration:", error);
    throw error;
  }
};

export const startTransportationRegistration = async (
  data: StartRegistrationRequest
) => {
  try {
    const response = await axios.post(
      `${API_URL}/registration/transportation/start`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error starting transportation registration:", error);
    throw error;
  }
};

export const endTransportationRegistration = async (
  data: EndRegistrationRequest
) => {
  try {
    const response = await axios.patch(
      `${API_URL}/registration/transportation/end`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error ending transportation registration:", error);
    throw error;
  }
};

export const employeeConfirmationRegistration = async (
  data: EmployeeConfirmationRegistrationRequest
) => {
  try {
    const response = await axios.patch(
      `${API_URL}/registration/confirmation`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error confirming registration:", error);
    throw error;
  }
};

export const employeeCorrectionRegistration = async (
  data: EmployeeCorrectionRegistrationRequest
) => {
  try {
    const response = await axios.post(
      `${API_URL}/registration/correction`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error(error);
    throw error;
  }
};

export const createAbsenceRegistration = async (
  data: CreateAbsenceRegistrationRequest
) => {
  try {
    const response = await axios.post(
      `${API_URL}/registration/admin/create/absence`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error creating absence registration:", error);
    throw error;
  }
};

export const updateRegistrationStatus = async (data: UpdateStatusRequest) => {
  try {
    const response = await axios.patch(
      `${API_URL}/registration/admin/update`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error updating registration status:", error);
    throw error;
  }
};

export const changeRegistrationType = async (data: ChangeTypeRequest) => {
  try {
    const response = await axios.patch(
      `${API_URL}/registration/admin/change`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error changing registration type:", error);
    throw error;
  }
};

export const adminCorrectionRegistration = async (
  data: AdminCorrectionRegistrationRequest
) => {
  try {
    const response = await axios.post(
      `${API_URL}/registration/admin/correction`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error with admin correction registration:", error);
    throw error;
  }
};
