CREATE TABLE IF NOT EXISTS Users (
    Id TEXT PRIMARY KEY,
    Email TEXT UNIQUE,
    PasswordHash TEXT,
    Name TEXT,
    CreatedAt TEXT,
    UpdatedAt TEXT
);

CREATE TABLE IF NOT EXISTS Tasks (
    Id TEXT PRIMARY KEY,
    Title TEXT,
    Description TEXT,
    Status TEXT,
    DueDate TEXT,
    UserId TEXT,
    CreatedAt TEXT,
    UpdatedAt TEXT,
    FOREIGN KEY(UserId) REFERENCES Users(Id)
);

INSERT OR IGNORE INTO Users (Id, Email, PasswordHash, Name, CreatedAt, UpdatedAt)
VALUES ('11111111-1111-1111-1111-111111111111', 'demo@ballastlane.com', 'Pa$$w0rd!', 'Demo User', '2024-01-01', '2024-01-01');

INSERT OR IGNORE INTO Tasks (Id, Title, Description, Status, DueDate, UserId, CreatedAt, UpdatedAt)
VALUES
('22222222-2222-2222-2222-222222222221', 'Prepare architecture draft', 'Define modules and boundaries', 'Pending', '2026-12-01T10:00:00Z', '11111111-1111-1111-1111-111111111111', '2024-01-01', '2024-01-01'),
('22222222-2222-2222-2222-222222222222', 'Implement auth flow', 'Register, login and JWT generation', 'InProgress', '2026-12-03T12:00:00Z', '11111111-1111-1111-1111-111111111111', '2024-01-01', '2024-01-01'),
('22222222-2222-2222-2222-222222222223', 'Review API endpoints', 'Validate status codes and payloads', 'Completed', '2026-12-05T16:00:00Z', '11111111-1111-1111-1111-111111111111', '2024-01-01', '2024-01-01');
