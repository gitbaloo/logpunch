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
        0
    ),
    (
        uuid_generate_v4 (),
        'admin@test.com',
        'test',
        'admin',
        'password1',
        NULL,
        1
    );

-- Insert test data into Logpunch_Clients
INSERT INTO
    Logpunch_Clients (Id, Name)
VALUES (uuid_generate_v4 (), 'LEGO'),
    (uuid_generate_v4 (), 'Nets'),
    (
        uuid_generate_v4 (),
        'Novo Nordisk'
    ),
    (
        uuid_generate_v4 (),
        'Statens IT'
    ),
    (
        uuid_generate_v4 (),
        'TopDanmark'
    ),
    (uuid_generate_v4 (), 'Zoles'),
    (
        uuid_generate_v4 (),
        'Danske Bank'
    ),
    (
        uuid_generate_v4 (),
        'Carlsberg'
    ),
    (uuid_generate_v4 (), 'Maersk'),
    (uuid_generate_v4 (), 'Vestas');

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
    ),
    Client1 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            0
    ),
    Client2 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            1
    ),
    Client3 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            2
    ),
    Client4 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            3
    ),
    Client5 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            4
    ),
    Client6 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            5
    ),
    Client7 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            6
    ),
    Client8 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            7
    ),
    Client9 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            8
    ),
    Client10 AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
        LIMIT 1
        OFFSET
            9
    )

INSERT INTO
    Logpunch_Registrations (
        Id,
        EmployeeId,
        Registration_Type,
        Amount,
        Registration_Start,
        Registration_End,
        CreatorId,
        ClientId,
        Creation_Time,
        Status_Type,
        First_Comment,
        Second_Comment,
        CorrectionOf_Id
    )
VALUES (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        300,
        '2023-06-01 09:00:00',
        '2023-06-01 14:00:00',
        (
            SELECT EmployeeId
            FROM Employee
        ),
        (
            SELECT ClientId
            FROM Client1
        ),
        '2023-06-01 09:00:00',
        5,
        'First comment 1',
        'Second comment 1',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        120,
        '2024-01-02 10:00:00',
        '2024-01-02 12:00:00',
        (
            SELECT EmployeeId
            FROM Employee
        ),
        (
            SELECT ClientId
            FROM Client2
        ),
        '2024-01-02 10:00:00',
        5,
        'First comment 2',
        'Second comment 2',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        180,
        '2024-02-15 11:00:00',
        '2024-02-15 14:00:00',
        (
            SELECT EmployeeId
            FROM Employee
        ),
        (
            SELECT ClientId
            FROM Client3
        ),
        '2024-02-15 11:00:00',
        5,
        'First comment 3',
        'Second comment 3',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        240,
        '2024-02-16 08:00:00',
        '2024-02-16 12:00:00',
        (
            SELECT EmployeeId
            FROM Employee
        ),
        (
            SELECT ClientId
            FROM Client4
        ),
        '2024-02-16 08:00:00',
        5,
        'First comment 4',
        'Second comment 4',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        360,
        '2024-06-17 09:00:00',
        '2024-06-17 15:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        (
            SELECT ClientId
            FROM Client5
        ),
        '2024-06-18 11:20:05',
        5,
        'First comment 5',
        'Second comment 5',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        180,
        '2024-06-18 10:00:00',
        '2024-06-18 13:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        (
            SELECT ClientId
            FROM Client6
        ),
        '2024-06-18 16:24:25',
        5,
        'First comment 6',
        'Second comment 6',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        240,
        '2024-06-21 09:00:00',
        '2024-06-21 13:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        (
            SELECT ClientId
            FROM Client7
        ),
        '2024-06-30 21:12:45',
        5,
        'First comment 7',
        'Second comment 7',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        300,
        '2024-07-02 08:00:00',
        '2024-07-02 13:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        (
            SELECT ClientId
            FROM Client8
        ),
        '2024-07-02 15:00:00',
        5,
        'First comment 8',
        'Second comment 8',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        360,
        '2024-07-03 09:00:00',
        '2024-07-03 15:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        (
            SELECT ClientId
            FROM Client9
        ),
        '2024-07-03 16:00:00',
        5,
        'First comment 9',
        'Second comment 9',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT EmployeeId
            FROM Employee
        ),
        0,
        240,
        '2024-07-08 10:00:00',
        '2024-07-08 14:00:00',
        (
            SELECT AdminId
            FROM Admin
        ),
        (
            SELECT ClientId
            FROM Client10
        ),
        '2024-07-09 10:00:00',
        5,
        'First comment 10',
        'Second comment 10',
        NULL
    );
