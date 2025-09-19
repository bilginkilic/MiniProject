-- Custom string splitting function for SQL Server
-- Created: 2024.03.19
-- Description: Splits a string by delimiter, compatible with older SQL Server versions

CREATE FUNCTION [dbo].[SplitString]
(
    @String NVARCHAR(MAX),
    @Delimiter CHAR(1)
)
RETURNS TABLE
AS
RETURN
(
    WITH Splits(InitialPosition, StartPosition, EndPosition) AS
    (
        SELECT 0 AS InitialPosition,
               1 AS StartPosition,
               CHARINDEX(@Delimiter, @String + @Delimiter) AS EndPosition
        UNION ALL
        SELECT EndPosition,
               EndPosition + 1,
               CHARINDEX(@Delimiter, @String + @Delimiter, EndPosition + 1)
        FROM Splits
        WHERE CHARINDEX(@Delimiter, @String + @Delimiter, EndPosition + 1) > 0
    )
    SELECT SUBSTRING(@String, StartPosition, 
            CASE
                WHEN EndPosition > 0 THEN EndPosition - StartPosition
                ELSE 0
            END) AS Value
    FROM Splits
    WHERE LTRIM(RTRIM(SUBSTRING(@String, StartPosition, 
            CASE
                WHEN EndPosition > 0 THEN EndPosition - StartPosition
                ELSE 0
            END))) <> ''
);
GO
