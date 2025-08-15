import React from "react";
import { Mail, MapPin, Phone, Stethoscope, User, Users } from "lucide-react";

import { Badge, Card } from "../../../shared/components";
import type { StaffCardProps } from "../types";

export const StaffCard: React.FC<StaffCardProps> = ({ staff, onClick }) => {
  const handleClick = () => {
    onClick(staff);
  };

  const getStatusBadge = () => {
    return staff.isActive ? (
      <Badge variant="success" size="sm">
        Active
      </Badge>
    ) : (
      <Badge variant="error" size="sm">
        Inactive
      </Badge>
    );
  };

  const getRoleBadge = () => {
    return staff.role === "Doctor" ? (
      <Badge variant="info" icon={<Stethoscope size={12} />} size="sm">
        Doctor
      </Badge>
    ) : (
      <Badge variant="default" icon={<Users size={12} />} size="sm">
        Receptionist
      </Badge>
    );
  };

  const renderRoleSpecificInfo = () => {
    if (staff.role === "Doctor") {
      return (
        <div className="space-y-1">
          {staff.licenseNumber && (
            <p className="text-sm text-gray-600">
              <span className="font-medium">License:</span>{" "}
              {staff.licenseNumber}
            </p>
          )}
          {staff.yearsExperience && (
            <p className="text-sm text-gray-600">
              <span className="font-medium">Experience:</span>{" "}
              {staff.yearsExperience} years
            </p>
          )}
          {staff.specializations.length > 0 && (
            <div className="flex flex-wrap gap-1 mt-2">
              {staff.specializations.slice(0, 2).map((spec) => (
                <Badge key={spec.id} variant="default" size="sm">
                  {spec.name}
                </Badge>
              ))}
              {staff.specializations.length > 2 && (
                <Badge variant="default" size="sm">
                  +{staff.specializations.length - 2} more
                </Badge>
              )}
            </div>
          )}
        </div>
      );
    } else {
      return (
        <div className="space-y-1">
          {staff.department && (
            <p className="text-sm text-gray-600">
              <span className="font-medium">Department:</span>{" "}
              {staff.department}
            </p>
          )}
        </div>
      );
    }
  };

  return (
    <Card
      variant="default"
      padding="md"
      className="cursor-pointer hover:shadow-lg transition-shadow duration-200"
      onClick={handleClick}
    >
      <div className="flex flex-col gap-3">
        {/* Header with name and status */}
        <div className="flex items-start justify-between">
          <div>
            <h3 className="font-semibold text-lg text-gray-900">
              {staff.profile.firstName} {staff.profile.lastName}
            </h3>
            <div className="flex gap-2 mt-1">
              {getRoleBadge()}
              {getStatusBadge()}
            </div>
          </div>
          <div className="flex-shrink-0">
            <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
              <User size={24} className="text-blue-600" />
            </div>
          </div>
        </div>

        {/* Contact Information */}
        <div className="space-y-1">
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <Mail size={14} />
            <span>{staff.profile.email}</span>
          </div>
          {staff.profile.phone && (
            <div className="flex items-center gap-2 text-sm text-gray-600">
              <Phone size={14} />
              <span>{staff.profile.phone}</span>
            </div>
          )}
          {staff.profile.city && staff.profile.state && (
            <div className="flex items-center gap-2 text-sm text-gray-600">
              <MapPin size={14} />
              <span>
                {staff.profile.city}, {staff.profile.state}
              </span>
            </div>
          )}
        </div>

        {/* Role-specific information */}
        {renderRoleSpecificInfo()}

        {/* Action button */}
        <button
          onClick={handleClick}
          className="mt-2 bg-blue-100 text-blue-700 px-4 py-2 rounded-lg hover:bg-blue-200 transition duration-150 w-fit text-sm font-medium"
        >
          View Details
        </button>
      </div>
    </Card>
  );
};
