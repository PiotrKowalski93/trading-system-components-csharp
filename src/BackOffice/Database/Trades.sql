CREATE TABLE Trades (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    AccountId NVARCHAR(100) NOT NULL,
    Symbol NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,4) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL
);

CREATE INDEX IX_Trades_AccountId ON Trades(AccountId);
CREATE INDEX IX_Trades_Symbol ON Trades(Symbol);