using System.Collections.Generic;

namespace FivePD.Server.Database.Sql.Dialect;

#pragma warning disable

public abstract class BaseDialect : ISqlDialect
{
    public abstract char QuoteOpen { get; }
    public abstract char QuoteClose { get; }
    public abstract string GetPaginationSql(int pageSize, int offset);
    public abstract string GetIdentitySql();
    public abstract string GetUpsertSql(string tableName, List<string> columns, List<string> keyColumns);
}