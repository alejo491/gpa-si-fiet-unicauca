using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcSEDOC.Models;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;

namespace MvcSEDOC.Controllers
{
    public class DepartmentChiefController : SEDOCController
    {
        //variable para almacenar el reporte
        public static int noAlmaceno;
        public static int siAlmaceno;
        public static int rangoFuera;
        public static List<DocenteReporte> listaReporte = new List<DocenteReporte>();
        public static List<ConsolidadoDocente> consolidadoDocente = new List<ConsolidadoDocente>();
        public static List<ConsolidadoDocente> consolidadoDocenteReporte;       
        //ADICIONADO AUTOEVALAUCION
        public static List<AutoevaluacionDocente> AutoevaluacionDocenteReporte;

        [Authorize]
        public ActionResult Index()
        {
            departamento departamento = (departamento)Session["depto"];
            return View(departamento);
        }       

        [Authorize]
        public ActionResult SetChief()
        {
            departamento depto = (departamento)Session["depto"];
            return View(depto);
        }

        [Authorize]
        // NUEVO 2 febrero  MODIFICADO POR CLARA
        public ActionResult AssignWork()
        {
            int currentper = (int)Session["currentAcadPeriod"];           
            periodo_academico per = GetLastAcademicPeriod();

            if (noAlmaceno == 1 && siAlmaceno != 1)
            {
                ViewBag.Error = "No se realizo ninguna de las asignaciones";                
            }
            if (noAlmaceno == 1 && siAlmaceno == 1)
            {
                ViewBag.Error = "Algunas asignaciones no se realizaron";                
            }
            if (noAlmaceno != 1 && siAlmaceno == 1)
            {
                ViewBag.Message = "Se realizaron todas las asignaciones";                
            }
            departamento depto = (departamento)Session["depto"];               
            noAlmaceno = 0;
            siAlmaceno = 0;
            ViewBag.datos = depto.nombre;

            if (currentper != per.idperiodo)
                ViewBag.periodo = 0;
            else
                ViewBag.periodo = 1;

            return View();
            
        }
        // FIN NUEVO 2 febrero
        public class UserDocente
        {
            public string nombres;
            public string apellidos;
            public int iddocente;
            public int iddepartamento;
        }       

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchDepartament(string term)
        {
            var response = dbEntity.departamento.Select(d => new { label = d.nombre, id = d.iddepartamento }).Where(d => d.label.ToUpper().Contains(term.ToUpper())).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        // NUEVO 29
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetDepartamentTeaching(string term)
        {
            int iddept = int.Parse(term);
          


            var dept_teach = dbEntity.usuario
                .Join(dbEntity.docente,
                usu => usu.idusuario,
                doc => doc.idusuario,
                (usu, doc) => new { usuario = usu, docente = doc })
                .Where(usu_doc => usu_doc.docente.iddepartamento == iddept);

            var response = dept_teach.Select(d => new { label = d.usuario.nombres, ape = d.usuario.apellidos, id = d.docente.iddocente, pos = d.docente.iddocente, iddepto = d.docente.iddepartamento }).OrderBy(d => d.ape).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);


        }
        // FIN NUEVO 29


        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetDepartamentFromTeaching(string term)
        {
            int idgroup = int.Parse(term);
            var response = dbEntity.pregunta.Select(q => new { label = q.pregunta1, id = q.idpregunta, idg = q.idgrupo }).Where(q => q.idg == idgroup).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //MODIFICADO POR CLARA
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetTechingWork(string term) 
        {          
            //int currentper = (int)Session["currentAcadPeriod"];
            departamento depto = (departamento)Session["depto"];
            periodo_academico currentper = GetLastAcademicPeriod();


            var idlaborestotales = (from p in dbEntity.participa
                                    join doc in dbEntity.docente on p.iddocente equals doc.iddocente                                    
                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
                                    join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                    where l.idperiodo == currentper.idperiodo && doc.iddepartamento == depto.iddepartamento
                                    select new Labor { idlabor = l.idlabor }).Distinct().ToList();

            var idlaboresjefe = (from d in dbEntity.dirige
                                 select new Labor { idlabor = d.idlabor }).Distinct().ToList();

          //  var idlabores = new List<Labor>().ToList();

            foreach (Labor labor in idlaboresjefe)
            {
                foreach (Labor laborj in idlaborestotales)
                {
                    if (labor.idlabor == laborj.idlabor)
                    {
                        idlaborestotales.Remove(laborj);
                        break;
                    }
                }
            }

            var response = getLabor(idlaborestotales);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public class Labor
        {
            public int idlabor;
            public String tipoLabor;
            public String descripcion;
            public String nombres; 
            // nuevo 2 febrero 
            public int horasSemana;
            // fin nuevo 2 febrero 
        }

        public List<Labor> getLabor(List<Labor> lista)
        {
            gestion gestion;
            social social;
            investigacion investigacion;
            trabajodegrado trabajoDeGrado;
            trabajodegradoinvestigacion trabajoDeGradoInvestigacion;
            desarrolloprofesoral desarrolloProfesoral;
            docencia docencia;
            materia materia;
            otras otrasLabores;
           
            foreach (Labor jefe in lista)
            {
                string sSQL = " select usuario.nombres + ' ' + usuario.apellidos nombrecompleto from participa " +
                " inner join docente on docente.iddocente = participa.iddocente " +
                " inner join usuario on usuario.idusuario = docente.idusuario where participa.idlabor = " + jefe.idlabor.ToString();
                jefe.nombres = (string)Models.Utilities.ExecuteScalar(sSQL);
                gestion = dbEntity.gestion.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (gestion != null)
                {
                    jefe.tipoLabor = "gestion";
                    jefe.descripcion = gestion.nombrecargo;
                    continue;
                }
                social = dbEntity.social.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (social != null)
                {
                    jefe.tipoLabor = "social";
                    jefe.descripcion = social.nombreproyecto;
                    continue;
                }
                investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (investigacion != null)
                {
                    jefe.tipoLabor = "investigacion";
                    jefe.descripcion = investigacion.nombreproyecto;
                    continue;
                }
                trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (trabajoDeGrado != null)
                {
                    jefe.tipoLabor = "trabajo de grado";
                    jefe.descripcion = trabajoDeGrado.titulotrabajo;
                    continue;
                }
                trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (trabajoDeGradoInvestigacion != null)
                {
                    jefe.tipoLabor = "trabajo de grado investigacion";
                    jefe.descripcion = trabajoDeGradoInvestigacion.titulotrabajo;
                    continue;
                }
                desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (desarrolloProfesoral != null)
                {
                    jefe.tipoLabor = "desarrollo profesoral";
                    jefe.descripcion = desarrolloProfesoral.nombreactividad;
                    continue;
                }
                docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (docencia != null)
                {
                    jefe.tipoLabor = "docencia";
                    materia = dbEntity.materia.SingleOrDefault(m => m.idmateria == docencia.idmateria);
                    jefe.descripcion = materia.nombremateria;
                    continue;
                }
                otrasLabores = dbEntity.otras.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (otrasLabores != null)
                {
                    jefe.tipoLabor = "otras";                    
                    jefe.descripcion = otrasLabores.descripcion;
                    continue;
                }                
            }

            return lista.OrderBy(e => e.tipoLabor).ThenBy(d => d.descripcion).ToList();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult searchTeching(string term)
        {
            var response = dbEntity.usuario.Select(d => new { label = d.nombres + " " + d.apellidos, id = d.idusuario, rol = d.rol }).Where(d => d.label.ToUpper().Contains(term.ToUpper()) && d.rol != "Administrador").ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #region Evaluación

        public evaluacion GetLastEvaluacion()
        {
            evaluacion lastEvaluacion; 
            var maxId = (from ev in dbEntity.evaluacion
                         select ev.idevaluacion).Max();

            lastEvaluacion = dbEntity.evaluacion.Single(q => q.idevaluacion == maxId);

            return lastEvaluacion;
        }


        public void SaveChiefAux(int idJ, int idL, int horas)
        {
            dirige eliminar = dbEntity.dirige.SingleOrDefault(d => d.idlabor == idL);
                    dirige dirige = new dirige();
                    dirige.idusuario = idJ;
                    dirige.idlabor = idL;
                    dirige.horasSemana = horas;

                    if (eliminar != null)
                    {
                        if (eliminar.idevaluacion != null)
                        {
                            int idEvaluacion = (int)eliminar.idevaluacion;
                            dirige.idevaluacion = idEvaluacion;
                        }
                        else
                        {
                            //crea  la nueva evaluación
                            evaluacion nuevaEvaluacion = new evaluacion();
                            nuevaEvaluacion.evaluacionautoevaluacion = -1;
                            nuevaEvaluacion.evaluacionjefe = -1;
                            nuevaEvaluacion.evaluacionestudiante = -1;
                            //agrega la nueva evaluación
                            dbEntity.evaluacion.AddObject(nuevaEvaluacion);
                            dbEntity.SaveChanges();
                            //obtiene el id de la evaluación creada
                            evaluacion evaluacionactual = GetLastEvaluacion();
                            dirige.idevaluacion = evaluacionactual.idevaluacion;
                        }

                        //INICO ADICIONADO POR CLARA
                        if (eliminar.idautoevaluacion != null)
                        {
                            int idAutoEvaluacion = (int)eliminar.idautoevaluacion;
                            dirige.idevaluacion = idAutoEvaluacion;
                        }
                        else
                        {

                            autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                            dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                            dbEntity.SaveChanges();
                            problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                            dbEntity.problema.AddObject(unproblema);
                            dbEntity.SaveChanges();
                            resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                            dbEntity.resultado.AddObject(unresultado);
                            dbEntity.SaveChanges();

                            dirige.idautoevaluacion = unaautoevalaucion.idautoevaluacion;

                        }
                        //FIN ADICIONADO POR CLARA

                        dbEntity.dirige.DeleteObject(eliminar);
                        dbEntity.SaveChanges();
                    }
                    else
                    {
                        //crea  la nueva evaluación
                        evaluacion nuevaEvaluacion = new evaluacion();
                        nuevaEvaluacion.evaluacionautoevaluacion = -1;
                        nuevaEvaluacion.evaluacionjefe = -1;
                        nuevaEvaluacion.evaluacionestudiante = -1;
                        //agrega la nueva evaluación
                        dbEntity.evaluacion.AddObject(nuevaEvaluacion);
                        dbEntity.SaveChanges();
                        //obtiene el id de la evaluación creada
                        evaluacion evaluacionactual = GetLastEvaluacion();
                        dirige.idevaluacion = evaluacionactual.idevaluacion;

                        //INICO ADICIONADO POR CLARA
                        autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                        dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                        dbEntity.SaveChanges();
                        problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                        dbEntity.problema.AddObject(unproblema);
                        dbEntity.SaveChanges();
                        resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                        dbEntity.resultado.AddObject(unresultado);
                        dbEntity.SaveChanges();

                        dirige.idautoevaluacion = unaautoevalaucion.idautoevaluacion;
                        //FIN ADICIONADO POR CLARA
                    }

                    dbEntity.dirige.AddObject(dirige);
                    dbEntity.SaveChanges();
        }
    
   // nuevo 1 febrero 
        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult saveChief(int idL, int idJ)
        {
            try
            {
                int horas = 1;
                if (horas > 0 && horas < 21)
                {
                    SaveChiefAux(idJ, idL, horas);
                    //dirige eliminar = dbEntity.dirige.SingleOrDefault(d => d.idlabor == idL);
                    //dirige dirige = new dirige();
                    //dirige.idusuario = idJ;
                    //dirige.idlabor = idL;
                    //dirige.horasSemana = horas;

                    //if (eliminar != null)
                    //{
                    //    if (eliminar.idevaluacion != null)
                    //    {
                    //        int idEvaluacion = (int)eliminar.idevaluacion;
                    //        dirige.idevaluacion = idEvaluacion;
                    //    }
                    //    else
                    //    {
                    //        //crea  la nueva evaluación
                    //        evaluacion nuevaEvaluacion = new evaluacion();
                    //        nuevaEvaluacion.evaluacionautoevaluacion = -1;
                    //        nuevaEvaluacion.evaluacionjefe = -1;
                    //        nuevaEvaluacion.evaluacionestudiante = -1;
                    //        //agrega la nueva evaluación
                    //        dbEntity.evaluacion.AddObject(nuevaEvaluacion);
                    //        dbEntity.SaveChanges();
                    //        //obtiene el id de la evaluación creada
                    //        evaluacion evaluacionactual = GetLastEvaluacion();
                    //        dirige.idevaluacion = evaluacionactual.idevaluacion;
                    //    }

                    //    //INICO ADICIONADO POR CLARA
                    //    if (eliminar.idautoevaluacion != null)
                    //    {
                    //        int idAutoEvaluacion = (int)eliminar.idautoevaluacion;
                    //        dirige.idevaluacion = idAutoEvaluacion;
                    //    }
                    //    else
                    //    {

                    //        autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                    //        dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                    //        dbEntity.SaveChanges();
                    //        problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                    //        dbEntity.problema.AddObject(unproblema);
                    //        dbEntity.SaveChanges();
                    //        resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                    //        dbEntity.resultado.AddObject(unresultado);
                    //        dbEntity.SaveChanges();

                    //        dirige.idautoevaluacion = unaautoevalaucion.idautoevaluacion;

                    //    }
                    //    //FIN ADICIONADO POR CLARA

                    //    dbEntity.dirige.DeleteObject(eliminar);
                    //    dbEntity.SaveChanges();
                    //}
                    //else
                    //{
                    //    //crea  la nueva evaluación
                    //    evaluacion nuevaEvaluacion = new evaluacion();
                    //    nuevaEvaluacion.evaluacionautoevaluacion = -1;
                    //    nuevaEvaluacion.evaluacionjefe = -1;
                    //    nuevaEvaluacion.evaluacionestudiante = -1;
                    //    //agrega la nueva evaluación
                    //    dbEntity.evaluacion.AddObject(nuevaEvaluacion);
                    //    dbEntity.SaveChanges();
                    //    //obtiene el id de la evaluación creada
                    //    evaluacion evaluacionactual = GetLastEvaluacion();
                    //    dirige.idevaluacion = evaluacionactual.idevaluacion;

                    //    //INICO ADICIONADO POR CLARA
                    //    autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                    //    dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                    //    dbEntity.SaveChanges();
                    //    problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                    //    dbEntity.problema.AddObject(unproblema);
                    //    dbEntity.SaveChanges();
                    //    resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                    //    dbEntity.resultado.AddObject(unresultado);
                    //    dbEntity.SaveChanges();

                    //    dirige.idautoevaluacion = unaautoevalaucion.idautoevaluacion;
                    //    //FIN ADICIONADO POR CLARA
                    //}

                    //dbEntity.dirige.AddObject(dirige);
                    //dbEntity.SaveChanges();
                    var tarea = new { respuesta = '1' };
                    siAlmaceno = 1;
                    return Json(tarea, JsonRequestBehavior.AllowGet);
                }
                else {
                    var tarea = new { respuesta = '0' };
                    noAlmaceno = 1;
                    return Json(tarea, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                var tarea = new { respuesta = '0' };
                noAlmaceno = 1;
                return Json(tarea, JsonRequestBehavior.AllowGet);
               
            }
           
        }
        // FIN 1 FEBRERO 

        public class DocenteReporte
        {
            public int iddocente;
            public int idusuario;
            public int idlabor;
            public string nombres;
            public string apellidos;
            public double social;
            public double gestion;
            public double investigacion;
            public double trabajoInvestigacion;
            public double trabajoGrado;
            public double dProfesoral;
            public double docencia;
            public double total;
        }
       
        public ActionResult CreateReport()
        {
            ViewBag.optionmenu = 3;
            departamento departamento = (departamento)Session["depto"];
            int currentper = (int)Session["currentAcadPeriod"];           
            consolidadoDocenteReporte = new List<ConsolidadoDocente>();

            List<DocenteReporte> docentes = (from u in dbEntity.usuario
                                             join d in dbEntity.docente on u.idusuario equals d.idusuario
                                             join p in dbEntity.participa on d.iddocente equals p.iddocente
                                             join l in dbEntity.labor on p.idlabor equals l.idlabor
                                             where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                                             select new DocenteReporte { iddocente = d.iddocente, idusuario = d.idusuario }).Distinct().ToList();

            foreach (DocenteReporte doc in docentes)
            {
                consolidadoDocenteReporte.Add(crearReporte(doc.idusuario));
            }

            ViewBag.lista = consolidadoDocenteReporte;
            ViewBag.departamento = departamento.nombre;
            return View();
        }
      
        public int obtenerFilas()
        {
            int filasReporte = 0;
            foreach (ConsolidadoDocente filas in consolidadoDocenteReporte)
            {
                filasReporte = filasReporte + filas.evaluacioneslabores.Count() + 2;
            }
            return filasReporte;
        }
        
        public ActionResult ExportToExcel()
        {
            SpreadsheetModel mySpreadsheet = new SpreadsheetModel();

            int total_horas = 0;
            double total_acumulado = 0;

            int tam = obtenerFilas();
            String[,] datos = new String[tam, 13];
            datos[0, 0] = "Docente";
            datos[0, 1] = "Jefe Departamento";
            datos[0, 2] = "Fecha Evaluación";
            datos[0, 3] = "Labor";
            datos[0, 4] = "Tipo";
            datos[0, 5] = "Periodo";
            datos[0, 6] = "H/S";
            datos[0, 7] = "%";
            datos[0, 8] = "Est";
            datos[0, 9] = "AutoEv";
            datos[0, 10] = "JefeNota";
            datos[0, 11] = "Total";
            datos[0, 12] = "Acumulado";

            int i = 1;
            foreach (ConsolidadoDocente labor in consolidadoDocenteReporte)
            {
                for (int j = 0; j < labor.evaluacioneslabores.Count(); j++)
                {
                    datos[i, 0] = labor.nombredocente;
                    datos[i, 1] = labor.nombrejefe;
                    datos[i, 2] = labor.fechaevaluacion;
                    datos[i, 3] = labor.evaluacioneslabores.ElementAt(j).descripcion;
                    datos[i, 4] = labor.evaluacioneslabores.ElementAt(j).tipolaborcorto;
                    datos[i, 5] = "" + labor.periodonum + " - " + labor.periodoanio;
                    datos[i, 6] = "" + labor.evaluacioneslabores.ElementAt(j).horasxsemana;
                    datos[i, 7] = "" + labor.evaluacioneslabores.ElementAt(j).porcentaje;
                    datos[i, 8] = "" + labor.evaluacioneslabores.ElementAt(j).evalest;
                    datos[i, 9] = "" + labor.evaluacioneslabores.ElementAt(j).evalauto;
                    datos[i, 10] = "" + labor.evaluacioneslabores.ElementAt(j).evaljefe;
                    datos[i, 11] = "" + labor.evaluacioneslabores.ElementAt(j).nota;
                    datos[i, 12] = "" + labor.evaluacioneslabores.ElementAt(j).acumula;
                    total_horas = total_horas + (int)labor.evaluacioneslabores.ElementAt(j).horasxsemana;
                    total_acumulado = total_acumulado + (double)labor.evaluacioneslabores.ElementAt(j).acumula;
                    i++;
                }
                datos[i, 0] = "";
                datos[i, 1] = "";
                datos[i, 2] = "";
                datos[i, 3] = "";
                datos[i, 4] = "";
                datos[i, 5] = "Totales" ;
                datos[i, 6] = "" + total_horas;
                datos[i, 7] = "" ;
                datos[i, 8] = "" ;
                datos[i, 9] = "" ;
                datos[i, 10] = "";
                datos[i, 11] = "" ;
                datos[i, 12] = "" + total_acumulado;                
                i++;
                i++;
                total_horas = 0;
                total_acumulado = 0;
            }
            mySpreadsheet.contents = datos;
            periodo_academico lastAcademicPeriod = GetLastAcademicPeriod();
            DateTime Hora = DateTime.Now;
            DateTime Hoy = DateTime.Today;
            string hora = Hora.ToString("HH:mm");
            string hoy = Hoy.ToString("dd-MM");
            mySpreadsheet.fileName = "Reporte" + lastAcademicPeriod.anio + "-" + lastAcademicPeriod.idperiodo + "_" + hoy + "_" + hora + ".xls";
            return View(mySpreadsheet);
        }
       
        [Authorize]
        public ActionResult CreateQuery()
        {
            ViewBag.optionmenu = 1;
            departamento departamento = (departamento)Session["depto"];
            int currentper = (int)Session["currentAcadPeriod"];           

            var docentes = (from u in dbEntity.usuario
                            join d in dbEntity.docente on u.idusuario equals d.idusuario
                            join p in dbEntity.participa on d.iddocente equals p.iddocente
                            join l in dbEntity.labor on p.idlabor equals l.idlabor
                            join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                            where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == p.idlabor
                            select u).Distinct().OrderBy(u => u.apellidos).ToList();

            ViewBag.lista = docentes;
            ViewBag.departamento = departamento.nombre;
            return View();
        }

        // NUEVO 2 FEBRERO 
        [Authorize]
        public ActionResult CreateQuery1()
        {
            ViewBag.optionmenu = 1;
            departamento departamento = (departamento)Session["depto"];
            int currentper = (int)Session["currentAcadPeriod"];

            var docentes = (from u in dbEntity.usuario
                            join d in dbEntity.decanoCoordinador on u.idusuario equals d.idusuario
                            join p in dbEntity.dirige on d.idusuario equals p.idusuario
                            join l in dbEntity.labor on p.idlabor equals l.idlabor
                            join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                            where l.idperiodo == currentper && d.idfacultadDepto == departamento.iddepartamento && l.idlabor == p.idlabor
                            select u).Distinct().OrderBy(u => u.apellidos).ToList();

            ViewBag.lista = docentes;
            ViewBag.departamento = departamento.nombre;
            return View();
        }

        // FIN NUEVO 2 FEBRERO

        #endregion

        //INICIO ADICIONADO POR CLARA

        #region ADICIONADO POR CLARA

        public class ProblemaLabor
        {
            public int idlabor;
            public String tipoLabor;
            public String descripcion;
            public String problemadescripcion;
            public String problemarespuesta;
            public String soluciondescripcion;
            public String solucionrespuesta;
        }

        #region Autoevaluación

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowAutoScores(int idusuario)
        {
            ViewBag.optionmenu = 1;
            ViewBag.reporte = crearReporteAutoevaluacion(idusuario);
            return View();
        }

        public AutoevaluacionDocente crearReporteAutoevaluacion(int idUsuario)
        {
            departamento departamento = (departamento)Session["depto"];           
            int currentper = (int)Session["currentAcadPeriod"];
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
            docente docenteactual = (docente)Session["docente"];
            int idjefe = docenteactual.idusuario;
            usuario jefe = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idjefe);
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();

            AutoevaluacionDocente AutoevaluacionDocenteReporte = new AutoevaluacionDocente() { nombredocente = user.nombres + " " + user.apellidos, nombrejefe = jefe.nombres + " " + jefe.apellidos, fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };

            List<ResAutoEvaluacionLabor> labores = (from u in dbEntity.usuario
                                                    join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                    join p in dbEntity.participa on d.iddocente equals p.iddocente
                                                    join aev in dbEntity.autoevaluacion on p.idautoevaluacion equals aev.idautoevaluacion
                                                    join pr in dbEntity.problema on aev.idautoevaluacion equals pr.idautoevaluacion
                                                    join re in dbEntity.resultado on aev.idautoevaluacion equals re.idautoevaluacion
                                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
                                                    join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                                    where l.idperiodo == PeriodoSeleccionado.idperiodo && u.idusuario == idUsuario
                                                    select new ResAutoEvaluacionLabor { idlabor = l.idlabor, nota = (int)aev.calificacion, problemadescripcion = pr.descripcion, problemasolucion = pr.solucion, resultadodescripcion = re.descripcion, resultadosolucion = re.ubicacion }).ToList();

            labores = setAELabor(labores);
            AutoevaluacionDocenteReporte.autoevaluacioneslabores = labores;
            return AutoevaluacionDocenteReporte;
        }

        public List<ResAutoEvaluacionLabor> setAELabor(List<ResAutoEvaluacionLabor> lista)
        {
            gestion gestion;
            social social;
            investigacion investigacion;
            trabajodegrado trabajoDeGrado;
            trabajodegradoinvestigacion trabajoDeGradoInvestigacion;
            desarrolloprofesoral desarrolloProfesoral;
            docencia docencia;
            otras otra;

            foreach (ResAutoEvaluacionLabor labor in lista)
            {
                gestion = dbEntity.gestion.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (gestion != null)
                {
                    labor.tipolabor = "Gestion";
                    labor.tipolaborcorto = "GES";
                    labor.descripcion = gestion.nombrecargo;
                    //labor.horasxsemana = (int)gestion.horassemana;
                    continue;
                }
                social = dbEntity.social.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (social != null)
                {
                    labor.tipolabor = "Social";
                    labor.tipolaborcorto = "SOC";
                    labor.descripcion = social.nombreproyecto;
                    //labor.horasxsemana = (int)social.horassemana;
                    continue;
                }
                investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (investigacion != null)
                {
                    labor.tipolabor = "Investigación";
                    labor.tipolaborcorto = "INV";
                    labor.descripcion = investigacion.nombreproyecto;
                    //labor.horasxsemana = (int)investigacion.horassemana;
                    continue;
                }
                trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (trabajoDeGrado != null)
                {
                    labor.tipolabor = "Trabajo de Grado";
                    labor.tipolaborcorto = "TDG";
                    labor.descripcion = trabajoDeGrado.titulotrabajo;
                    // labor.horasxsemana = (int)trabajoDeGrado.horassemana;
                    continue;
                }
                trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (trabajoDeGradoInvestigacion != null)
                {
                    labor.tipolabor = "Trabajo de Grado Investigación";
                    labor.tipolaborcorto = "TDGI";
                    labor.descripcion = trabajoDeGradoInvestigacion.titulotrabajo;
                    // labor.horasxsemana = (int)trabajoDeGradoInvestigacion.horassemana;
                    continue;
                }
                desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (desarrolloProfesoral != null)
                {
                    labor.tipolabor = "Desarrollo Profesoral";
                    labor.tipolaborcorto = "DP";
                    labor.descripcion = desarrolloProfesoral.nombreactividad;
                    //labor.horasxsemana = (int)desarrolloProfesoral.horassemana;
                    continue;
                }
                docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (docencia != null)
                {
                    materia materia = dbEntity.materia.SingleOrDefault(g => g.idmateria == docencia.idmateria);
                    labor.tipolabor = "Docencia Directa";
                    labor.tipolaborcorto = "DD";
                    labor.descripcion = materia.nombremateria;
                    //labor.horasxsemana = (int)docencia.horassemana;
                    continue;
                }
                otra = dbEntity.otras.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (otra != null)
                {
                    labor.tipolabor = "Otra";
                    labor.tipolaborcorto = "OTR";
                    labor.descripcion = otra.descripcion;
                    //labor.horasxsemana = (int)docencia.horassemana;
                    continue;
                }
            }
            return lista;
        }

        public ActionResult CreateReportAE()
        {
            ViewBag.optionmenu = 3;
            departamento departamento = (departamento)Session["depto"];
            int currentper = (int)Session["currentAcadPeriod"];           
            AutoevaluacionDocenteReporte = new List<AutoevaluacionDocente>();

            List<DocenteReporte> docentes = (from u in dbEntity.usuario
                                             join d in dbEntity.docente on u.idusuario equals d.idusuario
                                             join p in dbEntity.participa on d.iddocente equals p.iddocente
                                             join l in dbEntity.labor on p.idlabor equals l.idlabor
                                             where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                                             select new DocenteReporte { iddocente = d.iddocente, idusuario = d.idusuario }).Distinct().ToList();

            foreach (DocenteReporte doc in docentes)
            {
                AutoevaluacionDocenteReporte.Add(crearReporteAutoevaluacion(doc.idusuario));
            }

            ViewBag.lista = AutoevaluacionDocenteReporte;
            ViewBag.departamento = departamento.nombre;
            return View();
        }
       
        public int obtenerFilasAE()
        {
            int filasReporte = 0;
            foreach (AutoevaluacionDocente filas in AutoevaluacionDocenteReporte)
            {
                filasReporte = filasReporte + filas.autoevaluacioneslabores.Count() + 1;
            }
            return filasReporte;
        }

        public ActionResult ExportToExcelAutoevaluacion()
        {
            SpreadsheetModelAE mySpreadsheetAE = new SpreadsheetModelAE();
            int tam = obtenerFilasAE();
            String[,] datos = new String[tam, 10];

            datos[0, 0] = "Docente";
            datos[0, 1] = "ID Labor";
            datos[0, 2] = "Tipo Corto";
            datos[0, 3] = "Tipo";
            datos[0, 4] = "Labor";
            datos[0, 5] = "Nota";
            datos[0, 6] = "Descripción Problema";
            datos[0, 7] = "Solución Problema";
            datos[0, 8] = "Descripción Solucion";
            datos[0, 9] = "Ubicación Solucion";

            mySpreadsheetAE.fechaevaluacion = AutoevaluacionDocenteReporte[0].fechaevaluacion;
            mySpreadsheetAE.periodo = "" + AutoevaluacionDocenteReporte[0].periodonum + " - " + AutoevaluacionDocenteReporte[0].periodoanio;
            mySpreadsheetAE.nombrejefe = AutoevaluacionDocenteReporte[0].nombrejefe;


            int i = 1;

            foreach (AutoevaluacionDocente labor in AutoevaluacionDocenteReporte)
            {
                for (int j = 0; j < labor.autoevaluacioneslabores.Count(); j++)
                {
                    datos[i, 0] = labor.nombredocente;
                    datos[i, 1] = "" + labor.autoevaluacioneslabores.ElementAt(j).idlabor;
                    datos[i, 2] = labor.autoevaluacioneslabores.ElementAt(j).tipolaborcorto;
                    datos[i, 3] = labor.autoevaluacioneslabores.ElementAt(j).tipolabor;
                    datos[i, 4] = labor.autoevaluacioneslabores.ElementAt(j).descripcion;
                    datos[i, 5] = "" + labor.autoevaluacioneslabores.ElementAt(j).nota;
                    datos[i, 6] = "" + labor.autoevaluacioneslabores.ElementAt(j).problemadescripcion;
                    datos[i, 7] = "" + labor.autoevaluacioneslabores.ElementAt(j).problemasolucion;
                    datos[i, 8] = "" + labor.autoevaluacioneslabores.ElementAt(j).resultadodescripcion;
                    datos[i, 9] = "" + labor.autoevaluacioneslabores.ElementAt(j).resultadosolucion;
                    i++;
                }
                i++;
            }

            mySpreadsheetAE.labores = datos;
            periodo_academico lastAcademicPeriod = GetLastAcademicPeriod();
            DateTime Hora = DateTime.Now;
            DateTime Hoy = DateTime.Today;
            string hora = Hora.ToString("HH:mm");
            string hoy = Hoy.ToString("dd-MM");
            mySpreadsheetAE.fileName = "Reporte" + lastAcademicPeriod.anio + "-" + lastAcademicPeriod.idperiodo + "_" + hoy + "_" + hora + ".xls";
            return View(mySpreadsheetAE);
        }

        #endregion
        
        #region  Labores
        [Authorize]
        public ActionResult GetLaborType()
        {
            ViewBag.optionmenu = 1;
            docente docente = (docente)Session["docente"];
            int currentper = (int)Session["currentAcadPeriod"];
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            departamento departamento = (departamento)Session["depto"];

            var GenreLst = new List<int>();

            var NameLabor = new List<String>();

            var tipolabor = from p in dbEntity.participa
                            join d in dbEntity.docente on p.iddocente equals d.iddocente
                            join aev in dbEntity.autoevaluacion on p.idautoevaluacion equals aev.idautoevaluacion
                            join l in dbEntity.labor on p.idlabor equals l.idlabor
                            where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                            orderby l.idlabor
                            select p.idlabor;


            GenreLst.AddRange(tipolabor.Distinct());


            foreach (int id in GenreLst)
            {
                var gestion = dbEntity.gestion.SingleOrDefault(g => g.idlabor == id);
                if (gestion != null)
                {
                    NameLabor.Add("Gestión");
                    continue;
                }
                var social = dbEntity.social.SingleOrDefault(g => g.idlabor == id);
                if (social != null)
                {
                    NameLabor.Add("Social");
                    continue;
                }

                var investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == id);
                if (investigacion != null)
                {
                    NameLabor.Add("Investigación");
                    continue;
                }

                var trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == id);
                if (trabajoDeGrado != null)
                {
                    NameLabor.Add("Trabajo de grado");
                    continue;
                }

                var trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == id);
                if (trabajoDeGradoInvestigacion != null)
                {
                    NameLabor.Add("Trabajo de grado investigacion");
                    continue;
                }

                var desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == id);
                if (desarrolloProfesoral != null)
                {
                    NameLabor.Add("Desarrollo profesoral");
                    continue;
                }

                var docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == id);
                if (docencia != null)
                {
                    NameLabor.Add("Docencia");
                    continue;
                }

                var otra = dbEntity.otras.SingleOrDefault(g => g.idlabor == id);
                if (otra != null)
                {
                    NameLabor.Add("Otra");
                    continue;
                }
            }

            if (NameLabor.Count > 0)
            {
                //funcion que envia los datos a la lista desplegable del docente
                ViewBag.tipoLabor = new SelectList(NameLabor.Distinct());
                ViewBag.datos = 1;
            }
            else
            {
                ViewBag.datos = 0;
            }

            
            ViewBag.departamento = departamento.nombre;
            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchTipoLabor(string term)
        {

            docente docente = (docente)Session["docente"];
            int currentper = (int)Session["currentAcadPeriod"];
            departamento departamento = (departamento)Session["depto"];
            var labores = new List<Labor>();
            term = (term).ToLower();

            if (term == "gestión")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join g in dbEntity.gestion on p.idlabor equals g.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = g.nombrecargo }).Distinct().ToList();

            }

            if (term == "social")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join s in dbEntity.social on p.idlabor equals s.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = s.nombreproyecto }).Distinct().ToList();

            }


            if (term == "investigación")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join i in dbEntity.investigacion on p.idlabor equals i.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = i.nombreproyecto }).Distinct().ToList();

            }

            if (term == "trabajo de grado")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join t in dbEntity.trabajodegrado on p.idlabor equals t.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = t.titulotrabajo }).Distinct().ToList();

            }

            if (term == "trabajo de grado investigacion")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join t in dbEntity.trabajodegradoinvestigacion on p.idlabor equals t.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = t.titulotrabajo }).Distinct().ToList();

            }

            if (term == "desarrollo profesoral")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join dp in dbEntity.desarrolloprofesoral on p.idlabor equals dp.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = dp.nombreactividad }).Distinct().ToList();

            }

            if (term == "docencia")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join t in dbEntity.docencia on p.idlabor equals t.idlabor
                           join m in dbEntity.materia on t.idmateria equals m.idmateria
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = m.nombremateria }).Distinct().ToList();

            }


            if (term == "otra")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join o in dbEntity.otras on p.idlabor equals o.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = o.descripcion }).Distinct().ToList();

            }


            return Json(labores, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Problemas Labor

        public List<DepartamentoLabor> SearchProblemasLabor(int id, string term, int currentper, departamento departamento)
        {
            var labores = new List<DepartamentoLabor>();

            term = (term).ToLower();

            if (term == "gestion")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join g in dbEntity.gestion on p.idlabor equals g.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == id
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = g.nombrecargo, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }

            if (term == "social")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join s in dbEntity.social on p.idlabor equals s.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == id
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = s.nombreproyecto, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }


            if (term == "investigacion")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join i in dbEntity.investigacion on p.idlabor equals i.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == id
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = i.nombreproyecto, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }

            if (term == "trabajo de grado")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join t in dbEntity.trabajodegrado on p.idlabor equals t.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == id
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = t.titulotrabajo, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }

            if (term == "trabajo de grado investigacion")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join t in dbEntity.trabajodegradoinvestigacion on p.idlabor equals t.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == id
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = t.titulotrabajo, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }

            if (term == "desarrollo profesoral")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join dp in dbEntity.desarrolloprofesoral on p.idlabor equals dp.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == id
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = dp.nombreactividad, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }

            if (term == "docencia")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join t in dbEntity.docencia on p.idlabor equals t.idlabor
                           join m in dbEntity.materia on t.idmateria equals m.idmateria
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == id
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = m.nombremateria, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }


            if (term == "otras")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join o in dbEntity.otras on p.idlabor equals o.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && l.idlabor == id
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = o.descripcion, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }
            return labores;
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchProblemasLabor(int id, string term)
        {
            int currentper = (int)Session["currentAcadPeriod"];
            departamento departamento = (departamento)Session["depto"];

            var labores = SearchProblemasLabor(id, term, currentper, departamento);
            return Json(labores, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ProblemasLabor(int id)
        {
            ViewBag.optionmenu = 1;
            
            if (ModelState.IsValid)
            {
                var lista = new List<Labor>();
                lista.Add(new Labor { idlabor = id });
                var response = getLabor(lista);
                ViewBag.datos = response;
                return View();
            }

            return View();
        }

        


        #endregion



        public class DepartamentoLabor
        {
            public int idlabor;
            public string docente;
            public string descripcion;
            public string problemadescripcion;
            public string problemarespuesta;
            public string soluciondescripcion;
            public string solucionrespuesta;

        }

        #endregion

        //FIN ADICIONADO POR CLARA

        #region Gestion Archivos

        [Authorize]
        public ActionResult ListPdfByUser(string UserId)
        {
            ViewBag.optionmenu = 3;
            DirectoryInfo oInfo = new DirectoryInfo(Server.MapPath("~/pdfsupport/"));
            var oFiles = oInfo.GetFiles();
            List<BasicFileInfo> lstInfo = new List<BasicFileInfo>();
            string sFind = UserId.ToString() + '_';
            foreach (FileInfo oFile in oFiles)
            {
                if (oFile.Name.StartsWith(sFind))
                {
                    string sFullName = oFile.Name;
                    string[] sSplit = sFullName.Split('_');
                    BasicFileInfo oNew = new BasicFileInfo();
                    oNew.sFullPath = sFullName;
                    oNew.sPeriod = sSplit[1];
                    oNew.sType = sSplit[2];
                    oNew.sName = sSplit[3] ;
                    oNew.sDateTime = sSplit[4].Substring(0, sSplit[4].Length - 4);
                    lstInfo.Add(oNew);
                }
            }
            long lUserID = int.Parse(UserId);
            var oUser = dbEntity.usuario.SingleOrDefault(q => q.idusuario == lUserID);
            ViewBag.UserFullName = oUser.nombres + ' ' + oUser.apellidos;
            lstInfo.Sort();
            ViewBag.lstInfo = lstInfo;
            return View();
        }
        
        [Authorize]
        public ActionResult SearchPdfByUser()
        {
            ViewBag.optionmenu = 3;
            DirectoryInfo oInfo = new DirectoryInfo(Server.MapPath("~/pdfsupport/"));
            var oFiles = oInfo.GetFiles();
            List<BasicUserInfo> lstInfo = new List<BasicUserInfo>();
            foreach (FileInfo oFile in oFiles)
            {
                string sName = oFile.Name;
                string[] oNameSplit = sName.Split('_');
                BasicUserInfo oUserInfo = new BasicUserInfo();
                oUserInfo.lId = long.Parse(oNameSplit[0]);
                bool blnExist = false;
                foreach (BasicUserInfo oItem in lstInfo)
                {
                    if (oItem.lId == oUserInfo.lId)
                        blnExist = true;
                }
                if (!blnExist)
                {
                    var usuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == oUserInfo.lId);
                    oUserInfo.sFullName = usuario.nombres + ' ' + usuario.apellidos;
                    lstInfo.Add(oUserInfo);
                }                
            }
            lstInfo.Sort();
            ViewBag.lstInfo = lstInfo;
            return View();
        }

        public class BasicUserInfo : IComparable
        {
            public long lId { set; get; }
            public string sFullName { set; get; }


            public int CompareTo(object obj)
            {
                return sFullName.CompareTo(((BasicUserInfo)obj).sFullName);
            }
        }

        public class BasicFileInfo : IComparable
        {
            public string sName { set; get; }
            public string sFullPath { set; get; }
            public string sPeriod { set; get; }
            public string sType { set; get; }
            public string sDateTime { set; get; }
            public int CompareTo(object obj)
            {
                return sFullPath.CompareTo(((BasicFileInfo)obj).sFullPath);
            }
        }
        #endregion
        
        #region Evaluación

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowScores(int idusuario)
        {
            //string usuarioEntra = Request.Form["usuario"];
            //int idusuario = Convert.ToInt16(usuarioEntra);

            ViewBag.optionmenu = 1;
            ViewBag.reporte = crearReporte(idusuario);
            return View();
        }
        /// nuevo 22 febero 
        public ActionResult ShowScores1(int idusuario)
        {
            //string usuarioEntra = Request.Form["usuario"];
            //int idusuario = Convert.ToInt16(usuarioEntra);

            ViewBag.optionmenu = 1;
            ViewBag.reporte = crearReporte(idusuario);
            return View();
        }
         public ConsolidadoDocente crearReporte1(int idUsuario)
         {
             departamento departamento = (departamento)Session["depto"];
             int currentper = (int)Session["currentAcadPeriod"];
             periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
             usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
             docente docenteactual = (docente)Session["docente"];
             int idjefe = docenteactual.idusuario;
             usuario jefe = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idjefe);
             DateTime Hoy = DateTime.Today;
             string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
             fecha_actual = fecha_actual.ToUpper();

             ConsolidadoDocente reporte = new ConsolidadoDocente() { nombredocente = user.nombres + " " + user.apellidos, nombrejefe = jefe.nombres + " " + jefe.apellidos, fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };

             //List<ResEvaluacionLabor> labores = (from u in dbEntity.usuario
             //                                    join d in dbEntity.decanoCoordinador on u.idusuario equals d.idusuario
             //                                    join p in dbEntity.dirige on d.idusuario equals p.idusuario
             //                                    join ev in dbEntity.evaluacion on p.idevaluacion equals ev.idevaluacion
             //                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
             //                                    join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
             //                                    where l.idperiodo == currentper && u.idusuario == idUsuario
             //                                    select new ResEvaluacionLabor { idlabor = l.idlabor, evalest = (int)ev.evaluacionestudiante, evalauto = (int)ev.evaluacionautoevaluacion, evaljefe = (int)ev.evaluacionjefe }).ToList();

             //// NUEVO 1 FEBRERO

             List<ResEvaluacionLabor> labores = (from u in dbEntity.usuario
                                                      // join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                      join p in dbEntity.dirige on u.idusuario equals p.idusuario
                                                      join ev in dbEntity.evaluacion on p.idevaluacion equals ev.idevaluacion
                                                      join l in dbEntity.labor on p.idlabor equals l.idlabor
                                                      join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                                      where l.idperiodo == currentper && u.idusuario == idUsuario
                                                      select new ResEvaluacionLabor { idlabor = l.idlabor, evalest = (int)ev.evaluacionestudiante, evalauto = (int)ev.evaluacionautoevaluacion, evaljefe = (int)ev.evaluacionjefe }).ToList();

             labores = setLabor(labores, 0);
             // FIN NUEVO 1 FEBRERO                                   
            // labores = setLabor(labores, 1);

             // INICIO NUEVO 1 FEBRERO 
             //foreach (ResEvaluacionLabor lab in OtrasLabores)
             //{
             //    labores.Add(lab);
             //}
             // FIN NUEVO 1 FEBRERO


             reporte.totalhorassemana = (Double)labores.Sum(q => q.horasxsemana);
             reporte.evaluacioneslabores = calcularPonderados(labores);
             reporte.totalporcentajes = (Double)reporte.evaluacioneslabores.Sum(q => q.porcentaje);
             reporte.notafinal = (Double)reporte.evaluacioneslabores.Sum(q => q.acumula);

             return reporte;
         }
        /// fin nuevo febrero 2

       // NUEVO 29
        public ConsolidadoDocente crearReporte(int idUsuario)
        {
            departamento departamento = (departamento)Session["depto"];            
            int currentper = (int)Session["currentAcadPeriod"];
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
            docente docenteactual = (docente)Session["docente"];
            int idjefe = docenteactual.idusuario;
            usuario jefe = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idjefe);
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();

            ConsolidadoDocente reporte = new ConsolidadoDocente() { nombredocente = user.nombres + " " + user.apellidos, nombrejefe = jefe.nombres + " " + jefe.apellidos, fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };

            List<ResEvaluacionLabor> labores = (from u in dbEntity.usuario
                                                join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                join p in dbEntity.participa on d.iddocente equals p.iddocente
                                                join ev in dbEntity.evaluacion on p.idevaluacion equals ev.idevaluacion
                                                join l in dbEntity.labor on p.idlabor equals l.idlabor
                                                join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                                where l.idperiodo == currentper && u.idusuario == idUsuario
                                                select new ResEvaluacionLabor { idlabor = l.idlabor, evalest = (int)ev.evaluacionestudiante, evalauto = (int)ev.evaluacionautoevaluacion, evaljefe = (int)ev.evaluacionjefe }).ToList();
                                
            labores = setLabor(labores, 1);

            reporte.totalhorassemana = (Double)labores.Sum(q => q.horasxsemana);
            reporte.evaluacioneslabores = calcularPonderados(labores);
            reporte.totalporcentajes = (Double)reporte.evaluacioneslabores.Sum(q => q.porcentaje);
            reporte.notafinal = (Double)reporte.evaluacioneslabores.Sum(q => q.acumula);
            
            return reporte;
        }
   

        public List<ResEvaluacionLabor> setLabor(List<ResEvaluacionLabor> lista,int opcion)
        {
            gestion gestion;
            dirige dirige;
            social social;
            investigacion investigacion;
            trabajodegrado trabajoDeGrado;
            trabajodegradoinvestigacion trabajoDeGradoInvestigacion;
            desarrolloprofesoral desarrolloProfesoral;
            docencia docencia;
            otras otra;
            



            foreach (ResEvaluacionLabor labor in lista)
            {
                
                gestion = dbEntity.gestion.SingleOrDefault(g => g.idlabor == labor.idlabor);
                
                if (gestion != null)
                {
                    labor.tipolabor = "Gestión";
                    labor.tipolaborcorto = "GES";
                    if (opcion == 1)
                    {
                        labor.descripcion = gestion.nombrecargo;
                        labor.horasxsemana = (int)gestion.horassemana;
                    }
                    else {
                        dirige = dbEntity.dirige.SingleOrDefault(g => g.idlabor == labor.idlabor);
                        labor.horasxsemana = (int)dirige.horasSemana;
                        labor.descripcion = "Dirige " + gestion.nombrecargo;
                    }
                    
                    continue;
                }
                
                social = dbEntity.social.SingleOrDefault(g => g.idlabor == labor.idlabor);
               
                if (social != null)
                {
                    labor.tipolabor = "Social";
                    labor.tipolaborcorto = "SOC";
                    if (opcion == 1)
                    {
                        labor.descripcion = social.nombreproyecto;
                        labor.horasxsemana = (int)social.horassemana;
                    }
                    else {
                        dirige = dbEntity.dirige.SingleOrDefault(g => g.idlabor == labor.idlabor);
                        labor.horasxsemana = (int)dirige.horasSemana;
                        labor.descripcion = "Dirige " + social.nombreproyecto;
                    }
                   
                    continue;
                }
                
                investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == labor.idlabor);
               
                if (investigacion != null)
                {
                    labor.tipolabor = "Investigación";
                    labor.tipolaborcorto = "INV";
                    if (opcion == 1)
                    {
                        labor.descripcion = investigacion.nombreproyecto;
                        labor.horasxsemana = (int)investigacion.horassemana;
                    }
                    else {
                        dirige = dbEntity.dirige.SingleOrDefault(g => g.idlabor == labor.idlabor);
                        labor.horasxsemana = (int)dirige.horasSemana;
                        labor.descripcion = "Dirige " + investigacion.nombreproyecto;
                    }
                    
                    continue;
                }
                
                trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == labor.idlabor);
                
                if (trabajoDeGrado != null)
                {
                    labor.tipolabor = "Trabajo de Grado";
                    labor.tipolaborcorto = "TDG";
                    if (opcion == 1)
                    {
                        labor.descripcion = trabajoDeGrado.titulotrabajo;
                        labor.horasxsemana = (int)trabajoDeGrado.horassemana;
                    }
                    else {
                        dirige = dbEntity.dirige.SingleOrDefault(g => g.idlabor == labor.idlabor);
                        labor.horasxsemana = (int)dirige.horasSemana;
                        labor.descripcion = "Dirige " + trabajoDeGrado.titulotrabajo;
                    }
                    
                    continue;
                }
                
                trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == labor.idlabor);
                
                if (trabajoDeGradoInvestigacion != null)
                {
                    labor.tipolabor = "Trabajo de Grado Investigación";
                    labor.tipolaborcorto = "TDGI";
                    if (opcion == 1)
                    {
                        labor.descripcion = trabajoDeGradoInvestigacion.titulotrabajo;
                        labor.horasxsemana = (int)trabajoDeGradoInvestigacion.horassemana;
                    }
                    else {
                        dirige = dbEntity.dirige.SingleOrDefault(g => g.idlabor == labor.idlabor);
                        labor.horasxsemana = (int)dirige.horasSemana;
                        labor.descripcion = "Dirige " + trabajoDeGradoInvestigacion.titulotrabajo;
                    }
                    
                    continue;
                }
                
                desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == labor.idlabor);
                
                if (desarrolloProfesoral != null)
                {
                    labor.tipolabor = "Desarrollo Profesoral";
                    labor.tipolaborcorto = "DP";
                    if (opcion == 1)
                    {
                        labor.descripcion = desarrolloProfesoral.nombreactividad;
                        labor.horasxsemana = (int)desarrolloProfesoral.horassemana;
                    }
                    else {
                        dirige = dbEntity.dirige.SingleOrDefault(g => g.idlabor == labor.idlabor);
                        labor.horasxsemana = (int)dirige.horasSemana;
                        labor.descripcion = "Dirige "+ desarrolloProfesoral.nombreactividad;
                    }
                    
                    continue;
                }
               
                docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == labor.idlabor);
               
                if (docencia != null)
                {
                    materia materia = dbEntity.materia.SingleOrDefault(g => g.idmateria == docencia.idmateria);
                    labor.tipolabor = "Docencia Directa";
                    labor.tipolaborcorto = "DD";
                    if (opcion == 1)
                    {
                        labor.descripcion = materia.nombremateria;
                        labor.horasxsemana = (int)docencia.horassemana;
                    }
                    else {
                        dirige = dbEntity.dirige.SingleOrDefault(g => g.idlabor == labor.idlabor);
                        labor.horasxsemana = (int)dirige.horasSemana;
                        labor.descripcion = "Dirige " + materia.nombremateria;
                    }
                   
                    continue;
                }

                otra = dbEntity.otras.SingleOrDefault(g => g.idlabor == labor.idlabor);
               
                if (otra != null)
                {                    
                    labor.tipolabor = "OTRA";
                    labor.tipolaborcorto = "OT";
                    if (opcion == 1)
                    {
                        labor.descripcion = otra.descripcion;
                        labor.horasxsemana = (int)otra.horassemana;
                    }
                    else {
                        dirige = dbEntity.dirige.SingleOrDefault(g => g.idlabor == labor.idlabor);
                        labor.horasxsemana = (int)dirige.horasSemana;
                        labor.descripcion = "Dirige " + otra.descripcion;
                    }
                    
                    continue;
                }
            }
            return lista;
        }

        /// fin 1 febrero 
        // NUEVO 29
        public static List<ResEvaluacionLabor> calcularPonderados(List<ResEvaluacionLabor> lista)
        {
            Double totalHoras = lista.Sum(q => q.horasxsemana);
            foreach (ResEvaluacionLabor labor in lista)
            {
                Double porc = (labor.horasxsemana * 100) / totalHoras;
                labor.porcentaje = (Double)System.Math.Round(porc, 2);
                labor.nota = (Double)calculaNotaPromedio(labor.evalest, labor.evalauto, labor.evaljefe);
                Double acum = System.Math.Round((porc * labor.nota) / 100, 2);
                labor.acumula = (Double)acum;
            }
            return lista;
        }
        // FIN NUEVO 29
        public static Double calculaNotaPromedio(Double n1, Double n2, Double n3)
        {
            if (n1 == -1 && n2 == -1 && n3 == -1)
            {
                return 0;
            }

            int divisor = 3;

            if (n1 == -1) { n1 = 0; divisor--; }
            if (n2 == -1) { n2 = 0; divisor--; }
            if (n3 == -1) { n3 = 0; divisor--; }

            return System.Math.Round(((n1 + n2 + n3) / divisor), 2);
        }

        #endregion

        //INICIO ADICIONADO POR EDINSON

        #region ADICIONADO POR EDINSON

        [Authorize]
        // NUEVO 29 
        public ActionResult AssignWork1()
        {
            int currentper = (int)Session["currentAcadPeriod"];
            periodo_academico per = GetLastAcademicPeriod();
                        
            if (noAlmaceno == 1 && siAlmaceno != 1)
            {
                ViewBag.Error = "No se realizo ninguna de las asignaciones";
            }
            if (noAlmaceno == 1 && siAlmaceno == 1)
            {
                ViewBag.Error = "Algunas asignaciones o se realizaron";
            }
            if (noAlmaceno != 1 && siAlmaceno == 1)
            {
                ViewBag.Message = "Se realizaron todas las asignaciones";
            }
            
            departamento depto = (departamento)Session["depto"];
            noAlmaceno = 0;
            siAlmaceno = 0;
            ViewBag.datos = depto.nombre;

            if (currentper != per.idperiodo)
                ViewBag.periodo = 0;
            else
                ViewBag.periodo = 1;

            return View();
            
        }
      
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetTechingWork1(string term) // aqui toca comparar de alguna forma sacar el titulo, el nombre de la materia, etc de cada labor
        {
            int idD = int.Parse(term);



            departamento depto = (departamento)Session["depto"];

            int currentper = (int)Session["currentAcadPeriod"];

            var idlaborestotales = (from p in dbEntity.participa
                                    join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
                                    join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                    join dir in dbEntity.dirige on l.idlabor equals dir.idlabor
                                    where l.idperiodo == currentper && doc.iddepartamento == depto.iddepartamento
                                    select new Labor { idlabor = l.idlabor, horasSemana = (int)dir.horasSemana }).Distinct().ToList();
                                    

            var response = getLabor1(idlaborestotales);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // FIN NUEVO 29


        public List<Labor> getLabor1(List<Labor> lista)
        {
            gestion gestion;
            social social;
            investigacion investigacion;
            trabajodegrado trabajoDeGrado;
            trabajodegradoinvestigacion trabajoDeGradoInvestigacion;
            desarrolloprofesoral desarrolloProfesoral;
            docencia docencia;
            otras otrasLabores;
            materia materia;
            usuario usuarioDirige;
            dirige jefeDirige;

            foreach (Labor jefe in lista)
            {
               
                int idlabor = (int)jefe.idlabor;
                jefeDirige = dbEntity.dirige.SingleOrDefault(t => t.idlabor == idlabor);
                usuarioDirige = dbEntity.usuario.SingleOrDefault(u => u.idusuario == jefeDirige.idusuario);
                jefe.nombres = usuarioDirige.nombres + " " + usuarioDirige.apellidos;

                               
                gestion = dbEntity.gestion.SingleOrDefault(g => g.idlabor == jefe.idlabor);                
                if (gestion != null)
                {
                    jefe.tipoLabor = "gestion";
                    jefe.descripcion = gestion.nombrecargo;

                    continue;
                }
                social = dbEntity.social.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (social != null)
                {
                    jefe.tipoLabor = "social";
                    jefe.descripcion = social.nombreproyecto;
                    continue;
                }
                investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (investigacion != null)
                {
                    jefe.tipoLabor = "investigacion";
                    jefe.descripcion = investigacion.nombreproyecto;
                    continue;
                }
                trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (trabajoDeGrado != null)
                {
                    jefe.tipoLabor = "trabajo de grado";
                    jefe.descripcion = trabajoDeGrado.titulotrabajo;
                    continue;
                }
                trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (trabajoDeGradoInvestigacion != null)
                {
                    jefe.tipoLabor = "trabajo de grado investigacion";
                    jefe.descripcion = trabajoDeGradoInvestigacion.titulotrabajo;
                    continue;
                }
                desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (desarrolloProfesoral != null)
                {
                    jefe.tipoLabor = "desarrollo profesoral";
                    jefe.descripcion = desarrolloProfesoral.nombreactividad;
                    continue;
                }
                docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (docencia != null)
                {
                    jefe.tipoLabor = "docencia";
                    materia = dbEntity.materia.SingleOrDefault(m => m.idmateria == docencia.idmateria);
                    jefe.descripcion = materia.nombremateria;
                    continue;
                }
                otrasLabores = dbEntity.otras.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (otrasLabores != null)
                {
                    jefe.tipoLabor = "otras";                    
                    jefe.descripcion = otrasLabores.descripcion;
                    continue;
                }
                
            }
            return lista.OrderBy(d => d.tipoLabor).ThenBy(e => e.descripcion).ToList(); 
        }

        #endregion

        //FIN ADICIONADO POR EDINSON
    }
}
