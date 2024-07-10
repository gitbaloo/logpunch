CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DROP TABLE IF EXISTS Logpunch_Registrations;

DROP TABLE IF EXISTS Logpunch_Employee_Client_Relations;

DROP TABLE IF EXISTS Logpunch_Users;

DROP TABLE IF EXISTS Logpunch_Clients;

CREATE TABLE Logpunch_Users (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4 (),
    First_Name varchar(50) NOT NULL,
    Last_Name varchar(50) NOT NULL,
    Email varchar(255) NOT NULL,
    Password varchar(255) NOT NULL,
    Default_Query varchar(255) NULL,
    Role int NOT NULL,
    CONSTRAINT chk_role CHECK (Role IN (0, 1))
);

CREATE TABLE Logpunch_Clients (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4 (),
    Name varchar(255) NOT NULL
);

CREATE TABLE Logpunch_Registrations (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4 (),
    EmployeeId UUID NOT NULL,
    Registration_Type int NOT NULL,
    Amount int4 NULL,
    Registration_Start timestamptz NOT NULL,
    Registration_End timestamptz NULL,
    CreatorId UUID NOT NULL,
    ClientId UUID NULL,
    Creation_Time timestamptz NOT NULL,
    Status_Type int NOT NULL,
    Internal_Comment text NULL,
    External_Comment text NULL,
    CorrectionOf_Id UUID NULL,
    CONSTRAINT FKEmployee FOREIGN KEY (EmployeeId) REFERENCES Logpunch_Users (Id) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FKCreator FOREIGN KEY (CreatorId) REFERENCES Logpunch_Users (Id) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FKClient FOREIGN KEY (ClientId) REFERENCES Logpunch_Clients (Id) ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT FKCorrectionOf FOREIGN KEY (CorrectionOf_Id) REFERENCES Logpunch_Registrations (Id) ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT chk_registration_type CHECK (
        Registration_Type IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9)
    ),
    CONSTRAINT chk_status_type CHECK (
        Status_Type IN (0, 1, 2, 3, 4, 5)
    )
);

CREATE TABLE Logpunch_Employee_Client_Relations (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4 (),
    EmployeeId UUID NOT NULL,
    ClientId UUID NOT NULL,
    CONSTRAINT FKEmployee320576 FOREIGN KEY (EmployeeId) REFERENCES Logpunch_Users (Id) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FKClient320576 FOREIGN KEY (ClientId) REFERENCES Logpunch_Clients (Id) ON DELETE CASCADE ON UPDATE CASCADE
);
