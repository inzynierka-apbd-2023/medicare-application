-- Medicare Application Database Schema
-- Created: 2025-08-06
-- Optimized for SQL parser compatibility with full GUID support

-- =============================================
-- CORE TABLES
-- =============================================

-- Table: Role
CREATE TABLE Role (
    Id VARCHAR(36) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CONSTRAINT Role_pk PRIMARY KEY (Id),
    CONSTRAINT Role_Name_unique UNIQUE (Name)
);

-- Table: Schedule
CREATE TABLE Schedule (
    Id VARCHAR(36) NOT NULL,
    Name NVARCHAR(200),
    Description NVARCHAR(500),
    Created_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Schedule_pk PRIMARY KEY (Id)
);

-- Table: User (Core user entity)
CREATE TABLE [User] (
    Id VARCHAR(36) NOT NULL,
    Role_Id VARCHAR(36) NOT NULL,
    Schedule_Id VARCHAR(36) NOT NULL,
    Created_At DATETIME DEFAULT GETDATE(),
    Updated_At DATETIME DEFAULT GETDATE(),
    Is_Active BIT DEFAULT 1,
    CONSTRAINT User_pk PRIMARY KEY (Id),
    CONSTRAINT User_Role FOREIGN KEY (Role_Id) REFERENCES Role (Id),
    CONSTRAINT User_Schedule FOREIGN KEY (Schedule_Id) REFERENCES Schedule (Id)
);

-- Table: User_Profile (Personal information matching frontend User interface)
CREATE TABLE User_Profile (
    User_Id VARCHAR(36) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    DateOfBirth DATE,
    Gender NVARCHAR(20),
    Avatar_Url NVARCHAR(500),
    Address_Line1 NVARCHAR(200),
    Address_Line2 NVARCHAR(200),
    City NVARCHAR(100),
    State NVARCHAR(100),
    ZipCode NVARCHAR(20),
    Country NVARCHAR(100),
    Created_At DATETIME DEFAULT GETDATE(),
    Updated_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT User_Profile_pk PRIMARY KEY (User_Id),
    CONSTRAINT User_Profile_User FOREIGN KEY (User_Id) REFERENCES [User] (Id),
    CONSTRAINT User_Profile_Email_unique UNIQUE (Email)
);

-- Table: Service
CREATE TABLE Service (
    Id VARCHAR(36) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Duration_Minutes INT DEFAULT 30,
    Is_Active BIT DEFAULT 1,
    Created_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Service_pk PRIMARY KEY (Id)
);

-- Table: Specialization
CREATE TABLE Specialization (
    Id VARCHAR(36) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Service_Id VARCHAR(36) NOT NULL,
    Is_Active BIT DEFAULT 1,
    CONSTRAINT Specialization_pk PRIMARY KEY (Id),
    CONSTRAINT Specialization_Service FOREIGN KEY (Service_Id) REFERENCES Service (Id)
);

-- =============================================
-- USER TYPE TABLES
-- =============================================

-- Table: Doctor
CREATE TABLE Doctor (
    Id VARCHAR(36) NOT NULL,
    License_Number NVARCHAR(100),
    Years_Experience INT,
    Biography NVARCHAR(2000),
    Office_Address NVARCHAR(500),
    CONSTRAINT Doctor_pk PRIMARY KEY (Id),
    CONSTRAINT Doctor_User FOREIGN KEY (Id) REFERENCES [User] (Id)
);

-- Table: Patient
CREATE TABLE Patient (
    Id VARCHAR(36) NOT NULL,
    General_Doctor_Id VARCHAR(36) NOT NULL,
    Medical_Record_Number NVARCHAR(100),
    Blood_Type NVARCHAR(10),
    Height_cm DECIMAL(5,2),
    Weight_kg DECIMAL(5,2),
    CONSTRAINT Patient_pk PRIMARY KEY (Id),
    CONSTRAINT Patient_User FOREIGN KEY (Id) REFERENCES [User] (Id),
    CONSTRAINT Patient_Doctor FOREIGN KEY (General_Doctor_Id) REFERENCES Doctor (Id)
);

-- Table: Receptionist
CREATE TABLE Receptionist (
    Id VARCHAR(36) NOT NULL,
    Department NVARCHAR(200),
    CONSTRAINT Receptionist_pk PRIMARY KEY (Id),
    CONSTRAINT Receptionist_User FOREIGN KEY (Id) REFERENCES [User] (Id)
);

-- Table: Doctor_Specialization
CREATE TABLE Doctor_Specialization (
    Id VARCHAR(36) NOT NULL,
    Doctor_Id VARCHAR(36) NOT NULL,
    Specialization_Id VARCHAR(36) NOT NULL,
    Is_Primary BIT DEFAULT 0,
    Certified_Date DATE,
    CONSTRAINT Doctor_Specialization_pk PRIMARY KEY (Id),
    CONSTRAINT Doctor_Specialization_Doctor FOREIGN KEY (Doctor_Id) REFERENCES Doctor (Id),
    CONSTRAINT Doctor_Specialization_Specialization FOREIGN KEY (Specialization_Id) REFERENCES Specialization (Id)
);

-- =============================================
-- PATIENT EXTENDED INFORMATION
-- =============================================

-- Table: Insurance
CREATE TABLE Insurance (
    Id VARCHAR(36) NOT NULL,
    Patient_Id VARCHAR(36) NOT NULL,
    Provider_Name NVARCHAR(200) NOT NULL,
    Policy_Number NVARCHAR(100) NOT NULL,
    Group_Number NVARCHAR(100),
    Valid_From DATE NOT NULL,
    Valid_To DATE,
    Is_Primary BIT DEFAULT 1,
    Is_Active BIT DEFAULT 1,
    CONSTRAINT Insurance_pk PRIMARY KEY (Id),
    CONSTRAINT Insurance_Patient FOREIGN KEY (Patient_Id) REFERENCES Patient (Id)
);

-- Table: Emergency_Contact
CREATE TABLE Emergency_Contact (
    Id VARCHAR(36) NOT NULL,
    Patient_Id VARCHAR(36) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Relationship NVARCHAR(100) NOT NULL,
    Is_Primary BIT DEFAULT 0,
    Created_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Emergency_Contact_pk PRIMARY KEY (Id),
    CONSTRAINT Emergency_Contact_Patient FOREIGN KEY (Patient_Id) REFERENCES Patient (Id)
);

-- Table: Medical_Condition
CREATE TABLE Medical_Condition (
    Id VARCHAR(36) NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Category NVARCHAR(100),
    Is_Chronic BIT DEFAULT 0,
    CONSTRAINT Medical_Condition_pk PRIMARY KEY (Id),
    CONSTRAINT Medical_Condition_Code_unique UNIQUE (Code)
);

-- Table: Patient_Medical_Condition
CREATE TABLE Patient_Medical_Condition (
    Id VARCHAR(36) NOT NULL,
    Patient_Id VARCHAR(36) NOT NULL,
    Medical_Condition_Id VARCHAR(36) NOT NULL,
    Diagnosed_Date DATE,
    Status NVARCHAR(50) DEFAULT 'Active',
    Severity NVARCHAR(50),
    Notes NVARCHAR(1000),
    Created_At DATETIME DEFAULT GETDATE(),
    Updated_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Patient_Medical_Condition_pk PRIMARY KEY (Id),
    CONSTRAINT PMC_Patient FOREIGN KEY (Patient_Id) REFERENCES Patient (Id),
    CONSTRAINT PMC_Condition FOREIGN KEY (Medical_Condition_Id) REFERENCES Medical_Condition (Id)
);

-- Table: Patient_Status
CREATE TABLE Patient_Status (
    Id VARCHAR(36) NOT NULL,
    Patient_Id VARCHAR(36) NOT NULL,
    Doctor_Id VARCHAR(36) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Assigned_Date DATE NOT NULL,
    Status_Changed_Date DATE DEFAULT GETDATE(),
    Urgent_Notes NVARCHAR(1000),
    Created_At DATETIME DEFAULT GETDATE(),
    Updated_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Patient_Status_pk PRIMARY KEY (Id),
    CONSTRAINT Patient_Status_Patient FOREIGN KEY (Patient_Id) REFERENCES Patient (Id),
    CONSTRAINT Patient_Status_Doctor FOREIGN KEY (Doctor_Id) REFERENCES Doctor (Id)
);

-- =============================================
-- SCHEDULING SYSTEM
-- =============================================

-- Table: Doctor_Schedule
CREATE TABLE Doctor_Schedule (
    Id VARCHAR(36) NOT NULL,
    Doctor_Id VARCHAR(36) NOT NULL,
    Day_Of_Week TINYINT NOT NULL,
    Start_Time TIME NOT NULL,
    End_Time TIME NOT NULL,
    Is_Available BIT DEFAULT 1,
    Valid_From DATE NOT NULL,
    Valid_To DATE,
    Break_Start_Time TIME,
    Break_End_Time TIME,
    CONSTRAINT Doctor_Schedule_pk PRIMARY KEY (Id),
    CONSTRAINT Doctor_Schedule_Doctor FOREIGN KEY (Doctor_Id) REFERENCES Doctor (Id)
);

-- Table: Time_Slot
CREATE TABLE Time_Slot (
    Id VARCHAR(36) NOT NULL,
    Doctor_Id VARCHAR(36) NOT NULL,
    Start_DateTime DATETIME NOT NULL,
    End_DateTime DATETIME NOT NULL,
    Is_Available BIT DEFAULT 1,
    Duration_Minutes INT DEFAULT 30,
    Slot_Type NVARCHAR(50) DEFAULT 'Regular',
    Created_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Time_Slot_pk PRIMARY KEY (Id),
    CONSTRAINT Time_Slot_Doctor FOREIGN KEY (Doctor_Id) REFERENCES Doctor (Id)
);

-- Table: Schedule_Appointment_Status
CREATE TABLE Schedule_Appointment_Status (
    Id VARCHAR(36) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Color_Code NVARCHAR(7),
    CONSTRAINT Schedule_Appointment_Status_pk PRIMARY KEY (Id)
);

-- Table: Schedule_Appointment
CREATE TABLE Schedule_Appointment (
    Id VARCHAR(36) NOT NULL,
    Schedule_Id VARCHAR(36) NOT NULL,
    Time_Slot_Id VARCHAR(36),
    Day DATETIME NOT NULL,
    Duration_Minutes INT NOT NULL,
    Room NVARCHAR(255),
    Description NVARCHAR(1000),
    Appointment_Type NVARCHAR(50) DEFAULT 'in-person',
    Doctor_User_Id VARCHAR(36) NOT NULL,
    Patient_User_Id VARCHAR(36) NOT NULL,
    Receptionist_User_Id VARCHAR(36),
    Schedule_Appointment_Status_Id VARCHAR(36) NOT NULL,
    Total_Cost DECIMAL(10,2),
    Created_At DATETIME DEFAULT GETDATE(),
    Updated_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Schedule_Appointment_pk PRIMARY KEY (Id),
    CONSTRAINT Schedule_Appointment_Schedule FOREIGN KEY (Schedule_Id) REFERENCES Schedule (Id),
    CONSTRAINT Schedule_Appointment_Time_Slot FOREIGN KEY (Time_Slot_Id) REFERENCES Time_Slot (Id),
    CONSTRAINT Schedule_Appointment_Doctor FOREIGN KEY (Doctor_User_Id) REFERENCES Doctor (Id),
    CONSTRAINT Schedule_Appointment_Patient FOREIGN KEY (Patient_User_Id) REFERENCES Patient (Id),
    CONSTRAINT Schedule_Appointment_Receptionist FOREIGN KEY (Receptionist_User_Id) REFERENCES Receptionist (Id),
    CONSTRAINT Schedule_Appointment_Status FOREIGN KEY (Schedule_Appointment_Status_Id) REFERENCES Schedule_Appointment_Status (Id)
);

-- =============================================
-- DOCUMENTS SYSTEM
-- =============================================

-- Table: Document_Type
CREATE TABLE Document_Type (
    Id VARCHAR(36) NOT NULL,
    Code NVARCHAR(30) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Template_Path NVARCHAR(500),
    CONSTRAINT Document_Type_pk PRIMARY KEY (Id),
    CONSTRAINT Document_Type_Code_unique UNIQUE (Code)
);

-- Table: Document
CREATE TABLE Document (
    Id VARCHAR(36) NOT NULL,
    Created_At DATETIME NOT NULL,
    Notes NVARCHAR(1000),
    Type INT NOT NULL,
    Document_Type_Id VARCHAR(36) NOT NULL,
    Patient_Id VARCHAR(36) NOT NULL,
    Doctor_Id VARCHAR(36) NOT NULL,
    File_Path NVARCHAR(1000),
    File_Size_Bytes BIGINT,
    CONSTRAINT Document_pk PRIMARY KEY (Id),
    CONSTRAINT Document_Document_Type FOREIGN KEY (Document_Type_Id) REFERENCES Document_Type (Id),
    CONSTRAINT Document_Patient FOREIGN KEY (Patient_Id) REFERENCES Patient (Id),
    CONSTRAINT Document_Doctor FOREIGN KEY (Doctor_Id) REFERENCES Doctor (Id)
);

-- Table: Appointment
CREATE TABLE Appointment (
    Id VARCHAR(36) NOT NULL,
    Schedule_Appointment_Id VARCHAR(36) NOT NULL,
    Description NVARCHAR(1000),
    Created_At DATETIME DEFAULT GETDATE(),
    CONSTRAINT Appointment_pk PRIMARY KEY (Id),
    CONSTRAINT Appointment_Schedule_Appointment FOREIGN KEY (Schedule_Appointment_Id) REFERENCES Schedule_Appointment (Id)
);

-- Table: Documents_Assigned
CREATE TABLE Documents_Assigned (
    Id VARCHAR(36) NOT NULL,
    Assigned_At DATETIME NOT NULL,
    Appointment_Id VARCHAR(36) NOT NULL,
    Document_Id VARCHAR(36) NOT NULL,
    CONSTRAINT Documents_Assigned_pk PRIMARY KEY (Id),
    CONSTRAINT Documents_Assigned_Appointment FOREIGN KEY (Appointment_Id) REFERENCES Appointment (Id),
    CONSTRAINT Documents_Assigned_Document FOREIGN KEY (Document_Id) REFERENCES Document (Id)
);

-- =============================================
-- DOCUMENT SUBTYPES
-- =============================================

-- Table: Prescription
CREATE TABLE Prescription (
    Document_Id VARCHAR(36) NOT NULL,
    Medication NVARCHAR(200) NOT NULL,
    Dosage NVARCHAR(100) NOT NULL,
    Frequency NVARCHAR(100) NOT NULL,
    Duration_Days INT NOT NULL,
    Instructions NVARCHAR(MAX) NOT NULL,
    Pharmacy_Name NVARCHAR(200),
    Pharmacy_Phone NVARCHAR(20),
    Refills_Remaining INT DEFAULT 0,
    CONSTRAINT Prescription_pk PRIMARY KEY (Document_Id),
    CONSTRAINT Prescription_Document FOREIGN KEY (Document_Id) REFERENCES Document (Id)
);

-- Table: Referral
CREATE TABLE Referral (
    Document_Id VARCHAR(36) NOT NULL,
    Speciality NVARCHAR(100) NOT NULL,
    Referred_To NVARCHAR(255) NOT NULL,
    Valid_From DATETIME NOT NULL,
    Valid_To DATETIME NOT NULL,
    Reason NVARCHAR(1000),
    Urgency_Level NVARCHAR(50),
    CONSTRAINT Referral_pk PRIMARY KEY (Document_Id),
    CONSTRAINT Referral_Document FOREIGN KEY (Document_Id) REFERENCES Document (Id)
);

-- Table: Sick_Leave
CREATE TABLE Sick_Leave (
    Document_Id VARCHAR(36) NOT NULL,
    Start_Date DATETIME NOT NULL,
    End_Date DATETIME NOT NULL,
    Days_Off INT NOT NULL,
    Return_To_Work_Date DATETIME,
    Work_Restrictions NVARCHAR(1000),
    CONSTRAINT Sick_Leave_pk PRIMARY KEY (Document_Id),
    CONSTRAINT Sick_Leave_Document FOREIGN KEY (Document_Id) REFERENCES Document (Id)
);

-- Table: Visit_Document
CREATE TABLE Visit_Document (
    Document_Id VARCHAR(36) NOT NULL,
    Symptoms NVARCHAR(MAX) NOT NULL,
    Findings NVARCHAR(MAX) NOT NULL,
    Diagnosis NVARCHAR(MAX) NOT NULL,
    Recommendations NVARCHAR(MAX) NOT NULL,
    Vital_Signs NVARCHAR(1000),
    Treatment_Plan NVARCHAR(2000),
    Follow_Up_Date DATETIME,
    CONSTRAINT Visit_Document_pk PRIMARY KEY (Document_Id),
    CONSTRAINT Visit_Document_Document FOREIGN KEY (Document_Id) REFERENCES Document (Id)
);

-- =============================================
-- LAB RESULTS SYSTEM
-- =============================================

-- Table: Lab_Test_Type
CREATE TABLE Lab_Test_Type (
    Id VARCHAR(36) NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    Reference_Range NVARCHAR(200),
    Unit NVARCHAR(50),
    Category NVARCHAR(100),
    Normal_Min_Value DECIMAL(18,6),
    Normal_Max_Value DECIMAL(18,6),
    CONSTRAINT Lab_Test_Type_pk PRIMARY KEY (Id),
    CONSTRAINT Lab_Test_Type_Code_unique UNIQUE (Code)
);

-- Table: Lab_Results
CREATE TABLE Lab_Results (
    Document_Id VARCHAR(36) NOT NULL,
    Test_Type NVARCHAR(200),
    Test_Date DATETIME,
    Laboratory NVARCHAR(200),
    Overall_Status NVARCHAR(50),
    Interpretation NVARCHAR(MAX),
    Reference_Ranges NVARCHAR(1000),
    Technician_Name NVARCHAR(200),
    Doctor_Comments NVARCHAR(1000),
    CONSTRAINT Lab_Results_pk PRIMARY KEY (Document_Id),
    CONSTRAINT Lab_Results_Document FOREIGN KEY (Document_Id) REFERENCES Document (Id)
);

-- Table: Lab_Test_Result
CREATE TABLE Lab_Test_Result (
    Id VARCHAR(36) NOT NULL,
    Lab_Results_Document_Id VARCHAR(36) NOT NULL,
    Lab_Test_Type_Id VARCHAR(36) NOT NULL,
    Parameter_Name NVARCHAR(200) NOT NULL,
    Value NVARCHAR(100) NOT NULL,
    Numeric_Value DECIMAL(18,6),
    Unit NVARCHAR(50),
    Reference_Range NVARCHAR(200),
    Status NVARCHAR(50) NOT NULL,
    Notes NVARCHAR(1000),
    Is_Abnormal BIT DEFAULT 0,
    CONSTRAINT Lab_Test_Result_pk PRIMARY KEY (Id),
    CONSTRAINT Lab_Test_Result_Lab_Results FOREIGN KEY (Lab_Results_Document_Id) REFERENCES Lab_Results (Document_Id),
    CONSTRAINT Lab_Test_Result_Test_Type FOREIGN KEY (Lab_Test_Type_Id) REFERENCES Lab_Test_Type (Id)
);

-- =============================================
-- MESSAGING SYSTEM
-- =============================================

-- Table: Conversation
CREATE TABLE Conversation (
    Id VARCHAR(36) NOT NULL,
    Doctor_User_Id VARCHAR(36) NOT NULL,
    Patient_User_Id VARCHAR(36) NOT NULL,
    Subject NVARCHAR(500),
    Created_At DATETIME DEFAULT GETDATE(),
    Last_Message_At DATETIME DEFAULT GETDATE(),
    Is_Active BIT DEFAULT 1,
    CONSTRAINT Conversation_pk PRIMARY KEY (Id),
    CONSTRAINT Conversation_Doctor FOREIGN KEY (Doctor_User_Id) REFERENCES Doctor (Id),
    CONSTRAINT Conversation_Patient FOREIGN KEY (Patient_User_Id) REFERENCES Patient (Id)
);

-- Table: Message
CREATE TABLE Message (
    Id VARCHAR(36) NOT NULL,
    Conversation_Id VARCHAR(36) NOT NULL,
    Sender_Id VARCHAR(36) NOT NULL,
    Sender_Name NVARCHAR(200) NOT NULL,
    Sender_Type NVARCHAR(20) NOT NULL,
    Receiver_Id VARCHAR(36) NOT NULL,
    Receiver_Name NVARCHAR(200) NOT NULL,
    Receiver_Type NVARCHAR(20) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE(),
    Is_Read BIT DEFAULT 0,
    Message_Type NVARCHAR(50) DEFAULT 'text',
    Attachment_Path NVARCHAR(1000),
    CONSTRAINT Message_pk PRIMARY KEY (Id),
    CONSTRAINT Message_Conversation FOREIGN KEY (Conversation_Id) REFERENCES Conversation (Id),
    CONSTRAINT Message_Sender FOREIGN KEY (Sender_Id) REFERENCES [User] (Id),
    CONSTRAINT Message_Receiver FOREIGN KEY (Receiver_Id) REFERENCES [User] (Id)
);

-- =============================================
-- NOTIFICATION SYSTEM
-- =============================================

-- Table: Notification
CREATE TABLE Notification (
    Id VARCHAR(36) NOT NULL,
    Recipient_User_Id VARCHAR(36) NOT NULL,
    Description NVARCHAR(255) NOT NULL,
    Type TINYINT NOT NULL,
    Creation_Date DATETIME NOT NULL,
    Source_Service NVARCHAR(64) NOT NULL,
    Is_Read BIT DEFAULT 0,
    Action_Url NVARCHAR(500),
    Priority_Level NVARCHAR(20) DEFAULT 'Normal',
    Expires_At DATETIME,
    CONSTRAINT Notification_pk PRIMARY KEY (Id),
    CONSTRAINT Notification_User FOREIGN KEY (Recipient_User_Id) REFERENCES [User] (Id)
);

-- =============================================
-- RATING SYSTEM
-- =============================================

-- Table: Rate
CREATE TABLE Rate (
    Id VARCHAR(36) NOT NULL,
    Rate_Value TINYINT NOT NULL,
    Description NVARCHAR(1000),
    Patient_User_Id VARCHAR(36) NOT NULL,
    Doctor_User_Id VARCHAR(36) NOT NULL,
    Appointment_Id VARCHAR(36),
    Rated_At DATETIME DEFAULT GETDATE(),
    Is_Anonymous BIT DEFAULT 0,
    CONSTRAINT Rate_pk PRIMARY KEY (Id),
    CONSTRAINT Rate_Patient FOREIGN KEY (Patient_User_Id) REFERENCES Patient (Id),
    CONSTRAINT Rate_Doctor FOREIGN KEY (Doctor_User_Id) REFERENCES Doctor (Id),
    CONSTRAINT Rate_Appointment FOREIGN KEY (Appointment_Id) REFERENCES Schedule_Appointment (Id),
    CONSTRAINT Rate_Value_Check CHECK (Rate_Value BETWEEN 1 AND 5)
);

-- =============================================
-- PAYMENT SYSTEM
-- =============================================

-- Table: Subscription_Payment
CREATE TABLE Subscription_Payment (
    Id VARCHAR(36) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL,
    Status NVARCHAR(32) NOT NULL,
    Paid_At DATETIME,
    Renewal_Date DATETIME NOT NULL,
    Patient_Id VARCHAR(36) NOT NULL,
    Subscription_Type NVARCHAR(50) NOT NULL,
    Payment_Method NVARCHAR(100),
    Transaction_Id NVARCHAR(200),
    CONSTRAINT Subscription_Payment_pk PRIMARY KEY (Id),
    CONSTRAINT Subscription_Payment_Patient FOREIGN KEY (Patient_Id) REFERENCES Patient (Id)
);

-- Table: Appointment_Payment
CREATE TABLE Appointment_Payment (
    Id VARCHAR(36) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL,
    Status NVARCHAR(32) NOT NULL,
    Paid_At DATETIME,
    Renewal_Date DATETIME,
    Schedule_Appointment_Id VARCHAR(36) NOT NULL,
    Patient_Id VARCHAR(36) NOT NULL,
    Payment_Method NVARCHAR(100),
    Transaction_Id NVARCHAR(200),
    CONSTRAINT Appointment_Payment_pk PRIMARY KEY (Id),
    CONSTRAINT Appointment_Payment_Schedule_Appointment FOREIGN KEY (Schedule_Appointment_Id) REFERENCES Schedule_Appointment (Id),
    CONSTRAINT Appointment_Payment_Patient FOREIGN KEY (Patient_Id) REFERENCES Patient (Id)
);

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================

CREATE INDEX IX_User_Profile_Email ON User_Profile (Email);
CREATE INDEX IX_User_Role ON [User] (Role_Id);
CREATE INDEX IX_User_Active ON [User] (Is_Active);
CREATE INDEX IX_Schedule_Appointment_Doctor_Date ON Schedule_Appointment (Doctor_User_Id, Day);
CREATE INDEX IX_Schedule_Appointment_Patient_Date ON Schedule_Appointment (Patient_User_Id, Day);
CREATE INDEX IX_Schedule_Appointment_Status ON Schedule_Appointment (Schedule_Appointment_Status_Id);
CREATE INDEX IX_Time_Slot_Doctor_DateTime ON Time_Slot (Doctor_Id, Start_DateTime, Is_Available);
CREATE INDEX IX_Document_Patient_Type_Date ON Document (Patient_Id, Type, Created_At);
CREATE INDEX IX_Document_Doctor_Date ON Document (Doctor_Id, Created_At);
CREATE INDEX IX_Document_Type ON Document (Document_Type_Id);
CREATE INDEX IX_Patient_Status_Doctor ON Patient_Status (Doctor_Id, Status);
CREATE INDEX IX_Patient_Medical_Condition_Patient ON Patient_Medical_Condition (Patient_Id, Status);
CREATE INDEX IX_Emergency_Contact_Patient ON Emergency_Contact (Patient_Id, Is_Primary);
CREATE INDEX IX_Insurance_Patient ON Insurance (Patient_Id, Is_Active);
CREATE INDEX IX_Conversation_Doctor_Patient ON Conversation (Doctor_User_Id, Patient_User_Id);
CREATE INDEX IX_Message_Conversation_Timestamp ON Message (Conversation_Id, Timestamp);
CREATE INDEX IX_Message_Receiver_Unread ON Message (Receiver_Id, Is_Read);
CREATE INDEX IX_Notification_Recipient_Date ON Notification (Recipient_User_Id, Creation_Date);
CREATE INDEX IX_Notification_Unread ON Notification (Recipient_User_Id, Is_Read);
CREATE INDEX IX_Lab_Test_Result_Document ON Lab_Test_Result (Lab_Results_Document_Id);
CREATE INDEX IX_Lab_Results_Patient_Date ON Lab_Results (Document_Id);
CREATE INDEX IX_Appointment_Payment_Status ON Appointment_Payment (Status, Paid_At);
CREATE INDEX IX_Subscription_Payment_Patient_Status ON Subscription_Payment (Patient_Id, Status);

-- =============================================
-- SAMPLE DATA INSERTS
-- =============================================

INSERT INTO Role (Id, Name, Description) VALUES 
    (CAST(NEWID() AS VARCHAR(36)), 'Doctor', 'Medical practitioner'),
    (CAST(NEWID() AS VARCHAR(36)), 'Patient', 'Healthcare recipient'),
    (CAST(NEWID() AS VARCHAR(36)), 'Receptionist', 'Administrative staff');

INSERT INTO Schedule_Appointment_Status (Id, Name, Description, Color_Code) VALUES
    (CAST(NEWID() AS VARCHAR(36)), 'scheduled', 'Appointment scheduled', '#3B82F6'),
    (CAST(NEWID() AS VARCHAR(36)), 'confirmed', 'Appointment confirmed', '#10B981'),
    (CAST(NEWID() AS VARCHAR(36)), 'cancelled', 'Appointment cancelled', '#EF4444'),
    (CAST(NEWID() AS VARCHAR(36)), 'completed', 'Appointment completed', '#6B7280'),
    (CAST(NEWID() AS VARCHAR(36)), 'no-show', 'Patient did not show up', '#F59E0B');

INSERT INTO Document_Type (Id, Code, Name, Description) VALUES
    (CAST(NEWID() AS VARCHAR(36)), 'PRESCRIPTION', 'Prescription', 'Medical prescription document'),
    (CAST(NEWID() AS VARCHAR(36)), 'REFERRAL', 'Referral', 'Medical referral document'),
    (CAST(NEWID() AS VARCHAR(36)), 'SICK_LEAVE', 'Sick_Leave', 'Sick leave certificate'),
    (CAST(NEWID() AS VARCHAR(36)), 'VISIT_CARD', 'VisitCard', 'Visit summary document'),
    (CAST(NEWID() AS VARCHAR(36)), 'LAB_RESULTS', 'Lab_Results', 'Laboratory test results');

INSERT INTO Medical_Condition (Id, Code, Name, Description, Category) VALUES
    (CAST(NEWID() AS VARCHAR(36)), 'I10', 'Hypertension', 'High blood pressure', 'Cardiovascular'),
    (CAST(NEWID() AS VARCHAR(36)), 'E11', 'Diabetes Type 2', 'Type 2 diabetes mellitus', 'Endocrine'),
    (CAST(NEWID() AS VARCHAR(36)), 'E78.0', 'High Cholesterol', 'Pure hypercholesterolemia', 'Metabolic'),
    (CAST(NEWID() AS VARCHAR(36)), 'J45', 'Asthma', 'Bronchial asthma', 'Respiratory'),
    (CAST(NEWID() AS VARCHAR(36)), 'M79.3', 'Arthritis', 'Joint inflammation', 'Musculoskeletal'),
    (CAST(NEWID() AS VARCHAR(36)), 'F32', 'Depression', 'Depressive episode', 'Mental Health'),
    (CAST(NEWID() AS VARCHAR(36)), 'F41', 'Anxiety', 'Anxiety disorders', 'Mental Health'),
    (CAST(NEWID() AS VARCHAR(36)), 'T78.4', 'Allergies', 'Allergy, unspecified', 'Immunologic'),
    (CAST(NEWID() AS VARCHAR(36)), 'G43', 'Migraine', 'Migraine headache', 'Neurological');

-- =============================================
-- END OF SCHEMA
-- =============================================
