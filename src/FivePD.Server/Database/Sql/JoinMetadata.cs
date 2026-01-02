using System;

namespace FivePD.Server.Database.Sql;

#pragma warning disable

public readonly record struct JoinMetadata(string JoinSql, string SplitColumn);