DECLARE @sql nvarchar(max) = '';

SELECT @sql += 'DROP TABLE ' 
             + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13) + CHAR(10)
FROM sys.tables AS t
JOIN sys.schemas AS s
    ON s.schema_id = t.schema_id
WHERE s.name = 'dbo'
  AND t.name LIKE 'Serilog[_]%';

IF @sql = ''
BEGIN
    PRINT 'No matching tables found.';
END
ELSE
BEGIN
    PRINT @sql; -- review first
    --EXEC sys.sp_executesql @sql;  -- uncomment to delete, or run generated SQL script
END