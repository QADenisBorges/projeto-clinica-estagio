using GestaoClinica.Domain.Exceptions;

namespace GestaoClinica.Api.Middlewares;

// Único lugar da API que trata exceções. Toda requisição passa por aqui,
// por isso os controllers não precisam de try/catch.
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate proximo, ILogger<GlobalExceptionMiddleware> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        // A ordem importa: a exceção filha (404) precisa vir antes da mãe (400).
        catch (RegistroNaoEncontradoException excecao)
        {
            await ResponderAsync(contexto, StatusCodes.Status404NotFound, excecao.Message);
        }
        catch (NegocioException excecao)
        {
            await ResponderAsync(contexto, StatusCodes.Status400BadRequest, excecao.Message);
        }
        catch (Exception excecao)
        {
            // O detalhe do erro vai só para o log. O cliente nunca recebe stack trace.
            _logger.LogError(excecao, "Erro inesperado ao processar {Metodo} {Caminho}",
                contexto.Request.Method, contexto.Request.Path);

            await ResponderAsync(contexto, StatusCodes.Status500InternalServerError,
                "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }
    }

    private static Task ResponderAsync(HttpContext contexto, int status, string mensagem)
    {
        contexto.Response.StatusCode = status;
        return contexto.Response.WriteAsJsonAsync(new { mensagem });
    }
}
