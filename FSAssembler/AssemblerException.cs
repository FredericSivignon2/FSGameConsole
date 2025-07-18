
namespace FSAssembler;

/// <summary>
/// Exception spécifique aux erreurs d'assemblage
/// </summary>
public class AssemblerException : Exception
{
    public AssemblerException(string message) : base(message) { }
    public AssemblerException(string message, Exception innerException) : base(message, innerException) { }
}
