namespace OficinaCadastros.Domain.Entities;

public class Servico
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = string.Empty;
    public decimal PrecoUnitario { get; private set; }
    public decimal TempoEstimadoHoras { get; private set; }

    private Servico() { }

    public Servico(string nome, decimal precoUnitario, decimal tempoEstimadoHoras)
    {
        ValidarDados(nome, precoUnitario, tempoEstimadoHoras);
        Nome = nome;
        PrecoUnitario = precoUnitario;
        TempoEstimadoHoras = tempoEstimadoHoras;
    }

    public void Atualizar(string nome, decimal precoUnitario, decimal tempoEstimadoHoras)
    {
        ValidarDados(nome, precoUnitario, tempoEstimadoHoras);
        Nome = nome;
        PrecoUnitario = precoUnitario;
        TempoEstimadoHoras = tempoEstimadoHoras;
    }

    private static void ValidarDados(string nome, decimal precoUnitario, decimal tempoEstimadoHoras)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        if (precoUnitario < 0) throw new ArgumentException("Preço não pode ser negativo.", nameof(precoUnitario));
        if (tempoEstimadoHoras < 0) throw new ArgumentException("Tempo estimado inválido.", nameof(tempoEstimadoHoras));
    }
}
