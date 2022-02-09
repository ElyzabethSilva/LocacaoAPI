using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocacaoAPI.Data.DataModel {
    [Table("Filme")]
    public class Filme {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Titulo { get; set; }

        [Required]
        public int ClassificacaoIndicativa { get; set; }

        [Required]
        public bool Lancamento { get; set; }
    }
}
