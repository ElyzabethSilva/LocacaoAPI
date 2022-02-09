using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocacaoAPI.Data;
using LocacaoAPI.Models;
using System;
using LocacaoAPI.Data.DataModel;

namespace LocacaoAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context) {
            _context = context;
        }

        // GET: api/Clientes
        /// <summary>
        /// Retorna uma lista com todos os clintes cadastrados 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteModel>>> GetClientes() {
            var cliente = await _context.Clientes.ToListAsync();

            if (cliente.Count == 0)
                return NotFound("Não há clientes cadastrados");

            return cliente.Select(cliente => new ClienteModel() {
                Id = cliente.Id,
                Nome = cliente.Nome,
                CPF = cliente.CPF,
                DataNascimento = cliente.DataNascimento
            }).ToList();
        }

        // GET: api/Clientes/1
        /// <summary>
        /// Retorna o cliente de acordo com seu ID de cadastro (Se existir, se não retorna uma mensagem)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteModel>> GetClientePorId(int id) {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) {
                return NotFound($"Não há cliente com ID {id} cadastrado no sistema.");
            }

            return new ClienteModel() {
                Id = cliente.Id,
                Nome = cliente.Nome,
                CPF = cliente.CPF,
                DataNascimento = cliente.DataNascimento
            };
        }

        // POST: api/Clientes/create
        /// <summary>
        /// Cadastra um novo cliente
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<ActionResult<ClienteModel>> PostCliente(ClienteModel cliente) {
            try {
                if (ClienteExists(cliente.Id))
                    return BadRequest($"ID {cliente.Id} já associado a outro cliente. Verifique.");

                Cliente clienteNew = new Cliente();
                clienteNew.Id = cliente.Id;
                clienteNew.Nome = cliente.Nome;
                clienteNew.CPF = cliente.CPF;
                clienteNew.DataNascimento = cliente.DataNascimento;

                _context.Clientes.Add(clienteNew);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetClientePorId), new { id = cliente.Id }, cliente);
            }
            catch (Exception ex) {
                return BadRequest("Requisição inválida." + Environment.NewLine + ex.Message);
            }
        }

        // PUT: api/Clientes/update/1
        /// <summary>
        /// Realiza a atualização dos dados do cliente de acordo com seu ID (Se existir, se não retorna uma mensagem). 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cliente"></param>
        /// <returns></returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutCliente(int id, ClienteModel cliente) {
            try {
                var clienteEdit = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == cliente.Id);

                if (clienteEdit == null)
                    return NotFound($"ID {id} não encontrado.");

                clienteEdit.Nome = cliente.Nome;
                clienteEdit.CPF = cliente.CPF;
                clienteEdit.DataNascimento = cliente.DataNascimento;

                _context.Clientes.Update(clienteEdit);
                await _context.SaveChangesAsync();

                return Ok(cliente);
            }
            catch (DbUpdateConcurrencyException ex) {
                if (!ClienteExists(id)) {
                    return NotFound($"Cliente com ID {id} não cadastrado no sistema.");
                }
                else {
                    return BadRequest("Requisição inválida." + Environment.NewLine + ex.Message);
                }
            }
        }

        // DELETE: api/Clientes/delete/1
        /// <summary>
        /// Exclui um cliente já cadastrado a partir do seu ID. Se o ID não existir, uma mensagem é retornada
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCliente(int id) {
            var clienteDelete = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);

            if (clienteDelete == null)
                return NotFound($"O ID de cliente {id} não registrado no sistema.");

            _context.Clientes.Remove(clienteDelete);
            await _context.SaveChangesAsync();

            return Ok("Cliente excluído com sucesso!");
        }

        private bool ClienteExists(int id) {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
