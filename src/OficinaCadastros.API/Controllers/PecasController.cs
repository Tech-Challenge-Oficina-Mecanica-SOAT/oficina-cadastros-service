using Microsoft.AspNetCore.Mvc;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Application.UseCases;

namespace OficinaCadastros.API.Controllers;

[ApiController]
[Route("pecas")]
[Produces("application/json")]
public class PecasController : ControllerBase
{
    private readonly PecaUseCases _useCases;

    public PecasController(PecaUseCases useCases) => _useCases = useCases;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PecaDto>>> Listar(CancellationToken ct)
        => Ok(await _useCases.ListarAsync(ct));

    [HttpPost]
    [ProducesResponseType(typeof(PecaDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PecaDto>> Criar([FromBody] CriarPecaRequest request, CancellationToken ct)
    {
        var peca = await _useCases.CriarAsync(request, ct);
        return CreatedAtAction(nameof(BuscarPorId), new { id = peca.Id }, peca);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PecaDto>> BuscarPorId(Guid id, CancellationToken ct)
        => Ok(await _useCases.BuscarPorIdAsync(id, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PecaDto>> Atualizar(Guid id, [FromBody] AtualizarPecaRequest request, CancellationToken ct)
        => Ok(await _useCases.AtualizarAsync(id, request, ct));
}
