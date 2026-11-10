namespace GestaoClinica.Domain.Exceptions;

// Caso especial de erro de negócio: o registro pedido não existe.
// É uma classe separada para a API reconhecer o caso pelo tipo da exceção e responder 404.
public class RegistroNaoEncontradoException : NegocioException
{
    public RegistroNaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}
