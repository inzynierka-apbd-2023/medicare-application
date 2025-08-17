import { apiClient } from "./apiClient";

export interface AvailabilityResponse {
  emailExists?: boolean;
  usernameExists?: boolean;
}

export const availabilityApi = {
  async checkEmail(email: string, signal?: AbortSignal): Promise<boolean> {
    const res = await apiClient.get<AvailabilityResponse>(`/users/availability`, {
      params: { email },
      signal,
    });
    return Boolean(res.data.emailExists);
  },
  async checkUsername(username: string, signal?: AbortSignal): Promise<boolean> {
    const res = await apiClient.get<AvailabilityResponse>(`/users/availability`, {
      params: { username },
      signal,
    });
    return Boolean(res.data.usernameExists);
  },
};
