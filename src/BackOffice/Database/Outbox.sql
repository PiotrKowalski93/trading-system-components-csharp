CREATE TABLE OutboxMessages (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EventType NVARCHAR(200) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    Processed BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_Outbox_Processed ON OutboxMessages(Processed)
INCLUDE (CreatedAtUtc);