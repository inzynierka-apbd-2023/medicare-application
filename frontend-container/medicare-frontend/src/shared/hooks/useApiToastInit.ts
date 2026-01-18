import { useEffect } from "react";

import { setGlobalToastHandler } from "../services/api";
import { useToast } from "../toast";

export const useApiToastInit = () => {
  const { showSuccess, showError } = useToast();

  useEffect(() => {
    setGlobalToastHandler({ showSuccess, showError });
    return () => setGlobalToastHandler(null);
  }, [showSuccess, showError]);
};
