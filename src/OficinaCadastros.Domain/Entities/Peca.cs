namespace OficinaCadastros.Domain.Entities;

public class Peca
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public decimal PrecoUnitario { get; private set; }
    public int Estoque { get; private set; }

    private Peca() { }

    public Peca(string codigo, string nome, decimal precoUnitario, int estoque)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Código é obrigatório.", nameof(codigo));
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        if (precoUnitario < 0) throw new ArgumentException("Preço não pode ser negativo.", nameof(precoUnitario));
        if (estoque < 0) throw new ArgumentException("Estoque não pode ser negativo.", nameof(estoque));

        Codigo = codigo;
        Nome = nome;
        PrecoUnitario = precoUnitario;
        Estoque = estoque;
    }

    public void Atualizar(string nome, decimal precoUnitario, int estoque)
    {
        if (precoUnitario < 0) throw new ArgumentException("Preço não pode ser negativo.", nameof(precoUnitario));
        if (estoque < 0) throw new ArgumentException("Estoque não pode ser negativo.", nameof(estoque));
        Nome = nome;
        PrecoUnitario = precoUnitario;
        Estoque = estoque;
    }
}
