using LocacaoAPI.Data.DataModel;
using Microsoft.EntityFrameworkCore;

namespace LocacaoAPI.Data {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {

        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Filme> Filmes { get; set; }
        public DbSet<Locacao> Locacoes { get; set; }

    }
}
