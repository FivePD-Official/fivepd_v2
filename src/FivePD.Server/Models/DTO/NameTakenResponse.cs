using FivePD.Server.Attributes;
using Newtonsoft.Json;

#pragma warning disable

namespace FivePD.Server.Models.DTO;

[Obfuscate]
internal readonly record struct NameTakenResponse(bool Taken);