namespace GestaoClinica.Domain.Exceptions;

// Lançada quando uma regra de negócio é quebrada.
// A mensagem é escrita para o usuário final: a API devolve esse texto com status 400.
public class NegocioException : Exception
{
    public NegocioException(string mensagem) : base(mensagem)
    {
    }

    // Ainda não é usado: fica pronto para quando uma regra precisar de um status diferente de 400.
    public NegocioException(int codigoHttp, string mensagem) : base(mensagem)
    {
        CodigoHttp = codigoHttp;
    }

    public int CodigoHttp { get; } = 400;
}
