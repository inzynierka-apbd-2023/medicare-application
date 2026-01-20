import { useEffect } from "react";
import { setGlobalToastHandler } from "@shared/services/api";
import { useToast } from "@shared/toast";

export const useApiToastInit = () => {
  const { showSuccess, showError } = useToast();

  useEffect(() => {
    setGlobalToastHandler({ showSuccess, showError });
    return () => setGlobalToastHandler(null);
  }, [showSuccess, showError]);
};
