import axios from 'axios';
import type { AxiosRequestConfig } from 'axios';

// Axios client configurado para comunicar com a API .NET
const axiosInstance = axios.create({
  baseURL: import.meta.env.DEV ? '' : '', // Usar proxy do Vite em dev, ou raiz em produção
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: false,
});

// Interceptor para logs (útil em desenvolvimento)
axiosInstance.interceptors.request.use(
  (config) => {
    console.log(`[API Request] ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => {
    console.error('[API Request Error]', error);
    return Promise.reject(error);
  }
);

axiosInstance.interceptors.response.use(
  (response) => {
    console.log(`[API Response] ${response.config.method?.toUpperCase()} ${response.config.url} - ${response.status}`);
    return response;
  },
  (error) => {
    console.error('[API Response Error]', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

// Mutator customizado para Orval - retorna apenas os dados
export const apiClient = <T>(config: AxiosRequestConfig): Promise<T> => {
  return axiosInstance.request<T>(config).then(({ data }) => data);
};

export default axiosInstance;
