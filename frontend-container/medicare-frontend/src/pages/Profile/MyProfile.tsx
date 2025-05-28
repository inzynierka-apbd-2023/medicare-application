import Header from "../Header";

export default function MyProfile() {
  const profileData = {
    name: "John Doe",
    email: "john.doe@example.com",
    phone: "+1234567890",
    address: "123 Main Street, Anytown, USA",
    dob: "1990-01-01",
    insurance: "ACME Health Insurance",
    insuranceNumber: "INS-123456789",
  };

  return (
    <div className="min-h-screen bg-gray-100 overflow-x-hidden">
      <Header />
      <div className="container mx-auto max-w-[160rem] px-4 sm:px-6 lg:px-8">
        <main className="pt-24 pb-10">
          <h1 className="text-3xl font-bold text-blue-700 mb-8">My Profile</h1>

          <div className="bg-white rounded-2xl shadow-md p-8 flex flex-col md:flex-row md:space-x-8">
            {/* Profile Picture & Name */}
            <div className="flex flex-col items-center mb-6 md:mb-0">
              <div className="w-32 h-32 bg-blue-100 rounded-full flex items-center justify-center mb-4">
                <span className="text-5xl text-blue-500 font-semibold">JD</span>
              </div>
              <h2 className="text-xl font-semibold text-blue-600">{profileData.name}</h2>
            </div>

            {/* Profile Details */}
            <div className="flex-1 space-y-4">
              <div className="flex flex-col md:flex-row md:justify-between md:space-x-4">
                <div className="flex-1">
                  <label className="text-gray-500 text-sm">Email</label>
                  <div className="text-gray-800 font-medium">{profileData.email}</div>
                </div>
                <div className="flex-1">
                  <label className="text-gray-500 text-sm">Phone</label>
                  <div className="text-gray-800 font-medium">{profileData.phone}</div>
                </div>
              </div>

              <div className="flex flex-col">
                <label className="text-gray-500 text-sm">Address</label>
                <div className="text-gray-800 font-medium">{profileData.address}</div>
              </div>

              <div className="flex flex-col md:flex-row md:justify-between md:space-x-4">
                <div className="flex-1">
                  <label className="text-gray-500 text-sm">Date of Birth</label>
                  <div className="text-gray-800 font-medium">{profileData.dob}</div>
                </div>
                <div className="flex-1">
                  <label className="text-gray-500 text-sm">Insurance</label>
                  <div className="text-gray-800 font-medium">{profileData.insurance}</div>
                </div>
              </div>

              <div className="flex flex-col">
                <label className="text-gray-500 text-sm">Insurance Number</label>
                <div className="text-gray-800 font-medium">{profileData.insuranceNumber}</div>
              </div>

              <button className="mt-4 px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded-lg transition duration-150">
                Edit Profile
              </button>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
}
