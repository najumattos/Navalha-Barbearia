using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgendamentoController : ControllerBase
    {
        private readonly IAgendamentoService _agendamentoService;

        public AgendamentoController(IAgendamentoService agendamentoService)
        {
            _agendamentoService = agendamentoService;
        }

        [HttpGet("administrador")]
        public IActionResult ObterTodosParaAdministrador([FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                return Ok(_agendamentoService.ObterTodosParaAdministrador(tipoAcessoSolicitante));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("funcionario/{barbeiroId:int}")]
        public IActionResult ObterPorBarbeiro(int barbeiroId, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                return Ok(_agendamentoService.ObterPorBarbeiroId(barbeiroId, tipoAcessoSolicitante));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("cliente")]
        public IActionResult ObterPorCpfCliente([FromQuery] string cpf, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                return Ok(_agendamentoService.ObterPorCpfCliente(cpf, tipoAcessoSolicitante));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("{idAgendamento:int}")]
        public IActionResult ObterPorId(int idAgendamento, [FromQuery] int barbeiroIdSolicitante, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var agendamento = _agendamentoService.ObterPorId(idAgendamento, barbeiroIdSolicitante, tipoAcessoSolicitante);

                if (agendamento is null)
                {
                    return NotFound();
                }

                return Ok(agendamento);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPut("funcionario/{idAgendamento:int}")]
        public IActionResult AtualizarDoFuncionario(int idAgendamento, [FromQuery] int barbeiroIdSolicitante, [FromBody] AgendamentoModel agendamento)
        {
            try
            {
                var atualizado = _agendamentoService.AtualizarDoFuncionario(idAgendamento, barbeiroIdSolicitante, agendamento, TipoAcessoEnum.Funcionario);
                return Ok(atualizado);
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

        [HttpPut("cliente/{idAgendamento:int}/status")]
        public IActionResult AtualizarStatusDoCliente(int idAgendamento, [FromQuery] string cpfClienteSolicitante, [FromQuery] StatusAgendamentoEnum status)
        {
            try
            {
                var atualizado = _agendamentoService.AtualizarStatusDoCliente(idAgendamento, cpfClienteSolicitante, status, TipoAcessoEnum.Cliente);
                return Ok(atualizado);
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

        [HttpPost]
        public IActionResult Criar([FromBody] AgendamentoModel agendamento)
        {
            // A controller continua fina: recebe dados, delega ao service e devolve resposta HTTP.
            var novoAgendamento = _agendamentoService.Criar(agendamento);
            return CreatedAtAction(
                nameof(ObterPorId),
                new
                {
                    idAgendamento = novoAgendamento.IdAgendamento,
                    barbeiroIdSolicitante = novoAgendamento.Barbeiro.Id,
                    tipoAcessoSolicitante = TipoAcessoEnum.Funcionario
                },
                novoAgendamento);
        }
    }
}