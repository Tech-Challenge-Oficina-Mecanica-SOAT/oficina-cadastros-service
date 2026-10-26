namespace OficinaCadastros.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private Cliente() { }

    public Cliente(string nome, string cpf, string email, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        if (cpf is null || cpf.Length != 11 || !cpf.All(char.IsDigit))
            throw new ArgumentException("CPF deve ter 11 dígitos numéricos.", nameof(cpf));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("E-mail é obrigatório.", nameof(email));

        Nome = nome;
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
    }

    public void Atualizar(string nome, string email, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("E-mail é obrigatório.", nameof(email));
        Nome = nome;
        Email = email;
        Telefone = telefone;
    }

    public void Desativar()
    {
        if (!Ativo) throw new InvalidOperationException("Cliente já está desativado.");
        Ativo = false;
    }
}
