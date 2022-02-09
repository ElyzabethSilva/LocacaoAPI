using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocacaoAPI.Data;
using LocacaoAPI.Models;
using LocacaoAPI.Data.DataModel;

namespace LocacaoAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class LocacoesController : ControllerBase {
        private readonly AppDbContext _context;

        public LocacoesController(AppDbContext context) {
            _context = context;
        }

        // GET: api/Locacoes
        /// <summary>
        /// Retorna uma lista com todas as locações
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocacaoModel>>> GetLocacoes() {
            var locacoes = await _context.Locacoes.ToListAsync();

            if (locacoes.Count == 0)
                return NotFound("Não há locações cadastradas");

            return locacoes.Select(locacoes => new LocacaoModel() {
                Id = locacoes.Id,

                DataLocacao = locacoes.DataLocacao,
                DataDevolucao = locacoes.DataDevolucao,

                IdCliente = _context.Clientes.FirstOrDefault(c => c.Id == locacoes.Id_Cliente).Id,
                NomeCliente = _context.Clientes.FirstOrDefault(c => c.Id == locacoes.Id_Cliente).Nome,

                IdFilme = _context.Filmes.FirstOrDefault(f => f.Id == locacoes.Id_Filme).Id,
                TituloFilme = _context.Filmes.FirstOrDefault(f => f.Id == locacoes.Id_Filme).Titulo
            }).ToList();
        }

        // GET: api/Locacoes/3
        /// <summary>
        /// Retorna a Locação de acordo com o seu ID de cadastro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<LocacaoModel>> GetLocacaoPorId(int id) {
            var locacao = await _context.Locacoes.FirstOrDefaultAsync(f => f.Id == id);

            if (locacao == null) {
                return NotFound($"Não há locação com ID {id} cadastrado no sistema.");
            }

            return new LocacaoModel() {
                Id = locacao.Id,

                DataLocacao = locacao.DataLocacao,
                DataDevolucao = locacao.DataDevolucao,

                IdCliente = _context.Clientes.FirstOrDefault(c => c.Id == locacao.Id_Cliente).Id,
                NomeCliente = _context.Clientes.FirstOrDefault(c => c.Id == locacao.Id_Cliente).Nome,

                IdFilme = _context.Filmes.FirstOrDefault(f => f.Id == locacao.Id_Filme).Id,
                TituloFilme = _context.Filmes.FirstOrDefault(f => f.Id == locacao.Id_Filme).Titulo
            };
        }

        // POST: api/Locacoes/create
        /// <summary>
        /// Cadastra uma nova locação
        /// </summary>
        /// <param name="locacao"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<ActionResult<LocacaoModel>> PostLocacao(LocacaoModel locacao) {
            try {

                Locacao locacaoNew = new Locacao();
                locacaoNew.Id = locacao.Id;
                locacaoNew.Id_Cliente = locacao.IdCliente;
                locacaoNew.Id_Filme = locacao.IdFilme;
                locacaoNew.DataLocacao = locacao.DataLocacao;

                bool lancamento = _context.Filmes.FirstOrDefaultAsync(f => f.Id == locacao.IdFilme).Result.Lancamento;

                // Filmes do tipo lançamento, terão um prazo de entrega de 2 dias. Filmes comuns terão um prazo de entrega de 3 dias.
                locacaoNew.DataDevolucao = lancamento ? locacao.DataLocacao.AddDays(2) : locacao.DataLocacao.AddDays(3);

                _context.Locacoes.Add(locacaoNew);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetLocacaoPorId), new { id = locacao.Id }, locacao);
            }
            catch (Exception ex) {
                return BadRequest("Requisição inválida." + Environment.NewLine + ex.Message);
            }

        }

        // PUT: api/Locacoes/update/3
        /// <summary>
        /// Realiza a atualização de uma locação
        /// </summary>
        /// <param name="id"></param>
        /// <param name="locacao"></param>
        /// <returns></returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutLocacao(int id, LocacaoModel locacao) {
            try {
                var locacaoEdit = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == locacao.Id);

                if (locacaoEdit == null)
                    return NotFound($"Nenhuma locação encontrada com o ID {id}");

                locacaoEdit.Id_Cliente = locacao.IdCliente;
                locacaoEdit.Id_Filme = locacao.IdFilme;

                if (!string.IsNullOrEmpty(locacao.DataLocacao.ToShortDateString()))
                    locacaoEdit.DataLocacao = locacao.DataLocacao;
                else
                    locacaoEdit.DataLocacao = DateTime.Now;

                if (!string.IsNullOrEmpty(locacao.DataDevolucao.ToShortDateString()))
                    locacaoEdit.DataDevolucao = locacao.DataDevolucao;

                _context.Locacoes.Update(locacaoEdit);
                await _context.SaveChangesAsync();

                return Ok(locacao);
            }
            catch (DbUpdateConcurrencyException ex) {
                if (!LocacaoExists(id)) {
                    return NotFound($"Locação com ID {id} não cadastrado no sistema.");
                }
                else {
                    return BadRequest("Requisição inválida." + Environment.NewLine + ex.Message);
                }
            }
        }

        // DELETE: api/Locacoes/3
        /// <summary>
        /// Exclui uma locação a partir do seu ID. Se não existir, retornando uma mensagem
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLocacao(int id) {
            var locacaoDelete = await _context.Locacoes.FirstOrDefaultAsync(l => l.Id == id);

            if (locacaoDelete == null) {
                return NotFound($"O ID de locação {id} não registrado no sistema.");
            }

            _context.Locacoes.Remove(locacaoDelete);
            await _context.SaveChangesAsync();

            return Ok("Locação excluída com sucesso!");
        }

        private bool LocacaoExists(int id) {
            return _context.Locacoes.Any(e => e.Id == id);
        }
    }
}
