import type {
  ApiResponse,
  CreatePatientRequest,
  Doctor,
  PatientRegistryData,
  PatientRegistryFilters,
  PatientRegistryInfo,
} from "../types";

// Mock data for development
const generateMockPatients = (): PatientRegistryInfo[] => {
  const firstNames = [
    "Jan",
    "Anna",
    "Piotr",
    "Maria",
    "Tomasz",
    "Katarzyna",
    "Michał",
    "Agnieszka",
    "Krzysztof",
    "Magdalena",
  ];
  const lastNames = [
    "Kowalski",
    "Nowak",
    "Wiśniewski",
    "Wójcik",
    "Kowalczyk",
    "Kamińska",
    "Lewandowski",
    "Zielińska",
    "Szymański",
    "Woźniak",
  ];
  const bloodTypes = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];
  const genders = ["male", "female", "other"] as const;
  const cities = [
    "Warsaw",
    "Kraków",
    "Łódź",
    "Wrocław",
    "Poznań",
    "Gdańsk",
    "Szczecin",
    "Bydgoszcz",
  ];

  const patients: PatientRegistryInfo[] = [];

  for (let i = 1; i <= 50; i++) {
    const firstName = firstNames[Math.floor(Math.random() * firstNames.length)];
    const lastName = lastNames[Math.floor(Math.random() * lastNames.length)];
    const gender = genders[Math.floor(Math.random() * genders.length)];
    const bloodType = bloodTypes[Math.floor(Math.random() * bloodTypes.length)];
    const city = cities[Math.floor(Math.random() * cities.length)];

    // Generate random birth date (18-80 years old)
    const birthYear =
      new Date().getFullYear() - Math.floor(Math.random() * 62) - 18;
    const birthMonth = Math.floor(Math.random() * 12) + 1;
    const birthDay = Math.floor(Math.random() * 28) + 1;
    const dateOfBirth = `${birthYear}-${birthMonth.toString().padStart(2, "0")}-${birthDay.toString().padStart(2, "0")}`;

    patients.push({
      id: `patient-${i.toString().padStart(3, "0")}`,
      firstName,
      lastName,
      email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}@email.com`,
      phone: `+48 ${Math.floor(Math.random() * 900) + 100} ${Math.floor(Math.random() * 900) + 100} ${Math.floor(Math.random() * 900) + 100}`,
      dateOfBirth,
      gender,
      addressLine1: `ul. ${["Słoneczna", "Główna", "Krótka", "Długa", "Nowa"][Math.floor(Math.random() * 5)]} ${Math.floor(Math.random() * 100) + 1}`,
      city,
      state: "Mazowieckie",
      zipCode: `${Math.floor(Math.random() * 90) + 10}-${Math.floor(Math.random() * 900) + 100}`,
      country: "Poland",
      medicalRecordNumber: `MRN${i.toString().padStart(6, "0")}`,
      bloodType,
      height: Math.floor(Math.random() * 50) + 150, // 150-200 cm
      weight: Math.floor(Math.random() * 50) + 50, // 50-100 kg
      generalDoctorId: `doctor-${Math.floor(Math.random() * 10) + 1}`,
      insurance: [
        {
          id: `ins-${i}`,
          providerName: ["NFZ", "Allianz Care", "PZU Zdrowie", "Medicover"][
            Math.floor(Math.random() * 4)
          ],
          policyNumber: `POL${Math.floor(Math.random() * 1000000)}`,
          groupNumber: `GRP${Math.floor(Math.random() * 1000)}`,
          validFrom: `${new Date().getFullYear()}-01-01`,
          validTo: `${new Date().getFullYear() + 1}-12-31`,
          isPrimary: true,
          isActive: true,
        },
      ],
      emergencyContacts: [
        {
          id: `ec-${i}`,
          name: `${firstNames[Math.floor(Math.random() * firstNames.length)]} ${lastNames[Math.floor(Math.random() * lastNames.length)]}`,
          phone: `+48 ${Math.floor(Math.random() * 900) + 100} ${Math.floor(Math.random() * 900) + 100} ${Math.floor(Math.random() * 900) + 100}`,
          relationship: ["Spouse", "Parent", "Sibling", "Child", "Friend"][
            Math.floor(Math.random() * 5)
          ],
          isPrimary: true,
        },
      ],
      isActive: Math.random() > 0.1, // 90% active
      createdAt: new Date(
        Date.now() - Math.random() * 31536000000
      ).toISOString(), // Within last year
      updatedAt: new Date().toISOString(),
    });
  }

  return patients;
};

const generateMockDoctors = (): Doctor[] => {
  return [
    {
      id: "doctor-1",
      firstName: "Dr. Anna",
      lastName: "Kowalska",
      specialization: "Family Medicine",
      email: "anna.kowalska@medicare.com",
      phone: "+48 123 456 789",
    },
    {
      id: "doctor-2",
      firstName: "Dr. Piotr",
      lastName: "Nowak",
      specialization: "Internal Medicine",
      email: "piotr.nowak@medicare.com",
      phone: "+48 123 456 790",
    },
    {
      id: "doctor-3",
      firstName: "Dr. Maria",
      lastName: "Wiśniewska",
      specialization: "Cardiology",
      email: "maria.wisniewska@medicare.com",
      phone: "+48 123 456 791",
    },
    {
      id: "doctor-4",
      firstName: "Dr. Tomasz",
      lastName: "Wójcik",
      specialization: "Pediatrics",
      email: "tomasz.wojcik@medicare.com",
      phone: "+48 123 456 792",
    },
    {
      id: "doctor-5",
      firstName: "Dr. Katarzyna",
      lastName: "Kowalczyk",
      specialization: "Gynecology",
      email: "katarzyna.kowalczyk@medicare.com",
      phone: "+48 123 456 793",
    },
    {
      id: "doctor-6",
      firstName: "Dr. Michał",
      lastName: "Lewandowski",
      specialization: "Orthopedics",
      email: "michal.lewandowski@medicare.com",
      phone: "+48 123 456 794",
    },
    {
      id: "doctor-7",
      firstName: "Dr. Agnieszka",
      lastName: "Zielińska",
      specialization: "Dermatology",
      email: "agnieszka.zielinska@medicare.com",
      phone: "+48 123 456 795",
    },
    {
      id: "doctor-8",
      firstName: "Dr. Krzysztof",
      lastName: "Szymański",
      specialization: "Neurology",
      email: "krzysztof.szymanski@medicare.com",
      phone: "+48 123 456 796",
    },
    {
      id: "doctor-9",
      firstName: "Dr. Magdalena",
      lastName: "Woźniak",
      specialization: "Psychiatry",
      email: "magdalena.wozniak@medicare.com",
      phone: "+48 123 456 797",
    },
    {
      id: "doctor-10",
      firstName: "Dr. Łukasz",
      lastName: "Dąbrowski",
      specialization: "Emergency Medicine",
      email: "lukasz.dabrowski@medicare.com",
      phone: "+48 123 456 798",
    },
  ];
};

// Store mock data
let mockPatients = generateMockPatients();
const mockDoctors = generateMockDoctors();

// Helper function to create mock API response
const createMockResponse = <T>(
  data: T,
  delay = 300
): Promise<ApiResponse<T>> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve({
        success: true,
        data,
        message: "Success",
      });
    }, delay);
  });
};

export class PatientRegistryApiService {
  /**
   * Get paginated list of patients with optional filters
   */
  static async getPatients(
    page = 1,
    limit = 10,
    filters?: PatientRegistryFilters
  ): Promise<ApiResponse<PatientRegistryData>> {
    let filteredPatients = [...mockPatients];

    // Apply filters
    if (filters) {
      if (filters.searchTerm) {
        const searchLower = filters.searchTerm.toLowerCase();
        filteredPatients = filteredPatients.filter(
          (patient) =>
            `${patient.firstName} ${patient.lastName}`
              .toLowerCase()
              .includes(searchLower) ||
            patient.email.toLowerCase().includes(searchLower) ||
            (patient.phone && patient.phone.includes(filters.searchTerm!)) ||
            (patient.medicalRecordNumber &&
              patient.medicalRecordNumber.toLowerCase().includes(searchLower))
        );
      }

      if (filters.doctorId) {
        filteredPatients = filteredPatients.filter(
          (patient) => patient.generalDoctorId === filters.doctorId
        );
      }

      if (filters.bloodType) {
        filteredPatients = filteredPatients.filter(
          (patient) => patient.bloodType === filters.bloodType
        );
      }

      if (filters.isActive !== undefined) {
        filteredPatients = filteredPatients.filter(
          (patient) => patient.isActive === filters.isActive
        );
      }
    }

    // Sort by creation date (newest first)
    filteredPatients.sort(
      (a, b) =>
        new Date(b.createdAt || 0).getTime() -
        new Date(a.createdAt || 0).getTime()
    );

    // Paginate results
    const totalCount = filteredPatients.length;
    const totalPages = Math.ceil(totalCount / limit);
    const startIndex = (page - 1) * limit;
    const endIndex = startIndex + limit;
    const patients = filteredPatients.slice(startIndex, endIndex);

    const data: PatientRegistryData = {
      patients,
      totalCount,
      currentPage: page,
      totalPages,
    };

    return createMockResponse(data, 500);
  }

  /**
   * Get single patient by ID
   */
  static async getPatient(
    patientId: string
  ): Promise<ApiResponse<PatientRegistryInfo>> {
    const patient = mockPatients.find((p) => p.id === patientId);

    if (!patient) {
      return new Promise((resolve) => {
        setTimeout(() => {
          resolve({
            success: false,
            data: {} as PatientRegistryInfo,
            message: "Patient not found",
          });
        }, 300);
      });
    }

    return createMockResponse(patient);
  }

  /**
   * Create new patient
   */
  static async createPatient(
    patientData: CreatePatientRequest
  ): Promise<ApiResponse<PatientRegistryInfo>> {
    const newId = `patient-${(mockPatients.length + 1).toString().padStart(3, "0")}`;
    const now = new Date().toISOString();

    // Generate medical record number
    const medicalRecordNumber = `MRN${(mockPatients.length + 1).toString().padStart(6, "0")}`;

    const newPatient: PatientRegistryInfo = {
      id: newId,
      firstName: patientData.personalInfo.firstName,
      lastName: patientData.personalInfo.lastName,
      email: patientData.personalInfo.email,
      phone: patientData.personalInfo.phone,
      dateOfBirth: patientData.personalInfo.dateOfBirth,
      gender: patientData.personalInfo.gender,
      addressLine1: patientData.address.addressLine1,
      ...(patientData.address.addressLine2 && {
        addressLine2: patientData.address.addressLine2,
      }),
      city: patientData.address.city,
      state: patientData.address.state,
      zipCode: patientData.address.zipCode,
      country: patientData.address.country,
      medicalRecordNumber,
      ...(patientData.medicalInfo.bloodType && {
        bloodType: patientData.medicalInfo.bloodType,
      }),
      ...(patientData.medicalInfo.height && {
        height: patientData.medicalInfo.height,
      }),
      ...(patientData.medicalInfo.weight && {
        weight: patientData.medicalInfo.weight,
      }),
      ...(patientData.medicalInfo.generalDoctorId && {
        generalDoctorId: patientData.medicalInfo.generalDoctorId,
      }),
      ...(patientData.insurance && {
        insurance: [
          {
            id: `ins-${newId}`,
            providerName: patientData.insurance.providerName,
            policyNumber: patientData.insurance.policyNumber,
            ...(patientData.insurance.groupNumber && {
              groupNumber: patientData.insurance.groupNumber,
            }),
            validFrom: patientData.insurance.validFrom,
            ...(patientData.insurance.validTo && {
              validTo: patientData.insurance.validTo,
            }),
            isPrimary: patientData.insurance.isPrimary,
            isActive: true,
          },
        ],
      }),
      emergencyContacts: [
        {
          id: `ec-${newId}`,
          name: patientData.emergencyContact.name,
          phone: patientData.emergencyContact.phone,
          relationship: patientData.emergencyContact.relationship,
          isPrimary: true,
        },
      ],
      isActive: true,
      createdAt: now,
      updatedAt: now,
    };

    // Add to mock data
    mockPatients.push(newPatient);

    return createMockResponse(newPatient, 800);
  }

  /**
   * Update existing patient
   */
  static async updatePatient(
    patientId: string,
    patientData: Partial<PatientRegistryInfo>
  ): Promise<ApiResponse<PatientRegistryInfo>> {
    const patientIndex = mockPatients.findIndex((p) => p.id === patientId);

    if (patientIndex === -1) {
      return new Promise((resolve) => {
        setTimeout(() => {
          resolve({
            success: false,
            data: {} as PatientRegistryInfo,
            message: "Patient not found",
          });
        }, 300);
      });
    }

    // Update patient data
    mockPatients[patientIndex] = {
      ...mockPatients[patientIndex],
      ...patientData,
      updatedAt: new Date().toISOString(),
    };

    return createMockResponse(mockPatients[patientIndex], 600);
  }

  /**
   * Get all doctors for dropdown
   */
  static async getDoctors(): Promise<ApiResponse<Doctor[]>> {
    return createMockResponse(mockDoctors, 200);
  }

  /**
   * Check if email is already in use
   */
  static async checkEmailAvailability(
    email: string
  ): Promise<ApiResponse<{ available: boolean }>> {
    const exists = mockPatients.some(
      (p) => p.email.toLowerCase() === email.toLowerCase()
    );

    return createMockResponse({ available: !exists }, 300);
  }

  /**
   * Delete patient (soft delete - set isActive to false)
   */
  static async deletePatient(patientId: string): Promise<ApiResponse<boolean>> {
    const patientIndex = mockPatients.findIndex((p) => p.id === patientId);

    if (patientIndex === -1) {
      return new Promise((resolve) => {
        setTimeout(() => {
          resolve({
            success: false,
            data: false,
            message: "Patient not found",
          });
        }, 300);
      });
    }

    // Soft delete
    mockPatients[patientIndex].isActive = false;
    mockPatients[patientIndex].updatedAt = new Date().toISOString();

    return createMockResponse(true, 400);
  }
}
