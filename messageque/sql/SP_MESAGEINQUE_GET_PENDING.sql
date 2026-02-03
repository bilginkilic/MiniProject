-- Cron job için: Gönderilmeyi bekleyen kayıtları getir (Status = 0 Pending)
CREATE OR ALTER PROC [dbo].[SP_MESAGEINQUE_GET_PENDING]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, ToEmail, Subject, Body, IsBodyHtml, FromEmail, Cc, Bcc,
           Status, CreatedAt, SentAt, ErrorMessage
    FROM dbo.MESAGEINQUE
    WHERE Status = 0
    ORDER BY CreatedAt ASC;
END
GO
