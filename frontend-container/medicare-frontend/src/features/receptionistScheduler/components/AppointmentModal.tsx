import React, { useCallback, useEffect, useState } from "react";
import {
  Calendar,
  Clock,
  MapPin,
  Phone,
  Plus,
  Save,
  Trash2,
  User,
  Video,
} from "lucide-react";

import { Button, Input, Modal, SearchInput } from "../../../shared/components";
import { useDoctors, usePatients } from "../hooks/index";
import { ReceptionistSchedulerApiService } from "../services/receptionistSchedulerApiService";
import type {
  AppointmentModalProps,
  CreateAppointmentRequest,
  Doctor,
  Patient,
  TimeSlot,
  UpdateAppointmentRequest,
} from "../types";

export const AppointmentModal: React.FC<AppointmentModalProps> = ({
  isOpen,
  mode,
  appointment,
  selectedDate,
  onClose,
  onCreateSubmit,
  onUpdateSubmit,
  onCancelAppointment,
  onEdit,
}) => {
  const { patients: _patients, searchPatients } = usePatients();
  const { doctors } = useDoctors();

  const [formData, setFormData] = useState({
    patientId: "",
    doctorId: "",
    day: "",
    time: "",
    duration: 30,
    appointmentType: "in-person" as "in-person" | "video-call" | "phone",
    appointmentCategory: "consultation" as
      | "consultation"
      | "emergency"
      | "follow-up"
      | "procedure"
      | "surgery"
      | "check-up"
      | "vaccination",
    room: "",
    description: "",
  });

  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [selectedDoctor, setSelectedDoctor] = useState<Doctor | null>(null);
  const [patientSearchResults, setPatientSearchResults] = useState<Patient[]>(
    []
  );
  const [showPatientDropdown, setShowPatientDropdown] = useState(false);
  const [patientSearchTerm, setPatientSearchTerm] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Time slots state
  const [availableTimeSlots, setAvailableTimeSlots] = useState<TimeSlot[]>([]);
  const [isLoadingTimeSlots, setIsLoadingTimeSlots] = useState(false);
  const [showAddPatientButton, setShowAddPatientButton] = useState(false);

  // Initialize form data when appointment changes
  useEffect(() => {
    if (appointment && (mode === "edit" || mode === "view")) {
      setFormData({
        patientId: appointment.patientId,
        doctorId: appointment.doctorId,
        day: appointment.day,
        time: appointment.time,
        duration: appointment.duration,
        appointmentType: appointment.appointmentType,
        appointmentCategory: appointment.appointmentCategory || "consultation",
        room: appointment.room || "",
        description: appointment.description || "",
      });
      setSelectedPatient(appointment.patient || null);
      setSelectedDoctor(appointment.doctor || null);
      setPatientSearchTerm(
        appointment.patient
          ? `${appointment.patient.firstName} ${appointment.patient.lastName}`
          : ""
      );
    } else if (mode === "create") {
      const today = new Date();
      const defaultDate =
        selectedDate ||
        (() => {
          const tomorrow = new Date(today);
          tomorrow.setDate(today.getDate() + 1);
          return tomorrow.toISOString().split("T")[0];
        })();

      setFormData({
        patientId: "",
        doctorId: "",
        day: defaultDate,
        time: "09:00",
        duration: 30,
        appointmentType: "in-person",
        appointmentCategory: "consultation",
        room: "",
        description: "",
      });
      setSelectedPatient(null);
      setSelectedDoctor(null);
      setPatientSearchTerm("");
    }
  }, [appointment, mode, selectedDate]);

  // Search patients
  const handlePatientSearch = useCallback(
    async (e: React.ChangeEvent<HTMLInputElement>) => {
      const query = e.target.value;
      setPatientSearchTerm(query);
      if (query.trim().length > 2) {
        const results = await searchPatients(query);
        setPatientSearchResults(results);
        setShowPatientDropdown(true);
        setShowAddPatientButton(results.length === 0); // Show "Add Patient" if no results found
      } else {
        setPatientSearchResults([]);
        setShowPatientDropdown(false);
        setShowAddPatientButton(false);
      }
    },
    [searchPatients]
  );

  const loadTimeSlots = useCallback(async (doctorId: string, date: string) => {
    setIsLoadingTimeSlots(true);
    try {
      const slots = await ReceptionistSchedulerApiService.getDoctorAvailability(
        doctorId,
        date
      );
      setAvailableTimeSlots(slots);
    } catch (error) {
      console.error("Failed to load time slots:", error);
      setAvailableTimeSlots([]);
    } finally {
      setIsLoadingTimeSlots(false);
    }
  }, []);

  const handlePatientSelect = useCallback((patient: Patient) => {
    setSelectedPatient(patient);
    setFormData((prev) => ({ ...prev, patientId: patient.id }));
    setPatientSearchTerm(`${patient.firstName} ${patient.lastName}`);
    setShowPatientDropdown(false);
  }, []);

  const handleDoctorSelect = useCallback(
    (doctorId: string) => {
      const doctor = doctors.find((d: Doctor) => d.id === doctorId);
      setSelectedDoctor(doctor || null);
      setFormData((prev) => ({ ...prev, doctorId }));

      // Load time slots if both doctor and date are selected
      if (doctorId && formData.day) {
        loadTimeSlots(doctorId, formData.day);
      }
    },
    [doctors, formData.day, loadTimeSlots]
  );

  const handleDateChange = useCallback(
    (date: string) => {
      setFormData((prev) => ({ ...prev, day: date }));

      // Load time slots if both doctor and date are selected
      if (formData.doctorId && date) {
        loadTimeSlots(formData.doctorId, date);
      }
    },
    [formData.doctorId, loadTimeSlots]
  );

  const handleTimeSlotSelect = useCallback((slot: TimeSlot) => {
    const startTime = new Date(slot.startDateTime).toLocaleTimeString("en-US", {
      hour12: false,
      hour: "2-digit",
      minute: "2-digit",
    });
    setFormData((prev) => ({ ...prev, time: startTime }));
  }, []);

  const handleAddPatient = useCallback(() => {
    // TODO: This will be implemented later - open patient registration modal
    alert(
      "Add Patient functionality will be implemented next. Search term: " +
        patientSearchTerm
    );
  }, [patientSearchTerm]);

  const handleSubmit = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();
      if (isSubmitting) return;

      try {
        setIsSubmitting(true);

        if (mode === "create") {
          const createData: CreateAppointmentRequest = {
            patientId: formData.patientId,
            doctorId: formData.doctorId,
            day: formData.day,
            time: formData.time,
            duration: formData.duration,
            appointmentType: formData.appointmentType,
            appointmentCategory: formData.appointmentCategory,
            ...(formData.room && { room: formData.room }),
            ...(formData.description && { description: formData.description }),
          };
          await onCreateSubmit(createData);
        } else if (mode === "edit" && appointment) {
          const updateData: UpdateAppointmentRequest = {
            id: appointment.id,
            day: formData.day,
            time: formData.time,
            duration: formData.duration,
            appointmentType: formData.appointmentType,
            appointmentCategory: formData.appointmentCategory,
            ...(formData.room && { room: formData.room }),
            ...(formData.description && { description: formData.description }),
          };
          await onUpdateSubmit(updateData);
        }

        onClose();
      } catch (error) {
        console.error("Error saving appointment:", error);
      } finally {
        setIsSubmitting(false);
      }
    },
    [
      formData,
      mode,
      appointment,
      onCreateSubmit,
      onUpdateSubmit,
      onClose,
      isSubmitting,
    ]
  );

  const handleCancel = useCallback(async () => {
    if (appointment && onCancelAppointment) {
      if (window.confirm("Are you sure you want to cancel this appointment?")) {
        try {
          await onCancelAppointment(appointment.id);
          onClose();
        } catch (error) {
          console.error("Error cancelling appointment:", error);
        }
      }
    }
  }, [appointment, onCancelAppointment, onClose]);

  const getModalTitle = () => {
    switch (mode) {
      case "create":
        return "Schedule New Appointment";
      case "edit":
        return "Edit Appointment";
      case "view":
        return "Appointment Details";
      default:
        return "Appointment";
    }
  };

  const getAppointmentTypeIcon = (type: string) => {
    switch (type) {
      case "video-call":
        return <Video size={16} className="mr-2" />;
      case "phone":
        return <Phone size={16} className="mr-2" />;
      case "in-person":
        return <MapPin size={16} className="mr-2" />;
      default:
        return <User size={16} className="mr-2" />;
    }
  };

  const isReadOnly = mode === "view";

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <div className="p-6">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-semibold text-blue-700 flex items-center">
            <Calendar className="w-5 h-5 mr-2" />
            {getModalTitle()}
          </h2>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Patient Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Patient *
            </label>
            {isReadOnly ? (
              <div className="p-3 bg-gray-50 rounded-lg border">
                <div className="flex items-center">
                  <User size={16} className="mr-2 text-gray-500" />
                  <span className="font-medium">
                    {selectedPatient
                      ? `${selectedPatient.firstName} ${selectedPatient.lastName}`
                      : "Unknown Patient"}
                  </span>
                </div>
                {selectedPatient && (
                  <div className="mt-1 text-sm text-gray-600">
                    {selectedPatient.email} • {selectedPatient.phone}
                  </div>
                )}
              </div>
            ) : (
              <div className="relative">
                <SearchInput
                  value={patientSearchTerm}
                  onChange={handlePatientSearch}
                  placeholder="Search for patient by name, email, or MRN..."
                  className="w-full"
                />
                {(showPatientDropdown || showAddPatientButton) && (
                  <div className="absolute z-10 w-full mt-1 bg-white border rounded-lg shadow-lg max-h-60 overflow-y-auto">
                    {patientSearchResults.map((patient) => (
                      <button
                        key={patient.id}
                        type="button"
                        onClick={() => handlePatientSelect(patient)}
                        className="w-full p-3 text-left hover:bg-gray-50 border-b border-gray-100 last:border-b-0"
                      >
                        <div className="font-medium">
                          {patient.firstName} {patient.lastName}
                        </div>
                        <div className="text-sm text-gray-600">
                          {patient.email} • {patient.medicalRecordNumber}
                        </div>
                      </button>
                    ))}
                    {showAddPatientButton && (
                      <button
                        type="button"
                        onClick={handleAddPatient}
                        className="w-full p-3 text-left hover:bg-blue-50 border-b border-gray-100 last:border-b-0 text-blue-600 font-medium"
                      >
                        <div className="flex items-center">
                          <Plus size={16} className="mr-2" />
                          Add new patient "{patientSearchTerm}"
                        </div>
                        <div className="text-sm text-blue-500">
                          Click to add this patient to the system
                        </div>
                      </button>
                    )}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Doctor Selection */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Doctor *
            </label>
            {isReadOnly ? (
              <div className="p-3 bg-gray-50 rounded-lg border">
                <div className="flex items-center">
                  <User size={16} className="mr-2 text-gray-500" />
                  <span className="font-medium">
                    {selectedDoctor
                      ? `Dr. ${selectedDoctor.firstName} ${selectedDoctor.lastName}`
                      : "Unknown Doctor"}
                  </span>
                </div>
                {selectedDoctor && selectedDoctor.specializations[0] && (
                  <div className="mt-1 text-sm text-gray-600">
                    {selectedDoctor.specializations[0].name}
                  </div>
                )}
              </div>
            ) : (
              <select
                value={formData.doctorId}
                onChange={(e) => handleDoctorSelect(e.target.value)}
                className="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                required
              >
                <option value="">Select a doctor...</option>
                {doctors.map((doctor: Doctor) => (
                  <option key={doctor.id} value={doctor.id}>
                    Dr. {doctor.firstName} {doctor.lastName} -{" "}
                    {doctor.specializations[0]?.name || "General"}
                  </option>
                ))}
              </select>
            )}
          </div>

          {/* Date and Time Slots */}
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Date *
              </label>
              <Input
                type="date"
                value={formData.day}
                onChange={(e) => handleDateChange(e.target.value)}
                readOnly={isReadOnly}
                required
              />
            </div>

            {/* Time Slot Selection */}
            {formData.day && formData.doctorId && !isReadOnly && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Available Time Slots *
                </label>
                {isLoadingTimeSlots ? (
                  <div className="flex items-center justify-center p-4 border rounded-lg">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
                    <span className="ml-2 text-gray-600">
                      Loading available slots...
                    </span>
                  </div>
                ) : availableTimeSlots.length > 0 ? (
                  <div className="grid grid-cols-4 sm:grid-cols-6 lg:grid-cols-8 gap-2 max-h-48 overflow-y-auto border rounded-lg p-3">
                    {availableTimeSlots.map((slot) => {
                      const startTime = new Date(
                        slot.startDateTime
                      ).toLocaleTimeString("en-US", {
                        hour12: false,
                        hour: "2-digit",
                        minute: "2-digit",
                      });
                      const isSelected = formData.time === startTime;

                      return (
                        <button
                          key={slot.id}
                          type="button"
                          disabled={!slot.isAvailable}
                          onClick={() => handleTimeSlotSelect(slot)}
                          className={`
                            p-2 text-xs font-medium rounded border transition-colors
                            ${
                              isSelected
                                ? "bg-blue-600 text-white border-blue-600"
                                : slot.isAvailable
                                  ? "bg-white text-gray-700 border-gray-300 hover:bg-blue-50 hover:border-blue-300"
                                  : "bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed"
                            }
                          `}
                        >
                          <Clock size={12} className="mx-auto mb-1" />
                          {startTime}
                        </button>
                      );
                    })}
                  </div>
                ) : (
                  <div className="p-4 border rounded-lg text-center text-gray-500">
                    No available time slots for the selected date.
                  </div>
                )}
              </div>
            )}

            {/* Show selected time for read-only mode */}
            {isReadOnly && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Time
                </label>
                <div className="p-3 bg-gray-50 rounded-lg border">
                  <div className="flex items-center">
                    <Clock size={16} className="mr-2 text-gray-500" />
                    <span className="font-medium">{formData.time}</span>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Duration, Category and Type */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Duration (minutes) *
              </label>
              <select
                value={formData.duration}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    duration: parseInt(e.target.value),
                  }))
                }
                className="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                disabled={isReadOnly}
                required
              >
                <option value={15}>15 minutes</option>
                <option value={30}>30 minutes</option>
                <option value={45}>45 minutes</option>
                <option value={60}>1 hour</option>
                <option value={90}>1.5 hours</option>
                <option value={120}>2 hours</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Visit Category *
              </label>
              {isReadOnly ? (
                <div className="p-3 bg-gray-50 rounded-lg border">
                  <span className="capitalize">
                    {formData.appointmentCategory.replace("-", " ")}
                  </span>
                </div>
              ) : (
                <select
                  value={formData.appointmentCategory}
                  onChange={(e) => {
                    const category = e.target
                      .value as typeof formData.appointmentCategory;
                    // Set default duration based on category
                    const defaultDurations = {
                      consultation: 30,
                      emergency: 45,
                      "follow-up": 20,
                      procedure: 60,
                      surgery: 120,
                      "check-up": 30,
                      vaccination: 15,
                    };
                    setFormData((prev) => ({
                      ...prev,
                      appointmentCategory: category,
                      duration: defaultDurations[category] || 30,
                    }));
                  }}
                  className="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                >
                  <option value="consultation">Consultation</option>
                  <option value="emergency">Emergency</option>
                  <option value="follow-up">Follow-up</option>
                  <option value="procedure">Procedure</option>
                  <option value="surgery">Surgery</option>
                  <option value="check-up">Check-up</option>
                  <option value="vaccination">Vaccination</option>
                </select>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Appointment Type *
              </label>
              {isReadOnly ? (
                <div className="p-3 bg-gray-50 rounded-lg border flex items-center">
                  {getAppointmentTypeIcon(formData.appointmentType)}
                  <span className="capitalize">
                    {formData.appointmentType.replace("-", " ")}
                  </span>
                </div>
              ) : (
                <select
                  value={formData.appointmentType}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      appointmentType: e.target.value as
                        | "in-person"
                        | "video-call"
                        | "phone",
                    }))
                  }
                  className="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  required
                >
                  <option value="in-person">In-Person</option>
                  <option value="video-call">Video Call</option>
                  <option value="phone">Phone Call</option>
                </select>
              )}
            </div>
          </div>

          {/* Room (for in-person appointments) */}
          {formData.appointmentType === "in-person" && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Room
              </label>
              <Input
                value={formData.room}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, room: e.target.value }))
                }
                placeholder="e.g., Room 101, Consultation Room A"
                readOnly={isReadOnly}
              />
            </div>
          )}

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Description / Notes
            </label>
            <textarea
              value={formData.description}
              onChange={(e) =>
                setFormData((prev) => ({
                  ...prev,
                  description: e.target.value,
                }))
              }
              placeholder="Additional notes or reason for appointment..."
              className="w-full p-3 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 h-20 resize-none"
              readOnly={isReadOnly}
            />
          </div>

          {/* Action Buttons */}
          <div className="flex justify-between pt-4 border-t">
            <div>
              {mode === "edit" && (
                <Button
                  type="button"
                  variant="danger"
                  onClick={handleCancel}
                  className="flex items-center"
                >
                  <Trash2 size={16} className="mr-2" />
                  Cancel Appointment
                </Button>
              )}
            </div>

            <div className="flex space-x-3">
              <Button type="button" variant="outline" onClick={onClose}>
                {isReadOnly ? "Close" : "Cancel"}
              </Button>

              {isReadOnly && (
                <Button
                  type="button"
                  onClick={onEdit}
                  className="flex items-center"
                >
                  Edit Appointment
                </Button>
              )}

              {!isReadOnly && (
                <Button
                  type="submit"
                  disabled={
                    isSubmitting || !formData.patientId || !formData.doctorId
                  }
                  className="flex items-center"
                >
                  {isSubmitting ? (
                    <Clock size={16} className="mr-2 animate-spin" />
                  ) : (
                    <Save size={16} className="mr-2" />
                  )}
                  {mode === "create" ? "Schedule Appointment" : "Save Changes"}
                </Button>
              )}
            </div>
          </div>
        </form>
      </div>
    </Modal>
  );
};
