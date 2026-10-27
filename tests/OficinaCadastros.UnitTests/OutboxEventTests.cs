using OficinaCadastros.Application.Events;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.UnitTests;

public class OutboxEventTests
{
    private static OutboxEvent Novo() =>
        ClienteDesativadoEvent.Criar(Guid.NewGuid(), "outro", DateTime.UtcNow, "00-" + new string('a', 32) + "-" + new string('b', 16) + "-01");

    [Fact]
    public void Novo_NasceSemPublicacaoESemTentativas()
    {
        var e = Novo();

        Assert.Null(e.PublishedAt);
        Assert.Equal(0, e.Attempts);
        Assert.Null(e.LastError);
    }

    [Fact]
    public void MarcarPublicado_PreencheDataELimpaErro()
    {
        var e = Novo();
        e.RegistrarFalha("timeout");

        e.MarcarPublicado(new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc));

        Assert.NotNull(e.PublishedAt);
        Assert.Null(e.LastError);
    }

    [Fact]
    public void RegistrarFalha_IncrementaTentativasETruncaMensagem()
    {
        var e = Novo();

        e.RegistrarFalha(new string('x', 5000));
        e.RegistrarFalha("de novo");

        Assert.Equal(2, e.Attempts);
        Assert.Equal("de novo", e.LastError);
    }

    [Fact]
    public void RegistrarFalha_MensagemMuitoLonga_FicaLimitadaA1000()
    {
        var e = Novo();

        e.RegistrarFalha(new string('x', 5000));

        Assert.Equal(1000, e.LastError!.Length);
    }

    [Theory]
    [InlineData("", "{}")]
    [InlineData("cliente.desativado", "")]
    public void Criar_ComDadosInvalidos_Lanca(string tipo, string payload)
    {
        Assert.Throws<ArgumentException>(() => new OutboxEvent(Guid.NewGuid(), tipo, payload, DateTime.UtcNow));
    }

    [Fact]
    public void Criar_ComIdVazio_Lanca()
    {
        Assert.Throws<ArgumentException>(() => new OutboxEvent(Guid.Empty, "cliente.desativado", "{}", DateTime.UtcNow));
    }
}
