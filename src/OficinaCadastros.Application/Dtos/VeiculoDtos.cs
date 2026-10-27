using System.ComponentModel.DataAnnotations;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.Dtos;

public record VeiculoDto(Guid Id, string Placa, string Modelo, string Marca, int Ano, Guid ClienteId)
{
    public static VeiculoDto From(Veiculo v) =>
        new(v.Id, v.Placa, v.Modelo, v.Marca, v.Ano, v.ClienteId);
}

public class CriarVeiculoRequest
{
    [Required(ErrorMessage = "Placa é obrigatória.")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "Modelo é obrigatório.")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Marca é obrigatória.")]
    public string Marca { get; set; } = string.Empty;

    public int Ano { get; set; }

    public Guid ClienteId { get; set; }
}

public class AtualizarVeiculoRequest
{
    [Required(ErrorMessage = "Modelo é obrigatório.")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Marca é obrigatória.")]
    public string Marca { get; set; } = string.Empty;

    public int Ano { get; set; }
}
