using Microsoft.AspNetCore.Mvc;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Application.UseCases;

namespace OficinaCadastros.API.Controllers;

[ApiController]
[Route("servicos")]
[Produces("application/json")]
public class ServicosController : ControllerBase
{
    private readonly ServicoUseCases _useCases;

    public ServicosController(ServicoUseCases useCases) => _useCases = useCases;

    [HttpPost]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ServicoDto>> Criar([FromBody] CriarServicoRequest request, CancellationToken ct)
    {
        var servico = await _useCases.CriarAsync(request, ct);
        return CreatedAtAction(nameof(BuscarPorId), new { id = servico.Id }, servico);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServicoDto>> BuscarPorId(Guid id, CancellationToken ct)
        => Ok(await _useCases.BuscarPorIdAsync(id, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServicoDto>> Atualizar(Guid id, [FromBody] AtualizarServicoRequest request, CancellationToken ct)
        => Ok(await _useCases.AtualizarAsync(id, request, ct));
}
