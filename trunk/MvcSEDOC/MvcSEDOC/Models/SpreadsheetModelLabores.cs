using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcSEDOC.Models
{
    public class SpreadsheetModelLabores
    {
        public String fileName { get; set; }
        public String[] datos { get; set; }
        public string nombredocente { get; set; }
        public string nombrejefe { get; set; }
        public string fechaevaluacion { get; set; }
        public string periodo { get; set; }
        public List<String> detalleslabores { get; set; }
        public List<gestion> labGestion { get; set; }
        public List<social> labSocial { get; set; }        
        public List<DocenciaMateria> labDocencia { get; set; }
        public List<investigacion> labInvestigacion { get; set; }
        public List<trabajodegrado> labTrabajoDeGrado { get; set; }
        public List<trabajodegradoinvestigacion> labTrabajoDegradoInvestigacion { get; set; }
        public List<otras> labOtras { get; set; }
        public List<desarrolloprofesoral> labDesarrolloProfesoral { get; set; }   
             
       
    }
}
