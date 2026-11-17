using GestaoClinica.Api.DTO.Request;
using GestaoClinica.Api.DTO.Response;
using GestaoMedico.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GestaoMedico.Api.Controllers;

// O controller só recebe a requisição, chama o service e monta a resposta.
// Não tem try/catch: se o service lançar exceção, o GlobalExceptionMiddleware responde.
[ApiController]
[Route("api/Medicos")]
[Produces("application/json")]
public class MedicosController : ControllerBase
{
    private readonly IMedicoService _service;

    public MedicosController(IMedicoService service)
    {
        _service = service;
    }

    // Os [ProducesResponseType] só servem para o Swagger mostrar os status que cada endpoint devolve.
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MedicoResponse>>> Listar()
    {
        var Medicos = await _service.ListarAsync();
        return Ok(Medicos.Select(MedicoResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicoResponse>> BuscarPorId(int id)
    {
        var Medico = await _service.BuscarPorIdAsync(id);
        return Ok(MedicoResponse.DeEntidade(Medico));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MedicoResponse>> Criar(MedicoRequest request)
    {
        var Medico = await _service.CriarAsync(request.ParaEntidade());

        // 201 Created com o cabeçalho Location apontando para a clínica nova.
        return CreatedAtAction(nameof(BuscarPorId), new { id = Medico.Id }, MedicoResponse.DeEntidade(Medico));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicoResponse>> Atualizar(int id, MedicoRequest request)
    {
        var Medico = await _service.AtualizarAsync(id, request.ParaEntidade());
        return Ok(MedicoResponse.DeEntidade(Medico));
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
