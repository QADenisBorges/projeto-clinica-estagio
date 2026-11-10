using GestaoClinica.Api.DTO.Request;
using GestaoClinica.Api.DTO.Response;
using GestaoClinica.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoClinica.Api.Controllers;

// O controller só recebe a requisição, chama o service e monta a resposta.
// Não tem try/catch: se o service lançar exceção, o GlobalExceptionMiddleware responde.
[ApiController]
[Route("api/clinicas")]
[Produces("application/json")]
public class ClinicasController : ControllerBase
{
    private readonly ClinicaService _service;

    public ClinicasController(ClinicaService service)
    {
        _service = service;
    }

    // Os [ProducesResponseType] só servem para o Swagger mostrar os status que cada endpoint devolve.
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClinicaResponse>>> Listar()
    {
        var clinicas = await _service.ListarAsync();
        return Ok(clinicas.Select(ClinicaResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClinicaResponse>> BuscarPorId(int id)
    {
        var clinica = await _service.BuscarPorIdAsync(id);
        return Ok(ClinicaResponse.DeEntidade(clinica));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClinicaResponse>> Criar(ClinicaRequest request)
    {
        var clinica = await _service.CriarAsync(request.ParaEntidade());

        // 201 Created com o cabeçalho Location apontando para a clínica nova.
        return CreatedAtAction(nameof(BuscarPorId), new { id = clinica.Id }, ClinicaResponse.DeEntidade(clinica));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClinicaResponse>> Atualizar(int id, ClinicaRequest request)
    {
        var clinica = await _service.AtualizarAsync(id, request.ParaEntidade());
        return Ok(ClinicaResponse.DeEntidade(clinica));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        await _service.ExcluirAsync(id);
        return NoContent();
    }
}
