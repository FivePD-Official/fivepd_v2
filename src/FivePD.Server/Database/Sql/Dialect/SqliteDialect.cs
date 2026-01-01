using System.Collections.Generic;
using System.Linq;

namespace FivePD.Server.Database.Sql.Dialect;

#pragma warning disable

public class SqliteDialect : BaseDialect
{
    public override char QuoteOpen => '"';
    public override char QuoteClose => '"';
    public override string RandomFunction => "RANDOM()";

    public override string GetPaginationSql(int pageSize, int offset)
        => $" LIMIT {pageSize} OFFSET {offset}";

    public override string GetIdentitySql()
        => " SELECT last_insert_rowid();";

    public override string GetUpsertSql(string tableName, List<string> columns, List<string> keyColumns)
    {
        var keys = string.Join(", ", keyColumns);
        var updateColumns = columns.Except(keyColumns).ToList();
        if (!updateColumns.Any())
        {
            return $" ON CONFLICT ({keys}) DO NOTHING";
        }

        var updateSpecs = updateColumns
            .Select(c => $"{c} = EXCLUDED.{c}");

        return $" ON CONFLICT ({keys}) DO UPDATE SET {string.Join(", ", updateSpecs)}";
    }
}