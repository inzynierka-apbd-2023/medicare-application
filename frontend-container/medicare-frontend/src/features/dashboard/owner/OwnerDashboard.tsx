import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Activity,
  AlertTriangle,
  BarChart3,
  Clock,
  DollarSign,
  FileText,
  PieChart,
  Settings,
  Shield,
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
import { DashboardLayout } from "../shared/components";

// Types for Owner Dashboard Data
interface FinancialMetrics {
  dailyRevenue: number;
  monthlyRevenue: number;
  yearlyRevenue: number;
  monthlyExpenses: number;
  profitMargin: number;
  revenueGrowth: number;
  expenseGrowth: number;
}

interface PatientMetrics {
  totalPatients: number;
  newPatientsThisMonth: number;
  newPatientsLastMonth: number;
  patientRetentionRate: number;
  averagePatientValue: number;
  patientSatisfactionScore: number;
}

interface StaffMetrics {
  totalDoctors: number;
  totalStaff: number;
  doctorUtilization: number;
  averageAppointmentsPerDoctor: number;
  staffSatisfactionScore: number;
  appointmentCompletionRate: number;
}

interface ClinicEfficiency {
  appointmentUtilization: number;
  averageWaitTime: number;
  noShowRate: number;
  cancellationRate: number;
  peakHours: string[];
  roomUtilization: number;
}

interface OwnerDashboardData {
  financial: FinancialMetrics;
  patients: PatientMetrics;
  staff: StaffMetrics;
  efficiency: ClinicEfficiency;
  alerts: Alert[];
  recentActivities: ActivityLog[];
}

interface Alert {
  id: string;
  type: "warning" | "error" | "info";
  title: string;
  message: string;
  timestamp: Date;
}

interface ActivityLog {
  id: string;
  type: "financial" | "staff" | "patient" | "system";
  description: string;
  timestamp: Date;
}

const OwnerDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { isLoading, error, executeInitialLoad } = useLoadingService();
  const [dashboardData, setDashboardData] = useState<OwnerDashboardData | null>(
    null
  );
  const [selectedTimeframe, setSelectedTimeframe] = useState<
    "today" | "week" | "month" | "year"
  >("month");

  useEffect(() => {
    const fetchOwnerDashboardData = async () => {
      // Simulate API call - replace with actual API endpoint
      await new Promise((resolve) => setTimeout(resolve, 1000));

      const mockData: OwnerDashboardData = {
        financial: {
          dailyRevenue: 12500,
          monthlyRevenue: 285000,
          yearlyRevenue: 3200000,
          monthlyExpenses: 180000,
          profitMargin: 36.8,
          revenueGrowth: 15.2,
          expenseGrowth: 8.5,
        },
        patients: {
          totalPatients: 2847,
          newPatientsThisMonth: 156,
          newPatientsLastMonth: 132,
          patientRetentionRate: 89.2,
          averagePatientValue: 485,
          patientSatisfactionScore: 4.6,
        },
        staff: {
          totalDoctors: 12,
          totalStaff: 28,
          doctorUtilization: 87.5,
          averageAppointmentsPerDoctor: 28,
          staffSatisfactionScore: 4.2,
          appointmentCompletionRate: 94.3,
        },
        efficiency: {
          appointmentUtilization: 91.2,
          averageWaitTime: 12,
          noShowRate: 8.7,
          cancellationRate: 5.2,
          peakHours: ["9:00 AM", "2:00 PM", "4:00 PM"],
          roomUtilization: 82.4,
        },
        alerts: [
          {
            id: "1",
            type: "warning",
            title: "Equipment Maintenance Due",
            message: "MRI machine #2 requires scheduled maintenance next week",
            timestamp: new Date(),
          },
          {
            id: "2",
            type: "info",
            title: "Insurance Contract Renewal",
            message: "BlueCross contract expires in 30 days",
            timestamp: new Date(),
          },
          {
            id: "3",
            type: "error",
            title: "Staff Shortage Alert",
            message: "Cardiology department understaffed - 2 doctors on leave",
            timestamp: new Date(),
          },
        ],
        recentActivities: [
          {
            id: "1",
            type: "financial",
            description: "Monthly financial report generated",
            timestamp: new Date(),
          },
          {
            id: "2",
            type: "staff",
            description: "Dr. Johnson completed 35 appointments today",
            timestamp: new Date(),
          },
          {
            id: "3",
            type: "patient",
            description: "24 new patient registrations this week",
            timestamp: new Date(),
          },
        ],
      };

      setDashboardData(mockData);
    };

    executeInitialLoad(fetchOwnerDashboardData);
  }, [executeInitialLoad, selectedTimeframe]);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  const formatPercentage = (value: number) => {
    return `${value.toFixed(1)}%`;
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

  const getAlertIcon = (type: Alert["type"]) => {
    switch (type) {
      case "error":
        return <AlertTriangle className="w-5 h-5 text-red-500" />;
      case "warning":
        return <AlertTriangle className="w-5 h-5 text-yellow-500" />;
      case "info":
        return <Activity className="w-5 h-5 text-blue-500" />;
      default:
        return <Activity className="w-5 h-5 text-gray-500" />;
    }
  };

  if (error) {
    return (
      <div className="min-h-screen bg-gray-100">
        <Header />
        <div className="pt-20 pb-12">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <ErrorDisplay
              message={error}
              onRetry={() => window.location.reload()}
            />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <LoadingOverlay
        isLoading={isLoading}
        message="Loading owner dashboard..."
      >
        <DashboardLayout title="Owner Dashboard - Medicare Clinic Management">
          {dashboardData && (
            <div className="space-y-6">
              {/* Time Filter */}
              <div className="flex justify-between items-center">
                <div className="flex space-x-2">
                  {(["today", "week", "month", "year"] as const).map(
                    (timeframe) => (
                      <Button
                        key={timeframe}
                        variant={
                          selectedTimeframe === timeframe
                            ? "primary"
                            : "outline"
                        }
                        size="sm"
                        onClick={() => setSelectedTimeframe(timeframe)}
                        className="capitalize"
                      >
                        {timeframe}
                      </Button>
                    )
                  )}
                </div>
                <div className="flex space-x-2">
                  <Button
                    variant="outline"
                    size="sm"
                    leftIcon={<FileText className="w-4 h-4" />}
                    onClick={() => navigate("/reports")}
                  >
                    Generate Report
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    leftIcon={<Settings className="w-4 h-4" />}
                    onClick={() => navigate("/settings")}
                  >
                    Settings
                  </Button>
                </div>
              </div>

              {/* Financial Overview */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <Card variant="elevated" padding="lg">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-gray-600 mb-1">
                        Daily Revenue
                      </p>
                      <p className="text-2xl font-bold text-green-600">
                        {formatCurrency(dashboardData.financial.dailyRevenue)}
                      </p>
                    </div>
                    <DollarSign className="w-8 h-8 text-green-500" />
                  </div>
                </Card>

                <Card variant="elevated" padding="lg">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-gray-600 mb-1">
                        Monthly Revenue
                      </p>
                      <p className="text-2xl font-bold text-blue-600">
                        {formatCurrency(dashboardData.financial.monthlyRevenue)}
                      </p>
                      <div
                        className={`flex items-center gap-1 mt-1 ${getGrowthColor(dashboardData.financial.revenueGrowth)}`}
                      >
                        {getGrowthIcon(dashboardData.financial.revenueGrowth)}
                        <span className="text-sm">
                          {formatPercentage(
                            dashboardData.financial.revenueGrowth
                          )}
                        </span>
                      </div>
                    </div>
                    <TrendingUp className="w-8 h-8 text-blue-500" />
                  </div>
                </Card>

                <Card variant="elevated" padding="lg">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-gray-600 mb-1">
                        Profit Margin
                      </p>
                      <p className="text-2xl font-bold text-purple-600">
                        {formatPercentage(dashboardData.financial.profitMargin)}
                      </p>
                    </div>
                    <PieChart className="w-8 h-8 text-purple-500" />
                  </div>
                </Card>

                <Card variant="elevated" padding="lg">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-gray-600 mb-1">
                        Total Patients
                      </p>
                      <p className="text-2xl font-bold text-indigo-600">
                        {dashboardData.patients.totalPatients.toLocaleString()}
                      </p>
                    </div>
                    <Users className="w-8 h-8 text-indigo-500" />
                  </div>
                </Card>
              </div>

              {/* Main Content Grid */}
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Patient Metrics */}
                <Card variant="medical" padding="lg">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-gray-900">
                      Patient Analytics
                    </h3>
                    <Users className="w-6 h-6 text-blue-500" />
                  </div>
                  <div className="space-y-4">
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        New Patients (This Month)
                      </span>
                      <div className="text-right">
                        <span className="font-semibold">
                          {dashboardData.patients.newPatientsThisMonth}
                        </span>
                        <div className="text-xs text-green-600">
                          +
                          {dashboardData.patients.newPatientsThisMonth -
                            dashboardData.patients.newPatientsLastMonth}{" "}
                          from last month
                        </div>
                      </div>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Retention Rate
                      </span>
                      <span className="font-semibold">
                        {formatPercentage(
                          dashboardData.patients.patientRetentionRate
                        )}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Avg. Patient Value
                      </span>
                      <span className="font-semibold">
                        {formatCurrency(
                          dashboardData.patients.averagePatientValue
                        )}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Satisfaction Score
                      </span>
                      <div className="flex items-center gap-1">
                        <Star className="w-4 h-4 text-yellow-500 fill-current" />
                        <span className="font-semibold">
                          {dashboardData.patients.patientSatisfactionScore}/5.0
                        </span>
                      </div>
                    </div>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    className="w-full mt-4"
                    onClick={() => navigate("/analytics/patients")}
                  >
                    View Detailed Analytics
                  </Button>
                </Card>

                {/* Staff Performance */}
                <Card variant="medical" padding="lg">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-gray-900">
                      Staff Performance
                    </h3>
                    <UserCheck className="w-6 h-6 text-green-500" />
                  </div>
                  <div className="space-y-4">
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Total Doctors
                      </span>
                      <span className="font-semibold">
                        {dashboardData.staff.totalDoctors}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Doctor Utilization
                      </span>
                      <span className="font-semibold">
                        {formatPercentage(
                          dashboardData.staff.doctorUtilization
                        )}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Avg. Appointments/Doctor
                      </span>
                      <span className="font-semibold">
                        {dashboardData.staff.averageAppointmentsPerDoctor}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Completion Rate
                      </span>
                      <span className="font-semibold">
                        {formatPercentage(
                          dashboardData.staff.appointmentCompletionRate
                        )}
                      </span>
                    </div>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    className="w-full mt-4"
                    onClick={() => navigate("/staff-management")}
                  >
                    Manage Staff
                  </Button>
                </Card>

                {/* Clinic Efficiency */}
                <Card variant="medical" padding="lg">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-gray-900">
                      Clinic Efficiency
                    </h3>
                    <Activity className="w-6 h-6 text-purple-500" />
                  </div>
                  <div className="space-y-4">
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Appointment Utilization
                      </span>
                      <span className="font-semibold">
                        {formatPercentage(
                          dashboardData.efficiency.appointmentUtilization
                        )}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Avg. Wait Time
                      </span>
                      <span className="font-semibold">
                        {dashboardData.efficiency.averageWaitTime} min
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        No-Show Rate
                      </span>
                      <span className="font-semibold text-red-600">
                        {formatPercentage(dashboardData.efficiency.noShowRate)}
                      </span>
                    </div>
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-gray-600">
                        Room Utilization
                      </span>
                      <span className="font-semibold">
                        {formatPercentage(
                          dashboardData.efficiency.roomUtilization
                        )}
                      </span>
                    </div>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    className="w-full mt-4"
                    onClick={() => navigate("/operations")}
                  >
                    Optimize Operations
                  </Button>
                </Card>
              </div>

              {/* Alerts and Activities */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Critical Alerts */}
                <Card variant="elevated" padding="lg">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-gray-900">
                      Critical Alerts
                    </h3>
                    <AlertTriangle className="w-6 h-6 text-red-500" />
                  </div>
                  <div className="space-y-3">
                    {dashboardData.alerts.map((alert) => (
                      <div
                        key={alert.id}
                        className="flex items-start gap-3 p-3 rounded-lg bg-gray-50"
                      >
                        {getAlertIcon(alert.type)}
                        <div className="flex-1">
                          <p className="font-medium text-sm text-gray-900">
                            {alert.title}
                          </p>
                          <p className="text-xs text-gray-600 mt-1">
                            {alert.message}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                  <Button
                    variant="warning"
                    size="sm"
                    className="w-full mt-4"
                    onClick={() => navigate("/alerts")}
                  >
                    View All Alerts
                  </Button>
                </Card>

                {/* Recent Activities */}
                <Card variant="elevated" padding="lg">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold text-gray-900">
                      Recent Activities
                    </h3>
                    <Clock className="w-6 h-6 text-blue-500" />
                  </div>
                  <div className="space-y-3">
                    {dashboardData.recentActivities.map((activity) => (
                      <div
                        key={activity.id}
                        className="flex items-start gap-3 p-3 rounded-lg bg-gray-50"
                      >
                        <Activity className="w-4 h-4 text-blue-500 mt-0.5" />
                        <div className="flex-1">
                          <p className="text-sm text-gray-900">
                            {activity.description}
                          </p>
                          <p className="text-xs text-gray-500 mt-1">
                            {activity.timestamp.toLocaleTimeString()}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    className="w-full mt-4"
                    onClick={() => navigate("/activity-log")}
                  >
                    View Activity Log
                  </Button>
                </Card>
              </div>

              {/* Quick Actions */}
              <Card variant="elevated" padding="lg">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Quick Actions
                </h3>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <Button
                    variant="primary"
                    size="lg"
                    leftIcon={<BarChart3 className="w-5 h-5" />}
                    onClick={() => navigate("/analytics")}
                    className="flex-col h-auto py-4"
                  >
                    <span className="mt-2">View Analytics</span>
                  </Button>
                  <Button
                    variant="secondary"
                    size="lg"
                    leftIcon={<Users className="w-5 h-5" />}
                    onClick={() => navigate("/staff-management")}
                    className="flex-col h-auto py-4"
                  >
                    <span className="mt-2">Manage Staff</span>
                  </Button>
                  <Button
                    variant="success"
                    size="lg"
                    leftIcon={<DollarSign className="w-5 h-5" />}
                    onClick={() => navigate("/financial-reports")}
                    className="flex-col h-auto py-4"
                  >
                    <span className="mt-2">Financial Reports</span>
                  </Button>
                  <Button
                    variant="secondary"
                    size="lg"
                    leftIcon={<Shield className="w-5 h-5" />}
                    onClick={() => navigate("/compliance")}
                    className="flex-col h-auto py-4"
                  >
                    <span className="mt-2">Compliance</span>
                  </Button>
                </div>
              </Card>
            </div>
          )}
        </DashboardLayout>
      </LoadingOverlay>
    </div>
  );
};

export default OwnerDashboard;
