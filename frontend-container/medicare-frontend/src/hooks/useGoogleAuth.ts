import { useEffect, useState } from "react";
import { gapi } from "gapi-script";

// Pull from Vite’s import.meta.env
const CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID;
const API_KEY   = import.meta.env.VITE_GOOGLE_API_KEY;
const SCOPES    =
  "https://www.googleapis.com/auth/calendar.events " +
  "https://www.googleapis.com/auth/calendar.readonly";

export function useGoogleAuth() {
  const [inited, setInited] = useState(false);
  const [isSignedIn, setSignedIn] = useState(false);

  useEffect(() => {
    gapi.load("client:auth2", async () => {
      await gapi.client.init({
        apiKey:    API_KEY,
        clientId:  CLIENT_ID,
        discoveryDocs: [
          "https://www.googleapis.com/discovery/v1/apis/calendar/v3/rest",
        ],
        scope: SCOPES,
      });

      const auth = gapi.auth2.getAuthInstance();
      setSignedIn(auth.isSignedIn.get());
      auth.isSignedIn.listen(setSignedIn);
      setInited(true);
    });
  }, []);

  const signIn  = () => gapi.auth2.getAuthInstance().signIn();
  const signOut = () => gapi.auth2.getAuthInstance().signOut();

  return { inited, isSignedIn, signIn, signOut };
}
