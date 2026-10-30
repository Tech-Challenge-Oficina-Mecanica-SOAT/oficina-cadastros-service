namespace OficinaCadastros.BDD.Support;

public static class Espera
{
    /// <summary>Repete a busca até devolver algo ou estourar o tempo (o publicador do outbox é assíncrono).</summary>
    public static async Task<T?> AteAsync<T>(Func<Task<T?>> busca, int timeoutSegundos = 15) where T : class
    {
        var limite = DateTime.UtcNow.AddSeconds(timeoutSegundos);
        while (DateTime.UtcNow < limite)
        {
            var resultado = await busca();
            if (resultado is not null) return resultado;
            await Task.Delay(250);
        }
        return null;
    }
}
