using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcSEDOC.Models
{
    public class SpreadsheetModel
    {
        public String fileName { get; set; }
        public String[,] contents { get; set; }
    }
}
