using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcSEDOC.Models
{
    public class SubordinatesByWork
    {
        public int idLabor { get; set; }
        public string txtLabor { get; set; }
        public string tipoLabor { get; set; }
        public List<Subordinate> subordinateList;

        public SubordinatesByWork(int idL, string txtL, string tipL, List<Subordinate> sl)
        {
            this.idLabor = idL;
            this.txtLabor = txtL;
            this.tipoLabor = tipL;
            this.subordinateList = sl;
        }
    }

    public class Subordinate
    {
        public int idDocente { get; set; }
        public int idLabor { get; set; }
        public int idEvaluacion { get; set; }
        public int puntajeActual { get; set; }
        public int evalEstudiante { get; set; }
        public int autoEval { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }


        public Subordinate(int idD, int idL, int idE, int punt, int evalE, int autoE, string nom, string ape)
        {
            this.idDocente = idD;
            this.idLabor = idL;
            this.idEvaluacion = idE;
            this.puntajeActual = punt;
            this.evalEstudiante = evalE;
            this.autoEval = autoE;
            this.nombres = nom;
            this.apellidos = ape;
        }
    }

}