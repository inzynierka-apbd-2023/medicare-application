import React from "react";
import { X } from "lucide-react";

import { Modal } from "../../../../shared/components";
import { ReceptionistSchedulerPage } from "../../../receptionistScheduler/ReceptionistSchedulerPage";

interface SchedulerModalProps {
  isOpen: boolean;
  onClose: () => void;
  openDirectlyToBooking?: boolean;
}

export const SchedulerModal: React.FC<SchedulerModalProps> = ({
  isOpen,
  onClose,
  openDirectlyToBooking = false,
}) => {
  return (
    <Modal isOpen={isOpen} onClose={onClose} size="xl">
      <div className="flex items-center justify-between p-6 border-b border-gray-200">
        <h2 className="text-xl font-semibold text-gray-900">
          Appointment Scheduler
        </h2>
        <button
          onClick={onClose}
          className="text-gray-400 hover:text-gray-600 transition-colors"
        >
          <X className="h-6 w-6" />
        </button>
      </div>

      <div className="flex-1 overflow-auto max-h-[80vh]">
        <ReceptionistSchedulerPage
          autoOpenBooking={openDirectlyToBooking}
          isEmbedded={true}
        />
      </div>
    </Modal>
  );
};
