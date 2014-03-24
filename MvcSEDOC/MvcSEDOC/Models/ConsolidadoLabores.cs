using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSEDOC.Models
{
    public class ConsolidadoLabores
    {
        public String nombredocente { get; set; }
        public String nombrejefe { get; set; }
        public String fechaevaluacion { get; set; }
        public int notafinal { get; set; }
        public int periodoanio { get; set; }
        public int periodonum { get; set; }
        public Double totalhorassemana { get; set; }
        public int totalporcentajes { get; set; }
        public String observaciones { get; set; }
        public List<String> detalleslabores { get; set; }    
        public List<gestion> labGestion { get; set; }
        public List<social> labSocial { get; set; }
        //public List<docencia> labDocencia { get; set; }
        public List<DocenciaMateria> labDocencia { get; set; }
        public List<investigacion> labInvestigacion { get; set; }
        public List<trabajodegrado> labTrabajoDeGrado { get; set; }
        public List<trabajodegradoinvestigacion> labTrabajoDegradoInvestigacion { get; set; }
        public List<otras> labOtras { get; set; }
        public List<desarrolloprofesoral> labDesarrolloProfesoral { get; set; }   
    }

    public class DocenciaMateria
    {
        public docencia labDoc { get; set; }
        public materia DocMateria { get; set; }
        public programa MatPrograma { get; set; } 
    }

}