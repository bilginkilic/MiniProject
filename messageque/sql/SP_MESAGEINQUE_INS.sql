-- Yeni kayıt ekle (Insert)
CREATE OR ALTER PROC [dbo].[SP_MESAGEINQUE_INS]
    @ToEmail      NVARCHAR(256),
    @Subject      NVARCHAR(500),
    @Body         NVARCHAR(MAX)  = NULL,
    @IsBodyHtml   BIT            = 1,
    @FromEmail    NVARCHAR(256)  = NULL,
    @Cc           NVARCHAR(MAX)  = NULL,
    @Bcc          NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    INSERT INTO dbo.MESAGEINQUE (ToEmail, Subject, Body, IsBodyHtml, FromEmail, Cc, Bcc, Status)
    VALUES (@ToEmail, @Subject, @Body, @IsBodyHtml, @FromEmail, @Cc, @Bcc, 0);

    SELECT SCOPE_IDENTITY() AS Id;
END
GO
