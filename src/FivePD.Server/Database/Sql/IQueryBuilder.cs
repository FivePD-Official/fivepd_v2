using System;
using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Server.Database.Sql;

#pragma warning disable

public interface IQueryBuilder : IFilterable<IQueryBuilder>
{
    ISelectBuilder Select(string select, Type type = null);
    IDeleteBuilder Delete();
    SqlBuilderResult Build();
    SqlBuilderResult BuildCount();
}

public interface IFilterable<out T>
{
    T If(bool condition, Action<T> action);
    IQueryBuilder Where(string condition, object param = null);
    IQueryBuilder WhereIn<T>(string column, IEnumerable<T> values);
    IQueryBuilder WhereNotIn<T>(string column, IEnumerable<T> values);
    IQueryBuilder WhereContains(string column, string value);
    IQueryBuilder WhereAnyLicenseMatches(Player player);
    IQueryBuilder InnerJoin(string table, string alias, string on, Type type = null, string splitColumn = "Id");
    IQueryBuilder LeftJoin(string table, string alias, string on, Type type = null, string splitColumn = "Id");
    IQueryBuilder RightJoin(string table, string alias, string on, Type type = null, string splitColumn = "Id");
}

public interface ISelectBuilder : IFilterable<ISelectBuilder>
{
    ISelectBuilder GroupBy(string clause);
    ISelectBuilder OrderBy(string clause);
    SqlBuilderResult Build();
    SqlBuilderResult BuildCount();
}

public interface IDeleteBuilder : IFilterable<IDeleteBuilder>
{
    SqlBuilderResult Build();
    SqlBuilderResult BuildCount();
}