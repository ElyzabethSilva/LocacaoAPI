using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocacaoAPI.Data.DataModel {
    [Table("Locacao")]
    public class Locacao {
        [Key]
        public int Id { get; set; }

        public DateTime DataLocacao { get; set; }
        public DateTime DataDevolucao { get; set; }

        [Required]
        public int Id_Cliente { get; set; }

        [Required]
        public int Id_Filme { get; set; }

        [ForeignKey("Id_Cliente")]
        public Cliente Cliente { get; set; }

        [ForeignKey("Id_Filme")]
        public Filme Filme { get; set; }
    }
}
