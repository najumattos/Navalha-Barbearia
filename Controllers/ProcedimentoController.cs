using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcedimentoController : ControllerBase
    {
        private readonly IProcedimentoService _procedimentoService;

        public ProcedimentoController(IProcedimentoService procedimentoService)
        {
            _procedimentoService = procedimentoService;
        }

        [HttpGet]
        public IActionResult ObterTodos()
        {
            var procedimentos = _procedimentoService.ObterTodos();
            return Ok(procedimentos);
        }

        [HttpGet("{procedimentoEnum}")]
        public IActionResult ObterPorTipo(ProcedimentoEnum procedimentoEnum)
        {
            var procedimento = _procedimentoService.ObterPorTipo(procedimentoEnum);

            if (procedimento is null)
            {
                return NotFound();
            }

            return Ok(procedimento);
        }

        [HttpPut("{procedimentoEnum}")]
        public IActionResult AtualizarCatalogo(ProcedimentoEnum procedimentoEnum, [FromBody] ProcedimentoModel procedimento, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var procedimentoAtualizado = _procedimentoService.AtualizarCatalogo(procedimentoEnum, procedimento, tipoAcessoSolicitante);
                return Ok(procedimentoAtualizado);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}