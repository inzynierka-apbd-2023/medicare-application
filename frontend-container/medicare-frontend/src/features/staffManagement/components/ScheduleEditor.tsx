import React, { useEffect, useState } from "react";
import { Save } from "lucide-react";

import { Button, Input, Modal } from "../../../shared/components";
import { staffApi } from "../../../shared/services/staffApi";
import type { ScheduleEntry, StaffMember } from "../types";

interface ScheduleEditorProps {
  doctor: StaffMember | null;
  isOpen: boolean;
  onClose: () => void;
}

interface UnavailableSlotBackend {
  dayOfWeek?: number;
  DayOfWeek?: number;
  start?: string | number;
  Start?: string | number;
  startTime?: string | number;
  StartTime?: string | number;
  end?: string | number;
  End?: string | number;
  endTime?: string | number;
  EndTime?: string | number;
}

const dayNames = [
  "Sunday",
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
];

export const ScheduleEditor: React.FC<ScheduleEditorProps> = ({
  doctor,
  isOpen,
  onClose,
}) => {
  const [entries, setEntries] = useState<ScheduleEntry[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      if (!doctor || doctor.role !== "Doctor") return;
      setLoading(true);
      setError(null);
      try {
        const data = await staffApi.getAvailability(doctor.id);
        const mapped = ((data || []) as UnavailableSlotBackend[]).map((s) => ({
          dayOfWeek: s.dayOfWeek ?? s.DayOfWeek ?? 0,
          start:
            (s.start ?? s.Start ?? s.startTime ?? s.StartTime)
              ?.toString()
              .slice(0, 5) || "00:00",
          end:
            (s.end ?? s.End ?? s.endTime ?? s.EndTime)
              ?.toString()
              .slice(0, 5) || "00:00",
        })) as ScheduleEntry[];
        setEntries(mapped);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to load availability"
        );
      } finally {
        setLoading(false);
      }
    };
    if (isOpen) load();
  }, [doctor, isOpen]);

  const updateEntry = (
    idx: number,
    field: keyof ScheduleEntry,
    value: string | number
  ) => {
    setEntries((prev) =>
      prev.map((e, i) => (i === idx ? { ...e, [field]: value } : e))
    );
  };

  const addEntry = () => {
    setEntries((prev) => [
      ...prev,
      { dayOfWeek: 1, start: "09:00", end: "17:00" },
    ]);
  };

  const removeEntry = (idx: number) => {
    setEntries((prev) => prev.filter((_, i) => i !== idx));
  };

  const validate = (): string | null => {
    for (const e of entries) {
      if (e.start >= e.end) return "Start time must be before end time";
      if (e.dayOfWeek < 0 || e.dayOfWeek > 6) return "Invalid day of week";
    }
    // Check overlaps per day
    const byDay: Record<number, ScheduleEntry[]> = {};
    entries.forEach((e) => {
      byDay[e.dayOfWeek] = byDay[e.dayOfWeek] || [];
      byDay[e.dayOfWeek].push(e);
    });
    for (const day in byDay) {
      const list = byDay[Number(day)]
        .slice()
        .sort((a, b) => a.start.localeCompare(b.start));
      for (let i = 1; i < list.length; i++) {
        if (list[i].start < list[i - 1].end)
          return `Overlapping slots on ${dayNames[Number(day)]}`;
      }
    }
    return null;
  };

  const handleSave = async () => {
    if (!doctor) return;
    const err = validate();
    if (err) {
      setError(err);
      return;
    }
    setLoading(true);
    try {
      await staffApi.setAvailability(
        doctor.id,
        entries.map((e) => ({
          dayOfWeek: e.dayOfWeek,
          start: e.start,
          end: e.end,
        }))
      );
      onClose();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to save availability"
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <div className="space-y-4">
        <h3 className="text-xl font-semibold">Doctor Availability</h3>
        {error && <div className="text-red-600 text-sm">{error}</div>}
        <div className="space-y-3">
          {entries.map((e, idx) => (
            <div
              key={`${e.dayOfWeek}-${e.start}-${e.end}-${idx}`}
              className="grid grid-cols-12 gap-2 items-end"
            >
              <div className="col-span-4">
                <label
                  htmlFor={`day-${idx}`}
                  className="block text-sm font-medium text-gray-700 mb-1"
                >
                  Day
                </label>
                <select
                  className="w-full px-3 py-2 rounded-lg border border-gray-300"
                  id={`day-${idx}`}
                  value={e.dayOfWeek}
                  onChange={(ev) =>
                    updateEntry(idx, "dayOfWeek", parseInt(ev.target.value, 10))
                  }
                >
                  {dayNames.map((n, i) => (
                    <option key={`day-${n}`} value={i}>
                      {n}
                    </option>
                  ))}
                </select>
              </div>
              <div className="col-span-3">
                <Input
                  label="Start"
                  type="time"
                  value={e.start}
                  onChange={(ev) => updateEntry(idx, "start", ev.target.value)}
                />
              </div>
              <div className="col-span-3">
                <Input
                  label="End"
                  type="time"
                  value={e.end}
                  onChange={(ev) => updateEntry(idx, "end", ev.target.value)}
                />
              </div>
              <div className="col-span-2 flex gap-2">
                <Button variant="outline" onClick={() => removeEntry(idx)}>
                  Remove
                </Button>
              </div>
            </div>
          ))}
          <Button variant="outline" onClick={addEntry}>
            + Add Slot
          </Button>
        </div>
        <div className="flex justify-end gap-3 pt-4">
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleSave} disabled={loading}>
            <Save size={16} className="mr-2" /> Save
          </Button>
        </div>
      </div>
    </Modal>
  );
};

export default ScheduleEditor;
