using System.ComponentModel.DataAnnotations;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.Dtos;

public record ClienteDto(Guid Id, string Nome, string Cpf, string Email, string? Telefone, bool Ativo, DateTime CriadoEm)
{
    public static ClienteDto From(Cliente c) =>
        new(c.Id, c.Nome, c.Cpf, c.Email, c.Telefone, c.Ativo, c.CriadoEm);
}

public class CriarClienteRequest
{
    [Required(ErrorMessage = "Nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "CPF é obrigatório.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve ter 11 dígitos numéricos.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    public string? Telefone { get; set; }
}

public class AtualizarClienteRequest
{
    [Required(ErrorMessage = "Nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    public string? Telefone { get; set; }
}

public class DesativarClienteRequest
{
    /// <summary>solicitacao_cliente | inadimplencia | outro</summary>
    public string? Motivo { get; set; }
}
