using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSEDOC.Models
{
    public class ChiefEval
    {
        public int idlabor { get; set; }
        public int iddocente { get; set; }
        public int idevaluacion { get; set; }
        public int idcuestionario { get; set; }
        public int EvalEst { get; set; }
        public int EvalAut { get; set; }
        public List<CE_Question> calificaciones { get; set; }
    }

    public class StudentEval : ChiefEval
    {
        public string observaciones { set; get; }
    }

    public class CE_Question: IComparable
    {
        public int idgrupo { get; set; }
        public int idpregunta { get; set; }
        public int calificacion { get; set; }

        public int CompareTo(object obj)
        {
            return idgrupo.CompareTo(((CE_Question)obj).idgrupo);
        }
    }
}