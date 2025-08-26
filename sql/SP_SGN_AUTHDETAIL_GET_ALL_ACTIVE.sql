CREATE PROCEDURE [dbo].[SP_SGN_AUTHDETAIL_GET_ALL_ACTIVE]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.[ID], c.[CustomerNo], c.[CompanyTitle], 
        c.[IssuedDate], c.[ValidityDate], c.[InternalBylawsTsgDate],
        c.[SpecialCases], c.[CircularType], c.[CircularNotaryNo],
        c.[Description], c.[CircularStatus], c.[IsABoardOfDirectorsDecisionRequired],
        c.[MainCircularDate], c.[MainCircularRef], c.[AdditionalDocuments],
        c.[RecordStatus], c.[CreateDate], c.[LastUpdate], c.[Channel],
        -- Yetkili sayısını getir
        (SELECT COUNT(1) FROM [dbo].[SGN_AUTHDETAIL] a 
         WHERE a.[CircularID] = c.[ID] AND a.[RecordStatus] = 'A') as YetkiliCount
    FROM [dbo].[SGN_CIRCULAR] c
    WHERE c.[RecordStatus] = 'A'
    ORDER BY c.[CreateDate] DESC;
END;
