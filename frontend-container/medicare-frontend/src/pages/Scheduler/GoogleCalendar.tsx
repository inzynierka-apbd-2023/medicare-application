// Google Calendar integration removed
// This component has been replaced with a generic calendar view in the features/scheduler module
// Future Microsoft Graph API integration will be implemented in the shared services

export default function GoogleCalendarScheduler() {
  return (
    <div className="w-full h-full flex items-center justify-center">
      <div className="text-center">
        <p className="text-lg font-medium text-gray-600 mb-2">
          Calendar Integration Updating
        </p>
        <p className="text-sm text-gray-500">
          Google Calendar has been replaced with Microsoft Graph integration.
          Please use the main scheduler page.
        </p>
      </div>
    </div>
  );
}
