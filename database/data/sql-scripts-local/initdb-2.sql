-- Enable the UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Insert test data into Logpunch_Users
INSERT INTO
    Logpunch_Users (
        Id,
        Email,
        First_Name,
        Last_Name,
        Password,
        Default_Query,
        Role
    )
VALUES (
        uuid_generate_v4 (),
        'employee@test.com',
        'test',
        'employee',
        'password1',
        NULL,
        'Employee'
    ),
    (
        uuid_generate_v4 (),
        'admin@test.com',
        'test',
        'admin',
        'password1',
        NULL,
        'Admin'
    );

-- Insert test data into Logpunch_Clients
INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (uuid_generate_v4 (), 'LEGO');

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (uuid_generate_v4 (), 'Nets');

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (
        uuid_generate_v4 (),
        'Novo Nordisk'
    );

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (
        uuid_generate_v4 (),
        'Statens IT'
    );

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (
        uuid_generate_v4 (),
        'TopDanmark'
    );

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (uuid_generate_v4 (), 'Zoles');

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (
        uuid_generate_v4 (),
        'Danske Bank'
    );

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (
        uuid_generate_v4 (),
        'Carlsberg'
    );

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (uuid_generate_v4 (), 'Maersk');

INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (uuid_generate_v4 (), 'Vestas');

-- Get UUIDs of inserted users and clients
WITH
    Employee AS (
        SELECT Id AS EmployeeId
        FROM Logpunch_Users
        WHERE
            Email = 'employee@test.com'
    ),
    Admin AS (
        SELECT Id AS AdminId
        FROM Logpunch_Users
        WHERE
            Email = 'admin@test.com'
    ),
    Clients AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
    )
INSERT INTO
    Logpunch_Employee_Client_Relations (Id, EmployeeId, ClientId)
SELECT uuid_generate_v4 (), (
        SELECT EmployeeId
        FROM Employee
    ), ClientId
FROM Clients;

-- Insert test data into Logpunch_Registrations
WITH
    Employee AS (
        SELECT Id AS EmployeeId
        FROM Logpunch_Users
        WHERE
            Email = 'employee@test.com'
    ),
    Admin AS (
        SELECT Id AS AdminId
        FROM Logpunch_Users
        WHERE
            Email = 'admin@test.com'
    )
INSERT INTO
    Logpunch_Registrations (
        Id,
        EmployeeId,
        Registration_Type,
        Amount,
        Registration_Start,
        Registration_End,
        Created_By_Id,
        Creation_Time,
        Status_Type,
        Internal_Comment,
        External_Comment,
        TaskId
    )
VALUES (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        300,
        '2023-06-01 09:00:00',
        '2023-06-01 14:00:00',
        (
            SELECT EmployeeId
            FROM Employee
        ),
        '2023-06-01 09:00:00',
        'Settled',
        'Internal comment 1',
        'External comment 1',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        120,
        '2024-01-02 10:00:00',
        '2024-01-02 12:00:00',
        (
            SELECT EmployeeId
            FROM Employee
        ),


        '2024-01-02 10:00:00',
        'Settled',
        'Internal comment 2',
        'External comment 2',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        180,
        '2024-02-15 11:00:00',
        '2024-02-15 14:00:00',
        (
            SELECT EmployeeId
            FROM Employee
        ),
        '2024-02-15 11:00:00',
        'Settled',
        'Internal comment 3',
        'External comment 3',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        240,
        '2024-02-16 08:00:00',
        '2024-02-16 12:00:00',
        (
            SELECT EmployeeId
            FROM Employee
        ),
        '2024-02-16 08:00:00',
        'Settled',
        'Internal comment 4',
        'External comment 4',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        360,
        '2024-06-17 09:00:00',
        '2024-06-17 15:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        '2024-06-18 11:20:05',
        'Settled',
        'Internal comment 5',
        'External comment 5',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        180,
        '2024-06-18 10:00:00',
        '2024-06-18 13:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        '2024-06-18 16:24:25',
        'Settled',
        'Internal comment 6',
        'External comment 6',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        240,
        '2024-06-21 09:00:00',
        '2024-06-21 13:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        '2024-06-30 21:12:45',
        'Settled',
        'Internal comment 7',
        'External comment 7',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        300,
        '2024-07-02 08:00:00',
        '2024-07-02 13:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        '2024-07-02 15:00:00',
        'Settled',
        'Internal comment 8',
        'External comment 8',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        360,
        '2024-07-03 09:00:00',
        '2024-07-03 15:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        '2024-07-03 16:00:00',
        'Settled',
        'Internal comment 9',
        'External comment 9',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        'Work',
        240,
        '2024-07-08 10:00:00',
        '2024-07-08 14:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        '2024-07-09 10:00:00',
        'Settled',
        'Internal comment 10',
        'External comment 10',
        NULL
    );
