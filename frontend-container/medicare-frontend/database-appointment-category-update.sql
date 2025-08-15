-- Database modification for appointment categories
-- Add this to your database schema

-- Add appointment category to distinguish visit types
ALTER TABLE Schedule_Appointment 
ADD Appointment_Category NVARCHAR(50) DEFAULT 'consultation';

-- Update existing appointments to have 'consultation' category
UPDATE Schedule_Appointment 
SET Appointment_Category = 'consultation' 
WHERE Appointment_Category IS NULL;

-- Create appointment categories lookup table (optional but recommended)
CREATE TABLE Appointment_Category (
    Id VARCHAR(36) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Can_Book_By_Patient BIT DEFAULT 0,
    Can_Book_By_Receptionist BIT DEFAULT 1,
    Default_Duration_Minutes INT DEFAULT 30,
    Created_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Appointment_Category_pk PRIMARY KEY (Id),
    CONSTRAINT Appointment_Category_Name_unique UNIQUE (Name)
);

-- Insert predefined appointment categories
INSERT INTO Appointment_Category (Id, Name, Description, Can_Book_By_Patient, Can_Book_By_Receptionist, Default_Duration_Minutes) VALUES
    (CAST(NEWID() AS VARCHAR(36)), 'consultation', 'General consultation visit', 1, 1, 30),
    (CAST(NEWID() AS VARCHAR(36)), 'emergency', 'Emergency appointment', 0, 1, 45),
    (CAST(NEWID() AS VARCHAR(36)), 'follow-up', 'Follow-up appointment', 0, 1, 20),
    (CAST(NEWID() AS VARCHAR(36)), 'procedure', 'Medical procedure', 0, 1, 60),
    (CAST(NEWID() AS VARCHAR(36)), 'surgery', 'Surgical procedure', 0, 1, 120),
    (CAST(NEWID() AS VARCHAR(36)), 'check-up', 'Routine check-up', 0, 1, 30),
    (CAST(NEWID() AS VARCHAR(36)), 'vaccination', 'Vaccination appointment', 0, 1, 15);

-- Optional: Add foreign key constraint if you want to enforce category validation
-- ALTER TABLE Schedule_Appointment 
-- ADD CONSTRAINT Schedule_Appointment_Category 
-- FOREIGN KEY (Appointment_Category) REFERENCES Appointment_Category (Name);
