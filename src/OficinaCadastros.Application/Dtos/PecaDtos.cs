using System.ComponentModel.DataAnnotations;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.Dtos;

public record PecaDto(Guid Id, string Codigo, string Nome, decimal PrecoUnitario, int Estoque)
{
    public static PecaDto From(Peca p) => new(p.Id, p.Codigo, p.Nome, p.PrecoUnitario, p.Estoque);
}

public class CriarPecaRequest
{
    [Required(ErrorMessage = "Código é obrigatório.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    public decimal PrecoUnitario { get; set; }

    public int Estoque { get; set; }
}

public class AtualizarPecaRequest
{
    [Required(ErrorMessage = "Nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    public decimal PrecoUnitario { get; set; }

    public int Estoque { get; set; }
}
