namespace FivePD.Server.Models.DTO;

#pragma warning disable

internal record struct SimpleUserDTO
{
    public int Id { get; set; }
    
    public string Name { get; set; }
}