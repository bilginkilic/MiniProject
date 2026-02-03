-- Tüm kayıtları getir (Select All)
CREATE OR ALTER PROC [dbo].[SP_MESAGEINQUE_GET_ALL]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, ToEmail, Subject, Body, IsBodyHtml, FromEmail, Cc, Bcc,
           Status, CreatedAt, SentAt, ErrorMessage
    FROM dbo.MESAGEINQUE
    ORDER BY CreatedAt DESC;
END
GO
