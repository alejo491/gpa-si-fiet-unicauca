using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;




namespace MvcSEDOC.Views.Admin
{
    public class IndexViewModel
    {
        // Stores the selected value from the drop down box.        
        public int FacultadID { get; set; }

        // Contains the list of countries.
        public SelectList Facultades { get; set; }

    }
}