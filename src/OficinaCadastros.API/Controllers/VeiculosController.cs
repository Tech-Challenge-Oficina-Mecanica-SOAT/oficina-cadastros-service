using Microsoft.AspNetCore.Mvc;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Application.UseCases;

namespace OficinaCadastros.API.Controllers;

[ApiController]
[Route("veiculos")]
[Produces("application/json")]
public class VeiculosController : ControllerBase
{
    private readonly VeiculoUseCases _useCases;

    public VeiculosController(VeiculoUseCases useCases) => _useCases = useCases;

    [HttpPost]
    [ProducesResponseType(typeof(VeiculoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<VeiculoDto>> Criar([FromBody] CriarVeiculoRequest request, CancellationToken ct)
    {
        var veiculo = await _useCases.CriarAsync(request, ct);
        return CreatedAtAction(nameof(BuscarPorId), new { id = veiculo.Id }, veiculo);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VeiculoDto>> BuscarPorId(Guid id, CancellationToken ct)
        => Ok(await _useCases.BuscarPorIdAsync(id, ct));

    [HttpGet("placa/{placa}")]
    public async Task<ActionResult<VeiculoDto>> BuscarPorPlaca(string placa, CancellationToken ct)
        => Ok(await _useCases.BuscarPorPlacaAsync(placa, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VeiculoDto>> Atualizar(Guid id, [FromBody] AtualizarVeiculoRequest request, CancellationToken ct)
        => Ok(await _useCases.AtualizarAsync(id, request, ct));
}
