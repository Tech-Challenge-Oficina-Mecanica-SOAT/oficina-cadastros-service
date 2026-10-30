using Reqnroll;

namespace OficinaCadastros.BDD.Support;

/// <summary>Sobe a API (e o Postgres) uma única vez para toda a execução e derruba no final.</summary>
[Binding]
public static class AmbienteBdd
{
    private static CadastrosApiFactory? _factory;

    public static CadastrosApiFactory Factory =>
        _factory ?? throw new InvalidOperationException("O ambiente BDD ainda não foi iniciado.");

    [BeforeTestRun]
    public static async Task Iniciar()
    {
        _factory = new CadastrosApiFactory();
        await ((IAsyncLifetime)_factory).InitializeAsync();
    }

    [AfterTestRun]
    public static async Task Encerrar()
    {
        if (_factory is null) return;
        await ((IAsyncLifetime)_factory).DisposeAsync();
        _factory.Dispose();
        _factory = null;
    }
}
