using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LocacaoAPI.Data;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using LocacaoAPI.Data.DataModel;
using LocacaoAPI.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data;
using CsvHelper;

namespace LocacaoAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class FilmesController : ControllerBase {
        private readonly AppDbContext _context;

        public FilmesController(AppDbContext context) {
            _context = context;
        }

        // GET: api/Filmes
        /// <summary>
        /// Retorna uma lista com os filmes cadastrados
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FilmeModel>>> GetFilmes() {
            var filme = await _context.Filmes.ToListAsync();

            if (filme.Count == 0)
                return NotFound("Não há filmes cadastrados");


            return filme.Select(filme => new FilmeModel() {
                Id = filme.Id,
                Titulo = filme.Titulo,
                ClassificacaoIndicativa = filme.ClassificacaoIndicativa,
                Lancamento = filme.Lancamento
            }).ToList();
        }

        // GET: api/Filmes/2
        /// <summary>
        /// Retorna o filme de acordo com o ID cadastrado (Se não existir uma mensagem é retornada)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<FilmeModel>> GetFilmePorId(int id) {
            var filme = await _context.Filmes.FirstOrDefaultAsync(f => f.Id == id);

            if (filme == null) {
                return NotFound($"Não há filme com ID {id} cadastrado no sistema.");
            }

            return new FilmeModel() {
                Id = filme.Id,
                Titulo = filme.Titulo,
                ClassificacaoIndicativa = filme.ClassificacaoIndicativa,
                Lancamento = filme.Lancamento
            };
        }

        // PUT: api/Filmes/update/2
        /// <summary>
        /// Realiza a atualização dos dados de um filme de acordo com o ID (Se não existir, retorna uma mensagem)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filme"></param>
        /// <returns></returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutFilme(int id, FilmeModel filme) {
            try {
                var filmeEdit = await _context.Filmes.FirstOrDefaultAsync(c => c.Id == filme.Id);

                if (filmeEdit == null)
                    return NotFound($"ID {id} não encontrado.");

                filmeEdit.Titulo = filme.Titulo;
                filmeEdit.ClassificacaoIndicativa = filme.ClassificacaoIndicativa;
                filmeEdit.Lancamento = filme.Lancamento;

                _context.Filmes.Update(filmeEdit);
                await _context.SaveChangesAsync();
                return Ok(filme);
            }
            catch (DbUpdateConcurrencyException ex) {
                if (!FilmeExists(id)) {
                    return NotFound($"Filme com ID {id} não cadastrado no sistema.");
                }
                else {
                    return BadRequest("Requisição inválida." + Environment.NewLine + ex.Message);
                }
            }
        }

        // POST: api/Filmes/create
        /// <summary>
        /// Cadastra um novo Filme
        /// </summary>
        /// <param name="filme"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<ActionResult<FilmeModel>> PostFilme(FilmeModel filme) {
            if (FilmeExists(filme.Id))
                return BadRequest($"ID {filme.Id} já associado a outro filme. Verifique.");

            Filme filmeNew = new Filme();
            filmeNew.Id = filme.Id;
            filmeNew.Titulo = filme.Titulo;
            filmeNew.ClassificacaoIndicativa = filme.ClassificacaoIndicativa;
            filmeNew.Lancamento = filme.Lancamento;

            _context.Filmes.Add(filmeNew);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFilmePorId), new { id = filme.Id }, filme);
        }

        // DELETE: api/Filmes/delete/2
        /// <summary>
        /// Exclui um filme a partir do ID (Se não existir, retorna uma mensagem)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFilme(int id) {
            var filmeDelete = await _context.Filmes.FirstOrDefaultAsync(f => f.Id == id);

            if (filmeDelete == null)
                return NotFound($"ID {id} não existe na base de dados.");

            _context.Filmes.Remove(filmeDelete);
            await _context.SaveChangesAsync();

            return Ok("Filme excluído com sucesso!");
        }


        [HttpPost("import")]
        public async Task<ActionResult> ImportFile(IFormFile file) {
            try {

                if (file.FileName.EndsWith(".csv")) {
                    // Encontrar solução para importação de arquivo CSV. Tentativas com MemoryStream não deram certo
                    using (MemoryStream stream = new MemoryStream()) {
                        file.CopyTo(stream);

                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        // O primeiro erro começa aqui
                        using ExcelPackage excelPackage = new ExcelPackage(stream);

                        ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();

                        int columnCount = excelWorksheet.Dimension.End.Column;

                        int rowCount = excelWorksheet.Dimension.End.Row;

                        for (int row = 1; row < rowCount; row++) {
                            if (!int.TryParse(excelWorksheet.Cells[row, 1].Value.ToString(), out int id))
                                continue;

                            Filme filme = new Filme();
                            filme.Id = Convert.ToInt32(excelWorksheet.Cells[row, 1].Value);
                            filme.Titulo = excelWorksheet.Cells[row, 2].Value.ToString().Trim();
                            filme.Lancamento = bool.Parse(excelWorksheet.Cells[row, 3].Value.ToString());
                            filme.ClassificacaoIndicativa = Convert.ToInt32(excelWorksheet.Cells[row, 4].Value);

                            _context.Filmes.Add(filme);
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                else {
                    return BadRequest("Formato do arquivo não suportado");
                }

                return Ok("Arquivo importado com sucesso!");
            }
            catch (Exception ex) {
                return BadRequest("Requisição inválida." + Environment.NewLine + ex.Message);
            }
        }

        private bool FilmeExists(int id) {
            return _context.Filmes.Any(e => e.Id == id);
        }
    }
}
