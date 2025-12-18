import Cookies from "js-cookie";
import { decryptId } from "./encryptor";

export function checkAuthStatus() {
  try {
    const jwtToken = Cookies.get("jwtToken");
    const ssoData = Cookies.get("ssoData");
    const userData = Cookies.get("userData");
    const permissionData = Cookies.get("permissionData");

    console.log("Auth Status Check:");
    console.log("JWT Token exists:", !!jwtToken);
    console.log("SSO Data exists:", !!ssoData);
    console.log("User Data exists:", !!userData);
    console.log("Permission Data exists:", !!permissionData);

    if (!jwtToken || !ssoData || !userData) {
      return {
        isAuthenticated: false,
        reason: "Missing required cookies"
      };
    }

    // Try to decrypt and parse user data
    let parsedUserData = null;
    let parsedSsoData = null;
    
    try {
      if (userData) {
        const decryptedUserData = decryptId(decodeURIComponent(userData));
        parsedUserData = JSON.parse(decryptedUserData);
      }
      
      if (ssoData) {
        const decryptedSsoData = decryptId(decodeURIComponent(ssoData));
        parsedSsoData = JSON.parse(decryptedSsoData);
      }
    } catch (error) {
      console.error("Error parsing user/sso data:", error);
      return {
        isAuthenticated: false,
        reason: "Invalid cookie data"
      };
    }

    return {
      isAuthenticated: true,
      userData: parsedUserData,
      ssoData: parsedSsoData,
      permissions: permissionData ? JSON.parse(permissionData) : []
    };
  } catch (error) {
    console.error("Auth check error:", error);
    return {
      isAuthenticated: false,
      reason: "Auth check failed"
    };
  }
}

export function clearAuthCookies() {
  Cookies.remove("jwtToken");
  Cookies.remove("ssoData");
  Cookies.remove("userData");
  Cookies.remove("permissionData");
}