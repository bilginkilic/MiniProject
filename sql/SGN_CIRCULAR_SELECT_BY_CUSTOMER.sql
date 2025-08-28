CREATE PROCEDURE [dbo].[SGN_CIRCULAR_SELECT_BY_CUSTOMER]
    @CustomerNo nvarchar(100),
    @CircularStatus nvarchar(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT 
        c.[ID],
        c.[CustomerNo],
        c.[CompanyTitle],
        c.[IssuedDate],
        c.[ValidityDate],
        c.[InternalBylawsTsgDate],
        c.[SpecialCases],
        c.[CircularType],
        c.[CircularNotaryNo],
        c.[Description],
        c.[CircularStatus],
        c.[IsABoardOfDirectorsDecisionRequired],
        c.[MainCircularDate],
        c.[MainCircularRef],
        c.[AdditionalDocuments],
        c.[RecordStatus],
        c.[CreateDate],
        c.[LastUpdate],
        c.[Channel],
        -- Yetkili sayısını getir
        (SELECT COUNT(1) FROM [dbo].[SGN_AUTHDETAIL] a 
         WHERE a.[CircularID] = c.[ID] AND a.[RecordStatus] = 'A') as YetkiliCount
    FROM [dbo].[SGN_CIRCULAR] c
    WHERE c.[CustomerNo] = @CustomerNo
        AND (@CircularStatus IS NULL OR c.[CircularStatus] = @CircularStatus)
        AND c.[RecordStatus] = 'A'
    ORDER BY c.[CreateDate] DESC;
END;
