using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Application.Common;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.UseCases;

public class ClienteUseCases
{
    private static readonly string[] MotivosValidos = { "solicitacao_cliente", "inadimplencia", "outro" };

    private readonly IClienteRepository _clientes;
    private readonly IUnitOfWork _uow;

    public ClienteUseCases(IClienteRepository clientes, IUnitOfWork uow)
    {
        _clientes = clientes;
        _uow = uow;
    }

    public async Task<ClienteDto> CriarAsync(CriarClienteRequest request, CancellationToken ct = default)
    {
        var cliente = new Cliente(request.Nome, request.Cpf, request.Email, request.Telefone);

        if (await _clientes.ObterPorCpfAsync(cliente.Cpf, ct) is not null)
            throw new ConflictException("CPF_JA_CADASTRADO", "Já existe um cliente com este CPF.");

        await _clientes.AdicionarAsync(cliente, ct);
        await _uow.SaveChangesAsync(ct);
        return ClienteDto.From(cliente);
    }

    public async Task<ClienteDto> BuscarPorIdAsync(Guid id, CancellationToken ct = default)
        => ClienteDto.From(await ObterOuFalharAsync(id, ct));

    public async Task<ClienteDto> BuscarPorCpfAsync(string cpf, CancellationToken ct = default)
    {
        if (cpf is null || cpf.Length != 11 || !cpf.All(char.IsDigit))
            throw new ArgumentException("CPF deve ter 11 dígitos numéricos.");

        var cliente = await _clientes.ObterPorCpfAsync(cpf, ct)
            ?? throw new NotFoundException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");
        return ClienteDto.From(cliente);
    }

    public async Task<ClienteDto> AtualizarAsync(Guid id, AtualizarClienteRequest request, CancellationToken ct = default)
    {
        var cliente = await ObterOuFalharAsync(id, ct);
        cliente.Atualizar(request.Nome, request.Email, request.Telefone);
        await _uow.SaveChangesAsync(ct);
        return ClienteDto.From(cliente);
    }

    public async Task DesativarAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        if (motivo is not null && !MotivosValidos.Contains(motivo))
            throw new ArgumentException("Motivo inválido. Use: solicitacao_cliente, inadimplencia ou outro.");

        var cliente = await ObterOuFalharAsync(id, ct);

        if (!cliente.Ativo)
            throw new ConflictException("CLIENTE_JA_DESATIVADO", "Cliente já está desativado.");

        cliente.Desativar();

        // TODO semana 3: gravar o evento cliente.desativado na tabela outbox_events
        // dentro desta mesma transação (Outbox Pattern), usando o motivo recebido.
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<Cliente> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await _clientes.ObterPorIdAsync(id, ct)
           ?? throw new NotFoundException("CLIENTE_NAO_ENCONTRADO", "Cliente não encontrado.");
}
