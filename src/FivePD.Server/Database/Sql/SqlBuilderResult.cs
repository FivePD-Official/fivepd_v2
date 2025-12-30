using System;
using Dapper;

namespace FivePD.Server.Database.Sql;

#pragma warning disable

public readonly record struct SqlBuilderResult(string Sql, DynamicParameters Parameters, Type[] Types, string SplitOn);