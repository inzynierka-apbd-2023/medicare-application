import React, { useCallback, useEffect, useState } from "react";
import { PatientRegistryApiService } from "@features/patientRegistry/services/patientRegistryApi";
import type { PatientRegistryInfo } from "@features/patientRegistry/types";
import SchedulerApiService from "@features/scheduler/services/schedulerApiService";
import type {
  AppointmentModalProps,
  CreateAppointmentRequest,
  Doctor,
  Service,
  Specialization,
  TimeSlot,
  UpdateAppointmentRequest,
} from "@features/scheduler/types";
import { Button, Card } from "@shared/components";
import { Clock, MapPin, Phone, Search, User, Video, X } from "lucide-react";

// Helper for input
const formatDateForInput = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return "";
    return date.toISOString().split("T")[0];
  } catch {
    return "";
  }
};

interface ExtendedModalProps extends AppointmentModalProps {
  onEdit?: () => void;
  onCancel?: () => void;
}

export const AppointmentModal: React.FC<ExtendedModalProps> = ({
  isOpen,
  onClose,
  appointment,
  onSave,
  mode,
  patientId: contextPatientId,
  onEdit,
  onCancel,
}) => {
  const [formData, setFormData] = useState({
    patientId: "",
    doctorUserId: "",
    serviceId: "",
    specializationId: "",
    timeSlotId: "",
    appointmentType: "in-person" as "in-person" | "virtual" | "phone",
    description: "",
    statusId: "",
  });

  // ... (rest of state and loaders same as before)
  const [formState, setFormState] = useState({
    patients: [] as PatientRegistryInfo[],
    specializations: [] as Specialization[],
    doctors: [] as Doctor[],
    services: [] as Service[],
    timeSlots: [] as TimeSlot[],
    isLoading: false,
    error: null as string | null,
    step: 1,
    patientSearchTerm: "",
  });

  const [selectedDate, setSelectedDate] = useState<string>("");

  useEffect(() => {
    if (isOpen) {
      if (appointment && mode !== "create") {
        setFormData({
          patientId: appointment.patientId,
          doctorUserId: appointment.doctorUserId,
          serviceId: appointment.serviceId || "",
          specializationId: appointment.doctor?.specializationId || "",
          timeSlotId: appointment.timeSlotId || "",
          appointmentType: appointment.appointmentType || "in-person",
          description: appointment.description || "",
          statusId: appointment.statusId || "",
        });
        setSelectedDate(formatDateForInput(appointment.day));
        setFormState((prev) => ({ ...prev, step: 1 }));
      } else {
        const initialPatientId = contextPatientId || "";
        setFormData({
          patientId: initialPatientId,
          doctorUserId: "",
          serviceId: "",
          specializationId: "",
          timeSlotId: "",
          appointmentType: "in-person",
          description: "",
          statusId: "",
        });
        setSelectedDate("");
        const startingStep = contextPatientId ? 1 : 0;
        setFormState((prev) => ({
          ...prev,
          step: startingStep,
          patientSearchTerm: "",
          patients: [],
        }));
        if (startingStep === 1) {
          loadSpecializations();
        }
      }
    }
  }, [isOpen, appointment, mode, contextPatientId]);

  const searchPatients = async (term: string) => {
    setFormState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const result = await PatientRegistryApiService.getPatients(1, 10, {
        searchTerm: term,
      });
      setFormState((prev) => ({
        ...prev,
        patients: result.data.patients || [],
        isLoading: false,
      }));
    } catch (e) {
      console.error(e);
      setFormState((prev) => ({ ...prev, isLoading: false }));
    }
  };

  useEffect(() => {
    if (formState.step === 0 && isOpen) {
      const timer = setTimeout(() => {
        searchPatients(formState.patientSearchTerm);
      }, 500);
      return () => clearTimeout(timer);
    }
    return undefined;
  }, [formState.patientSearchTerm, formState.step, isOpen]);

  const loadSpecializations = async () => {
    setFormState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const specializations = await SchedulerApiService.getSpecializations();
      setFormState((prev) => ({ ...prev, specializations, isLoading: false }));
    } catch {
      setFormState((prev) => ({
        ...prev,
        error: "Failed to load specializations",
        isLoading: false,
      }));
    }
  };

  const loadServices = async (specializationId: string) => {
    setFormState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const services =
        await SchedulerApiService.getServicesBySpecialization(specializationId);
      setFormState((prev) => ({ ...prev, services, isLoading: false }));
    } catch {
      setFormState((prev) => ({
        ...prev,
        error: "Failed to load services",
        isLoading: false,
      }));
    }
  };

  const loadDoctors = async (specializationId: string) => {
    setFormState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const doctors =
        await SchedulerApiService.getDoctorsBySpecialization(specializationId);
      setFormState((prev) => ({ ...prev, doctors, isLoading: false }));
    } catch {
      setFormState((prev) => ({
        ...prev,
        error: "Failed to load doctors",
        isLoading: false,
      }));
    }
  };

  const loadTimeSlots = async (doctorId: string, date: string) => {
    setFormState((prev) => ({ ...prev, isLoading: true, error: null }));
    try {
      const timeSlots = await SchedulerApiService.getAvailableTimeSlots({
        doctorId,
        startDate: date,
        endDate: date,
      });
      setFormState((prev) => ({ ...prev, timeSlots, isLoading: false }));
    } catch {
      setFormState((prev) => ({
        ...prev,
        error: "Failed to load time slots",
        isLoading: false,
      }));
    }
  };

  const handlePatientSelect = (pId: string) => {
    setFormData((prev) => ({ ...prev, patientId: pId }));
    setFormState((prev) => ({ ...prev, step: 1 }));
    loadSpecializations();
  };

  const handleSpecializationSelect = (specializationId: string) => {
    setFormData((prev) => ({
      ...prev,
      specializationId,
      serviceId: "",
      doctorUserId: "",
      timeSlotId: "",
    }));
    loadServices(specializationId);
    loadDoctors(specializationId);
    setFormState((prev) => ({ ...prev, step: 2 }));
  };

  const handleServiceSelect = (serviceId: string) => {
    setFormData((prev) => ({ ...prev, serviceId }));
    setFormState((prev) => ({ ...prev, step: 3 }));
  };

  const handleDoctorSelect = (doctorId: string) => {
    setFormData((prev) => ({ ...prev, doctorUserId: doctorId }));
    setFormState((prev) => ({ ...prev, step: 4 }));
  };

  const handleDateSelect = (date: string) => {
    setSelectedDate(date);
    if (formData.doctorUserId) {
      loadTimeSlots(formData.doctorUserId, date);
    }
  };

  const handleTimeSlotSelect = (timeSlotId: string) => {
    setFormData((prev) => ({ ...prev, timeSlotId }));
    setFormState((prev) => ({ ...prev, step: 5 }));
  };

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();
      try {
        if (mode === "create") {
          const createData: CreateAppointmentRequest = {
            patientId: formData.patientId,
            doctorUserId: formData.doctorUserId,
            serviceId: formData.serviceId,
            timeSlotId: formData.timeSlotId,
            appointmentType: formData.appointmentType,
            description: formData.description,
            appointmentCategory: "consultation",
          };
          await onSave(createData);
        } else if (mode === "edit" && appointment) {
          const updateData = {
            appointmentType: formData.appointmentType,
            appointmentCategory: "consultation",
          } as UpdateAppointmentRequest & { statusId?: string }; // Cast to allow status update if handled by backend/service wrapper

          if (formData.statusId) {
            updateData.statusId = formData.statusId;
          }

          if (
            formData.timeSlotId &&
            formData.timeSlotId !== appointment.timeSlotId
          ) {
            updateData.timeSlotId = formData.timeSlotId;
          }
          await onSave(updateData);
        }
        onClose();
      } catch (error) {
        setFormState((prev) => ({
          ...prev,
          error:
            error instanceof Error
              ? error.message
              : "Failed to save appointment",
        }));
      }
    },
    [mode, formData, appointment, onSave, onClose]
  );

  const getStepTitle = () => {
    switch (formState.step) {
      case 0:
        return "Select Patient";
      case 1:
        return "Select Specialization";
      case 2:
        return "Select Service";
      case 3:
        return "Select Doctor";
      case 4:
        return "Select Date & Time";
      case 5:
        return "Appointment Details";
      default:
        return "Book Appointment";
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-screen overflow-y-auto m-4">
        <div className="flex items-center justify-between p-6 border-b">
          <h2 className="text-xl font-semibold text-gray-900">
            {mode === "view"
              ? "Appointment Details"
              : mode === "edit"
                ? "Edit Appointment"
                : "Book New Appointment"}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        <div className="p-6">
          {mode !== "view" && (
            <div className="mb-6">
              <div className="flex items-center justify-between text-sm text-gray-600 mb-2">
                <span>Step {formState.step} of 5</span>
                <span>{getStepTitle()}</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                  style={{
                    width: `${(Math.max(formState.step, 0.5) / 5) * 100}%`,
                  }}
                />
              </div>
            </div>
          )}

          {formState.error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700">
              {formState.error}
            </div>
          )}

          {mode === "view" && appointment ? (
            <>
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Patient
                    </label>
                    <p className="text-gray-900">
                      {appointment.patient
                        ? `${appointment.patient.firstName} ${appointment.patient.lastName}`
                        : "Unknown"}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Doctor
                    </label>
                    <p className="text-gray-900">
                      {appointment.doctor?.firstName}{" "}
                      {appointment.doctor?.lastName}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Date & Time
                    </label>
                    <p className="text-gray-900">
                      {new Date(appointment.day).toLocaleDateString()} at{" "}
                      {new Date(appointment.day).toLocaleTimeString([], {
                        hour: "2-digit",
                        minute: "2-digit",
                      })}
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Type
                    </label>
                    <div className="flex items-center">
                      <span className="capitalize">
                        {appointment.appointmentType}
                      </span>
                    </div>
                  </div>
                </div>
                {appointment.description && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Description
                    </label>
                    <p className="text-gray-900">{appointment.description}</p>
                  </div>
                )}
              </div>

              <div className="flex justify-end space-x-3 pt-6 border-t mt-4">
                <Button
                  variant="outline"
                  className="text-red-600 border-red-200 hover:bg-red-50"
                  onClick={onCancel}
                >
                  Cancel
                </Button>
                <Button onClick={onEdit}>Edit</Button>
              </div>
            </>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Step 0: Patient Selection */}
              {formState.step === 0 && (
                <div>
                  <h3 className="text-lg font-medium mb-4">Select Patient</h3>
                  <div className="relative mb-4">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                    <input
                      type="text"
                      placeholder="Search patients by name..."
                      className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      value={formState.patientSearchTerm}
                      onChange={(e) =>
                        setFormState((prev) => ({
                          ...prev,
                          patientSearchTerm: e.target.value,
                        }))
                      }
                      autoFocus
                    />
                  </div>
                  <div className="space-y-2 max-h-60 overflow-y-auto">
                    {formState.patients.map((p) => (
                      <Card
                        key={p.id}
                        className={`cursor-pointer hover:bg-gray-50 p-3 flex justify-between items-center ${formData.patientId === p.id ? "border-blue-500 bg-blue-50" : ""}`}
                        onClick={() => handlePatientSelect(p.id || "")}
                      >
                        <div>
                          <div className="font-medium">
                            {p.firstName} {p.lastName}
                          </div>
                          <div className="text-sm text-gray-500">{p.email}</div>
                        </div>
                      </Card>
                    ))}
                    {formState.patients.length === 0 &&
                      !formState.isLoading && (
                        <div className="text-center text-gray-500 py-4">
                          {formState.patientSearchTerm
                            ? "No patients found"
                            : "Type to search..."}
                        </div>
                      )}
                  </div>
                </div>
              )}

              {/* Step 1: Specialization */}
              {formState.step === 1 && (
                <div>
                  <h3 className="text-lg font-medium mb-4">
                    Choose a specialization
                  </h3>
                  <div className="grid grid-cols-1 gap-3">
                    {formState.specializations.map((spec) => (
                      <Card
                        key={spec.id}
                        className={`cursor-pointer transition-colors hover:border-blue-500 ${formData.specializationId === spec.id ? "border-blue-500 bg-blue-50" : ""}`}
                        onClick={() => handleSpecializationSelect(spec.id)}
                      >
                        <div className="p-4">
                          <h4 className="font-medium text-gray-900">
                            {spec.name}
                          </h4>
                          {spec.description && (
                            <p className="text-sm text-gray-600 mt-1">
                              {spec.description}
                            </p>
                          )}
                        </div>
                      </Card>
                    ))}
                  </div>
                </div>
              )}

              {/* Step 2: Service */}
              {formState.step === 2 && (
                <div>
                  <h3 className="text-lg font-medium mb-4">Choose a service</h3>
                  <div className="grid grid-cols-1 gap-3">
                    {formState.services.map((service) => (
                      <Card
                        key={service.id}
                        className={`cursor-pointer transition-colors hover:border-blue-500 ${formData.serviceId === service.id ? "border-blue-500 bg-blue-50" : ""}`}
                        onClick={() => handleServiceSelect(service.id)}
                      >
                        <div className="p-4">
                          <div className="flex items-center justify-between">
                            <h4 className="font-medium text-gray-900">
                              {service.name}
                            </h4>
                            <span className="text-sm text-gray-500">
                              {service.durationMinutes} min
                            </span>
                          </div>
                        </div>
                      </Card>
                    ))}
                  </div>
                </div>
              )}

              {/* Step 3: Doctor */}
              {formState.step === 3 && (
                <div>
                  <h3 className="text-lg font-medium mb-4">Choose a doctor</h3>
                  <div className="grid grid-cols-1 gap-3">
                    {formState.doctors.map((doctor) => (
                      <Card
                        key={doctor.id}
                        className={`cursor-pointer transition-colors hover:border-blue-500 ${formData.doctorUserId === doctor.userId || formData.doctorUserId === doctor.id ? "border-blue-500 bg-blue-50" : ""}`}
                        onClick={() =>
                          handleDoctorSelect(doctor.userId || doctor.id)
                        }
                      >
                        <div className="p-4">
                          <div className="flex items-center">
                            <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center mr-4">
                              <User className="w-6 h-6 text-blue-600" />
                            </div>
                            <div className="flex-1">
                              <h4 className="font-medium text-gray-900">
                                Dr. {doctor.firstName} {doctor.lastName}
                              </h4>
                              <p className="text-sm text-gray-600">
                                {doctor.specialization?.name}
                              </p>
                            </div>
                          </div>
                        </div>
                      </Card>
                    ))}
                  </div>
                </div>
              )}

              {/* Step 4: Date & Time */}
              {formState.step === 4 && (
                <div>
                  <h3 className="text-lg font-medium mb-4">
                    Select date and time
                  </h3>
                  <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Select Date
                    </label>
                    <input
                      type="date"
                      value={selectedDate}
                      onChange={(e) => handleDateSelect(e.target.value)}
                      min={formatDateForInput(new Date().toISOString())}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  {selectedDate && formState.timeSlots.length > 0 && (
                    <div className="grid grid-cols-3 gap-2">
                      {formState.timeSlots.map((slot) => (
                        <Button
                          key={slot.id}
                          type="button"
                          variant={
                            formData.timeSlotId === slot.id
                              ? "primary"
                              : "outline"
                          }
                          size="sm"
                          onClick={() => handleTimeSlotSelect(slot.id)}
                          className="text-sm"
                        >
                          <Clock className="w-4 h-4 mr-1" />
                          {new Date(slot.startDateTime).toLocaleTimeString([], {
                            hour: "2-digit",
                            minute: "2-digit",
                          })}
                        </Button>
                      ))}
                    </div>
                  )}
                  {selectedDate &&
                    formState.timeSlots.length === 0 &&
                    !formState.isLoading && (
                      <p className="text-gray-500 text-center py-4">
                        No available time slots.
                      </p>
                    )}
                </div>
              )}

              {/* Step 5: Details */}
              {formState.step === 5 && (
                <div className="space-y-4">
                  <h3 className="text-lg font-medium mb-4">
                    Appointment details
                  </h3>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Appointment Type
                    </label>
                    <div className="grid grid-cols-3 gap-3">
                      {[
                        {
                          value: "in-person",
                          label: "In-Person",
                          icon: MapPin,
                        },
                        { value: "virtual", label: "Virtual", icon: Video },
                        { value: "phone", label: "Phone", icon: Phone },
                      ].map(({ value, label, icon: Icon }) => (
                        <Button
                          key={value}
                          type="button"
                          variant={
                            formData.appointmentType === value
                              ? "primary"
                              : "outline"
                          }
                          size="sm"
                          onClick={() =>
                            setFormData((prev) => ({
                              ...prev,
                              appointmentType: value as
                                | "in-person"
                                | "virtual"
                                | "phone",
                            }))
                          }
                          className="flex items-center justify-center"
                        >
                          <Icon className="w-4 h-4 mr-1" />
                          {label}
                        </Button>
                      ))}
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Description (Optional)
                    </label>
                    <textarea
                      value={formData.description}
                      onChange={(e) =>
                        setFormData((prev) => ({
                          ...prev,
                          description: e.target.value,
                        }))
                      }
                      rows={3}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="Describe symptoms..."
                    />
                  </div>
                </div>
              )}

              <div className="flex justify-between pt-4">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    if (formState.step > (contextPatientId ? 1 : 0)) {
                      setFormState((prev) => ({
                        ...prev,
                        step: prev.step - 1,
                      }));
                    } else {
                      onClose();
                    }
                  }}
                >
                  {formState.step > (contextPatientId ? 1 : 0)
                    ? "Back"
                    : "Cancel"}
                </Button>
                {formState.step < 5 ? (
                  <Button
                    type="button"
                    disabled={
                      (formState.step === 0 && !formData.patientId) ||
                      (formState.step === 1 && !formData.specializationId) ||
                      (formState.step === 2 && !formData.serviceId) ||
                      (formState.step === 3 && !formData.doctorUserId) ||
                      (formState.step === 4 && !formData.timeSlotId)
                    }
                    onClick={() =>
                      setFormState((prev) => ({ ...prev, step: prev.step + 1 }))
                    }
                  >
                    Next
                  </Button>
                ) : (
                  <Button type="submit" disabled={formState.isLoading}>
                    {mode === "edit"
                      ? "Update Appointment"
                      : "Book Appointment"}
                  </Button>
                )}
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};

export default AppointmentModal;
