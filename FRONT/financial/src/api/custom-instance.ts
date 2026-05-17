import axios, { type AxiosRequestConfig, type AxiosResponse, type AxiosError } from 'axios';
import { useAuthStore } from '../store/authStore';

export const AXIOS_INSTANCE = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    headers: {
        'Content-Type': 'application/json',
        
    }
});

AXIOS_INSTANCE.interceptors.request.use((config) => {
    const token = useAuthStore.getState().accessToken;

    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

AXIOS_INSTANCE.interceptors.response.use(
    (response) => response, 
    async (error: AxiosError) => {
        const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

        if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
            originalRequest._retry = true;

            const refreshToken = useAuthStore.getState().refreshToken;

            if (refreshToken) {
                try {
                    const response = await axios.post(
                        `${import.meta.env.VITE_API_URL}/api/Jwt/refreshToken`,
                        `"${refreshToken}"`, 
                        { headers: { 'Content-Type': 'application/json' } }
                    );

                    const newAccessToken = response.data;

                    useAuthStore.getState().setTokens(newAccessToken, refreshToken);

                    if (originalRequest.headers) {
                        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                    }

                    return AXIOS_INSTANCE(originalRequest);

                } catch (refreshError) {
                    console.error("Refresh Token Expired");
                    useAuthStore.getState().clearAuth(); 
                    window.location.href = '/login'; 
                    return Promise.reject(refreshError);
                }
            } else {
                useAuthStore.getState().clearAuth();
                window.location.href = '/login';
            }
        }

        return Promise.reject(error);
    }
);

export const customInstance = <T>(
    config: AxiosRequestConfig,
    options?: AxiosRequestConfig,
): Promise<T> => {
    return AXIOS_INSTANCE({
        ...config,
        ...options,
    }).then((response: AxiosResponse<T>) => response.data);
};