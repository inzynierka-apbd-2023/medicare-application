import React, { useEffect, useState } from "react";
import {
  Activity,
  BarChart3,
  Clock,
  DollarSign,
  FileText,
  Star,
  TrendingDown,
  TrendingUp,
  UserCheck,
  Users,
} from "lucide-react";

import Header from "../../../layout/Header";
import {
  Button,
  Card,
  ErrorDisplay,
  LoadingOverlay,
} from "../../../shared/components";
import { useLoadingService } from "../../../shared/hooks/useLoadingService";
import { analyticsApi } from "../../../shared/services/analyticsApi";
import {
  appointmentMetricsApi,
  AppointmentMetricsResponse as ApptMetrics,
} from "../../../shared/services/appointmentMetricsApi";
import {
  doctorPerformanceApi,
  DoctorPerformanceSummaryResponse,
} from "../../../shared/services/doctorPerformanceApi";
import {
  patientMetricsApi,
  PatientMetricsResponse,
} from "../../../shared/services/patientMetricsApi";
import { revenueMetricsApi } from "../../../shared/services/revenueMetricsApi";
import { DashboardLayout } from "../shared/components";

// Database-aligned types based on your actual schema
interface RevenueMetrics {
  dailyRevenue: number;
  monthlyRevenue: number;
  yearlyRevenue: number;
  revenueGrowth: number;
  totalAppointmentPayments: number;
  totalSubscriptionPayments: number;
}

interface PatientMetrics {
  totalActivePatients: number;
  newPatientsThisMonth: number;
  patientRetentionRate: number;
  averageRating: number;
  totalRatings: number;
}

interface AppointmentMetrics {
  totalAppointments: number;
  appointmentsThisMonth: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowAppointments: number;
  appointmentCompletionRate: number;
}

interface DoctorMetrics {
  totalDoctors: number;
  averageAppointmentsPerDoctor: number;
  topRatedDoctor: string;
  doctorAverageRating: number;
}

interface OwnerDashboardData {
  revenue: RevenueMetrics;
  patients: PatientMetrics;
  appointments: AppointmentMetrics;
  doctors: DoctorMetrics;
  recentActivities: ActivityLog[];
}

interface ActivityLog {
  id: string;
  type: "payment" | "appointment" | "patient" | "rating";
  description: string;
  timestamp: Date;
}

const OwnerDashboard: React.FC = () => {
  const { isLoading, error, executeInitialLoad } = useLoadingService();
  const [dashboardData, setDashboardData] = useState<OwnerDashboardData | null>(
    null
  );

  useEffect(() => {
    const fetchOwnerDashboardData = async () => {
      try {
        const startDate = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)
          .toISOString()
          .split("T")[0];
        const endDate = new Date().toISOString().split("T")[0];

        const [
          analyticsResponse,
          patientMetricsResponse,
          appointmentMetricsResponse,
          doctorPerformanceSummaryResponse,
          dailyRevenueResponse,
          monthlyRevenueResponse,
          yearlyRevenueResponse,
        ] = await Promise.all([
          analyticsApi.getDashboardData({ startDate, endDate }),
          patientMetricsApi.getPatientMetrics({ startDate, endDate }),
          appointmentMetricsApi.getAppointmentMetrics({ startDate, endDate }),
          doctorPerformanceApi.getSummary({ startDate, endDate }),
          revenueMetricsApi.getDailyRevenue(endDate),
          revenueMetricsApi.getMonthlyRevenue(
            new Date().getFullYear(),
            new Date().getMonth() + 1
          ),
          revenueMetricsApi.getYearlyRevenue(new Date().getFullYear()),
        ]);

        if (!analyticsResponse.success)
          throw new Error(
            analyticsResponse.error || "Failed to fetch analytics data"
          );
        if (!patientMetricsResponse.success)
          throw new Error(
            patientMetricsResponse.error || "Failed to fetch patient metrics"
          );
        if (!appointmentMetricsResponse.success)
          throw new Error(
            appointmentMetricsResponse.error ||
              "Failed to fetch appointment metrics"
          );
        if (!doctorPerformanceSummaryResponse.success)
          throw new Error(
            doctorPerformanceSummaryResponse.error ||
              "Failed to fetch doctor performance summary"
          );

        const patientMetrics: PatientMetricsResponse =
          patientMetricsResponse.data!;
        const apptMetrics: ApptMetrics = appointmentMetricsResponse.data!;
        const doctorPerfSummary: DoctorPerformanceSummaryResponse =
          doctorPerformanceSummaryResponse.data!;
        const dailyRevenue = dailyRevenueResponse;
        const monthlyRevenue = monthlyRevenueResponse;
        const yearlyRevenue = yearlyRevenueResponse;

        const transformedData: OwnerDashboardData = {
          revenue: {
            dailyRevenue: dailyRevenue.totalRevenue,
            monthlyRevenue: monthlyRevenue.totalRevenue,
            yearlyRevenue: yearlyRevenue.totalRevenue,
            revenueGrowth: monthlyRevenue.growthPercentage ?? 0,
            totalAppointmentPayments: monthlyRevenue.totalRevenue,
            totalSubscriptionPayments: 0,
          },
          patients: {
            totalActivePatients: patientMetrics.totalActivePatients,
            newPatientsThisMonth: patientMetrics.newPatients,
            patientRetentionRate: patientMetrics.retentionRate,
            averageRating: patientMetrics.averageRating,
            totalRatings: patientMetrics.totalRatings,
          },
          appointments: {
            totalAppointments: apptMetrics.totalAppointments,
            appointmentsThisMonth: apptMetrics.appointmentsThisMonth,
            completedAppointments: apptMetrics.completedAppointments,
            cancelledAppointments: apptMetrics.cancelledAppointments,
            noShowAppointments: apptMetrics.noShowAppointments,
            appointmentCompletionRate: apptMetrics.completionRate,
          },
          doctors: {
            totalDoctors: doctorPerfSummary.totalDoctors,
            averageAppointmentsPerDoctor:
              doctorPerfSummary.averageAppointmentsPerDoctor,
            topRatedDoctor: doctorPerfSummary.topRatedDoctor || "No data",
            doctorAverageRating: doctorPerfSummary.doctorAverageRating,
          },
          recentActivities: [
            {
              id: "1",
              type: "payment",
              description: `Received $${monthlyRevenue.totalRevenue.toLocaleString()} in monthly revenue`,
              timestamp: new Date(),
            },
            {
              id: "2",
              type: "appointment",
              description: `${apptMetrics.completedAppointments} appointments completed`,
              timestamp: new Date(),
            },
            {
              id: "3",
              type: "patient",
              description: `${patientMetrics.totalActivePatients} active patients in system`,
              timestamp: new Date(),
            },
            {
              id: "4",
              type: "rating",
              description: `Average rating: ${patientMetrics.averageRating.toFixed(1)}/5`,
              timestamp: new Date(),
            },
          ],
        };
        setDashboardData(transformedData);
      } catch (error) {
        console.error("Failed to fetch owner dashboard data:", error);
        setDashboardData({
          revenue: {
            dailyRevenue: 0,
            monthlyRevenue: 0,
            yearlyRevenue: 0,
            revenueGrowth: 0,
            totalAppointmentPayments: 0,
            totalSubscriptionPayments: 0,
          },
          patients: {
            totalActivePatients: 0,
            newPatientsThisMonth: 0,
            patientRetentionRate: 0,
            averageRating: 0,
            totalRatings: 0,
          },
          appointments: {
            totalAppointments: 0,
            appointmentsThisMonth: 0,
            completedAppointments: 0,
            cancelledAppointments: 0,
            noShowAppointments: 0,
            appointmentCompletionRate: 0,
          },
          doctors: {
            totalDoctors: 0,
            averageAppointmentsPerDoctor: 0,
            topRatedDoctor: "No data",
            doctorAverageRating: 0,
          },
          recentActivities: [],
        });
      }
    };

    executeInitialLoad(fetchOwnerDashboardData);
  }, [executeInitialLoad]);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  const formatPercentage = (value: number | undefined | null) => {
    if (value === undefined || value === null) return "0.0%";
    return `${value >= 0 ? "+" : ""}${value.toFixed(1)}%`;
  };

  const getGrowthIcon = (growth: number) => {
    return growth >= 0 ? (
      <TrendingUp className="w-4 h-4 text-green-500" />
    ) : (
      <TrendingDown className="w-4 h-4 text-red-500" />
    );
  };

  const getGrowthColor = (growth: number) => {
    return growth >= 0 ? "text-green-600" : "text-red-600";
  };

  const getActivityIcon = (type: ActivityLog["type"]) => {
    switch (type) {
      case "payment":
        return <DollarSign className="w-4 h-4 text-green-500" />;
      case "appointment":
        return <Clock className="w-4 h-4 text-blue-500" />;
      case "patient":
        return <Users className="w-4 h-4 text-purple-500" />;
      case "rating":
        return <Star className="w-4 h-4 text-yellow-500" />;
      default:
        return <Activity className="w-4 h-4 text-gray-500" />;
    }
  };

  if (error) {
    return <ErrorDisplay message={error} />;
  }

  if (isLoading || !dashboardData) {
    return (
      <LoadingOverlay isLoading={true}>
        <div></div>
      </LoadingOverlay>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <DashboardLayout title="Owner Dashboard">
        <div className="space-y-6">
          {/* Header Section */}
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">
                Owner Dashboard
              </h1>
              <p className="text-gray-600 mt-1">
                Overview of your medical practice performance
              </p>
            </div>
            <div className="flex gap-3">
              <Button
                variant="outline"
                leftIcon={<BarChart3 className="w-4 h-4" />}
              >
                View Reports
              </Button>
              <Button
                variant="primary"
                leftIcon={<FileText className="w-4 h-4" />}
              >
                Export Data
              </Button>
            </div>
          </div>

          {/* Revenue Overview Row */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <Card variant="elevated" className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">
                    Daily Revenue
                  </p>
                  <p className="text-2xl font-bold text-gray-900">
                    {formatCurrency(dashboardData.revenue.dailyRevenue)}
                  </p>
                </div>
                <DollarSign className="w-8 h-8 text-green-500" />
              </div>
            </Card>

            <Card variant="elevated" className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">
                    Monthly Revenue
                  </p>
                  <p className="text-2xl font-bold text-gray-900">
                    {formatCurrency(dashboardData.revenue.monthlyRevenue)}
                  </p>
                  <div
                    className={`flex items-center gap-1 mt-1 ${getGrowthColor(dashboardData.revenue.revenueGrowth)}`}
                  >
                    {getGrowthIcon(dashboardData.revenue.revenueGrowth)}
                    <span className="text-sm font-medium">
                      {formatPercentage(dashboardData.revenue.revenueGrowth)}
                    </span>
                  </div>
                </div>
                <TrendingUp className="w-8 h-8 text-blue-500" />
              </div>
            </Card>

            <Card variant="elevated" className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">
                    Yearly Revenue
                  </p>
                  <p className="text-2xl font-bold text-gray-900">
                    {formatCurrency(dashboardData.revenue.yearlyRevenue)}
                  </p>
                </div>
                <BarChart3 className="w-8 h-8 text-purple-500" />
              </div>
            </Card>

            <Card variant="elevated" className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">
                    Payment Types
                  </p>
                  <p className="text-sm text-gray-500">
                    Appointments:{" "}
                    {formatCurrency(
                      dashboardData.revenue.totalAppointmentPayments
                    )}
                  </p>
                  <p className="text-sm text-gray-500">
                    Subscriptions:{" "}
                    {formatCurrency(
                      dashboardData.revenue.totalSubscriptionPayments
                    )}
                  </p>
                </div>
                <FileText className="w-8 h-8 text-orange-500" />
              </div>
            </Card>
          </div>

          {/* Main Metrics Row */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Patient Metrics */}
            <Card variant="elevated" className="p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900">
                  Patient Metrics
                </h3>
                <Users className="w-6 h-6 text-blue-500" />
              </div>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">
                    Total Active Patients
                  </span>
                  <span className="text-xl font-bold text-gray-900">
                    {dashboardData.patients.totalActivePatients.toLocaleString()}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">New This Month</span>
                  <span className="text-lg font-semibold text-green-600">
                    +{dashboardData.patients.newPatientsThisMonth}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Retention Rate</span>
                  <span className="text-lg font-semibold text-blue-600">
                    {formatPercentage(
                      dashboardData.patients.patientRetentionRate
                    )}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Average Rating</span>
                  <div className="flex items-center gap-1">
                    <Star className="w-4 h-4 text-yellow-500 fill-current" />
                    <span className="text-lg font-semibold text-gray-900">
                      {dashboardData.patients.averageRating}/5.0
                    </span>
                    <span className="text-xs text-gray-500">
                      ({dashboardData.patients.totalRatings})
                    </span>
                  </div>
                </div>
              </div>
            </Card>

            {/* Appointment Metrics */}
            <Card variant="elevated" className="p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900">
                  Appointments
                </h3>
                <Clock className="w-6 h-6 text-green-500" />
              </div>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">
                    Total Appointments
                  </span>
                  <span className="text-xl font-bold text-gray-900">
                    {dashboardData.appointments.totalAppointments.toLocaleString()}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">This Month</span>
                  <span className="text-lg font-semibold text-blue-600">
                    {dashboardData.appointments.appointmentsThisMonth}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Completion Rate</span>
                  <span className="text-lg font-semibold text-green-600">
                    {formatPercentage(
                      dashboardData.appointments.appointmentCompletionRate
                    )}
                  </span>
                </div>
                <div className="text-xs text-gray-500 pt-2 border-t">
                  <div className="flex justify-between">
                    <span>
                      Completed:{" "}
                      {dashboardData.appointments.completedAppointments}
                    </span>
                    <span>
                      Cancelled:{" "}
                      {dashboardData.appointments.cancelledAppointments}
                    </span>
                  </div>
                  <div className="flex justify-between mt-1">
                    <span>
                      No-shows: {dashboardData.appointments.noShowAppointments}
                    </span>
                  </div>
                </div>
              </div>
            </Card>

            {/* Doctor Metrics */}
            <Card variant="elevated" className="p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900">
                  Doctor Performance
                </h3>
                <UserCheck className="w-6 h-6 text-purple-500" />
              </div>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">Total Doctors</span>
                  <span className="text-xl font-bold text-gray-900">
                    {dashboardData.doctors.totalDoctors}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">
                    Avg Appointments/Doctor
                  </span>
                  <span className="text-lg font-semibold text-blue-600">
                    {dashboardData.doctors.averageAppointmentsPerDoctor}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">
                    Top Rated Doctor
                  </span>
                  <span className="text-sm font-medium text-gray-900">
                    {dashboardData.doctors.topRatedDoctor}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-600">
                    Doctor Avg Rating
                  </span>
                  <div className="flex items-center gap-1">
                    <Star className="w-4 h-4 text-yellow-500 fill-current" />
                    <span className="text-lg font-semibold text-gray-900">
                      {dashboardData.doctors.doctorAverageRating}/5.0
                    </span>
                  </div>
                </div>
              </div>
            </Card>
          </div>

          {/* Recent Activities */}
          <Card variant="elevated" className="p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">
                Recent Activities
              </h3>
              <Activity className="w-6 h-6 text-gray-500" />
            </div>
            <div className="space-y-3">
              {dashboardData.recentActivities.map((activity) => (
                <div
                  key={activity.id}
                  className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg"
                >
                  {getActivityIcon(activity.type)}
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900">
                      {activity.description}
                    </p>
                    <p className="text-xs text-gray-500">
                      {activity.timestamp.toLocaleString()}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </DashboardLayout>
    </div>
  );
};

export default OwnerDashboard;
