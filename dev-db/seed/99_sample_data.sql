-- Roles
INSERT INTO dbo.Role (Name, Description) VALUES 
('Admin', 'System Administrator'),
('Doctor', 'Medical Doctor'),
('Patient', 'Patient User');

-- Sample users with authentication
DECLARE @adminUserId VARCHAR(36) = CAST(NEWID() AS VARCHAR(36)),
        @docUserId VARCHAR(36) = CAST(NEWID() AS VARCHAR(36)),
        @patUserId VARCHAR(36) = CAST(NEWID() AS VARCHAR(36));

-- Insert users with authentication
INSERT INTO dbo.[User](Id, Role_Id, Username, PasswordHash, Is_Active)
VALUES 
    (@adminUserId, (SELECT Id FROM dbo.Role WHERE Name='Admin'), 'admin', '$2a$11$K8jWNl3FzjzR1KQJS1qZsO3LJVtF5LqgZTJfVJQYRVJHZVFQTZXRG', 1), -- password: admin123
    (@docUserId, (SELECT Id FROM dbo.Role WHERE Name='Doctor'), 'doctor1', '$2a$11$K8jWNl3FzjzR1KQJS1qZsO3LJVtF5LqgZTJfVJQYRVJHZVFQTZXRG', 1), -- password: doctor123
    (@patUserId, (SELECT Id FROM dbo.Role WHERE Name='Patient'), 'patient1', '$2a$11$K8jWNl3FzjzR1KQJS1qZsO3LJVtF5LqgZTJfVJQYRVJHZVFQTZXRG', 1); -- password: patient123

-- Insert user profiles
INSERT INTO dbo.User_Profile(User_Id, FirstName, LastName, Email, Phone, DateOfBirth)
VALUES 
    (@adminUserId, 'System', 'Administrator', 'admin@medicare.com', '555-0100', '1980-01-01'),
    (@docUserId, 'Dr. John', 'Smith', 'doctor1@medicare.com', '555-0101', '1975-05-15'),
    (@patUserId, 'Jane', 'Doe', 'patient1@medicare.com', '555-0102', '1985-06-15');

-- Insert doctor-specific data
INSERT INTO dbo.Doctor (Id, License_Num, Years_Experi, Biography)
VALUES (@docUserId, 'PL-12345', 10, 'Experienced family medicine doctor');

-- Insert patient-specific data
INSERT INTO dbo.Patient (General_Doctor_Id, Medical_Record_Numbe, Blood_Type)
VALUES (@docUserId, 'MRN-0001', 'O+');

-- Appointment status
INSERT INTO dbo.Schedule_Appointment_Status (Name, Color_Code)
VALUES ('Booked', '#2ecc71'), ('Cancelled', '#e74c3c'), ('Pending', '#f1c40f');
