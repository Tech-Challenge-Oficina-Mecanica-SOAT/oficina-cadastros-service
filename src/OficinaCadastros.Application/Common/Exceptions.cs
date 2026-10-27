namespace OficinaCadastros.Application.Common;

public class NotFoundException : Exception
{
    public string Code { get; }
    public NotFoundException(string code, string message) : base(message) => Code = code;
}

public class ConflictException : Exception
{
    public string Code { get; }
    public ConflictException(string code, string message) : base(message) => Code = code;
}
