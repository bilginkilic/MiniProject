CREATE PROCEDURE [dbo].[SGN_CIRCULAR_SEARCH]
    @CustomerNo nvarchar(100) = NULL,
    @CompanyTitle nvarchar(max) = NULL,
    @CircularType nvarchar(max) = NULL,
    @CircularStatus nvarchar(max) = NULL,
    @StartDate datetime = NULL,
    @EndDate datetime = NULL
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
    WHERE c.[RecordStatus] = 'A'
        AND (@CustomerNo IS NULL OR c.[CustomerNo] LIKE '%' + @CustomerNo + '%')
        AND (@CompanyTitle IS NULL OR c.[CompanyTitle] LIKE '%' + @CompanyTitle + '%')
        AND (@CircularType IS NULL OR c.[CircularType] = @CircularType)
        AND (@CircularStatus IS NULL OR c.[CircularStatus] = @CircularStatus)
        AND (@StartDate IS NULL OR c.[CreateDate] >= @StartDate)
        AND (@EndDate IS NULL OR c.[CreateDate] <= @EndDate)
    ORDER BY c.[CreateDate] DESC;
END;
