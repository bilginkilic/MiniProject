-- Id ile tek kayıt getir (Select)
CREATE OR ALTER PROC [dbo].[SP_MESAGEINQUE_GET_BY_ID]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, ToEmail, Subject, Body, IsBodyHtml, FromEmail, Cc, Bcc,
           Status, CreatedAt, SentAt, ErrorMessage
    FROM dbo.MESAGEINQUE
    WHERE Id = @Id;
END
GO
