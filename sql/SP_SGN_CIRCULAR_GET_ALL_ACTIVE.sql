CREATE PROCEDURE [dbo].[SP_SGN_CIRCULAR_GET_ALL_ACTIVE]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.[ID],
        c.[CircularRef] as [Sirküler Ref.],
        c.[CustomerNo] as [Müşteri No],
        c.[CompanyTitle] as [Firma Ünvanı],
        c.[IssuedDate] as [Düzenleme Tarihi],
        c.[ValidityDate] as [Geçerlilik Tarihi],
        c.[CircularType] as [Sirküler Tipi],
        c.[CircularNotaryNo] as [Sirküler Noter No],
        c.[SpecialCases] as [Özel Durumlar],
        c.[CircularStatus] as [Sirküler Durumu],
        'Görüntüle' as [Sirküler Belge],
        -- Diğer alanlar
        c.[InternalBylawsTsgDate],
        c.[Description],
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
    ORDER BY c.[CreateDate] DESC;
END;