-- MESAGEINQUE tablosu: E-posta kuyruğu (sadece sabah 9 ve akşam 5'te gönderim)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MESAGEINQUE')
BEGIN
    CREATE TABLE dbo.MESAGEINQUE
    (
        Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ToEmail         NVARCHAR(256)    NOT NULL,
        Subject         NVARCHAR(500)    NOT NULL,
        Body            NVARCHAR(MAX)    NULL,
        IsBodyHtml      BIT             NOT NULL DEFAULT 1,
        FromEmail       NVARCHAR(256)    NULL,
        Cc              NVARCHAR(MAX)    NULL,   -- Virgülle ayrılmış adresler
        Bcc             NVARCHAR(MAX)    NULL,   -- Virgülle ayrılmış adresler
        Status          TINYINT         NOT NULL DEFAULT 0,  -- 0:Pending, 1:Sent, 2:Failed
        CreatedAt       DATETIME        NOT NULL DEFAULT GETDATE(),
        SentAt          DATETIME        NULL,
        ErrorMessage    NVARCHAR(MAX)   NULL
    );

    CREATE INDEX IX_MESAGEINQUE_Status ON dbo.MESAGEINQUE (Status);
    CREATE INDEX IX_MESAGEINQUE_CreatedAt ON dbo.MESAGEINQUE (CreatedAt);
END
GO
