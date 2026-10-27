namespace OficinaCadastros.Domain.Entities;

public class Veiculo
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Placa { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public string Marca { get; private set; } = string.Empty;
    public int Ano { get; private set; }
    public Guid ClienteId { get; private set; }

    private Veiculo() { }

    public Veiculo(string placa, string modelo, string marca, int ano, Guid clienteId)
    {
        if (string.IsNullOrWhiteSpace(placa)) throw new ArgumentException("Placa é obrigatória.", nameof(placa));
        ValidarDados(modelo, marca, ano);

        Placa = NormalizarPlaca(placa);
        Modelo = modelo;
        Marca = marca;
        Ano = ano;
        ClienteId = clienteId;
    }

    public static string NormalizarPlaca(string placa) => placa.Trim().ToUpperInvariant();

    public void Atualizar(string modelo, string marca, int ano)
    {
        ValidarDados(modelo, marca, ano);
        Modelo = modelo;
        Marca = marca;
        Ano = ano;
    }

    private static void ValidarDados(string modelo, string marca, int ano)
    {
        if (string.IsNullOrWhiteSpace(modelo)) throw new ArgumentException("Modelo é obrigatório.", nameof(modelo));
        if (string.IsNullOrWhiteSpace(marca)) throw new ArgumentException("Marca é obrigatória.", nameof(marca));
        if (ano < 1900 || ano > DateTime.UtcNow.Year + 1) throw new ArgumentException("Ano inválido.", nameof(ano));
    }
}
