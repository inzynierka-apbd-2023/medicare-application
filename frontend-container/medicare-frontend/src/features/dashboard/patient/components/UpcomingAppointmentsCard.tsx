import { Button, Card } from "@shared/components";
import { Calendar, Clock, MapPin, Phone, User } from "lucide-react";

interface Appointment {
  id: string;
  doctorName: string;
  specialty: string;
  date: Date;
  time: string;
  type: "in-person" | "phone";
  location?: string;
  status: "upcoming" | "completed" | "cancelled";
}

interface UpcomingAppointmentsCardProps {
  appointments?: Appointment[];
  onBookNew?: () => void;
  onViewAll?: () => void;
}

const getAppointmentTypeIcon = (type: Appointment["type"]) => {
  switch (type) {
    case "phone":
      return <Phone className="w-4 h-4" />;
    case "in-person":
      return <MapPin className="w-4 h-4" />;
    default:
      return <Calendar className="w-4 h-4" />;
  }
};

const getStatusColor = (status: Appointment["status"]) => {
  switch (status) {
    case "upcoming":
      return "text-blue-600 bg-blue-50 border-blue-200";
    case "completed":
      return "text-green-600 bg-green-50 border-green-200";
    case "cancelled":
      return "text-red-600 bg-red-50 border-red-200";
    default:
      return "text-gray-600 bg-gray-50 border-gray-200";
  }
};

export default function UpcomingAppointmentsCard({
  appointments = [],
  onBookNew,
  onViewAll,
}: UpcomingAppointmentsCardProps) {
  const upcomingAppointments = appointments
    .filter((apt) => apt.status === "upcoming")
    .sort((a, b) => a.date.getTime() - b.date.getTime())
    .slice(0, 3);

  return (
    <Card variant="medical" padding="md">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-blue-600 flex items-center gap-2">
          <Calendar className="w-5 h-5" />
          Upcoming Appointments
        </h3>
        <div className="flex gap-2">
          {onBookNew && (
            <Button
              variant="primary"
              size="sm"
              onClick={onBookNew}
              className="flex items-center gap-1"
            >
              Book New
            </Button>
          )}
        </div>
      </div>

      {upcomingAppointments.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          <Calendar className="w-12 h-12 mx-auto mb-2 text-gray-300" />
          <p className="mb-4">No upcoming appointments scheduled.</p>
          {onBookNew && (
            <Button variant="primary" size="sm" onClick={onBookNew}>
              Schedule Your First Appointment
            </Button>
          )}
        </div>
      ) : (
        <>
          <div className="space-y-3">
            {upcomingAppointments.map((appointment) => (
              <div
                key={appointment.id}
                className={`p-4 rounded-lg border-l-4 ${getStatusColor(appointment.status)} hover:shadow-md transition-shadow`}
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <User className="w-4 h-4 text-gray-600" />
                      <p className="font-semibold text-gray-900">
                        Dr. {appointment.doctorName}
                      </p>
                      <span className="text-sm text-gray-500">
                        • {appointment.specialty}
                      </span>
                    </div>

                    <div className="flex items-center gap-3 text-sm text-gray-600">
                      <div className="flex items-center gap-1">
                        <Calendar className="w-4 h-4" />
                        <span>{appointment.date.toLocaleDateString()}</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Clock className="w-4 h-4" />
                        <span>{appointment.time}</span>
                      </div>
                      <div className="flex items-center gap-1">
                        {getAppointmentTypeIcon(appointment.type)}
                        <span className="capitalize">
                          {appointment.type.replace("-", " ")}
                        </span>
                      </div>
                    </div>

                    {appointment.location &&
                      appointment.type === "in-person" && (
                        <div className="flex items-center gap-1 mt-1 text-sm text-gray-500">
                          <MapPin className="w-4 h-4" />
                          <span>{appointment.location}</span>
                        </div>
                      )}
                  </div>

                  <div className="flex flex-col items-end gap-2">
                    <span
                      className={`px-2 py-1 text-xs rounded-full capitalize ${getStatusColor(appointment.status).replace("border-", "border ")}`}
                    >
                      {appointment.status}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {appointments.length > 3 && onViewAll && (
            <div className="mt-4 text-center">
              <Button variant="outline" size="sm" onClick={onViewAll}>
                View All Appointments ({appointments.length})
              </Button>
            </div>
          )}
        </>
      )}
    </Card>
  );
}
