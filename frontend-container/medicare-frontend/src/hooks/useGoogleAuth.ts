// DEPRECATED: Google Calendar integration has been removed
// This hook has been replaced with Microsoft Graph API integration
// Please use the scheduler feature in src/features/scheduler instead

import { useEffect, useState } from "react";

// Legacy Google Auth hook - marked for removal
// Keeping for backward compatibility during transition
export function useGoogleAuth() {
  const [inited, setInited] = useState(false);
  const [isSignedIn, setSignedIn] = useState(false);

  useEffect(() => {
    console.warn(
      "useGoogleAuth is deprecated. Use the new scheduler feature with Microsoft Graph integration."
    );

    // Return placeholder values
    setInited(true);
    setSignedIn(false);
  }, []);

  const signIn = () => {
    console.warn(
      "Google Auth has been deprecated. Please use Microsoft Graph authentication."
    );
  };

  const signOut = () => {
    console.warn(
      "Google Auth has been deprecated. Please use Microsoft Graph authentication."
    );
  };

  return { inited, isSignedIn, signIn, signOut };
}
