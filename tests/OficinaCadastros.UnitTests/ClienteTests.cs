using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.UnitTests;

public class ClienteTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveNascerAtivo()
    {
        var c = new Cliente("Maria", "12345678901", "maria@email.com", null);
        Assert.True(c.Ativo);
        Assert.NotEqual(Guid.Empty, c.Id);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890a")]
    [InlineData("")]
    public void Criar_ComCpfInvalido_DeveLancar(string cpf)
    {
        Assert.Throws<ArgumentException>(() => new Cliente("Maria", cpf, "maria@email.com", null));
    }

    [Fact]
    public void Desativar_DuasVezes_DeveLancar()
    {
        var c = new Cliente("Maria", "12345678901", "maria@email.com", null);
        c.Desativar();
        Assert.False(c.Ativo);
        Assert.Throws<InvalidOperationException>(() => c.Desativar());
    }
}
