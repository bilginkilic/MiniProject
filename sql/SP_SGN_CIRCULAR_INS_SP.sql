ALTER PROC [dbo].[SGN_CIRCULAR_INS_SP]
    @CustomerNo INT NULL,
    @CompanyTitle nvarchar(max) NULL,
    @IssuedDate date NULL,
    @ValidityDate date NULL,
    @InternalBylawsTsgDate date NULL,
    @SpecialCases nvarchar(max) NULL,
    @CircularType nvarchar(max) NULL,
    @CircularNotaryNo nvarchar(max) NULL,
    @Description nvarchar(max) NULL,
    @CircularStatus nvarchar(max) NULL,
    @IsABoardOfDirectorsDecisionRequired nvarchar(50) NULL,
    @MainCircularDate date NULL,
    @MainCircularRef nvarchar(50) NULL,
    @AdditionalDocuments nvarchar(max) NULL,
    @Channel nvarchar(max) NULL
AS
BEGIN
    SET NOCOUNT ON
    SET XACT_ABORT ON

    INSERT INTO dbo.SGN_CIRCULAR 
    (
        CustomerNo, CompanyTitle, IssuedDate, ValidityDate,
        InternalBylawsTsgDate, SpecialCases, CircularType, CircularNotaryNo,
        Description, CircularStatus, IsABoardOfDirectorsDecisionRequired,
        MainCircularDate, MainCircularRef, AdditionalDocuments,
        Channel, CreateDate, LastUpdate, RecordStatus
    )
    VALUES
    (
        @CustomerNo, @CompanyTitle, @IssuedDate, @ValidityDate,
        @InternalBylawsTsgDate, @SpecialCases, @CircularType, @CircularNotaryNo,
        @Description, @CircularStatus, @IsABoardOfDirectorsDecisionRequired,
        @MainCircularDate, @MainCircularRef, @AdditionalDocuments,
        @Channel, GETDATE(), GETDATE(), 'A'
    )

    SELECT SCOPE_IDENTITY() AS ID
END