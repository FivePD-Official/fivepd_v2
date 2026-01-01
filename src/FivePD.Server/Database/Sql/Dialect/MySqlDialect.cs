using System.Collections.Generic;
using System.Linq;

namespace FivePD.Server.Database.Sql.Dialect;

#pragma warning disable

public class MySqlDialect : BaseDialect
{
    public override char QuoteOpen => '`';
    public override char QuoteClose => '`';
    public override string RandomFunction  => "RAND()";
    public override string GetPaginationSql(int pageSize, int offset) => $" LIMIT {offset}, {pageSize}";
    public override string GetIdentitySql() => " SELECT LAST_INSERT_ID();";

    public override string GetUpsertSql(string tableName, List<string> columns, List<string> keyColumns)
    {
        var updateColumns = columns.Except(keyColumns).ToList();
        if (!updateColumns.Any())
        {
            var firstKey = keyColumns.First();
            return $" ON DUPLICATE KEY UPDATE {firstKey} = {firstKey}";
        }

        var updateSpecs = updateColumns.Select(c => $"{c} = VALUES({c})");
        return $" ON DUPLICATE KEY UPDATE {string.Join(", ", updateSpecs)}";
    }
}