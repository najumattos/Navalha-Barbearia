using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarbeiroController : ControllerBase
    {
        private readonly IBarbeiroService _barbeiroService;

        public BarbeiroController(IBarbeiroService barbeiroService)
        {
            _barbeiroService = barbeiroService;
        }

        [HttpGet]
        public IActionResult ObterTodos()
        {
            var barbeiros = _barbeiroService.ObterTodos();
            return Ok(barbeiros);
        }

        [HttpGet("{id:int}")]
        public IActionResult ObterPorId(int id)
        {
            var barbeiro = _barbeiroService.ObterPorId(id);

            if (barbeiro is null)
            {
                return NotFound();
            }

            return Ok(barbeiro);
        }

        [HttpPost]
        public IActionResult Criar([FromBody] BarbeiroModel barbeiro)
        {
            var novoBarbeiro = _barbeiroService.Criar(barbeiro);
            return CreatedAtAction(nameof(ObterPorId), new { id = novoBarbeiro.Id }, novoBarbeiro);
        }

        [HttpPost("{barbeiroId:int}/procedimentos/{procedimentoEnum}")]
        public IActionResult AdicionarProcedimento(int barbeiroId, ProcedimentoEnum procedimentoEnum, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var procedimento = _barbeiroService.AdicionarProcedimentoAoBarbeiro(barbeiroId, procedimentoEnum, tipoAcessoSolicitante);
                return Ok(procedimento);
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

        [HttpDelete("{barbeiroId:int}/procedimentos/{procedimentoEnum}")]
        public IActionResult RemoverProcedimento(int barbeiroId, ProcedimentoEnum procedimentoEnum, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                _barbeiroService.RemoverProcedimentoDoBarbeiro(barbeiroId, procedimentoEnum, tipoAcessoSolicitante);
                return NoContent();
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

        [HttpPut("{barbeiroId:int}/procedimentos/{procedimentoEnum}/preco")]
        public IActionResult AtualizarPrecoPorBarbeiro(int barbeiroId, ProcedimentoEnum procedimentoEnum, [FromQuery] decimal precoPorBarbeiro, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var procedimento = _barbeiroService.AtualizarPrecoPorBarbeiro(barbeiroId, procedimentoEnum, precoPorBarbeiro, tipoAcessoSolicitante);
                return Ok(procedimento);
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