class AuthService {
  // Function to handle user login
  public static login(token: string) {
    const encodedToken = btoa(token);
    localStorage.setItem("token", encodedToken);
  }

  // Function to handle user logout
  public static logout() {
    localStorage.removeItem("token");
  }

  // Function to check if the user is authenticated
  public static isAuthenticated() {
    const tokenData = this.getPayload();
    // If no token is stored
    if (!tokenData || !("exp" in tokenData)) {
      return false;
    }
    try {
      // Checking for expiration
      if (
        typeof tokenData.exp === "number" &&
        Date.now() >= tokenData.exp * 1000
      ) {
        // Token has expired and is removed from localstorage
        this.logout();
        return false;
      }
      return true; // Token is deemed valid and not expired
    } catch (error) {
      console.error("Error decoding or parsing token:", error);
      return false;
    }
  }

  // Function to get the stored token
  public static getToken() {
    const storedToken = localStorage.getItem("token");
    if (storedToken) {
      return atob(storedToken);
    }
    return null;
  }

  public static getPayload() {
    const storedToken = this.getToken();

    // If no token is stored
    if (!storedToken) {
      return null;
    } else {
      return this.decodeToken(storedToken);
    }
  }

  private static decodeToken(token: string) {
    const payloadEncoded = token.split(".")[1];
    const payload = JSON.parse(atob(payloadEncoded));
    return payload;
  }
}

export default AuthService;
