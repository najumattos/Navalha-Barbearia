using Microsoft.AspNetCore.Mvc;
using Navalha_Barbearia.Enums;
using Navalha_Barbearia.Models;
using Navalha_Barbearia.Services.Interfaces;

namespace Navalha_Barbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet("administrador")]
        public IActionResult ObterTodosParaAdministrador([FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                return Ok(_clienteService.ObterTodosParaAdministrador(tipoAcessoSolicitante));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("barbeiro/{barbeiroId:int}")]
        public IActionResult ObterPorBarbeiro(int barbeiroId, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                return Ok(_clienteService.ObterPorBarbeiro(barbeiroId, tipoAcessoSolicitante));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("por-cpf")]
        public IActionResult ObterPorCpfPublico([FromQuery] string cpf)
        {
            var cliente = _clienteService.ObterPorCpfPublico(cpf);
            if (cliente is null)
            {
                return NotFound();
            }

            return Ok(cliente);
        }

        [HttpPost("barbeiro/{barbeiroId:int}")]
        public IActionResult CadastrarPorBarbeiro(int barbeiroId, [FromBody] ClienteModel cliente, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var novoCliente = _clienteService.CadastrarPorBarbeiro(barbeiroId, cliente, tipoAcessoSolicitante);
                return Ok(novoCliente);
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

        [HttpPut("barbeiro/{barbeiroId:int}/{clienteId:int}")]
        public IActionResult AtualizarPorBarbeiro(int barbeiroId, int clienteId, [FromBody] ClienteModel cliente, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var clienteAtualizado = _clienteService.AtualizarPorBarbeiro(clienteId, barbeiroId, cliente, tipoAcessoSolicitante);
                return Ok(clienteAtualizado);
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

        [HttpPatch("barbeiro/{barbeiroId:int}/{clienteId:int}/desativar")]
        public IActionResult DesativarPorBarbeiro(int barbeiroId, int clienteId, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var cliente = _clienteService.DesativarPorBarbeiro(clienteId, barbeiroId, tipoAcessoSolicitante);
                return Ok(cliente);
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

        [HttpPut("administrador/{clienteId:int}")]
        public IActionResult AtualizarPorAdministrador(int clienteId, [FromBody] ClienteModel cliente, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var clienteAtualizado = _clienteService.AtualizarPorAdministrador(clienteId, cliente, tipoAcessoSolicitante);
                return Ok(clienteAtualizado);
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

        [HttpPatch("administrador/{clienteId:int}/desativar")]
        public IActionResult DesativarPorAdministrador(int clienteId, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                var cliente = _clienteService.DesativarPorAdministrador(clienteId, tipoAcessoSolicitante);
                return Ok(cliente);
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

        [HttpDelete("administrador/{clienteId:int}")]
        public IActionResult ExcluirPorAdministrador(int clienteId, [FromQuery] TipoAcessoEnum tipoAcessoSolicitante)
        {
            try
            {
                _clienteService.ExcluirPorAdministrador(clienteId, tipoAcessoSolicitante);
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
    }
}