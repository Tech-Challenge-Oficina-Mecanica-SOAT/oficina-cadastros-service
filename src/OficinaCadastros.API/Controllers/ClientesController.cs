using Microsoft.AspNetCore.Mvc;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Application.UseCases;

namespace OficinaCadastros.API.Controllers;

[ApiController]
[Route("clientes")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly ClienteUseCases _useCases;

    public ClientesController(ClienteUseCases useCases) => _useCases = useCases;

    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ClienteDto>> Criar([FromBody] CriarClienteRequest request, CancellationToken ct)
    {
        var cliente = await _useCases.CriarAsync(request, ct);
        return CreatedAtAction(nameof(BuscarPorId), new { id = cliente.Id }, cliente);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteDto>> BuscarPorId(Guid id, CancellationToken ct)
        => Ok(await _useCases.BuscarPorIdAsync(id, ct));

    [HttpGet("cpf/{cpf}")]
    public async Task<ActionResult<ClienteDto>> BuscarPorCpf(string cpf, CancellationToken ct)
        => Ok(await _useCases.BuscarPorCpfAsync(cpf, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClienteDto>> Atualizar(Guid id, [FromBody] AtualizarClienteRequest request, CancellationToken ct)
        => Ok(await _useCases.AtualizarAsync(id, request, ct));

    [HttpPost("{id:guid}/desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Desativar(
        Guid id,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] DesativarClienteRequest? request,
        CancellationToken ct)
    {
        await _useCases.DesativarAsync(id, request?.Motivo, ct);
        return NoContent();
    }
}
