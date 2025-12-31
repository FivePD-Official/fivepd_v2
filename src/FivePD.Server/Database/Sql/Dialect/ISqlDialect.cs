using System.Collections.Generic;

namespace FivePD.Server.Database.Sql.Dialect;

#pragma warning disable

public interface ISqlDialect
{
    char QuoteOpen { get; }
    char QuoteClose { get; }
    string GetPaginationSql(int pageSize, int offset);
    string GetIdentitySql();
    string GetUpsertSql(string tableName, List<string> columns, List<string> keyColumns);
}