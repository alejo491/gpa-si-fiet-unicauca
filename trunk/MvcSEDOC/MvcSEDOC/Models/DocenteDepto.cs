using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSEDOC.Models
{
    public class DocenteDepto
    {
        public String nombre { get; set; }
        public String apellido { get; set; }
        public String estado { get; set; }
        public int iddetp { get; set; }
        public int iddocente { get; set; }
        public int idususaio { get; set; }
    }
}