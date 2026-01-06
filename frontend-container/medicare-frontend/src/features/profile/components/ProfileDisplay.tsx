import React from "react";
import { Calendar, CreditCard, Mail, MapPin, Phone, User } from "lucide-react";

import { Button, Card } from "../../../shared/components";
import type { ProfileDisplayProps } from "../types";

import { SubscriptionCard } from "./SubscriptionCard";

export const ProfileDisplay: React.FC<ProfileDisplayProps> = ({
  profileData,
  onEdit,
  onPasswordChange,
}) => {
  const profileFields: {
    icon: React.ReactNode;
    label: string;
    value: string;
  }[] = [];
  profileFields.push(
    {
      icon: <Mail className="w-4 h-4" />,
      label: "Email",
      value: profileData.email,
    },
    {
      icon: <Phone className="w-4 h-4" />,
      label: "Phone",
      value: profileData.phone,
    },
    {
      icon: <MapPin className="w-4 h-4" />,
      label: "Address",
      value: profileData.address,
    }
  );
  if (profileData.dateOfBirth) {
    profileFields.push({
      icon: <Calendar className="w-4 h-4" />,
      label: "Date of Birth",
      value: new Date(profileData.dateOfBirth).toLocaleDateString(),
    });
  }
  if (profileData.membershipName) {
    profileFields.push({
      icon: <CreditCard className="w-4 h-4" />,
      label: "Role",
      value: profileData.membershipName,
    });
  }

  return (
    <div className="space-y-6">
      {/* Profile Header */}
      <Card variant="medical" className="text-center">
        <div className="flex flex-col items-center space-y-4">
          <div className="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center">
            {profileData.profilePicture ? (
              <img
                src={profileData.profilePicture}
                alt="Profile"
                className="w-20 h-20 rounded-full object-cover"
              />
            ) : (
              <User className="w-8 h-8 text-blue-600" />
            )}
          </div>
          <div>
            <h2 className="text-2xl font-bold text-gray-900">
              {profileData.name}
            </h2>
            {profileData.membershipName && (
              <p className="text-gray-600">{profileData.membershipName}</p>
            )}
          </div>
          <Button variant="secondary" onClick={onEdit} className="mt-4">
            Edit Profile
          </Button>
        </div>
      </Card>

      {/* Profile Information */}
      <Card
        variant="medical"
        header={
          <h3 className="text-xl font-semibold text-blue-600">
            Personal Information
          </h3>
        }
      >
        <div className="space-y-4">
          {profileFields.map((field, index) => (
            <div key={index} className="flex items-start space-x-3">
              <div className="text-blue-600 mt-1">{field.icon}</div>
              <div className="flex-1">
                <span className="font-semibold text-gray-600">
                  {field.label}:
                </span>
                <span className="ml-2 text-gray-900">{field.value}</span>
              </div>
            </div>
          ))}
        </div>
      </Card>

      {/* Subscription Information - only show for patients, not doctors */}
      {profileData.membershipName !== "Doctor" && <SubscriptionCard />}

      {/* Security Section */}
      <Card
        variant="medical"
        header={
          <h3 className="text-xl font-semibold text-blue-600">Security</h3>
        }
      >
        <div className="space-y-4">
          <p className="text-gray-600">
            Keep your account secure by regularly updating your password.
          </p>
          <Button variant="outline" onClick={onPasswordChange}>
            Change Password
          </Button>
        </div>
      </Card>
    </div>
  );
};
