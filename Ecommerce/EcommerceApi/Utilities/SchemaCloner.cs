using EcommerceApi.Entities.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Utilities;
public class SchemaCloner(TenantDbContext context)
{
    public async Task CloneSchema(string @sourceSchema, string @targetSchema)
    {
        FormattableString sql = $@"
DECLARE @SourceSchema NVARCHAR(128) = {sourceSchema};
DECLARE @TargetSchema NVARCHAR(128) = {targetSchema};
DECLARE @sql NVARCHAR(MAX) = '';
DECLARE @tableName NVARCHAR(128);

-- Create schema if not exists
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = @TargetSchema)
    EXEC('CREATE SCHEMA [' + @TargetSchema + ']');

-- Step 1: Copy tables (structure only)
DECLARE table_cursor CURSOR FOR
SELECT t.name
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = @SourceSchema;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @tableName;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = '
    SELECT TOP 0 * INTO [' + @TargetSchema + '].[' + @tableName + '] 
    FROM [' + @SourceSchema + '].[' + @tableName + '];';

    EXEC sp_executesql @sql;

    FETCH NEXT FROM table_cursor INTO @tableName;
END
CLOSE table_cursor;
DEALLOCATE table_cursor;

-- Step 2: Copy default constraints
SELECT @sql = STRING_AGG('
ALTER TABLE [' + @TargetSchema + '].[' + t.name + '] 
ADD CONSTRAINT [' + dc.name + '] DEFAULT ' + dc.definition + 
' FOR [' + c.name + '];', CHAR(10))
FROM sys.default_constraints dc
JOIN sys.columns c ON c.default_object_id = dc.object_id
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = @SourceSchema;

EXEC sp_executesql @sql;

-- Step 3: Copy primary keys and unique constraints
SELECT @sql = STRING_AGG(CONSTRAINT_SQL, CHAR(10))
FROM (
    SELECT 'ALTER TABLE [' + @TargetSchema + '].[' + t.name + '] 
    ADD CONSTRAINT [' + i.name + '] ' +
    'PRIMARY KEY ' +
    CASE WHEN i.type = 1 THEN 'CLUSTERED ' ELSE 'NONCLUSTERED ' END +
    '(' + STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) + ');' AS CONSTRAINT_SQL
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    JOIN sys.tables t ON t.object_id = i.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE i.is_primary_key = 1 AND s.name = @SourceSchema
    GROUP BY i.name, i.type, t.name
) AS keys;

EXEC sp_executesql @sql;

-- Step 4: Copy indexes (non-unique, non-primary)
SELECT @sql = STRING_AGG(IndexSQL, CHAR(10))
FROM (
    SELECT 'CREATE ' + 
        CASE WHEN i.is_unique = 1 THEN 'UNIQUE ' ELSE '' END +
        'NONCLUSTERED INDEX [' + i.name + '] ON [' + @TargetSchema + '].[' + t.name + '] (' +
        STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) + ');' AS IndexSQL
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    JOIN sys.tables t ON t.object_id = i.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE i.is_primary_key = 0 AND i.is_unique_constraint = 0 AND s.name = @SourceSchema
    GROUP BY i.name, i.is_unique, t.name
) AS idx;

EXEC sp_executesql @sql;

-- Step 5: Copy foreign keys
SELECT @sql = STRING_AGG(FKSQL, CHAR(10))
FROM (
    SELECT 'ALTER TABLE [' + @TargetSchema + '].[' + parent.name + '] 
    ADD CONSTRAINT [' + fk.name + '] FOREIGN KEY (' +
    STRING_AGG(pc.name, ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) + 
    ') REFERENCES [' + @TargetSchema + '].[' + referenced.name + '] (' +
    STRING_AGG(rc.name, ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) + ');' AS FKSQL
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    JOIN sys.tables parent ON parent.object_id = fk.parent_object_id
    JOIN sys.tables referenced ON referenced.object_id = fk.referenced_object_id
    JOIN sys.columns pc ON pc.object_id = fk.parent_object_id AND pc.column_id = fkc.parent_column_id
    JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fkc.referenced_column_id
    JOIN sys.schemas s ON parent.schema_id = s.schema_id
    WHERE s.name = @SourceSchema
    GROUP BY fk.name, parent.name, referenced.name
) AS fks;

EXEC sp_executesql @sql;
";

        await context.Database.ExecuteSqlInterpolatedAsync(sql);

    }
}