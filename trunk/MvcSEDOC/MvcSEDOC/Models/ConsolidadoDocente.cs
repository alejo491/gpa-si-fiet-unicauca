using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSEDOC.Models
{
    // NUEVO 29
    public class ConsolidadoDocente
    {
        public String nombredocente { get; set; }
        public String nombrejefe { get; set; }
        public String fechaevaluacion { get; set; }
        public Double notafinal { get; set; }
        public int periodoanio { get; set; }
        public int periodonum { get; set; }
        public Double totalhorassemana { get; set; }
        public Double totalporcentajes { get; set; }
        public String observaciones { get; set; }
        public List<ResEvaluacionLabor> evaluacioneslabores { get; set; }
    }
  
    public class ResEvaluacionLabor
    {
        public int idlabor { get; set; }
        public String descripcion { get; set; }
        public String tipolabor { get; set; }
        public String tipolaborcorto { get; set; }
        public Double horasxsemana { get; set; }
        public Double porcentaje { get; set; }
        public Double evalest { get; set; }
        public Double evalauto { get; set; }
        public Double evaljefe { get; set; }
        public Double nota { get; set; }
        public Double acumula { get; set; }
    }
    // FIN NUEVO 29
}