import React from "react";
import { Card } from "@shared/components";
import {
  Building2,
  MapPin,
  Shield,
  Thermometer,
  Wifi,
  Zap,
} from "lucide-react";

interface FacilityData {
  rooms: {
    total: number;
    occupied: number;
    maintenance: number;
    available: number;
  };
  equipment: {
    total: number;
    operational: number;
    maintenance: number;
    utilization: number;
  };
  utilities: {
    power: "normal" | "backup" | "issue";
    internet: "excellent" | "good" | "poor";
    hvac: "optimal" | "adjusting" | "issue";
    security: "secure" | "warning" | "alert";
  };
  capacity: {
    current: number;
    maximum: number;
    recommended: number;
  };
  maintenance: {
    scheduled: number;
    overdue: number;
    completed: number;
  };
}

interface FacilityOverviewCardProps {
  data: FacilityData;
}

const FacilityOverviewCard: React.FC<FacilityOverviewCardProps> = ({
  data,
}) => {
  const formatPercentage = (value: number) => {
    return `${value.toFixed(1)}%`;
  };

  const getUtilityIcon = (type: keyof FacilityData["utilities"]) => {
    switch (type) {
      case "power":
        return <Zap className="w-4 h-4" />;
      case "internet":
        return <Wifi className="w-4 h-4" />;
      case "hvac":
        return <Thermometer className="w-4 h-4" />;
      case "security":
        return <Shield className="w-4 h-4" />;
      default:
        return <Building2 className="w-4 h-4" />;
    }
  };

  const getUtilityColor = (status: string) => {
    switch (status) {
      case "normal":
      case "excellent":
      case "optimal":
      case "secure":
        return "text-green-600 bg-green-100";
      case "backup":
      case "good":
      case "adjusting":
      case "warning":
        return "text-yellow-600 bg-yellow-100";
      case "issue":
      case "poor":
      case "alert":
        return "text-red-600 bg-red-100";
      default:
        return "text-gray-600 bg-gray-100";
    }
  };

  const getRoomUtilization = () => {
    return (data.rooms.occupied / data.rooms.total) * 100;
  };

  const getCapacityUtilization = () => {
    return (data.capacity.current / data.capacity.maximum) * 100;
  };

  const getCapacityColor = () => {
    const utilization = getCapacityUtilization();
    if (utilization > 90) return "text-red-600 bg-red-100";
    if (utilization > 75) return "text-yellow-600 bg-yellow-100";
    return "text-green-600 bg-green-100";
  };

  return (
    <Card
      variant="elevated"
      padding="lg"
      className="bg-gradient-to-br from-orange-50 to-amber-50 border-orange-100"
    >
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <Building2 className="w-6 h-6 text-orange-600" />
          <h3 className="text-lg font-semibold text-gray-900">
            Facility Overview
          </h3>
        </div>
        <MapPin className="w-5 h-5 text-gray-400" />
      </div>

      {/* Room Status */}
      <div className="mb-4">
        <div className="flex justify-between items-center mb-2">
          <span className="text-sm font-medium text-gray-700">
            Room Utilization
          </span>
          <span className="text-sm font-bold text-orange-600">
            {formatPercentage(getRoomUtilization())}
          </span>
        </div>
        <div className="bg-gray-200 rounded-full h-2 mb-3">
          <div
            className="bg-orange-500 rounded-full h-2 transition-all duration-300"
            style={{ width: `${getRoomUtilization()}%` }}
          />
        </div>

        <div className="grid grid-cols-4 gap-2 text-center">
          <div className="bg-white rounded-lg p-2">
            <p className="text-lg font-bold text-gray-900">
              {data.rooms.total}
            </p>
            <p className="text-xs text-gray-600">Total</p>
          </div>
          <div className="bg-white rounded-lg p-2">
            <p className="text-lg font-bold text-blue-600">
              {data.rooms.occupied}
            </p>
            <p className="text-xs text-gray-600">Occupied</p>
          </div>
          <div className="bg-white rounded-lg p-2">
            <p className="text-lg font-bold text-green-600">
              {data.rooms.available}
            </p>
            <p className="text-xs text-gray-600">Available</p>
          </div>
          <div className="bg-white rounded-lg p-2">
            <p className="text-lg font-bold text-red-600">
              {data.rooms.maintenance}
            </p>
            <p className="text-xs text-gray-600">Maintenance</p>
          </div>
        </div>
      </div>

      {/* Equipment Status */}
      <div className="mb-4">
        <h4 className="text-sm font-medium text-gray-700 mb-2">
          Equipment Status
        </h4>
        <div className="grid grid-cols-2 gap-3">
          <div className="bg-white rounded-lg p-3">
            <div className="flex justify-between items-center mb-1">
              <span className="text-xs text-gray-600">Operational</span>
              <span className="text-sm font-bold text-green-600">
                {data.equipment.operational}/{data.equipment.total}
              </span>
            </div>
            <div className="bg-gray-200 rounded-full h-1">
              <div
                className="bg-green-500 rounded-full h-1"
                style={{
                  width: `${(data.equipment.operational / data.equipment.total) * 100}%`,
                }}
              />
            </div>
          </div>
          <div className="bg-white rounded-lg p-3">
            <div className="flex justify-between items-center mb-1">
              <span className="text-xs text-gray-600">Utilization</span>
              <span className="text-sm font-bold text-blue-600">
                {formatPercentage(data.equipment.utilization)}
              </span>
            </div>
            <div className="bg-gray-200 rounded-full h-1">
              <div
                className="bg-blue-500 rounded-full h-1"
                style={{ width: `${data.equipment.utilization}%` }}
              />
            </div>
          </div>
        </div>
      </div>

      {/* Utility Systems */}
      <div className="mb-4">
        <h4 className="text-sm font-medium text-gray-700 mb-2">
          Utility Systems
        </h4>
        <div className="grid grid-cols-2 gap-2">
          {Object.entries(data.utilities).map(([key, status]) => (
            <div
              key={key}
              className="flex items-center gap-2 bg-white rounded-lg p-2"
            >
              {getUtilityIcon(key as keyof FacilityData["utilities"])}
              <span className="text-xs text-gray-600 capitalize flex-1">
                {key}
              </span>
              <span
                className={`text-xs px-2 py-1 rounded-full capitalize ${getUtilityColor(status)}`}
              >
                {status}
              </span>
            </div>
          ))}
        </div>
      </div>

      {/* Capacity Management */}
      <div className="mb-4">
        <div className="flex justify-between items-center mb-2">
          <span className="text-sm font-medium text-gray-700">
            Facility Capacity
          </span>
          <span
            className={`text-sm font-bold px-2 py-1 rounded-full ${getCapacityColor()}`}
          >
            {data.capacity.current}/{data.capacity.maximum}
          </span>
        </div>
        <div className="bg-gray-200 rounded-full h-2 mb-2">
          <div
            className="bg-orange-500 rounded-full h-2 transition-all duration-300"
            style={{ width: `${getCapacityUtilization()}%` }}
          />
        </div>
        <p className="text-xs text-gray-600">
          Recommended capacity: {data.capacity.recommended} people
        </p>
      </div>

      {/* Maintenance Schedule */}
      <div className="bg-white rounded-lg p-3">
        <h4 className="text-sm font-medium text-gray-700 mb-2">
          Maintenance Status
        </h4>
        <div className="grid grid-cols-3 gap-2 text-center">
          <div>
            <p className="text-sm font-bold text-blue-600">
              {data.maintenance.scheduled}
            </p>
            <p className="text-xs text-gray-600">Scheduled</p>
          </div>
          <div>
            <p className="text-sm font-bold text-green-600">
              {data.maintenance.completed}
            </p>
            <p className="text-xs text-gray-600">Completed</p>
          </div>
          <div>
            <p className="text-sm font-bold text-red-600">
              {data.maintenance.overdue}
            </p>
            <p className="text-xs text-gray-600">Overdue</p>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="mt-4 grid grid-cols-2 gap-2">
        <button className="px-3 py-2 text-xs font-medium text-orange-600 bg-orange-50 rounded-lg hover:bg-orange-100 transition-colors">
          Schedule Maintenance
        </button>
        <button className="px-3 py-2 text-xs font-medium text-gray-600 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors">
          Facility Report
        </button>
      </div>
    </Card>
  );
};

export default FacilityOverviewCard;
