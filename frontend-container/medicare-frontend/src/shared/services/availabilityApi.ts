import { api } from "./api";

export interface AvailabilityResponse {
  emailExists?: boolean;
  usernameExists?: boolean;
}

export const availabilityApi = {
  async checkEmail(email: string, signal?: AbortSignal): Promise<boolean> {
    const res = await api.get<AvailabilityResponse>(`/users/availability`, {
      params: { email },
      ...(signal ? { signal } : {}),
    });
    return Boolean(res.emailExists);
  },
  async checkUsername(
    username: string,
    signal?: AbortSignal
  ): Promise<boolean> {
    const res = await api.get<AvailabilityResponse>(`/users/availability`, {
      params: { username },
      ...(signal ? { signal } : {}),
    });
    return Boolean(res.usernameExists);
  },
};
