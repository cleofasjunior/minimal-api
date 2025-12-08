using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Services;

public class TokenService
{
    public static object GerarToken(Administrador adm, string key)
    {
        // 1. Prepara a chave de criptografia (tem que ser a mesma do appsettings)
        var keyBytes = Encoding.ASCII.GetBytes(key);

        // 2. Define o conteúdo do token (Claims)
        var tokenConfig = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, adm.Email),
                new Claim(ClaimTypes.Role, adm.Perfil) // Aqui definimos se é Adm ou Editor
            }),
            Expires = DateTime.UtcNow.AddHours(3), // Token expira em 3 horas
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        // 3. Gera e retorna o token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenConfig);

        return new
        {
            token = tokenHandler.WriteToken(token)
        };
    }
}