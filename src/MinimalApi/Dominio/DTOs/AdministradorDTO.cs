using MinimalApi.Dominio.Enums;

namespace MinimalApi.Dominio.DTOs;

public record AdministradorDTO
{
    public string Email { get; set; } = default!;
    public string Senha { get; set; } = default!;
    public string Perfil { get; set; } = default!; // Adm ou Editor
}