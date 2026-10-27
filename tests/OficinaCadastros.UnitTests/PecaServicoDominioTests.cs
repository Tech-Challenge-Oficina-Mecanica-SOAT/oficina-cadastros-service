using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.UnitTests;

public class PecaServicoDominioTests
{
    [Fact]
    public void Peca_Criar_NormalizaCodigo()
    {
        var p = new Peca("  flt-001 ", "Filtro", 10m, 1);
        Assert.Equal("FLT-001", p.Codigo);
    }

    [Theory]
    [InlineData("", 10, 1)]
    [InlineData("Filtro", -1, 1)]
    [InlineData("Filtro", 10, -1)]
    public void Peca_Atualizar_ComDadosInvalidos_Lanca(string nome, int preco, int estoque)
    {
        var p = new Peca("FLT-001", "Filtro", 10m, 1);
        Assert.Throws<ArgumentException>(() => p.Atualizar(nome, preco, estoque));
    }

    [Theory]
    [InlineData("", 10, 1)]
    [InlineData("Troca", -1, 1)]
    [InlineData("Troca", 10, -1)]
    public void Servico_Atualizar_ComDadosInvalidos_Lanca(string nome, int preco, int tempo)
    {
        var s = new Servico("Troca", 10m, 1m);
        Assert.Throws<ArgumentException>(() => s.Atualizar(nome, preco, tempo));
    }
}
