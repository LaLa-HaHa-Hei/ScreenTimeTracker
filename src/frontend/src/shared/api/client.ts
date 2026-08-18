import axios, { type AxiosInstance } from "axios";

export const baseUrl = import.meta.env.DEV ? "http://localhost:5124" : "";

export const baseApiUrl = `${baseUrl}/api`;

export const apiClient: AxiosInstance = axios.create({
  baseURL: baseApiUrl,
  timeout: 2000,
  paramsSerializer: {
    indexes: null,
  },
  headers: {
    "Content-Type": "application/json",
  },
});
