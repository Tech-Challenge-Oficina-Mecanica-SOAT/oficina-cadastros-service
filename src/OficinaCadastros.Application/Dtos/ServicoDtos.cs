using System.ComponentModel.DataAnnotations;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.Dtos;

public record ServicoDto(Guid Id, string Nome, decimal PrecoUnitario, decimal TempoEstimadoHoras)
{
    public static ServicoDto From(Servico s) => new(s.Id, s.Nome, s.PrecoUnitario, s.TempoEstimadoHoras);
}

public class CriarServicoRequest
{
    [Required(ErrorMessage = "Nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    public decimal PrecoUnitario { get; set; }

    public decimal TempoEstimadoHoras { get; set; }
}

public class AtualizarServicoRequest
{
    [Required(ErrorMessage = "Nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    public decimal PrecoUnitario { get; set; }

    public decimal TempoEstimadoHoras { get; set; }
}
