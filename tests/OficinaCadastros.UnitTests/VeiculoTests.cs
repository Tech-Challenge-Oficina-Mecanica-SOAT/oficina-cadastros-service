using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.UnitTests;

public class VeiculoTests
{
    [Fact]
    public void Criar_NormalizaPlaca()
    {
        var v = new Veiculo("  abc1d23 ", "Onix", "Chevrolet", 2021, Guid.NewGuid());
        Assert.Equal("ABC1D23", v.Placa);
    }

    [Theory]
    [InlineData("", "Chevrolet", 2021)]
    [InlineData("Onix", "", 2021)]
    [InlineData("Onix", "Chevrolet", 1800)]
    public void Atualizar_ComDadosInvalidos_Lanca(string modelo, string marca, int ano)
    {
        var v = new Veiculo("ABC1D23", "Onix", "Chevrolet", 2021, Guid.NewGuid());
        Assert.Throws<ArgumentException>(() => v.Atualizar(modelo, marca, ano));
    }
}
