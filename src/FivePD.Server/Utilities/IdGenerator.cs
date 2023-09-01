using System;

namespace FivePD.Server.Utilities;

#pragma warning disable

public static class IdGenerator
{
    public static string Generate()
    {
        var guid = Guid.NewGuid().ToString();
        var parts = guid.Split('-');
        return $"{parts[0]}{parts[1]}{parts[2]}";
    }
}