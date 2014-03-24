using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcSEDOC.Models
{
    public class SpreadsheetModelAE
    {
        public String fileName { get; set; }
        public String[,] labores { get; set; }
        public string nombredocente { get; set; }
        public string nombrejefe { get; set; }
        public string fechaevaluacion { get; set; }
        public string periodo { get; set; }
             
       
    }
}
