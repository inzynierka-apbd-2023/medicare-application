import { AuthResponse } from "./authService";

// Mock credentials for testing
const MOCK_CREDENTIALS = {
  patient: {
    username: "patient",
    password: "test",
    user: {
      id: "1",
      username: "patient",
      email: "patient@test.com",
      role: "Patient",
      firstName: "John",
      lastName: "Doe",
    },
  },
  doctor: {
    username: "doctor",
    password: "test",
    user: {
      id: "2",
      username: "doctor",
      email: "doctor@test.com",
      role: "Doctor",
      firstName: "Dr. Jane",
      lastName: "Smith",
    },
  },
  owner: {
    username: "owner",
    password: "test",
    user: {
      id: "3",
      username: "owner",
      email: "owner@test.com",
      role: "Owner",
      firstName: "Admin",
      lastName: "User",
    },
  },
  receptionist: {
    username: "receptionist",
    password: "test",
    user: {
      id: "4",
      username: "receptionist",
      email: "receptionist@test.com",
      role: "Receptionist",
      firstName: "Mary",
      lastName: "Johnson",
    },
  },
};

const TOKEN_KEY = "authToken";

export const mockAuthService = {
  async login(username: string, password: string): Promise<AuthResponse> {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 500));

    // Check mock credentials
    const mockUser = Object.values(MOCK_CREDENTIALS).find(
      (cred) => cred.username === username && cred.password === password
    );

    if (mockUser) {
      const token = `mock-token-${mockUser.user.role.toLowerCase()}-${Date.now()}`;
      this.persistToken(token);

      return {
        token,
        user: mockUser.user,
      };
    } else {
      throw new Error("Invalid credentials");
    }
  },

  async register(data: {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    dateOfBirth?: string;
    role?: string;
  }): Promise<AuthResponse> {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 500));

    // Auto-create as patient
    const token = `mock-token-patient-${Date.now()}`;
    this.persistToken(token);

    return {
      token,
      user: {
        id: String(Date.now()),
        username: data.username,
        email: data.email,
        role: data.role || "Patient",
        firstName: data.firstName,
        lastName: data.lastName,
      },
    };
  },

  logout() {
    localStorage.removeItem(TOKEN_KEY);
  },

  getToken() {
    return localStorage.getItem(TOKEN_KEY);
  },

  persistToken(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
  },

  // Helper method to auto-login as specific role
  async autoLogin(
    role: "patient" | "doctor" | "owner" | "receptionist" = "patient"
  ): Promise<AuthResponse> {
    const mockUser = MOCK_CREDENTIALS[role];
    return this.login(mockUser.username, mockUser.password);
  },
};

// Export mock credentials for easy reference
export { MOCK_CREDENTIALS };
