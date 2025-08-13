-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2025-08-13 13:18:29.393

-- tables
-- Table: Appointment
CREATE TABLE Appointment (
    Id varchar(36)  NOT NULL,
    Schedule_Appointment_Id varchar(36)  NOT NULL,
    Description nvarchar(1000)  NULL,
    Created_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT Appointment_pk PRIMARY KEY  (Id)
);

-- Table: Appointment_Payment
CREATE TABLE Appointment_Payment (
    Id varchar(36)  NOT NULL,
    Amount decimal(10,2)  NOT NULL,
    Currency nvarchar(10)  NOT NULL,
    Status nvarchar(32)  NOT NULL,
    Paid_At datetime  NULL,
    Renewal_Date datetime  NULL,
    Schedule_Appointment_Id varchar(36)  NOT NULL,
    Patient_Id varchar(36)  NOT NULL,
    Payment_Method nvarchar(100)  NULL,
    Transaction_Id nvarchar(200)  NULL,
    CONSTRAINT Appointment_Payment_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Appointment_Payment_Status on Appointment_Payment (Status ASC,Paid_At ASC)
;

-- Table: Conversation
CREATE TABLE Conversation (
    Id varchar(36)  NOT NULL,
    Doctor_User_Id varchar(36)  NOT NULL,
    Patient_User_Id varchar(36)  NOT NULL,
    Subject nvarchar(500)  NULL,
    Created_At datetime  NULL DEFAULT getdate(),
    Last_Message_At datetime  NULL DEFAULT getdate(),
    Is_Active bit  NULL DEFAULT 1,
    CONSTRAINT Conversation_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Conversation_Doctor_Patient on Conversation (Doctor_User_Id ASC,Patient_User_Id ASC)
;

-- Table: Doctor
CREATE TABLE Doctor (
    Id varchar(36)  NOT NULL,
    License_Number nvarchar(100)  NULL,
    Years_Experience int  NULL,
    Biography nvarchar(2000)  NULL,
    Office_Address nvarchar(500)  NULL,
    CONSTRAINT Doctor_pk PRIMARY KEY  (Id)
);

-- Table: Doctor_Schedule
CREATE TABLE Doctor_Schedule (
    Id varchar(36)  NOT NULL,
    Doctor_Id varchar(36)  NOT NULL,
    Day_Of_Week tinyint  NOT NULL,
    Start_Time time  NOT NULL,
    End_Time time  NOT NULL,
    Is_Available bit  NULL DEFAULT 1,
    Valid_From date  NOT NULL,
    Valid_To date  NULL,
    Break_Start_Time time  NULL,
    Break_End_Time time  NULL,
    CONSTRAINT Doctor_Schedule_pk PRIMARY KEY  (Id)
);

-- Table: Doctor_Specialization
CREATE TABLE Doctor_Specialization (
    Id varchar(36)  NOT NULL,
    Doctor_Id varchar(36)  NOT NULL,
    Specialization_Id varchar(36)  NOT NULL,
    Is_Primary bit  NULL DEFAULT 0,
    Certified_Date date  NULL,
    CONSTRAINT Doctor_Specialization_pk PRIMARY KEY  (Id)
);

-- Table: Document
CREATE TABLE Document (
    Id varchar(36)  NOT NULL,
    Created_At datetime  NOT NULL,
    Notes nvarchar(1000)  NULL,
    Type int  NOT NULL,
    Document_Type_Id varchar(36)  NOT NULL,
    Patient_Id varchar(36)  NOT NULL,
    Doctor_Id varchar(36)  NOT NULL,
    File_Path nvarchar(1000)  NULL,
    File_Size_Bytes bigint  NULL,
    CONSTRAINT Document_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Document_Patient_Type_Date on Document (Patient_Id ASC,Type ASC,Created_At ASC)
;

CREATE INDEX IX_Document_Doctor_Date on Document (Doctor_Id ASC,Created_At ASC)
;

CREATE INDEX IX_Document_Type on Document (Document_Type_Id ASC)
;

-- Table: Document_Type
CREATE TABLE Document_Type (
    Id varchar(36)  NOT NULL,
    Code nvarchar(30)  NOT NULL,
    Name nvarchar(100)  NOT NULL,
    Description nvarchar(255)  NULL,
    Template_Path nvarchar(500)  NULL,
    CONSTRAINT Document_Type_Code_unique UNIQUE (Code),
    CONSTRAINT Document_Type_pk PRIMARY KEY  (Id)
);

-- Table: Documents_Assigned
CREATE TABLE Documents_Assigned (
    Id varchar(36)  NOT NULL,
    Assigned_At datetime  NOT NULL,
    Appointment_Id varchar(36)  NOT NULL,
    Document_Id varchar(36)  NOT NULL,
    CONSTRAINT Documents_Assigned_pk PRIMARY KEY  (Id)
);

-- Table: Emergency_Contact
CREATE TABLE Emergency_Contact (
    Id varchar(36)  NOT NULL,
    Patient_Id varchar(36)  NOT NULL,
    Name nvarchar(200)  NOT NULL,
    Phone nvarchar(20)  NOT NULL,
    Relationship nvarchar(100)  NOT NULL,
    Is_Primary bit  NULL DEFAULT 0,
    Created_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT Emergency_Contact_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Emergency_Contact_Patient on Emergency_Contact (Patient_Id ASC,Is_Primary ASC)
;

-- Table: Insurance
CREATE TABLE Insurance (
    Id varchar(36)  NOT NULL,
    Patient_Id varchar(36)  NOT NULL,
    Provider_Name nvarchar(200)  NOT NULL,
    Policy_Number nvarchar(100)  NOT NULL,
    Group_Number nvarchar(100)  NULL,
    Valid_From date  NOT NULL,
    Valid_To date  NULL,
    Is_Primary bit  NULL DEFAULT 1,
    Is_Active bit  NULL DEFAULT 1,
    CONSTRAINT Insurance_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Insurance_Patient on Insurance (Patient_Id ASC,Is_Active ASC)
;

-- Table: Lab_Results
CREATE TABLE Lab_Results (
    Document_Id varchar(36)  NOT NULL,
    Test_Type nvarchar(200)  NULL,
    Test_Date datetime  NULL,
    Laboratory nvarchar(200)  NULL,
    Overall_Status nvarchar(50)  NULL,
    Interpretation nvarchar(max)  NULL,
    Reference_Ranges nvarchar(1000)  NULL,
    Technician_Name nvarchar(200)  NULL,
    Doctor_Comments nvarchar(1000)  NULL,
    CONSTRAINT Lab_Results_pk PRIMARY KEY  (Document_Id)
);

CREATE INDEX IX_Lab_Results_Patient_Date on Lab_Results (Document_Id ASC)
;

-- Table: Lab_Test_Result
CREATE TABLE Lab_Test_Result (
    Id varchar(36)  NOT NULL,
    Lab_Results_Document_Id varchar(36)  NOT NULL,
    Lab_Test_Type_Id varchar(36)  NOT NULL,
    Parameter_Name nvarchar(200)  NOT NULL,
    Value nvarchar(100)  NOT NULL,
    Numeric_Value decimal(18,6)  NULL,
    Unit nvarchar(50)  NULL,
    Reference_Range nvarchar(200)  NULL,
    Status nvarchar(50)  NOT NULL,
    Notes nvarchar(1000)  NULL,
    Is_Abnormal bit  NULL DEFAULT 0,
    CONSTRAINT Lab_Test_Result_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Lab_Test_Result_Document on Lab_Test_Result (Lab_Results_Document_Id ASC)
;

-- Table: Lab_Test_Type
CREATE TABLE Lab_Test_Type (
    Id varchar(36)  NOT NULL,
    Code nvarchar(50)  NOT NULL,
    Name nvarchar(200)  NOT NULL,
    Description nvarchar(500)  NULL,
    Reference_Range nvarchar(200)  NULL,
    Unit nvarchar(50)  NULL,
    Category nvarchar(100)  NULL,
    Normal_Min_Value decimal(18,6)  NULL,
    Normal_Max_Value decimal(18,6)  NULL,
    CONSTRAINT Lab_Test_Type_Code_unique UNIQUE (Code),
    CONSTRAINT Lab_Test_Type_pk PRIMARY KEY  (Id)
);

-- Table: Medical_Condition
CREATE TABLE Medical_Condition (
    Id varchar(36)  NOT NULL,
    Code nvarchar(50)  NOT NULL,
    Name nvarchar(200)  NOT NULL,
    Description nvarchar(1000)  NULL,
    Category nvarchar(100)  NULL,
    Is_Chronic bit  NULL DEFAULT 0,
    CONSTRAINT Medical_Condition_Code_unique UNIQUE (Code),
    CONSTRAINT Medical_Condition_pk PRIMARY KEY  (Id)
);

-- Table: Message
CREATE TABLE Message (
    Id varchar(36)  NOT NULL,
    Conversation_Id varchar(36)  NOT NULL,
    Sender_Id varchar(36)  NOT NULL,
    Sender_Name nvarchar(200)  NOT NULL,
    Sender_Type nvarchar(20)  NOT NULL,
    Receiver_Id varchar(36)  NOT NULL,
    Receiver_Name nvarchar(200)  NOT NULL,
    Receiver_Type nvarchar(20)  NOT NULL,
    Content nvarchar(max)  NOT NULL,
    Timestamp datetime  NULL DEFAULT getdate(),
    Is_Read bit  NULL DEFAULT 0,
    Message_Type nvarchar(50)  NULL DEFAULT text,
    Attachment_Path nvarchar(1000)  NULL,
    CONSTRAINT Message_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Message_Conversation_Timestamp on Message (Conversation_Id ASC,Timestamp ASC)
;

CREATE INDEX IX_Message_Receiver_Unread on Message (Receiver_Id ASC,Is_Read ASC)
;

-- Table: Notification
CREATE TABLE Notification (
    Id varchar(36)  NOT NULL,
    Recipient_User_Id varchar(36)  NOT NULL,
    Description nvarchar(255)  NOT NULL,
    Type tinyint  NOT NULL,
    Creation_Date datetime  NOT NULL,
    Source_Service nvarchar(64)  NOT NULL,
    Is_Read bit  NULL DEFAULT 0,
    Action_Url nvarchar(500)  NULL,
    Priority_Level nvarchar(20)  NULL DEFAULT normal,
    Expires_At datetime  NULL,
    CONSTRAINT Notification_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Notification_Recipient_Date on Notification (Recipient_User_Id ASC,Creation_Date ASC)
;

CREATE INDEX IX_Notification_Unread on Notification (Recipient_User_Id ASC,Is_Read ASC)
;

-- Table: Patient
CREATE TABLE Patient (
    Id varchar(36)  NOT NULL,
    General_Doctor_Id varchar(36)  NOT NULL,
    Medical_Record_Number nvarchar(100)  NULL,
    Blood_Type nvarchar(10)  NULL,
    Height_cm decimal(5,2)  NULL,
    Weight_kg decimal(5,2)  NULL,
    CONSTRAINT Patient_pk PRIMARY KEY  (Id)
);

-- Table: Patient_Medical_Condition
CREATE TABLE Patient_Medical_Condition (
    Id varchar(36)  NOT NULL,
    Patient_Id varchar(36)  NOT NULL,
    Medical_Condition_Id varchar(36)  NOT NULL,
    Diagnosed_Date date  NULL,
    Status nvarchar(50)  NULL DEFAULT active,
    Severity nvarchar(50)  NULL,
    Notes nvarchar(1000)  NULL,
    Created_At datetime  NULL DEFAULT getdate(),
    Updated_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT Patient_Medical_Condition_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Patient_Medical_Condition_Patient on Patient_Medical_Condition (Patient_Id ASC,Status ASC)
;

-- Table: Patient_Status
CREATE TABLE Patient_Status (
    Id varchar(36)  NOT NULL,
    Patient_Id varchar(36)  NOT NULL,
    Doctor_Id varchar(36)  NOT NULL,
    Status nvarchar(50)  NOT NULL,
    Assigned_Date date  NOT NULL,
    Status_Changed_Date date  NULL DEFAULT getdate(),
    Urgent_Notes nvarchar(1000)  NULL,
    Created_At datetime  NULL DEFAULT getdate(),
    Updated_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT Patient_Status_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Patient_Status_Doctor on Patient_Status (Doctor_Id ASC,Status ASC)
;

-- Table: Prescription
CREATE TABLE Prescription (
    Document_Id varchar(36)  NOT NULL,
    Medication nvarchar(200)  NOT NULL,
    Dosage nvarchar(100)  NOT NULL,
    Frequency nvarchar(100)  NOT NULL,
    Duration_Days int  NOT NULL,
    Instructions nvarchar(max)  NOT NULL,
    Pharmacy_Name nvarchar(200)  NULL,
    Pharmacy_Phone nvarchar(20)  NULL,
    Refills_Remaining int  NULL DEFAULT 0,
    CONSTRAINT Prescription_pk PRIMARY KEY  (Document_Id)
);

-- Table: Rate
CREATE TABLE Rate (
    Id varchar(36)  NOT NULL,
    Rate_Value tinyint  NOT NULL,
    Description nvarchar(1000)  NULL,
    Patient_User_Id varchar(36)  NOT NULL,
    Doctor_User_Id varchar(36)  NOT NULL,
    Appointment_Id varchar(36)  NULL,
    Rated_At datetime  NULL DEFAULT getdate(),
    Is_Anonymous bit  NULL DEFAULT 0,
    CONSTRAINT Rate_Value_Check CHECK (( Rate_Value BETWEEN 1 AND 5 )),
    CONSTRAINT Rate_pk PRIMARY KEY  (Id)
);

-- Table: Receptionist
CREATE TABLE Receptionist (
    Id varchar(36)  NOT NULL,
    Department nvarchar(200)  NULL,
    CONSTRAINT Receptionist_pk PRIMARY KEY  (Id)
);

-- Table: Referral
CREATE TABLE Referral (
    Document_Id varchar(36)  NOT NULL,
    Speciality nvarchar(100)  NOT NULL,
    Referred_To nvarchar(255)  NOT NULL,
    Valid_From datetime  NOT NULL,
    Valid_To datetime  NOT NULL,
    Reason nvarchar(1000)  NULL,
    Urgency_Level nvarchar(50)  NULL,
    CONSTRAINT Referral_pk PRIMARY KEY  (Document_Id)
);

-- Table: Role
CREATE TABLE Role (
    Id varchar(36)  NOT NULL,
    Name nvarchar(100)  NOT NULL,
    Description nvarchar(500)  NULL,
    CONSTRAINT Role_Name_unique UNIQUE (Name),
    CONSTRAINT Role_pk PRIMARY KEY  (Id)
);

-- Table: Schedule
CREATE TABLE Schedule (
    Id varchar(36)  NOT NULL,
    Name nvarchar(200)  NULL,
    Description nvarchar(500)  NULL,
    Created_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT Schedule_pk PRIMARY KEY  (Id)
);

-- Table: Schedule_Appointment
CREATE TABLE Schedule_Appointment (
    Id varchar(36)  NOT NULL,
    Schedule_Id varchar(36)  NOT NULL,
    Time_Slot_Id varchar(36)  NULL,
    Day datetime  NOT NULL,
    Duration_Minutes int  NOT NULL,
    Room nvarchar(255)  NULL,
    Description nvarchar(1000)  NULL,
    Appointment_Type nvarchar(50)  NULL DEFAULT in-person,
    Doctor_User_Id varchar(36)  NOT NULL,
    Patient_User_Id varchar(36)  NOT NULL,
    Receptionist_User_Id varchar(36)  NULL,
    Schedule_Appointment_Status_Id varchar(36)  NOT NULL,
    Total_Cost decimal(10,2)  NULL,
    Created_At datetime  NULL DEFAULT getdate(),
    Updated_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT Schedule_Appointment_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Schedule_Appointment_Doctor_Date on Schedule_Appointment (Doctor_User_Id ASC,Day ASC)
;

CREATE INDEX IX_Schedule_Appointment_Patient_Date on Schedule_Appointment (Patient_User_Id ASC,Day ASC)
;

CREATE INDEX IX_Schedule_Appointment_Status on Schedule_Appointment (Schedule_Appointment_Status_Id ASC)
;

-- Table: Schedule_Appointment_Status
CREATE TABLE Schedule_Appointment_Status (
    Id varchar(36)  NOT NULL,
    Name nvarchar(100)  NOT NULL,
    Description nvarchar(500)  NULL,
    Color_Code nvarchar(7)  NULL,
    CONSTRAINT Schedule_Appointment_Status_pk PRIMARY KEY  (Id)
);

-- Table: Service
CREATE TABLE Service (
    Id varchar(36)  NOT NULL,
    Name nvarchar(200)  NOT NULL,
    Description nvarchar(1000)  NULL,
    Duration_Minutes int  NULL DEFAULT 30,
    Is_Active bit  NULL DEFAULT 1,
    Created_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT Service_pk PRIMARY KEY  (Id)
);

-- Table: Sick_Leave
CREATE TABLE Sick_Leave (
    Document_Id varchar(36)  NOT NULL,
    Start_Date datetime  NOT NULL,
    End_Date datetime  NOT NULL,
    Days_Off int  NOT NULL,
    Return_To_Work_Date datetime  NULL,
    Work_Restrictions nvarchar(1000)  NULL,
    CONSTRAINT Sick_Leave_pk PRIMARY KEY  (Document_Id)
);

-- Table: Specialization
CREATE TABLE Specialization (
    Id varchar(36)  NOT NULL,
    Name nvarchar(200)  NOT NULL,
    Description nvarchar(1000)  NULL,
    Service_Id varchar(36)  NOT NULL,
    Is_Active bit  NULL DEFAULT 1,
    CONSTRAINT Specialization_pk PRIMARY KEY  (Id)
);

-- Table: Subscription_Payment
CREATE TABLE Subscription_Payment (
    Id varchar(36)  NOT NULL,
    Amount decimal(10,2)  NOT NULL,
    Currency nvarchar(10)  NOT NULL,
    Status nvarchar(32)  NOT NULL,
    Paid_At datetime  NULL,
    Renewal_Date datetime  NOT NULL,
    Patient_Id varchar(36)  NOT NULL,
    Subscription_Type nvarchar(50)  NOT NULL,
    Payment_Method nvarchar(100)  NULL,
    Transaction_Id nvarchar(200)  NULL,
    CONSTRAINT Subscription_Payment_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Subscription_Payment_Patient_Status on Subscription_Payment (Patient_Id ASC,Status ASC)
;

-- Table: Time_Slot
CREATE TABLE Time_Slot (
    Id varchar(36)  NOT NULL,
    Doctor_Id varchar(36)  NOT NULL,
    Start_DateTime datetime  NOT NULL,
    End_DateTime datetime  NOT NULL,
    Is_Available bit  NULL DEFAULT 1,
    Duration_Minutes int  NULL DEFAULT 30,
    Slot_Type nvarchar(50)  NULL DEFAULT regular,
    Created_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT Time_Slot_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_Time_Slot_Doctor_DateTime on Time_Slot (Doctor_Id ASC,Start_DateTime ASC,Is_Available ASC)
;

-- Table: User
CREATE TABLE "User" (
    Id varchar(36)  NOT NULL,
    Role_Id varchar(36)  NOT NULL,
    Schedule_Id varchar(36)  NOT NULL,
    Created_At datetime  NULL DEFAULT getdate(),
    Updated_At datetime  NULL DEFAULT getdate(),
    Is_Active bit  NULL DEFAULT 1,
    CONSTRAINT User_pk PRIMARY KEY  (Id)
);

CREATE INDEX IX_User_Role on "User" (Role_Id ASC)
;

CREATE INDEX IX_User_Active on "User" (Is_Active ASC)
;

-- Table: User_Profile
CREATE TABLE User_Profile (
    User_Id varchar(36)  NOT NULL,
    FirstName nvarchar(100)  NOT NULL,
    LastName nvarchar(100)  NOT NULL,
    Email nvarchar(255)  NOT NULL,
    Phone nvarchar(20)  NULL,
    DateOfBirth date  NULL,
    Gender nvarchar(20)  NULL,
    Avatar_Url nvarchar(500)  NULL,
    Address_Line1 nvarchar(200)  NULL,
    Address_Line2 nvarchar(200)  NULL,
    City nvarchar(100)  NULL,
    State nvarchar(100)  NULL,
    ZipCode nvarchar(20)  NULL,
    Country nvarchar(100)  NULL,
    Created_At datetime  NULL DEFAULT getdate(),
    Updated_At datetime  NULL DEFAULT getdate(),
    CONSTRAINT User_Profile_Email_unique UNIQUE (Email),
    CONSTRAINT User_Profile_pk PRIMARY KEY  (User_Id)
);

CREATE INDEX IX_User_Profile_Email on User_Profile (Email ASC)
;

-- Table: Visit_Document
CREATE TABLE Visit_Document (
    Document_Id varchar(36)  NOT NULL,
    Symptoms nvarchar(max)  NOT NULL,
    Findings nvarchar(max)  NOT NULL,
    Diagnosis nvarchar(max)  NOT NULL,
    Recommendations nvarchar(max)  NOT NULL,
    Vital_Signs nvarchar(1000)  NULL,
    Treatment_Plan nvarchar(2000)  NULL,
    Follow_Up_Date datetime  NULL,
    CONSTRAINT Visit_Document_pk PRIMARY KEY  (Document_Id)
);

-- foreign keys
-- Reference: Appointment_Payment_Patient (table: Appointment_Payment)
ALTER TABLE Appointment_Payment ADD CONSTRAINT Appointment_Payment_Patient
    FOREIGN KEY (Patient_Id)
    REFERENCES Patient (Id);

-- Reference: Appointment_Payment_Schedule_Appointment (table: Appointment_Payment)
ALTER TABLE Appointment_Payment ADD CONSTRAINT Appointment_Payment_Schedule_Appointment
    FOREIGN KEY (Schedule_Appointment_Id)
    REFERENCES Schedule_Appointment (Id);

-- Reference: Appointment_Schedule_Appointment (table: Appointment)
ALTER TABLE Appointment ADD CONSTRAINT Appointment_Schedule_Appointment
    FOREIGN KEY (Schedule_Appointment_Id)
    REFERENCES Schedule_Appointment (Id);

-- Reference: Conversation_Doctor (table: Conversation)
ALTER TABLE Conversation ADD CONSTRAINT Conversation_Doctor
    FOREIGN KEY (Doctor_User_Id)
    REFERENCES Doctor (Id);

-- Reference: Conversation_Patient (table: Conversation)
ALTER TABLE Conversation ADD CONSTRAINT Conversation_Patient
    FOREIGN KEY (Patient_User_Id)
    REFERENCES Patient (Id);

-- Reference: Doctor_Schedule_Doctor (table: Doctor_Schedule)
ALTER TABLE Doctor_Schedule ADD CONSTRAINT Doctor_Schedule_Doctor
    FOREIGN KEY (Doctor_Id)
    REFERENCES Doctor (Id);

-- Reference: Doctor_Specialization_Doctor (table: Doctor_Specialization)
ALTER TABLE Doctor_Specialization ADD CONSTRAINT Doctor_Specialization_Doctor
    FOREIGN KEY (Doctor_Id)
    REFERENCES Doctor (Id);

-- Reference: Doctor_Specialization_Specialization (table: Doctor_Specialization)
ALTER TABLE Doctor_Specialization ADD CONSTRAINT Doctor_Specialization_Specialization
    FOREIGN KEY (Specialization_Id)
    REFERENCES Specialization (Id);

-- Reference: Doctor_User (table: Doctor)
ALTER TABLE Doctor ADD CONSTRAINT Doctor_User
    FOREIGN KEY (Id)
    REFERENCES "User" (Id);

-- Reference: Document_Doctor (table: Document)
ALTER TABLE Document ADD CONSTRAINT Document_Doctor
    FOREIGN KEY (Doctor_Id)
    REFERENCES Doctor (Id);

-- Reference: Document_Document_Type (table: Document)
ALTER TABLE Document ADD CONSTRAINT Document_Document_Type
    FOREIGN KEY (Document_Type_Id)
    REFERENCES Document_Type (Id);

-- Reference: Document_Patient (table: Document)
ALTER TABLE Document ADD CONSTRAINT Document_Patient
    FOREIGN KEY (Patient_Id)
    REFERENCES Patient (Id);

-- Reference: Documents_Assigned_Appointment (table: Documents_Assigned)
ALTER TABLE Documents_Assigned ADD CONSTRAINT Documents_Assigned_Appointment
    FOREIGN KEY (Appointment_Id)
    REFERENCES Appointment (Id);

-- Reference: Documents_Assigned_Document (table: Documents_Assigned)
ALTER TABLE Documents_Assigned ADD CONSTRAINT Documents_Assigned_Document
    FOREIGN KEY (Document_Id)
    REFERENCES Document (Id);

-- Reference: Emergency_Contact_Patient (table: Emergency_Contact)
ALTER TABLE Emergency_Contact ADD CONSTRAINT Emergency_Contact_Patient
    FOREIGN KEY (Patient_Id)
    REFERENCES Patient (Id);

-- Reference: Insurance_Patient (table: Insurance)
ALTER TABLE Insurance ADD CONSTRAINT Insurance_Patient
    FOREIGN KEY (Patient_Id)
    REFERENCES Patient (Id);

-- Reference: Lab_Results_Document (table: Lab_Results)
ALTER TABLE Lab_Results ADD CONSTRAINT Lab_Results_Document
    FOREIGN KEY (Document_Id)
    REFERENCES Document (Id);

-- Reference: Lab_Test_Result_Lab_Results (table: Lab_Test_Result)
ALTER TABLE Lab_Test_Result ADD CONSTRAINT Lab_Test_Result_Lab_Results
    FOREIGN KEY (Lab_Results_Document_Id)
    REFERENCES Lab_Results (Document_Id);

-- Reference: Lab_Test_Result_Test_Type (table: Lab_Test_Result)
ALTER TABLE Lab_Test_Result ADD CONSTRAINT Lab_Test_Result_Test_Type
    FOREIGN KEY (Lab_Test_Type_Id)
    REFERENCES Lab_Test_Type (Id);

-- Reference: Message_Conversation (table: Message)
ALTER TABLE Message ADD CONSTRAINT Message_Conversation
    FOREIGN KEY (Conversation_Id)
    REFERENCES Conversation (Id);

-- Reference: Message_Receiver (table: Message)
ALTER TABLE Message ADD CONSTRAINT Message_Receiver
    FOREIGN KEY (Receiver_Id)
    REFERENCES "User" (Id);

-- Reference: Message_Sender (table: Message)
ALTER TABLE Message ADD CONSTRAINT Message_Sender
    FOREIGN KEY (Sender_Id)
    REFERENCES "User" (Id);

-- Reference: Notification_User (table: Notification)
ALTER TABLE Notification ADD CONSTRAINT Notification_User
    FOREIGN KEY (Recipient_User_Id)
    REFERENCES "User" (Id);

-- Reference: PMC_Condition (table: Patient_Medical_Condition)
ALTER TABLE Patient_Medical_Condition ADD CONSTRAINT PMC_Condition
    FOREIGN KEY (Medical_Condition_Id)
    REFERENCES Medical_Condition (Id);

-- Reference: PMC_Patient (table: Patient_Medical_Condition)
ALTER TABLE Patient_Medical_Condition ADD CONSTRAINT PMC_Patient
    FOREIGN KEY (Patient_Id)
    REFERENCES Patient (Id);

-- Reference: Patient_Doctor (table: Patient)
ALTER TABLE Patient ADD CONSTRAINT Patient_Doctor
    FOREIGN KEY (General_Doctor_Id)
    REFERENCES Doctor (Id);

-- Reference: Patient_Status_Doctor (table: Patient_Status)
ALTER TABLE Patient_Status ADD CONSTRAINT Patient_Status_Doctor
    FOREIGN KEY (Doctor_Id)
    REFERENCES Doctor (Id);

-- Reference: Patient_Status_Patient (table: Patient_Status)
ALTER TABLE Patient_Status ADD CONSTRAINT Patient_Status_Patient
    FOREIGN KEY (Patient_Id)
    REFERENCES Patient (Id);

-- Reference: Patient_User (table: Patient)
ALTER TABLE Patient ADD CONSTRAINT Patient_User
    FOREIGN KEY (Id)
    REFERENCES "User" (Id);

-- Reference: Prescription_Document (table: Prescription)
ALTER TABLE Prescription ADD CONSTRAINT Prescription_Document
    FOREIGN KEY (Document_Id)
    REFERENCES Document (Id);

-- Reference: Rate_Appointment (table: Rate)
ALTER TABLE Rate ADD CONSTRAINT Rate_Appointment
    FOREIGN KEY (Appointment_Id)
    REFERENCES Schedule_Appointment (Id);

-- Reference: Rate_Doctor (table: Rate)
ALTER TABLE Rate ADD CONSTRAINT Rate_Doctor
    FOREIGN KEY (Doctor_User_Id)
    REFERENCES Doctor (Id);

-- Reference: Rate_Patient (table: Rate)
ALTER TABLE Rate ADD CONSTRAINT Rate_Patient
    FOREIGN KEY (Patient_User_Id)
    REFERENCES Patient (Id);

-- Reference: Receptionist_User (table: Receptionist)
ALTER TABLE Receptionist ADD CONSTRAINT Receptionist_User
    FOREIGN KEY (Id)
    REFERENCES "User" (Id);

-- Reference: Referral_Document (table: Referral)
ALTER TABLE Referral ADD CONSTRAINT Referral_Document
    FOREIGN KEY (Document_Id)
    REFERENCES Document (Id);

-- Reference: Schedule_Appointment_Doctor (table: Schedule_Appointment)
ALTER TABLE Schedule_Appointment ADD CONSTRAINT Schedule_Appointment_Doctor
    FOREIGN KEY (Doctor_User_Id)
    REFERENCES Doctor (Id);

-- Reference: Schedule_Appointment_Patient (table: Schedule_Appointment)
ALTER TABLE Schedule_Appointment ADD CONSTRAINT Schedule_Appointment_Patient
    FOREIGN KEY (Patient_User_Id)
    REFERENCES Patient (Id);

-- Reference: Schedule_Appointment_Receptionist (table: Schedule_Appointment)
ALTER TABLE Schedule_Appointment ADD CONSTRAINT Schedule_Appointment_Receptionist
    FOREIGN KEY (Receptionist_User_Id)
    REFERENCES Receptionist (Id);

-- Reference: Schedule_Appointment_Schedule (table: Schedule_Appointment)
ALTER TABLE Schedule_Appointment ADD CONSTRAINT Schedule_Appointment_Schedule
    FOREIGN KEY (Schedule_Id)
    REFERENCES Schedule (Id);

-- Reference: Schedule_Appointment_Status (table: Schedule_Appointment)
ALTER TABLE Schedule_Appointment ADD CONSTRAINT Schedule_Appointment_Status
    FOREIGN KEY (Schedule_Appointment_Status_Id)
    REFERENCES Schedule_Appointment_Status (Id);

-- Reference: Schedule_Appointment_Time_Slot (table: Schedule_Appointment)
ALTER TABLE Schedule_Appointment ADD CONSTRAINT Schedule_Appointment_Time_Slot
    FOREIGN KEY (Time_Slot_Id)
    REFERENCES Time_Slot (Id);

-- Reference: Sick_Leave_Document (table: Sick_Leave)
ALTER TABLE Sick_Leave ADD CONSTRAINT Sick_Leave_Document
    FOREIGN KEY (Document_Id)
    REFERENCES Document (Id);

-- Reference: Specialization_Service (table: Specialization)
ALTER TABLE Specialization ADD CONSTRAINT Specialization_Service
    FOREIGN KEY (Service_Id)
    REFERENCES Service (Id);

-- Reference: Subscription_Payment_Patient (table: Subscription_Payment)
ALTER TABLE Subscription_Payment ADD CONSTRAINT Subscription_Payment_Patient
    FOREIGN KEY (Patient_Id)
    REFERENCES Patient (Id);

-- Reference: Time_Slot_Doctor (table: Time_Slot)
ALTER TABLE Time_Slot ADD CONSTRAINT Time_Slot_Doctor
    FOREIGN KEY (Doctor_Id)
    REFERENCES Doctor (Id);

-- Reference: User_Profile_User (table: User_Profile)
ALTER TABLE User_Profile ADD CONSTRAINT User_Profile_User
    FOREIGN KEY (User_Id)
    REFERENCES "User" (Id);

-- Reference: User_Role (table: User)
ALTER TABLE "User" ADD CONSTRAINT User_Role
    FOREIGN KEY (Role_Id)
    REFERENCES Role (Id);

-- Reference: User_Schedule (table: User)
ALTER TABLE "User" ADD CONSTRAINT User_Schedule
    FOREIGN KEY (Schedule_Id)
    REFERENCES Schedule (Id);

-- Reference: Visit_Document_Document (table: Visit_Document)
ALTER TABLE Visit_Document ADD CONSTRAINT Visit_Document_Document
    FOREIGN KEY (Document_Id)
    REFERENCES Document (Id);

-- End of file.

