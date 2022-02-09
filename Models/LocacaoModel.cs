using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LocacaoAPI.Models {
    public class LocacaoModel {
        public int Id { get; set; }
        public DateTime DataLocacao { get; set; }
        public DateTime DataDevolucao { get; set; }

        public int IdCliente { get; set; }
        public string NomeCliente { get; set; }

        public int IdFilme { get; set; }
        public string TituloFilme { get; set; }
    }
}
