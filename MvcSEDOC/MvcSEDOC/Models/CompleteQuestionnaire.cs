using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSEDOC.Models
{
    public class CompleteQuestionnaire
    {
        public int idCuestionario { get; set; }
        public List<QuestionnaireGroup> listaGrupos { get; set; }

        public CompleteQuestionnaire(int idq, List<QuestionnaireGroup> lqg)
        {
            this.idCuestionario = idq;
            this.listaGrupos = lqg;
        }
    }

    public class QuestionnaireGroup
    {
        public int idGrupo { get; set; }
        public string nombre { get; set; }
        public double porcentaje { get; set; }
        public List<Question> listaPregs { get; set; }

        public QuestionnaireGroup(int idg, string nom, double porc, List<Question> lp)
        {
            this.idGrupo = idg;
            this.nombre = nom;
            this.porcentaje = porc;
            this.listaPregs = lp;
        }
    }

    public class Question
    {
        public int idPregunta { get; set; }
        public string txtPregunta { get; set; }

        public Question(int idp, string txtp)
        {
            this.idPregunta = idp;
            this.txtPregunta = txtp;
        }
    }
}