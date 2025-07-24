import { useState } from "react";
import Header from "../../layout/Header";
import ChangePasswordModal from "./ChangePasswordModal";

export default function Settings() {
  const profileData = {
    name: "John Doe",
    phone: "+1234567890",
    address: "Słoneczna 3, 12-254, Warsaw",
    dob: "1990-01-01",
    membershipName: "Gold Health Membership",
  };

  const [showPasswordModal, setShowPasswordModal] = useState(false);

  return (
    <div className="min-h-screen bg-gray-100">
      <Header />
      <main className="pt-24 px-4 md:px-8 pb-10 flex justify-center">
        <div className="w-full max-w-xl bg-white rounded-2xl shadow-md p-8 flex flex-col items-center">
          <h1 className="text-3xl font-bold text-blue-700 mb-8">
            Account Settings
          </h1>

          {/* Account Information */}
          <div className="w-full mb-8">
            <h2 className="text-xl font-semibold text-blue-600 mb-2">
              Account Information
            </h2>
            <div className="mb-2">
              <span className="font-semibold text-gray-600">Name:</span>
              <span className="ml-2 text-gray-700">{profileData.name}</span>
            </div>
            <div className="mb-2">
              <span className="font-semibold text-gray-600">Phone:</span>
              <span className="ml-2 text-gray-700">{profileData.phone}</span>
            </div>
            <div className="mb-2">
              <span className="font-semibold text-gray-600">Address:</span>
              <span className="ml-2 text-gray-700">{profileData.address}</span>
            </div>
            <div className="mb-2">
              <span className="font-semibold text-gray-600">
                Date of Birth:
              </span>
              <span className="ml-2 text-gray-700">{profileData.dob}</span>
            </div>
            <div className="mb-2">
              <span className="font-semibold text-gray-600">
                Membership Level:
              </span>
              <span className="ml-2 text-gray-700">
                {profileData.membershipName}
              </span>
            </div>
          </div>

          {/* Change Password Section */}
          <div className="w-full mb-2">
            <h2 className="text-xl font-semibold text-blue-600 mb-2">
              Change Password
            </h2>
            <button
              onClick={() => setShowPasswordModal(true)}
              className="px-4 py-2 bg-blue-700 text-white rounded-lg hover:bg-blue-800 transition font-semibold"
            >
              Change Password
            </button>
          </div>
        </div>
      </main>
      <ChangePasswordModal
        open={showPasswordModal}
        onClose={() => setShowPasswordModal(false)}
      />
    </div>
  );
}
