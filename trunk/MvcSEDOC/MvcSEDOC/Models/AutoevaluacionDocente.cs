using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSEDOC.Models
{    

    public class AutoevaluacionDocente
    {
        public String nombredocente { get; set; }
        public String nombrejefe { get; set; }
        public String fechaevaluacion { get; set; }       
        public int periodoanio { get; set; }
        public int periodonum { get; set; }
        public Double totalhorassemana { get; set; }       
        public List<ResAutoEvaluacionLabor> autoevaluacioneslabores { get; set; }
    }

    public class ResAutoEvaluacionLabor
    {
        public int idlabor { get; set; }
        public String descripcion { get; set; }
        public String tipolabor { get; set; }
        public String tipolaborcorto { get; set; }
        public int nota { get; set; }
        public String problemadescripcion { get; set; }
        public String problemasolucion { get; set; }
        public String resultadodescripcion { get; set; }
        public String resultadosolucion { get; set; }
    }
    
}