import React, { useCallback, useEffect, useState } from 'react';
import axios from 'axios';
import {
  Activity,
  Droplets,
  FileText,
  Heart,
  Ruler,
  Scale,
  Thermometer,
  Wind,
} from 'lucide-react';

import { Modal } from '../../../shared/components/Modal/Modal';
import { useDebounce } from '../../../shared/hooks/useDebounce';

// ICD-10 type
interface Icd10Code {
  code: string;
  title: string;
  status?: string;
}

// Vital signs structure
interface VitalSigns {
  bloodPressureSystolic?: string;
  bloodPressureDiastolic?: string;
  heartRate?: string;
  temperature?: string;
  spO2?: string;
  respiratoryRate?: string;
  weight?: string;
  height?: string;
}

// Visit Note data structure
interface VisitNoteData {
  documentId?: string;
  symptoms: string;
  findings: string;
  diagnosis: string;
  treatmentPlan: string;
  recommendations: string;
  vitalSignsJson: string;
  followUpDate: string;
}

interface VisitNoteModalProps {
  readonly isOpen: boolean;
  readonly onClose: () => void;
  readonly appointmentId: string;
  readonly patientName: string;
  readonly appointmentDate: string;
  readonly isEditMode: boolean;
  readonly isReadOnly?: boolean;
  readonly existingVisitNote?: VisitNoteData | null;
  readonly onSave: (data: VisitNoteData) => Promise<void>;
}

// Micro-field component for vital signs
interface VitalSignFieldProps {
  readonly icon: React.ReactNode;
  readonly label: string;
  readonly value: string;
  readonly onChange: (value: string) => void;
  readonly placeholder: string;
  readonly unit?: string;
  readonly className?: string;
  readonly readOnly?: boolean;
}

function VitalSignField({
  icon,
  label,
  value,
  onChange,
  placeholder,
  unit,
  className = '',
  readOnly = false,
}: Readonly<VitalSignFieldProps>) {
  return (
    <div className={`flex flex-col ${className}`}>
      <label className="text-xs text-gray-500 mb-1 flex items-center gap-1">
        {icon}
        {label}
      </label>
      <div className="relative">
        <input
          type="text"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder={placeholder}
          readOnly={readOnly}
          className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
        />
        {unit && (
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-xs text-gray-400">
            {unit}
          </span>
        )}
      </div>
    </div>
  );
}

export function VisitNoteModal({
  isOpen,
  onClose,
  appointmentId,
  patientName,
  appointmentDate,
  isEditMode,
  isReadOnly = false,
  existingVisitNote,
  onSave,
}: VisitNoteModalProps) {
  // Form state
  const [symptoms, setSymptoms] = useState('');
  const [findings, setFindings] = useState('');
  const [diagnosis, setDiagnosis] = useState('');
  const [treatmentPlan, setTreatmentPlan] = useState('');
  const [recommendations, setRecommendations] = useState('');
  const [followUpDate, setFollowUpDate] = useState('');

  // Vital signs state
  const [vitalSigns, setVitalSigns] = useState<VitalSigns>({
    bloodPressureSystolic: '',
    bloodPressureDiastolic: '',
    heartRate: '',
    temperature: '',
    spO2: '',
    respiratoryRate: '',
    weight: '',
    height: '',
  });

  // ICD-10 search state
  const [icd10SearchTerm, setIcd10SearchTerm] = useState('');
  const [icd10Results, setIcd10Results] = useState<Icd10Code[]>([]);
  const [selectedIcd10Codes, setSelectedIcd10Codes] = useState<Icd10Code[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [showIcd10Dropdown, setShowIcd10Dropdown] = useState(false);

  // Loading state
  const [isSaving, setIsSaving] = useState(false);

  // Parse existing vital signs from JSON
  const parseVitalSigns = useCallback((json: string): VitalSigns => {
    try {
      return JSON.parse(json) as VitalSigns;
    } catch {
      return {};
    }
  }, []);

  // Parse existing diagnosis to ICD-10 codes
  const parseDiagnosisToIcd10 = useCallback((diagnosisText: string): Icd10Code[] => {
    if (!diagnosisText) return [];
    
    // Parse format: "A01.0 - Typhoid fever, B02.9 - Zoster"
    const codes: Icd10Code[] = [];
    const parts = diagnosisText.split(',').map(p => p.trim());
    
    const regex = /^([A-Z]\d{2}(?:\.\d{1,2})?)\s*-\s*(.+)$/;
    for (const part of parts) {
      const match = regex.exec(part);
      if (match) {
        codes.push({ code: match[1], title: match[2] });
      }
    }
    
    return codes;
  }, []);

  // Initialize form with existing data
  useEffect(() => {
    if (existingVisitNote) {
      setSymptoms(existingVisitNote.symptoms || '');
      setFindings(existingVisitNote.findings || '');
      setDiagnosis(existingVisitNote.diagnosis || '');
      setTreatmentPlan(existingVisitNote.treatmentPlan || '');
      setRecommendations(existingVisitNote.recommendations || '');
      setFollowUpDate(existingVisitNote.followUpDate?.split('T')[0] || '');
      
      if (existingVisitNote.vitalSignsJson) {
        setVitalSigns(parseVitalSigns(existingVisitNote.vitalSignsJson));
      }
      
      // Parse existing diagnosis to ICD-10 codes
      const existingCodes = parseDiagnosisToIcd10(existingVisitNote.diagnosis || '');
      setSelectedIcd10Codes(existingCodes);
    } else {
      // Reset form
      setSymptoms('');
      setFindings('');
      setDiagnosis('');
      setTreatmentPlan('');
      setRecommendations('');
      setFollowUpDate('');
      setVitalSigns({});
      setSelectedIcd10Codes([]);
    }
    setIcd10SearchTerm('');
    setIcd10Results([]);
  }, [existingVisitNote, isOpen, parseVitalSigns, parseDiagnosisToIcd10]);

  // Debounced search term
  const debouncedSearchTerm = useDebounce(icd10SearchTerm, 300);

  // ICD-10 search effect - triggered by debounced term
  useEffect(() => {
    const searchIcd10 = async (query: string) => {
      if (query.length < 2) {
        setIcd10Results([]);
        return;
      }

      setIsSearching(true);
      try {
        const response = await axios.get<Icd10Code[]>(`/api/catalog/icd10`, {
          params: { q: query },
        });
        setIcd10Results(response.data || []);
        setShowIcd10Dropdown(true);
      } catch (error) {
        console.error('Error searching ICD-10 codes:', error);
        setIcd10Results([]);
      } finally {
        setIsSearching(false);
      }
    };

    searchIcd10(debouncedSearchTerm);
  }, [debouncedSearchTerm]);

  // Handle ICD-10 search input change
  const handleIcd10SearchChange = (value: string) => {
    setIcd10SearchTerm(value);
  };

  // Handle ICD-10 code selection
  const handleSelectIcd10 = (code: Icd10Code) => {
    if (!selectedIcd10Codes.some(c => c.code === code.code)) {
      const newCodes = [...selectedIcd10Codes, code];
      setSelectedIcd10Codes(newCodes);
      // Update diagnosis text
      setDiagnosis(newCodes.map(c => `${c.code} - ${c.title}`).join(', '));
    }
    setIcd10SearchTerm('');
    setIcd10Results([]);
    setShowIcd10Dropdown(false);
  };

  // Handle removing ICD-10 code
  const handleRemoveIcd10 = (codeToRemove: string) => {
    const newCodes = selectedIcd10Codes.filter(c => c.code !== codeToRemove);
    setSelectedIcd10Codes(newCodes);
    setDiagnosis(newCodes.map(c => `${c.code} - ${c.title}`).join(', '));
  };

  // Update vital sign
  const updateVitalSign = (field: keyof VitalSigns, value: string) => {
    setVitalSigns(prev => ({ ...prev, [field]: value }));
  };

  // Handle save
  const handleSave = async () => {
    if (isReadOnly) {
      onClose();
      return;
    }
    setIsSaving(true);
    try {
      const vitalSignsJson = JSON.stringify(vitalSigns);
      
      await onSave({
        symptoms,
        findings,
        diagnosis,
        treatmentPlan,
        recommendations,
        vitalSignsJson,
        followUpDate: followUpDate || '',
      });
      
      onClose();
    } catch (error) {
      console.error('Error saving visit note:', error);
    } finally {
      setIsSaving(false);
    }
  };

  let modalTitle = 'Generate Visit Note';
  if (isReadOnly) {
    modalTitle = 'View Visit Note';
  } else if (isEditMode) {
    modalTitle = 'Edit Visit Note';
  }

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={modalTitle}
      size="lg"
    >
      <div className="space-y-4 overflow-y-auto px-2 pb-2" style={{ maxHeight: 'calc(80vh - 180px)' }}>
        {/* Patient Info Header */}
        <div className="bg-blue-50 p-3 rounded-lg border border-blue-100">
          <div className="flex items-center gap-2 text-blue-700">
            <FileText className="w-4 h-4" />
            <span className="font-medium">{patientName}</span>
          </div>
          <div className="text-sm text-blue-600 mt-1">
            Appointment: {new Date(appointmentDate).toLocaleDateString('en-US', {
              weekday: 'long',
              year: 'numeric',
              month: 'long',
              day: 'numeric',
              hour: '2-digit',
              minute: '2-digit',
            })}
          </div>
          <div className="text-xs text-blue-500 mt-1">ID: {appointmentId}</div>
        </div>

        {/* Symptoms */}
        <div>
          <label htmlFor="symptoms" className="block text-sm font-medium text-gray-700 mb-1">
            Symptoms / Chief Complaint
          </label>
          <textarea
            id="symptoms"
            value={symptoms}
            onChange={(e) => setSymptoms(e.target.value)}
            placeholder="Describe the patient's symptoms and chief complaint..."
            readOnly={isReadOnly}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            rows={3}
          />
        </div>

        {/* Vital Signs - Second field, micro-fields layout */}
        <div>
          <span className="block text-sm font-medium text-gray-700 mb-2">
            Vital Signs <span className="text-gray-400 text-xs">(optional)</span>
          </span>
          <div className="bg-gray-50 p-3 rounded-lg border border-gray-200">
            {/* Row 1: Blood Pressure, Heart Rate, Temperature */}
            <div className="grid grid-cols-4 gap-3 mb-3">
              {/* Blood Pressure - takes 2 columns */}
              <div className="col-span-2">
                <label className="text-xs text-gray-500 mb-1 flex items-center gap-1">
                  <Activity className="w-3 h-3" />
                  Blood Pressure
                </label>
                <div className="flex items-center gap-1">
                  <input
                    type="text"
                    value={vitalSigns.bloodPressureSystolic || ''}
                    onChange={(e) => updateVitalSign('bloodPressureSystolic', e.target.value)}
                    placeholder="120"
                    readOnly={isReadOnly}
                    className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
                  />
                  <span className="text-gray-400">/</span>
                  <input
                    type="text"
                    value={vitalSigns.bloodPressureDiastolic || ''}
                    onChange={(e) => updateVitalSign('bloodPressureDiastolic', e.target.value)}
                    placeholder="80"
                    readOnly={isReadOnly}
                    className="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
                  />
                  <span className="text-xs text-gray-400 whitespace-nowrap">mmHg</span>
                </div>
              </div>

              <VitalSignField
                icon={<Heart className="w-3 h-3" />}
                label="Heart Rate"
                value={vitalSigns.heartRate || ''}
                onChange={(v) => updateVitalSign('heartRate', v)}
                placeholder="72"
                unit="bpm"
                readOnly={isReadOnly}
              />

              <VitalSignField
                icon={<Thermometer className="w-3 h-3" />}
                label="Temperature"
                value={vitalSigns.temperature || ''}
                onChange={(v) => updateVitalSign('temperature', v)}
                placeholder="36.6"
                unit="°C"
                readOnly={isReadOnly}
              />
            </div>

            {/* Row 2: SpO2, Respiratory Rate, Weight, Height */}
            <div className="grid grid-cols-4 gap-3">
              <VitalSignField
                icon={<Droplets className="w-3 h-3" />}
                label="SpO2"
                value={vitalSigns.spO2 || ''}
                onChange={(v) => updateVitalSign('spO2', v)}
                placeholder="98"
                unit="%"
                readOnly={isReadOnly}
              />

              <VitalSignField
                icon={<Wind className="w-3 h-3" />}
                label="Resp. Rate"
                value={vitalSigns.respiratoryRate || ''}
                onChange={(v) => updateVitalSign('respiratoryRate', v)}
                placeholder="16"
                unit="/min"
                readOnly={isReadOnly}
              />

              <VitalSignField
                icon={<Scale className="w-3 h-3" />}
                label="Weight"
                value={vitalSigns.weight || ''}
                onChange={(v) => updateVitalSign('weight', v)}
                placeholder="70"
                unit="kg"
                readOnly={isReadOnly}
              />

              <VitalSignField
                icon={<Ruler className="w-3 h-3" />}
                label="Height"
                value={vitalSigns.height || ''}
                onChange={(v) => updateVitalSign('height', v)}
                placeholder="175"
                unit="cm"
                readOnly={isReadOnly}
              />
            </div>
          </div>
        </div>

        {/* Findings */}
        <div>
          <label htmlFor="findings" className="block text-sm font-medium text-gray-700 mb-1">
            Clinical Findings
          </label>
          <textarea
            id="findings"
            value={findings}
            onChange={(e) => setFindings(e.target.value)}
            placeholder="Document your clinical findings from the examination..."
            readOnly={isReadOnly}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            rows={3}
          />
        </div>

        {/* Diagnosis with ICD-10 Search */}
        <div>
          <label htmlFor="icd10Search" className="block text-sm font-medium text-gray-700 mb-1">
            Diagnosis (ICD-10)
          </label>
          
          {/* Selected ICD-10 codes */}
          {selectedIcd10Codes.length > 0 && (
            <div className="flex flex-wrap gap-2 mb-2">
              {selectedIcd10Codes.map((code) => (
                <span
                  key={code.code}
                  className="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-800 text-sm rounded-full"
                >
                  <span className="font-medium">{code.code}</span>
                  <span className="text-blue-600">-</span>
                  <span className="truncate max-w-[200px]">{code.title}</span>
                  <button
                    type="button"
                    onClick={() => handleRemoveIcd10(code.code)}
                    disabled={isReadOnly}
                    className="ml-1 text-blue-600 hover:text-blue-800 disabled:opacity-50"
                  >
                    ×
                  </button>
                </span>
              ))}
            </div>
          )}

          {/* ICD-10 Search Input */}
          <div className="relative">
            <input
              id="icd10Search"
              type="text"
              value={icd10SearchTerm}
              onChange={(e) => handleIcd10SearchChange(e.target.value)}
              onFocus={() => icd10Results.length > 0 && setShowIcd10Dropdown(true)}
              placeholder="Search ICD-10 codes (e.g., 'diabetes' or 'E11')..."
              disabled={isReadOnly}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            />
            {isSearching && (
              <div className="absolute right-3 top-1/2 -translate-y-1/2">
                <div className="animate-spin rounded-full h-4 w-4 border-2 border-blue-500 border-t-transparent"></div>
              </div>
            )}

            {/* ICD-10 Search Results Dropdown */}
            {!isReadOnly && showIcd10Dropdown && icd10Results.length > 0 && (
              <div className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-md shadow-lg max-h-60 overflow-y-auto">
                {icd10Results.map((result) => (
                  <button
                    key={result.code}
                    type="button"
                    onClick={() => handleSelectIcd10(result)}
                    className="w-full px-3 py-2 text-left hover:bg-blue-50 flex items-start gap-2 border-b border-gray-100 last:border-0"
                  >
                    <span className="font-mono text-sm font-medium text-blue-600 whitespace-nowrap">
                      {result.code}
                    </span>
                    <span className="text-sm text-gray-700">{result.title}</span>
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Hidden input to store full diagnosis text */}
          <input type="hidden" value={diagnosis} />
          
          <p className="text-xs text-gray-500 mt-1">
            Search and select ICD-10 diagnosis codes. Type at least 2 characters to search.
          </p>
        </div>

        {/* Treatment Plan */}
        <div>
          <label htmlFor="treatmentPlan" className="block text-sm font-medium text-gray-700 mb-1">
            Treatment Plan
          </label>
          <textarea
            id="treatmentPlan"
            value={treatmentPlan}
            onChange={(e) => setTreatmentPlan(e.target.value)}
            placeholder="Describe the treatment plan including medications, procedures..."
            readOnly={isReadOnly}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            rows={3}
          />
        </div>

        {/* Recommendations */}
        <div>
          <label htmlFor="recommendations" className="block text-sm font-medium text-gray-700 mb-1">
            Recommendations
          </label>
          <textarea
            id="recommendations"
            value={recommendations}
            onChange={(e) => setRecommendations(e.target.value)}
            placeholder="Additional recommendations for the patient..."
            readOnly={isReadOnly}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
            rows={2}
          />
        </div>

        {/* Follow-up Date */}
        <div>
          <label htmlFor="followUpDate" className="block text-sm font-medium text-gray-700 mb-1">
            Follow-up Date <span className="text-gray-400 text-xs">(optional)</span>
          </label>
          <input
            id="followUpDate"
            type="date"
            value={followUpDate}
            onChange={(e) => setFollowUpDate(e.target.value)}
            disabled={isReadOnly}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-1 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex justify-end gap-3 mt-6 pt-4 border-t">
        <button
          type="button"
          onClick={onClose}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
          disabled={isSaving}
        >
          {isReadOnly ? 'Close' : 'Cancel'}
        </button>
        {!isReadOnly && (
          <button
            type="button"
            onClick={handleSave}
            disabled={isSaving}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50"
          >
            {(() => {
              if (isSaving) return 'Saving...';
              return isEditMode ? 'Update Visit Note' : 'Save Visit Note';
            })()}
          </button>
        )}
      </div>
    </Modal>
  );
}

export type { Icd10Code,VisitNoteData, VitalSigns };
