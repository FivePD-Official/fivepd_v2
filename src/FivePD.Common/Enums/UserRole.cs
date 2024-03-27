using System;

namespace FivePD.Common.Enums;

#pragma warning disable

[Flags]
public enum UserRole
{
    None = 0,
    Admin = 1,
    Developer = 2,
    Dispatcher = 4,
}