IF DB_ID(N'Ticketing') IS NULL
BEGIN
    CREATE DATABASE Ticketing;
END;
GO

USE Ticketing;
GO

IF OBJECT_ID(N'dbo.Tickets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tickets
    (
        Code nvarchar(30) NOT NULL,
        TicketType nvarchar(30) NOT NULL,
        Title nvarchar(200) NOT NULL,
        Description nvarchar(max) NOT NULL,
        Priority int NOT NULL,
        CreatedAt datetimeoffset(0) NOT NULL,
        AffectedService nvarchar(100) NOT NULL,

        CONSTRAINT PK_Tickets PRIMARY KEY (Code),
        CONSTRAINT CK_Tickets_Priority
            CHECK (Priority BETWEEN 1 AND 3)
    );
END;
GO