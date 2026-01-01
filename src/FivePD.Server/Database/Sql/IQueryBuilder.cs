using System;
using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Server.Database.Sql;

#pragma warning disable

public interface IFilterable<out T>
{
    ISelectBuilder Select(string select, Type type = null);
    T If(bool condition, Action<T> action);
    IQueryBuilder Where(string condition, object param = null);
    IQueryBuilder WhereEquals(string column, object param);
    IQueryBuilder WhereNotEquals(string column, object param);
    IQueryBuilder WhereIn<T>(string column, IEnumerable<T> values);
    IQueryBuilder WhereNotIn<T>(string column, IEnumerable<T> values);
    IQueryBuilder WhereContains(string column, string value);
    IQueryBuilder WhereAnyLicenseMatches(Player player);
    IQueryBuilder InnerJoin(string table, string alias, string on, string splitColumn = "Id");
    IQueryBuilder LeftJoin(string table, string alias, string on, string splitColumn = "Id");
    IQueryBuilder RightJoin(string table, string alias, string on, string splitColumn = "Id");
}

public interface IQueryBuilder : IFilterable<IQueryBuilder>
{
    IDeleteBuilder Delete();
    SqlBuilderResult Build();
    SqlBuilderResult BuildCount();
}

public interface ISelectBuilder : IFilterable<ISelectBuilder>
{
    ISelectBuilder GroupBy(string clause);
    ISelectBuilder OrderBy(string clause);
    SqlBuilderResult Build();
    SqlBuilderResult BuildCount();
}

public interface IInsertTableBuilder
{
    IInsertBuilder Insert(object entity);
}

public interface IInsertBuilder
{
    IInsertBuilder OnDuplicateKeyUpdate(params string[] customKeys);
    SqlBuilderResult Build();
}

public interface IDeleteBuilder : IFilterable<IDeleteBuilder>
{
    SqlBuilderResult Build();
    SqlBuilderResult BuildCount();
}

public interface IUpdateBuilder : IFilterable<IUpdateBuilder>
{
    IUpdateBuilder Set(string column, object value);
    IUpdateBuilder SetTracked<T>(T tracker, string idColumn = null, object id = null) where T : ChangeTracker;
    SqlBuilderResult Build();
}