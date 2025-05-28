import Header from "../Header";

export default function MyProfile() {
  const profileData = {
    name: "John Doe",
    type: "patient",
    email: "john.doe@example.com",
    phone: "+1234567890",
    address: "Słoneczna 3, 12-254, Warsaw",
    dob: "1990-01-01",
    membershipName: "Gold Health Membership",
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <div className="container mx-auto max-w-2xl px-4 sm:px-6 lg:px-8">
        <main className="pt-24 pb-10">
          <h1 className="text-3xl font-bold text-center text-blue-700 mb-8">My Profile</h1>

          <div className="bg-white rounded-2xl shadow-md p-8 flex flex-col items-center">
            <div className="w-32 h-32 bg-blue-100 rounded-full flex items-center justify-center mb-4">
              <span className="text-4xl text-blue-500 font-semibold">JD</span>
            </div>
            <h2 className="text-xl font-semibold text-blue-600 mb-6">{profileData.name}</h2>

            <div className="text-center space-y-3">
              <div>
                <span className="text-sm text-gray-500">Email</span>
                <div className="text-gray-800 font-medium">{profileData.email}</div>
              </div>
              <div>
                <span className="text-sm text-gray-500">Phone</span>
                <div className="text-gray-800 font-medium">{profileData.phone}</div>
              </div>
              <div>
                <span className="text-sm text-gray-500">Address</span>
                <div className="text-gray-800 font-medium">{profileData.address}</div>
              </div>
              <div>
                <span className="text-sm text-gray-500">Date of Birth</span>
                <div className="text-gray-800 font-medium">{profileData.dob}</div>
              </div>
              <div>
                <span className="text-sm text-gray-500">Membership Level</span>
                <div className="text-gray-800 font-medium">{profileData.membershipName}</div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
