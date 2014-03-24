using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSEDOC.Models
{
    public class ConsolidadoDocencia
    {
        public String nombredocente { get; set; }
        public String nombrejefe { get; set; }
        public String fechaevaluacion { get; set; }        
        public int periodoanio { get; set; }
        public int periodonum { get; set; }
        public String observaciones { get; set; }
        public List<DetalleDocencia> labDocencia { get; set; }
          
    }
     
    public class DetalleDocencia
    {
        public int idLabDoc { get; set; }
        public String grupo { get; set; }
        public String tipo { get; set; }
        public int semestre { get; set; }
        public int creditos { get; set; }
        public int idmateria { get; set; }
        public int horassemana { get; set; }
        public int semanaslaborales { get; set; }
        public String codmateria { get; set; }
        public String nombremateria { get; set; }
        public int idprograma { get; set; }
        public String nombreprograma { get; set; }
        public int evaluacionestudiante { get; set; }
        public int evaluacionjefe { get; set; }
        public int evaluacionautoevaluacion { get; set; }
        public String problemadescripcion { get; set; }
        public String problemasolucion { get; set; }
        public String resultadodescripcion { get; set; }
        public String resultadosolucion { get; set; }
    }

}