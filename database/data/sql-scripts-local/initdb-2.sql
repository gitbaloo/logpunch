-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Insert into Logpunch_Users
INSERT INTO
    Logpunch_Users (
        Id,
        First_Name,
        Last_Name,
        Email,
        Password,
        Default_Query,
        Role
    )
VALUES (
        uuid_generate_v4 (),
        'test1',
        'employee1',
        'employee1@test.com',
        'password1',
        NULL,
        0
    ),
    (
        uuid_generate_v4 (),
        'test2',
        'employee2',
        'employee2@test.com',
        'password2',
        NULL,
        0
    ),
    (
        uuid_generate_v4 (),
        'test',
        'admin',
        'admin@test.com',
        'password1',
        NULL,
        1
    );

-- Insert into Logpunch_Clients
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

-- Insert into Logpunch_Employee_Client_Relations
WITH
    Employees AS (
        SELECT Id AS EmployeeId
        FROM Logpunch_Users
        WHERE
            Email IN (
                'employee1@test.com',
                'employee2@test.com'
            )
    ),
    Clients AS (
        SELECT Id AS ClientId
        FROM Logpunch_Clients
    )
INSERT INTO
    Logpunch_Employee_Client_Relations (Id, EmployeeId, ClientId)
SELECT uuid_generate_v4 (), Employees.EmployeeId, Clients.ClientId
FROM Employees, Clients;

-- Insert into Logpunch_Registrations
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
VALUES
    -- Employee 1 registrations
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        300,
        '2023-06-01 09:00:00',
        '2023-06-01 14:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'LEGO'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        120,
        '2024-01-02 10:00:00',
        '2024-01-02 12:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Nets'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        180,
        '2024-02-15 11:00:00',
        '2024-02-15 14:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Novo Nordisk'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        240,
        '2024-02-16 08:00:00',
        '2024-02-16 12:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Statens IT'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        360,
        '2024-06-17 09:00:00',
        '2024-06-17 15:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'TopDanmark'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        180,
        '2024-06-18 10:00:00',
        '2024-06-18 13:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Zoles'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        240,
        '2024-06-21 09:00:00',
        '2024-06-21 13:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Danske Bank'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        300,
        '2024-07-02 08:00:00',
        '2024-07-02 13:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Carlsberg'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        360,
        '2024-07-03 09:00:00',
        '2024-07-03 15:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Maersk'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        240,
        '2024-07-08 10:00:00',
        '2024-07-08 14:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Vestas'
        ),
        '2024-07-09 10:00:00',
        5,
        'First comment 10',
        'Second comment 10',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        1,
        5,
        '2024-07-08 00:00:00',
        '2024-07-13 00:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        NULL,
        '2024-07-09 10:00:00',
        3,
        'First comment 11',
        'Second comment 11',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        0,
        180,
        '2024-08-01 09:00:00',
        '2024-08-01 12:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee1@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Vestas'
        ),
        '2024-08-01 12:00:00',
        2,
        'Work on project X',
        'Met client Y',
        NULL
    ),
    -- Employee 2 registrations
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        480,
        '2023-06-01 09:00:00',
        '2023-06-01 17:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'LEGO'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        60,
        '2024-01-02 10:00:00',
        '2024-01-02 11:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Nets'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        157,
        '2024-02-15 11:00:00',
        '2024-02-15 13:37:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Novo Nordisk'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        300,
        '2024-02-15 08:00:00',
        '2024-02-15 13:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Statens IT'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        360,
        '2024-06-13 09:00:00',
        '2024-06-13 15:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'TopDanmark'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        180,
        '2024-06-19 10:00:00',
        '2024-06-19 13:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Zoles'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        120,
        '2024-06-28 09:00:00',
        '2024-06-28 11:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Danske Bank'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        240,
        '2024-07-07 08:00:00',
        '2024-07-07 12:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Carlsberg'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        360,
        '2024-07-03 09:00:00',
        '2024-07-03 15:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Maersk'
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
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        240,
        '2024-07-08 10:00:00',
        '2024-07-08 14:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Vestas'
        ),
        '2024-07-09 10:00:00',
        5,
        'First comment 10',
        'Second comment 10',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        1,
        5,
        '2024-07-08 00:00:00',
        '2024-07-13 00:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        NULL,
        '2024-07-09 10:00:00',
        3,
        'First comment 11',
        'Second comment 11',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        0,
        180,
        '2024-08-01 09:00:00',
        '2024-08-01 12:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Vestas'
        ),
        '2024-08-01 12:00:00',
        2,
        'Work on project X',
        'Met client Y',
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        1,
        5,
        '2024-06-01 00:00:00',
        '2024-07-05 00:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        NULL,
        '2024-07-10 12:00:00',
        3,
        'Vacation leave',
        NULL,
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        2,
        3,
        '2024-07-10 00:00:00',
        '2024-07-13 00:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        NULL,
        '2024-07-13 10:00:00',
        1,
        'Sick leave due to illness',
        NULL,
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        3,
        2,
        '2024-05-15 00:00:00',
        '2024-05-17 00:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'admin@test.com'
        ),
        NULL,
        '2024-05-18 14:00:00',
        5,
        'Leave for personal reasons',
        NULL,
        NULL
    ),
    (
        uuid_generate_v4 (),
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        4,
        60,
        '2024-08-03 09:00:00',
        '2024-08-03 10:00:00',
        (
            SELECT Id
            FROM Logpunch_Users
            WHERE
                Email = 'employee2@test.com'
        ),
        (
            SELECT Id
            FROM Logpunch_Clients
            WHERE
                Name = 'Vestas'
        ),
        '2024-08-03 10:11:00',
        4,
        'Travel to client site',
        'Traffic was light',
        NULL
    );