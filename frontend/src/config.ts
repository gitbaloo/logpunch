//Adjust to correct URL once being deployed
// const API_BASE_URL = "https://localhost:7206";
let API_BASE_URL;
if (location.hostname === "localhost") {
  API_BASE_URL = "https://localhost:7206/api";
} else {
  API_BASE_URL = "/api";
}
const LOGO = "/punchlog.svg";

export { API_BASE_URL, LOGO };
