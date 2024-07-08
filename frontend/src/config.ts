let API_BASE_URL;

if (import.meta.env.VITE_APP_ENV === "docker") {
  API_BASE_URL = "http://localhost:7206/api";
} else {
  API_BASE_URL = "";
}

const LOGO = "/punchlog.svg";

export { API_BASE_URL, LOGO };
