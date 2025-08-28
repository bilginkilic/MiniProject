-- Drop all foreign key constraints
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SGN_AUTHDETAIL_SGN_CIRCULAR')
    ALTER TABLE [dbo].[SGN_AUTHDETAIL] DROP CONSTRAINT [FK_SGN_AUTHDETAIL_SGN_CIRCULAR];

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SGN_AUTHDETAIL_SIGNATURES_SGN_AUTHDETAIL')
    ALTER TABLE [dbo].[SGN_AUTHDETAIL_SIGNATURES] DROP CONSTRAINT [FK_SGN_AUTHDETAIL_SIGNATURES_SGN_AUTHDETAIL];

-- Drop any FK constraints from SGN_CIRCULAR table if they exist
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('dbo.SGN_CIRCULAR');

IF @sql > ''
    EXEC sp_executesql @sql;
