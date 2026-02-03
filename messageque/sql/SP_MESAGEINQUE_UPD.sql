-- Kayıt güncelle (Update) - Status, SentAt, ErrorMessage
CREATE OR ALTER PROC [dbo].[SP_MESAGEINQUE_UPD]
    @Id           INT,
    @Status       TINYINT        = NULL,
    @SentAt       DATETIME       = NULL,
    @ErrorMessage NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.MESAGEINQUE
    SET Status       = ISNULL(@Status, Status),
        SentAt       = ISNULL(@SentAt, SentAt),
        ErrorMessage = @ErrorMessage
    WHERE Id = @Id;
END
GO
