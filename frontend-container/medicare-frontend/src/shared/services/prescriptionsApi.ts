import {
  Doctor,
  Medication,
  Patient,
  Pharmacy,
  Prescription,
  PrescriptionFilter,
  PrescriptionFormData,
} from "../../features/prescriptions/types";

// Mock data
const mockPatients: Patient[] = [
  {
    id: "1",
    name: "John Doe",
    email: "john.doe@email.com",
    phone: "+1-555-0123",
    dateOfBirth: new Date("1985-03-15"),
    allergies: ["Penicillin", "Shellfish"],
    medicalHistory: ["Hypertension", "Type 2 Diabetes"],
  },
  {
    id: "2",
    name: "Jane Smith",
    email: "jane.smith@email.com",
    phone: "+1-555-0124",
    dateOfBirth: new Date("1992-07-22"),
    allergies: ["Latex"],
    medicalHistory: ["Asthma"],
  },
  {
    id: "3",
    name: "Robert Johnson",
    email: "robert.johnson@email.com",
    phone: "+1-555-0125",
    dateOfBirth: new Date("1978-11-03"),
    allergies: [],
    medicalHistory: ["High Cholesterol", "Arthritis"],
  },
];

const mockDoctors: Doctor[] = [
  {
    id: "doc1",
    name: "Dr. Sarah Wilson",
    specialization: "Internal Medicine",
    licenseNumber: "MD123456",
    email: "dr.wilson@clinic.com",
    phone: "+1-555-0200",
  },
  {
    id: "doc2",
    name: "Dr. Michael Chen",
    specialization: "Cardiology",
    licenseNumber: "MD789012",
    email: "dr.chen@clinic.com",
    phone: "+1-555-0201",
  },
];

const mockPharmacies: Pharmacy[] = [
  {
    id: "ph1",
    name: "City Pharmacy",
    address: "123 Main St, City, ST 12345",
    phone: "+1-555-0300",
    email: "info@citypharmacy.com",
  },
  {
    id: "ph2",
    name: "HealthCare Pharmacy",
    address: "456 Oak Ave, Town, ST 67890",
    phone: "+1-555-0301",
    email: "contact@healthcarepharmacy.com",
  },
];

const mockMedications: Medication[] = [
  {
    id: "med1",
    name: "Lisinopril",
    genericName: "Lisinopril",
    dosage: "10mg",
    frequency: "Once daily",
    duration: "30 days",
    instructions: "Take with or without food",
    quantity: 30,
    unit: "tablets",
    refills: 5,
    isGenericAllowed: true,
  },
  {
    id: "med2",
    name: "Metformin",
    genericName: "Metformin HCl",
    dosage: "500mg",
    frequency: "Twice daily",
    duration: "30 days",
    instructions: "Take with meals to reduce stomach upset",
    quantity: 60,
    unit: "tablets",
    refills: 5,
    isGenericAllowed: true,
  },
];

const mockPrescriptions: Prescription[] = [
  {
    id: "rx1",
    patientId: "1",
    doctorId: "doc1",
    appointmentId: "apt1",
    medications: [mockMedications[0]],
    diagnosis: "Hypertension",
    notes: "Monitor blood pressure regularly",
    status: "active",
    createdAt: new Date("2024-01-15"),
    updatedAt: new Date("2024-01-15"),
    validUntil: new Date("2024-07-15"),
    issuedAt: new Date("2024-01-15"),
    pharmacyId: "ph1",
  },
  {
    id: "rx2",
    patientId: "1",
    doctorId: "doc1",
    medications: [mockMedications[1]],
    diagnosis: "Type 2 Diabetes Mellitus",
    notes: "Check blood glucose levels as directed",
    status: "partially_dispensed",
    createdAt: new Date("2024-01-20"),
    updatedAt: new Date("2024-01-25"),
    validUntil: new Date("2024-07-20"),
    issuedAt: new Date("2024-01-20"),
    pharmacyId: "ph1",
    dispensedAt: new Date("2024-01-21"),
  },
  {
    id: "rx3",
    patientId: "2",
    doctorId: "doc1",
    medications: [
      {
        id: "med3",
        name: "Albuterol Inhaler",
        genericName: "Albuterol Sulfate",
        dosage: "90mcg",
        frequency: "As needed",
        duration: "30 days",
        instructions: "Use as rescue inhaler for breathing difficulties",
        quantity: 1,
        unit: "inhaler",
        refills: 2,
        isGenericAllowed: false,
      },
    ],
    diagnosis: "Asthma",
    notes: "Patient should carry inhaler at all times",
    status: "active",
    createdAt: new Date("2024-02-01"),
    updatedAt: new Date("2024-02-01"),
    validUntil: new Date("2024-08-01"),
    issuedAt: new Date("2024-02-01"),
  },
];

// Simulation delay
const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

class PrescriptionsApi {
  async getPrescriptions(
    filters: PrescriptionFilter = {}
  ): Promise<Prescription[]> {
    await delay(300);

    let filtered = [...mockPrescriptions];

    if (filters.status) {
      filtered = filtered.filter((rx) => rx.status === filters.status);
    }

    if (filters.patientId) {
      filtered = filtered.filter((rx) => rx.patientId === filters.patientId);
    }

    if (filters.doctorId) {
      filtered = filtered.filter((rx) => rx.doctorId === filters.doctorId);
    }

    if (filters.dateFrom) {
      filtered = filtered.filter((rx) => rx.createdAt >= filters.dateFrom!);
    }

    if (filters.dateTo) {
      filtered = filtered.filter((rx) => rx.createdAt <= filters.dateTo!);
    }

    if (filters.searchTerm) {
      const searchLower = filters.searchTerm.toLowerCase();
      filtered = filtered.filter(
        (rx) =>
          rx.diagnosis.toLowerCase().includes(searchLower) ||
          rx.medications.some(
            (med) =>
              med.name.toLowerCase().includes(searchLower) ||
              (med.genericName &&
                med.genericName.toLowerCase().includes(searchLower))
          ) ||
          rx.notes?.toLowerCase().includes(searchLower)
      );
    }

    return filtered.sort(
      (a, b) => b.createdAt.getTime() - a.createdAt.getTime()
    );
  }

  async getPrescriptionById(id: string): Promise<Prescription | null> {
    await delay(200);
    return mockPrescriptions.find((rx) => rx.id === id) || null;
  }

  async createPrescription(data: PrescriptionFormData): Promise<Prescription> {
    await delay(500);

    const newPrescription: Prescription = {
      id: `rx${Date.now()}`,
      patientId: data.patientId,
      doctorId: "doc1", // Current user
      medications: data.medications.map((med, index) => ({
        id: `med${Date.now()}_${index}`,
        ...med,
      })),
      diagnosis: data.diagnosis,
      ...(data.notes && { notes: data.notes }),
      status: "active",
      createdAt: new Date(),
      updatedAt: new Date(),
      validUntil: data.validUntil,
      issuedAt: new Date(),
    };

    mockPrescriptions.unshift(newPrescription);
    return newPrescription;
  }

  async updatePrescription(
    id: string,
    data: Partial<PrescriptionFormData>
  ): Promise<Prescription> {
    await delay(500);

    const prescriptionIndex = mockPrescriptions.findIndex((rx) => rx.id === id);
    if (prescriptionIndex === -1) {
      throw new Error("Prescription not found");
    }

    const existingPrescription = mockPrescriptions[prescriptionIndex];
    const updatedPrescription: Prescription = {
      ...existingPrescription,
      ...data,
      medications: data.medications
        ? data.medications.map((med, index) => ({
            id: `med${Date.now()}_${index}`,
            ...med,
          }))
        : existingPrescription.medications,
      updatedAt: new Date(),
    };

    mockPrescriptions[prescriptionIndex] = updatedPrescription;
    return updatedPrescription;
  }

  async deletePrescription(id: string): Promise<void> {
    await delay(300);

    const index = mockPrescriptions.findIndex((rx) => rx.id === id);
    if (index === -1) {
      throw new Error("Prescription not found");
    }

    mockPrescriptions.splice(index, 1);
  }

  async getPatients(): Promise<Patient[]> {
    await delay(200);
    return [...mockPatients];
  }

  async getDoctors(): Promise<Doctor[]> {
    await delay(200);
    return [...mockDoctors];
  }

  async getPharmacies(): Promise<Pharmacy[]> {
    await delay(200);
    return [...mockPharmacies];
  }

  async updatePrescriptionStatus(
    id: string,
    status: Prescription["status"]
  ): Promise<Prescription> {
    await delay(300);

    const prescriptionIndex = mockPrescriptions.findIndex((rx) => rx.id === id);
    if (prescriptionIndex === -1) {
      throw new Error("Prescription not found");
    }

    const updatedPrescription = {
      ...mockPrescriptions[prescriptionIndex],
      status,
      updatedAt: new Date(),
      ...(status === "fully_dispensed" && { dispensedAt: new Date() }),
    };

    mockPrescriptions[prescriptionIndex] = updatedPrescription;
    return updatedPrescription;
  }

  async generatePrescriptionPDF(id: string): Promise<Blob> {
    await delay(1000);

    // Mock PDF generation
    const pdfContent = `Prescription ID: ${id}\nGenerated on: ${new Date().toISOString()}`;
    return new Blob([pdfContent], { type: "application/pdf" });
  }
}

export const prescriptionsApi = new PrescriptionsApi();
