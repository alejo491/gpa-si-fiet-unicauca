using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcSEDOC.Models;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace MvcSEDOC.Controllers
{
    public class TeacherController : SEDOCController
    {

        public static AutoevaluacionDocente AutoevaluacionDocenteReporte;
       // NUEVO 29
        public object SessionValue(string keyValue)
        {
            try
            {
                return Session[keyValue];
            }
            catch
            {
                return RedirectPermanent("/sedoc");
            }
        }

        public double Calificacion(int n1, int n2, int n3, int totalHoras, int totallabor)
        {
            if (n1 == -1 && n2 == -1 && n3 == -1)
            {
                return -1;
            }

            int divisor = 3;

            if (n1 == -1) { n1 = 0; divisor--; }
            if (n2 == -1) { n2 = 0; divisor--; }
            if (n3 == -1) { n3 = 0; divisor--; }
            double nota1 = Double.Parse(n1.ToString());
            double nota2 = Double.Parse(n2.ToString());
            double nota3 = Double.Parse(n3.ToString());
            double totalH = Double.Parse(totalHoras.ToString());
            double totalL = Double.Parse(totallabor.ToString());

            double divicion = (nota1 + nota2 + nota3) / divisor;
            double porcentaje = (((100 * totalL) / totalH) / 100);
            double resultado = divicion * porcentaje;

            return System.Math.Round(resultado, 2);
            //return System.Math.Round((((nota1 + nota2 + nota3) / divisor) * (((100 * totalL) / totalH)/100)), 2);
        }
        // FIN NUEVO 29

        [Authorize]
        public ActionResult ChangePass()
        {            
            int periodoActualSelec = 0;
            periodoActualSelec = (int)SessionValue("periodoActual");
            if (periodoActualSelec == 1)
            {
                return RedirectPermanent("/Teacher/Index");
            }

            ViewBag.optionmenu = 1;
            return View();
        }

        [Authorize]
        public ActionResult ChangePass1()
        {

            int periodoActualSelec = 0;
            periodoActualSelec = (int)SessionValue("periodoActual");
            if (periodoActualSelec == 1)
            {
                return RedirectPermanent("/Teacher/Index");
            }

            ViewBag.Value1 = "";
            ViewBag.Value2 = "";
            ViewBag.Value3 = "";

            string anterior = "";
            string nueva = "";
            string repite = "";
            string claveA = "";

            int campo1 = 0;
            int campo2 = 0;
            int campo3 = 0;
            //int entradas = 7;
            //Boolean entro = false;
            //Boolean error = false;


            anterior = Request.Form["anterior"];
            if (anterior == "")
            {
                campo1 = 1;
                ViewBag.Error1 = "entro";

            }
            else
            {
                ViewBag.Value1 = anterior;
            }
            nueva = Request.Form["nueva"];
            if (nueva == "")
            {
                campo2 = 1;
                ViewBag.Error2 = "entro";
            }
            else
            {
                ViewBag.Value2 = nueva;
            }
            repite = Request.Form["repite"];
            if (repite == "")
            {
                campo3 = 1;
                ViewBag.Error3 = "entro";
            }
            else
            {
                ViewBag.Value3 = repite;
            }


            ViewBag.optionmenu = 1;
            if (campo1 == 1 || campo2 == 1 || campo3 == 1)
            {
                ViewBag.Error = "Los campos con asterisco son obligatorios";
                return View();
            }

            claveA = AdminController.md5(anterior);

            try
            {
                docente logueado = (docente)SessionValue("docente");
                usuario NuevaContrasena = dbEntity.usuario.Single(u => u.idusuario == logueado.idusuario && u.password == claveA);
                if (nueva != repite)
                {
                    ViewBag.Error = "No coinciden la nueva contraseña con la anterior";
                    return View();
                }
                else
                {
                    AdminController oControl = new AdminController();
                    if (!oControl.ActualizarContrasenaUsuarioActual(nueva, anterior))
                    {
                        throw new Exception("Invalid Password");
                    }
                    nueva = AdminController.md5(nueva);
                    NuevaContrasena.password = nueva;

                    //  dbEntity.usuario.Attach(NuevaContrasena);
                    dbEntity.ObjectStateManager.ChangeObjectState(NuevaContrasena, EntityState.Modified);
                    dbEntity.SaveChanges();
                    ViewBag.Value1 = null;
                    ViewBag.Value2 = null;
                    ViewBag.Value3 = null;
                    ViewBag.Message = "La contraseña se cambio exitosamente";
                    return View();
                }
            }
            catch
            {
                ViewBag.Error = "La contraseña anterior es incorrecta";
                return View();
            }

        }
        // fin cambiar contraseña
        // nuevo  2 febero 

        public ActionResult ShowScores(int idusuario)
        {
            ViewBag.optionmenu = 1;
            ViewBag.reporte = crearReporte(idusuario);
            return View();
        }
        public ConsolidadoDocente crearReporte(int idUsuario)
        {
            departamento departamento = (departamento)SessionValue("depto");
            int currentper = (int)SessionValue("currentAcadPeriod");
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
            docente docenteactual = (docente)SessionValue("docente");
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

            // NUEVO 1 FEBRERO

            //List<ResEvaluacionLabor> OtrasLabores = (from u in dbEntity.usuario
            //                                         // join d in dbEntity.docente on u.idusuario equals d.idusuario
            //                                         join p in dbEntity.dirige on u.idusuario equals p.idusuario
            //                                         join ev in dbEntity.evaluacion on p.idevaluacion equals ev.idevaluacion
            //                                         join l in dbEntity.labor on p.idlabor equals l.idlabor
            //                                         join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
            //                                         where l.idperiodo == currentper && u.idusuario == idUsuario
            //                                         select new ResEvaluacionLabor { idlabor = l.idlabor, evalest = (int)ev.evaluacionestudiante, evalauto = (int)ev.evaluacionautoevaluacion, evaljefe = (int)ev.evaluacionjefe }).ToList();

            //OtrasLabores = setLabor(OtrasLabores, 0);
            // FIN NUEVO 1 FEBRERO                                   
            labores = setLabor(labores, 1);

            //// INICIO NUEVO 1 FEBRERO 
            //foreach (ResEvaluacionLabor lab in OtrasLabores)
            //{
            //    labores.Add(lab);
            //}
            // FIN NUEVO 1 FEBRERO


            reporte.totalhorassemana = (Double)labores.Sum(q => q.horasxsemana);
            reporte.evaluacioneslabores = DepartmentChiefController.calcularPonderados(labores);
            reporte.totalporcentajes = (Double)reporte.evaluacioneslabores.Sum(q => q.porcentaje);
            reporte.notafinal = (Double)reporte.evaluacioneslabores.Sum(q => q.acumula);

            return reporte;
        }

        public List<ResEvaluacionLabor> setLabor(List<ResEvaluacionLabor> lista, int opcion)
        {
            gestion gestion;
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
                    }
                    else
                    {
                        labor.descripcion = "Dirige " + gestion.nombrecargo;
                    }
                    labor.horasxsemana = (int)gestion.horassemana;
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
                    }
                    else
                    {
                        labor.descripcion = "Dirige " + social.nombreproyecto;
                    }
                    labor.horasxsemana = (int)social.horassemana;
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
                    }
                    else
                    {
                        labor.descripcion = "Dirige " + investigacion.nombreproyecto;
                    }
                    labor.horasxsemana = (int)investigacion.horassemana;
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
                    }
                    else
                    {
                        labor.descripcion = "Dirige " + trabajoDeGrado.titulotrabajo;
                    }
                    labor.horasxsemana = (int)trabajoDeGrado.horassemana;
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
                    }
                    else
                    {
                        labor.descripcion = "Dirige " + trabajoDeGradoInvestigacion.titulotrabajo;
                    }
                    labor.horasxsemana = (int)trabajoDeGradoInvestigacion.horassemana;
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
                    }
                    else
                    {
                        labor.descripcion = "Dirige " + desarrolloProfesoral.nombreactividad;
                    }
                    labor.horasxsemana = (int)desarrolloProfesoral.horassemana;
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
                    }
                    else
                    {
                        labor.descripcion = "Dirige " + materia.nombremateria;
                    }
                    labor.horasxsemana = (int)docencia.horassemana;
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
                    }
                    else
                    {
                        labor.descripcion = "Dirige " + otra.descripcion;
                    }
                    labor.horasxsemana = (int)otra.horassemana;
                    continue;
                }
            }
            return lista;
        }
             

        [Authorize]
        public ActionResult Index()
        {
            docente docente = (docente)SessionValue("docente");
            return View(docente);
        }

        [Authorize]
        public ActionResult MessegeTeach()
        {
            return View();
        }

        #region Labores

        [Authorize]
        public ActionResult selectWork()
        {
            docente docente = (docente)SessionValue("docente");
            int currentper = (int)SessionValue("currentAcadPeriod");
            var users = (from p in dbEntity.participa
                         join u in dbEntity.usuario on docente.idusuario equals u.idusuario
                         join l in dbEntity.labor on p.idlabor equals l.idlabor
                         join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                         join ev in dbEntity.evaluacion on p.idevaluacion equals ev.idevaluacion
                         where l.idperiodo == currentper && p.iddocente == docente.iddocente                        
                         select new DocenteLabor { idlabor = l.idlabor,iddocente = docente.iddocente, nombres = u.nombres, apellidos = u.apellidos, rol = u.rol }).OrderBy(u => u.apellidos).ToList();
 
            if (users.Count == 0)
            {
                return RedirectToAction("MessegeTeach", "Teacher");
            }
            users = setLaborTeach(users);
            ViewBag.lista = users;

            return View(docente);
        }

        //MODIFICADO POR CLARA
        public List<DocenteLabor> setLaborTeach(List<DocenteLabor> lista)
        {
            gestion gestion;
            social social;
            investigacion investigacion;
            trabajodegrado trabajoDeGrado;
            trabajodegradoinvestigacion trabajoDeGradoInvestigacion;
            desarrolloprofesoral desarrolloProfesoral;
            docencia docencia;
            materia materia;
            otras otra; 

            foreach (DocenteLabor jefe in lista)
            {
                gestion = dbEntity.gestion.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                
                if (gestion != null)
                {
                    jefe.tipoLabor = "Gestión";
                    jefe.descripcion = gestion.nombrecargo;
                    continue;
                }
                
                social = dbEntity.social.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                
                if (social != null)
                {
                    jefe.tipoLabor = "Social";
                    jefe.descripcion = social.nombreproyecto;
                    continue;
                }
                
                investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                
                if (investigacion != null)
                {
                    jefe.tipoLabor = "Investigación";
                    jefe.descripcion = investigacion.nombreproyecto;
                    continue;
                }
               
                trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                
                if (trabajoDeGrado != null)
                {
                    jefe.tipoLabor = "Trabajo De Grado";
                    jefe.descripcion = trabajoDeGrado.titulotrabajo;
                    continue;
                }
                
                trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                
                if (trabajoDeGradoInvestigacion != null)
                {
                    jefe.tipoLabor = "Trabajo De Grado De Investigación";
                    jefe.descripcion = trabajoDeGradoInvestigacion.titulotrabajo;
                    continue;
                }
                
                desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                
                if (desarrolloProfesoral != null)
                {
                    jefe.tipoLabor = "Desarrollo Profesoral";
                    jefe.descripcion = desarrolloProfesoral.nombreactividad;
                    continue;
                }
                
                docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                
                if (docencia != null)
                {
                    jefe.tipoLabor = "Docencia";
                    materia = dbEntity.materia.SingleOrDefault(m => m.idmateria == docencia.idmateria);
                    jefe.descripcion = materia.nombremateria;
                    continue;
                }
                
                otra = dbEntity.otras.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                
                if (otra != null)
                {
                    jefe.tipoLabor = "Otra";
                    jefe.descripcion = otra.descripcion;
                    continue;
                }
            }
            
            return lista;
        }

        //MODIFICADO POR CLARA EDV
        // NUEVO 29
        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchLabor(string term)
        {
            docente docente = (docente)SessionValue("docente");
            int currentper = (int)SessionValue("currentAcadPeriod");
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);


            var users = new List<DocenteLabor>();
            var horasgestion = new List<Horas>();
            var horassocial = new List<Horas>();
            var horasinvestigacion = new List<Horas>();
            var horasstrabajodegrado = new List<Horas>();
            var horasstrabajodeinvestigacion = new List<Horas>();
            var horasdesarrolloprofe = new List<Horas>();
            var horasdocencia = new List<Horas>();
            var horasOtras = new List<Horas>();

            int totalgestion = 0;
            int totalsocial = 0;
            int totalinvestigacion = 0;
            int totaltrabajodegrado = 0;
            int totaltrabajoinvestigacion = 0;
            int totaldesarrollo = 0;
            int totaldocencia = 0;
            int totalOtras = 0;

            int totalhoras = 0;

            horasgestion = (from p in dbEntity.participa
                            join g in dbEntity.gestion on p.idlabor equals g.idlabor
                            join l in dbEntity.labor on p.idlabor equals l.idlabor
                            where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                            where p.iddocente == docente.iddocente
                            select new Horas { Horassemanales = (int)g.horassemana, semanaslaborales = (int)g.semanaslaborales }).ToList();

            horassocial = (from p in dbEntity.participa
                           join s in dbEntity.social on p.idlabor equals s.idlabor
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                           where p.iddocente == docente.iddocente
                           select new Horas { Horassemanales = (int)s.horassemana, semanaslaborales = (int)s.semanaslaborales }).ToList();

            horasinvestigacion = (from p in dbEntity.participa
                                  join i in dbEntity.investigacion on p.idlabor equals i.idlabor
                                  join l in dbEntity.labor on p.idlabor equals l.idlabor
                                  where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                                  where p.iddocente == docente.iddocente
                                  select new Horas { Horassemanales = (int)i.horassemana, semanaslaborales = (int)i.semanaslaborales }).ToList();

            horasstrabajodegrado = (from p in dbEntity.participa
                                    join tg in dbEntity.trabajodegrado on p.idlabor equals tg.idlabor
                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
                                    where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                                    where p.iddocente == docente.iddocente
                                    select new Horas { Horassemanales = (int)tg.horassemana, semanaslaborales = (int)tg.semanaslaborales }).ToList();

            horasstrabajodeinvestigacion = (from p in dbEntity.participa
                                            join tgi in dbEntity.trabajodegradoinvestigacion on p.idlabor equals tgi.idlabor
                                            join l in dbEntity.labor on p.idlabor equals l.idlabor
                                            where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                                            where p.iddocente == docente.iddocente
                                            select new Horas { Horassemanales = (int)tgi.horassemana, semanaslaborales = (int)tgi.semanaslaborales }).ToList();

            horasdesarrolloprofe = (from p in dbEntity.participa
                                    join dp in dbEntity.desarrolloprofesoral on p.idlabor equals dp.idlabor
                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
                                    where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                                    where p.iddocente == docente.iddocente
                                    select new Horas { Horassemanales = (int)dp.horassemana, semanaslaborales = (int)dp.semanaslaborales }).ToList();

            horasdocencia = (from p in dbEntity.participa
                             join d in dbEntity.docencia on p.idlabor equals d.idlabor
                             join l in dbEntity.labor on p.idlabor equals l.idlabor
                             where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                             where p.iddocente == docente.iddocente
                             select new Horas { Horassemanales = (int)d.horassemana, semanaslaborales = (int)d.semanaslaborales }).ToList();
            
            horasOtras = (from p in dbEntity.participa
                                 join ot in dbEntity.otras on p.idlabor equals ot.idlabor
                                 join l in dbEntity.labor on p.idlabor equals l.idlabor
                                 where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                                 where p.iddocente == docente.iddocente
                                 select new Horas { Horassemanales = (int)ot.horassemana, semanaslaborales = 4 }).ToList();



            foreach (Horas h in horasgestion)
            {
                //totalgestion = totalgestion + h.semanaslaborales * h.Horassemanales;
                totalgestion = totalgestion + h.Horassemanales;
            }

            foreach (Horas h in horassocial)
            {
                //totalsocial = totalsocial + h.semanaslaborales * h.Horassemanales;
                totalsocial = totalsocial + h.Horassemanales;
            }

            foreach (Horas h in horasinvestigacion)
            {
                //totalinvestigacion = totalinvestigacion + h.semanaslaborales * h.Horassemanales;
                totalinvestigacion = totalinvestigacion + h.Horassemanales;
            }

            foreach (Horas h in horasstrabajodegrado)
            {
                //totaltrabajodegrado = totaltrabajodegrado + h.semanaslaborales * h.Horassemanales;
                totaltrabajodegrado = totaltrabajodegrado + h.Horassemanales;
            }

            foreach (Horas h in horasstrabajodeinvestigacion)
            {
                //totaltrabajoinvestigacion = totaltrabajoinvestigacion + h.semanaslaborales * h.Horassemanales;
                totaltrabajoinvestigacion = totaltrabajoinvestigacion + h.Horassemanales;
            }

            foreach (Horas h in horasdesarrolloprofe)
            {
                //totaldesarrollo = totaldesarrollo + h.semanaslaborales * h.Horassemanales;
                totaldesarrollo = totaldesarrollo + h.Horassemanales;
            }

            foreach (Horas h in horasdocencia)
            {
                //totaldocencia = totaldocencia + h.semanaslaborales * h.Horassemanales;
                totaldocencia = totaldocencia + h.Horassemanales;
            }

            foreach (Horas h in horasOtras)
            {
                //totaldocencia = totaldocencia + h.semanaslaborales * h.Horassemanales;
                totalOtras = totalOtras + h.Horassemanales;
            }

            totalhoras = totalgestion + totalsocial + totalinvestigacion + totaltrabajodegrado + totaltrabajoinvestigacion + totaldesarrollo + totaldocencia + totalOtras;

            switch (term)
            {
                case "Gestión":

                    users = (from p in dbEntity.participa
                             join g in dbEntity.gestion on p.idlabor equals g.idlabor
                             join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                             join l in dbEntity.labor on g.idlabor equals l.idlabor
                             where p.iddocente == docente.iddocente && l.idperiodo == PeriodoSeleccionado.idperiodo
                             orderby p.idlabor
                             select new DocenteLabor { idlabor = p.idlabor, descripcion = g.nombrecargo, evaluacionestudiante = (int)e.evaluacionestudiante, evaluacionjefe = (int)e.evaluacionjefe, evaluacionauto = (int)e.evaluacionautoevaluacion, horasPorLabor = (int)g.horassemana }).ToList();

                    foreach (DocenteLabor d in users)
                    {
                        d.calificacion = Calificacion(d.evaluacionauto, d.evaluacionestudiante, d.evaluacionjefe, totalhoras, d.horasPorLabor);
                    }
                    break;

                case "Social":

                    users = (from p in dbEntity.participa
                             join s in dbEntity.social on p.idlabor equals s.idlabor
                             join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                             join l in dbEntity.labor on s.idlabor equals l.idlabor
                             where p.iddocente == docente.iddocente && l.idperiodo == PeriodoSeleccionado.idperiodo
                             orderby p.idlabor
                             select new DocenteLabor { idlabor = p.idlabor, descripcion = s.nombreproyecto, evaluacionestudiante = (int)e.evaluacionestudiante, evaluacionjefe = (int)e.evaluacionjefe, evaluacionauto = (int)e.evaluacionautoevaluacion, horasPorLabor = (int) s.horassemana }).ToList();

                    foreach (DocenteLabor d in users)
                    {
                        d.calificacion = Calificacion(d.evaluacionauto, d.evaluacionestudiante, d.evaluacionjefe, totalhoras, d.horasPorLabor);
                    }
                    break;

                case "Investigación":

                    users = (from p in dbEntity.participa
                             join i in dbEntity.investigacion on p.idlabor equals i.idlabor
                             join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                             join l in dbEntity.labor on i.idlabor equals l.idlabor
                             where p.iddocente == docente.iddocente && l.idperiodo == PeriodoSeleccionado.idperiodo
                             orderby p.idlabor
                             select new DocenteLabor { idlabor = p.idlabor, descripcion = i.nombreproyecto, evaluacionestudiante = (int)e.evaluacionestudiante, evaluacionjefe = (int)e.evaluacionjefe, evaluacionauto = (int)e.evaluacionautoevaluacion, horasPorLabor = (int)i.horassemana }).ToList();

                    foreach (DocenteLabor d in users)
                    {
                        d.calificacion = Calificacion(d.evaluacionauto, d.evaluacionestudiante, d.evaluacionjefe, totalhoras, d.horasPorLabor);
                    }
                    break;

                case "Trabajo de grado":

                    users = (from p in dbEntity.participa
                             join tg in dbEntity.trabajodegrado on p.idlabor equals tg.idlabor
                             join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                             join l in dbEntity.labor on tg.idlabor equals l.idlabor
                             where p.iddocente == docente.iddocente && l.idperiodo == PeriodoSeleccionado.idperiodo
                             orderby p.idlabor
                             select new DocenteLabor { idlabor = p.idlabor, descripcion = tg.titulotrabajo, evaluacionestudiante = (int)e.evaluacionestudiante, evaluacionjefe = (int)e.evaluacionjefe, evaluacionauto = (int)e.evaluacionautoevaluacion, horasPorLabor = (int)tg.horassemana }).ToList();

                    foreach (DocenteLabor d in users)
                    {
                        d.calificacion = Calificacion(d.evaluacionauto, d.evaluacionestudiante, d.evaluacionjefe, totalhoras, d.horasPorLabor);
                    }
                    break;

                case "Trabajo de grado investigacion":

                    users = (from p in dbEntity.participa
                             join tgi in dbEntity.trabajodegradoinvestigacion on p.idlabor equals tgi.idlabor
                             join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                             join l in dbEntity.labor on tgi.idlabor equals l.idlabor
                             where p.iddocente == docente.iddocente && l.idperiodo == PeriodoSeleccionado.idperiodo
                             orderby p.idlabor
                             select new DocenteLabor { idlabor = p.idlabor, descripcion = tgi.titulotrabajo, evaluacionestudiante = (int)e.evaluacionestudiante, evaluacionjefe = (int)e.evaluacionjefe, evaluacionauto = (int)e.evaluacionautoevaluacion, horasPorLabor = (int)tgi.horassemana }).ToList();

                    foreach (DocenteLabor d in users)
                    {
                        d.calificacion = Calificacion(d.evaluacionauto, d.evaluacionestudiante, d.evaluacionjefe, totalhoras, d.horasPorLabor);
                    }
                    break;

                case "Desarrollo profesoral":

                    users = (from p in dbEntity.participa
                             join dp in dbEntity.desarrolloprofesoral on p.idlabor equals dp.idlabor
                             join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                             join l in dbEntity.labor on dp.idlabor equals l.idlabor
                             where p.iddocente == docente.iddocente && l.idperiodo == PeriodoSeleccionado.idperiodo
                             orderby p.idlabor
                             select new DocenteLabor { idlabor = p.idlabor, descripcion = dp.nombreactividad, evaluacionestudiante = (int)e.evaluacionestudiante, evaluacionjefe = (int)e.evaluacionjefe, evaluacionauto = (int)e.evaluacionautoevaluacion, horasPorLabor = (int)dp.horassemana }).ToList();

                    foreach (DocenteLabor d in users)
                    {
                        d.calificacion = Calificacion(d.evaluacionauto, d.evaluacionestudiante, d.evaluacionjefe, totalhoras, d.horasPorLabor);
                    }
                    break;

                case "Docencia":

                    users = (from p in dbEntity.participa
                             join d in dbEntity.docencia on p.idlabor equals d.idlabor
                             join m in dbEntity.materia on d.idmateria equals m.idmateria
                             join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                             join l in dbEntity.labor on d.idlabor equals l.idlabor
                             where p.iddocente == docente.iddocente && l.idperiodo == PeriodoSeleccionado.idperiodo
                             orderby p.idlabor
                             select new DocenteLabor { idlabor = p.idlabor, descripcion = m.nombremateria, evaluacionestudiante = (int)e.evaluacionestudiante, evaluacionjefe = (int)e.evaluacionjefe, evaluacionauto = (int)e.evaluacionautoevaluacion, horasPorLabor = (int)d.horassemana }).ToList();

                    foreach (DocenteLabor d in users)
                    {
                        d.calificacion = Calificacion(d.evaluacionauto, d.evaluacionestudiante, d.evaluacionjefe, totalhoras, d.horasPorLabor);
                    }
                    break;

                case "Otra":

                    users = (from p in dbEntity.participa
                             join ot in dbEntity.otras on p.idlabor equals ot.idlabor
                             join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                             join l in dbEntity.labor on ot.idlabor equals l.idlabor
                             where p.iddocente == docente.iddocente && l.idperiodo == PeriodoSeleccionado.idperiodo
                             orderby p.idlabor
                             select new DocenteLabor { idlabor = p.idlabor, descripcion = ot.descripcion, evaluacionestudiante = (int)e.evaluacionestudiante, evaluacionjefe = (int)e.evaluacionjefe, evaluacionauto = (int)e.evaluacionautoevaluacion, horasPorLabor = (int)ot.horassemana }).ToList();

                    foreach (DocenteLabor d in users)
                    {
                        d.calificacion = Calificacion(d.evaluacionauto, d.evaluacionestudiante, d.evaluacionjefe, totalhoras, d.horasPorLabor);
                    }
                    break;

            }

            return Json(users, JsonRequestBehavior.AllowGet);
        }
        // FIN NUEVO 29
        //MODIFICADO POR CLARA EDV
        [Authorize]
        public ActionResult GetLaborType()
        {
            ViewBag.optionmenu = 1;
            docente docente = (docente)SessionValue("docente");
            int currentper = (int)SessionValue("currentAcadPeriod");
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);


            var GenreLst = new List<int>();
            var NameLabor = new List<String>();


            var tipolabor = from p in dbEntity.participa                           
                            join l in dbEntity.labor on p.idlabor equals l.idlabor
                            where l.idperiodo.Equals(PeriodoSeleccionado.idperiodo)
                            where p.iddocente == docente.iddocente
                            orderby p.iddocente
                            select p.idlabor;

                           //join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                           //where l.idperiodo == currentper && p.iddocente == docente.iddocente && p.idautoevaluacion == null
                           //select new DocenteLabor { idlabor = l.idlabor, iddocente = docente.iddocente, nombres = u.nombres, apellidos = u.apellidos, rol = u.rol }).OrderBy(u => u.apellidos).ToList();

            //var tipolabor = from p in dbEntity.participa
            //                where p.iddocente == docente.iddocente
            //                //where p.idperiodo.Equals(ultimoPeriodo.idperiodo)
            //                orderby p.iddocente
            //                select p.idlabor;

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

            //funcion que envia los datos a la lista desplegable del docente
            ViewBag.tipoLabor = new SelectList(NameLabor.Distinct());

            return View();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetTechingWork(string term)
        {
            int idD = int.Parse(term);

            int currentper = (int)SessionValue("currentAcadPeriod");

            var work_ids = dbEntity.participa
                .Join(dbEntity.labor,
                part => part.idlabor,
                lab => lab.idlabor,
                (part, lab) => new { participa = part, labor = lab })
                .Where(part_lab => part_lab.participa.iddocente == idD && part_lab.labor.idperiodo == currentper);

            var gestion_works = dbEntity.gestion
                .Join(work_ids,
                gest => gest.idlabor,
                w_ids => w_ids.labor.idlabor,
                (gest, w_ids) => new { gestion = gest, work_ids = w_ids });

            var response = gestion_works.Select(l => new { nombreL = l.gestion.nombrecargo, idL = l.gestion.idlabor, idLD = l.work_ids.participa.iddocente }).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWork(string term)
        {
            int idL = int.Parse(term);
            return View();
        }
        
        #endregion

        // INICIO ADICIONADO POR CLARA

        #region ADICIONADO POR CLARA

        #region Labores Docentes

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetLaboresDocente(string term)
        {
            docente docente = (docente)SessionValue("docente");
            int currentper = (int)SessionValue("currentAcadPeriod");
            int iddocente = int.Parse(term);

            var labores = (from p in dbEntity.participa
                           join u in dbEntity.usuario on docente.idusuario equals u.idusuario
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion 
                           where l.idperiodo == currentper && p.iddocente == docente.iddocente && e.calificacion == -1
                           select new DocenteLabor { idlabor = l.idlabor, iddocente = docente.iddocente, nombres = u.nombres, apellidos = u.apellidos, rol = u.rol }).OrderBy(u => u.apellidos).ToList();

            labores = setLaborTeach(labores);

            return Json(labores, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ListaLaborDocente()
        {
            docente docente = (docente)SessionValue("docente");
            int currentper = (int)SessionValue("currentAcadPeriod");
            var users = (from p in dbEntity.participa
                         join u in dbEntity.usuario on docente.idusuario equals u.idusuario
                         join l in dbEntity.labor on p.idlabor equals l.idlabor
                         join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                         where l.idperiodo == currentper
                         where p.iddocente == docente.iddocente
                         select new DocenteLabor { idlabor = l.idlabor, iddocente = docente.iddocente, nombres = u.nombres, apellidos = u.apellidos, rol = u.rol }).OrderBy(u => u.apellidos).ToList();

            if (users.Count == 0)
            {
                return RedirectToAction("MessegeTeach", "Teacher");
            }
            users = setLaborTeach(users);
            ViewBag.lista = users;

            return View(docente);
        }

        #endregion

        #region Autoevaluación

        [Authorize]
        public ActionResult EvaluarLabor()
        {
            docente docente = (docente)SessionValue("docente");
            int currentper = (int)SessionValue("currentAcadPeriod");           
            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();
           
            if (currentper != ultimoPeriodo.idperiodo)
            {
                ViewBag.Error = " No puede realizar autoevaluaciones de periodos anteriores";
            }

            return View(docente);
        }
        
        [Authorize]
        public ActionResult Evaluar(int id)
        {
            docente docente = (docente)SessionValue("docente");
            int currentper = (int)SessionValue("currentAcadPeriod");
            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();
                       
            if (currentper == ultimoPeriodo.idperiodo)
            {
                if (ModelState.IsValid)
                {

                    var labores = (from p in dbEntity.participa
                                   join u in dbEntity.usuario on docente.idusuario equals u.idusuario
                                   join l in dbEntity.labor on p.idlabor equals l.idlabor
                                   join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                   where p.idlabor == id && p.iddocente == docente.iddocente 
                                   select new DocenteLabor { idlabor = l.idlabor, iddocente = docente.iddocente, nombres = u.nombres, apellidos = u.apellidos, rol = u.rol }).OrderBy(u => u.apellidos).ToList();

                    labores = setLaborTeach(labores);

                    var response = labores.Select(l => new DocenteLabor { idlabor = l.idlabor, iddocente = docente.iddocente, nombres = l.nombres, apellidos = l.apellidos, rol = l.rol, tipoLabor = l.tipoLabor, descripcion = l.descripcion }).ToList();

                    ViewBag.datos = response;

                    return View();
                }
            }
            else
            {
                ViewBag.Error = " No puede autoevaluarse en periodos anteriores";
            }

            return View();
        }

        

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult saveAutoevaluacion(int idL, int idD, int val,string pdes, string psol, string rdes, string rsol )
        {
            participa auxParticipa = dbEntity.participa.SingleOrDefault(p => p.idlabor == idL && p.iddocente == idD);

            if (auxParticipa != null)
            {               
                evaluacion auxEvaluacion = dbEntity.evaluacion.SingleOrDefault(e => e.idevaluacion == auxParticipa.idevaluacion);
                autoevaluacion auxAutoEvalaucion = dbEntity.autoevaluacion.SingleOrDefault(a => a.idautoevaluacion == auxParticipa.idautoevaluacion);

                auxAutoEvalaucion.calificacion = val;  
                dbEntity.SaveChanges();
                auxEvaluacion.evaluacionautoevaluacion = (int)auxAutoEvalaucion.calificacion;
                dbEntity.SaveChanges();

                problema auxProblema = dbEntity.problema.SingleOrDefault(p=> p.idautoevaluacion == auxAutoEvalaucion.idautoevaluacion);
                auxProblema.descripcion = pdes;
                auxProblema.solucion = psol;
                dbEntity.SaveChanges();

                resultado auxresultado = dbEntity.resultado.SingleOrDefault(r=>r.idautoevaluacion == auxAutoEvalaucion.idautoevaluacion);
                auxresultado.descripcion = rdes;
                auxresultado.ubicacion = rsol;
                dbEntity.SaveChanges();

                /// Se genera PDF de soporte de autoevaluacion
                Models.Utilities oUtilities = new Models.Utilities();
                string spath = Server.MapPath("~/pdfsupport/");
                docente oDocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == idD);
                usuario oUsuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == oDocente.idusuario);
                string sDescription = TeacherController.getLaborDescripcion(idL);
                int currentper = (int)SessionValue("currentAcadPeriod");
                periodo_academico oPeriodo = dbEntity.periodo_academico.SingleOrDefault(q => q.idperiodo == currentper);

                Document oDocument = oUtilities.StartPdfWriter(
                    oDocente.idusuario.ToString(), "autoev", 
                    oPeriodo.anio.ToString() + "-" + oPeriodo.numeroperiodo.ToString() , sDescription, spath);

                Font oContentFont = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.NORMAL);
                System.Text.StringBuilder oBuilder = new System.Text.StringBuilder();
                oBuilder.Append("\n\nDocente: " + oUsuario.nombres + " " + oUsuario.apellidos);
                oBuilder.Append("\nPeriodo: " + oPeriodo.anio.ToString() + "-" + oPeriodo.numeroperiodo.ToString() );
                oBuilder.Append("\nNombre de Labor: " + sDescription );
                oBuilder.Append("\nTipo de Evaluación: Autoevaluación");
                oBuilder.Append("\nFecha y hora: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                oBuilder.Append("\nCALIFICACIÓN: " + val.ToString() + "\n\n");
                Paragraph oContent = new Paragraph(oBuilder.ToString());
                oContent.Font = oContentFont;
                oContent.Alignment = Element.ALIGN_JUSTIFIED;
                oDocument.Add(oContent);
                PdfPTable oTable = new PdfPTable(2);
                oTable.WidthPercentage = 100;
                Rectangle rect = new Rectangle(100, 1000);
                oTable.SetWidthPercentage(new float[] { 15, 85 }, rect);
                
                PdfPCell oCell0 = new PdfPCell(new Phrase("PROBLEMAS"));
                oCell0.Colspan = 2;
                oCell0.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                oCell0.BorderWidth = 0;
                oTable.AddCell(oCell0);
                
                PdfPCell oCell1 = new PdfPCell(new Phrase("Descripción"));
                oCell1.BorderWidth = 0;
                oTable.AddCell(oCell1);
                
                PdfPCell oCell2 = new PdfPCell(new Phrase(pdes));
                oCell2.BorderWidth = 0;
                oCell2.HorizontalAlignment = PdfPCell.ALIGN_JUSTIFIED;
                oTable.AddCell(oCell2);
                
                PdfPCell oCell3 = new PdfPCell(new Phrase("Solución"));
                oCell3.BorderWidth = 0;
                oTable.AddCell(oCell3);
                
                PdfPCell oCell4 = new PdfPCell(new Phrase(psol));
                oCell4.BorderWidth = 0;
                oCell4.HorizontalAlignment = PdfPCell.ALIGN_JUSTIFIED;
                oTable.AddCell(oCell4);

                PdfPCell oCell5 = new PdfPCell(new Phrase("\nRESULTADOS"));
                oCell5.BorderWidth = 0;
                oCell5.Colspan = 2;
                oCell5.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                oTable.AddCell(oCell5);

                PdfPCell oCell6 = new PdfPCell(new Phrase("Descripción"));
                oCell6.BorderWidth = 0;
                oTable.AddCell(oCell6);
                
                PdfPCell oCell7 = new PdfPCell(new Phrase(rdes));
                oCell7.BorderWidth = 0;
                oCell7.HorizontalAlignment = PdfPCell.ALIGN_JUSTIFIED;
                oTable.AddCell(oCell7);
                
                PdfPCell oCell8 = new PdfPCell(new Phrase("Ubicación"));
                oCell8.BorderWidth = 0;
                oTable.AddCell(oCell8);
                
                PdfPCell oCell9 = new PdfPCell(new Phrase(rsol));
                oCell9.BorderWidth = 0;
                oCell9.HorizontalAlignment = PdfPCell.ALIGN_JUSTIFIED;
                oTable.AddCell(oCell9);

                oDocument.Add(oTable);
                oDocument.Close();
                /// Fin de la generacion del PDF de soporte 
                var tarea = new { respuesta = 1 };
                return Json(tarea, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public static string getLaborDescripcion(long idLabor)
        {
            string sSQLDescripcion = " select ISNULL(materia.nombremateria, isnull(otras.descripcion, isnull(trabajodegrado.titulotrabajo, " +
           " isnull(trabajodegradoinvestigacion.titulotrabajo, isnull(gestion.nombrecargo, isnull(social.nombreproyecto,  " +
           " isnull(investigacion.nombreproyecto, isnull(desarrolloprofesoral.nombreactividad,'')))))))) as descripcion " +
           " from labor left join docencia on docencia.idlabor = labor.idlabor " +
           " left join materia on docencia.idmateria = materia.idmateria " +
           " left join otras on otras.idlabor = labor.idlabor " +
           " left join trabajodegrado on trabajodegrado.idlabor = labor.idlabor " +
           " left join trabajodegradoinvestigacion on trabajodegradoinvestigacion.idlabor = labor.idlabor " +
           " left join gestion on gestion.idlabor = labor.idlabor " +
           " left join social on social.idlabor = labor.idlabor " +
           " left join investigacion on investigacion.idlabor = labor.idlabor " +
           " left join desarrolloprofesoral on desarrolloprofesoral.idlabor = labor.idlabor " +
           " where labor.idlabor = " + idLabor.ToString();
           sSQLDescripcion = (string)Models.Utilities.ExecuteScalar(sSQLDescripcion);
           return sSQLDescripcion;
        }

        public autoevaluacion GetLastAutoEvaluacion()
        {
            autoevaluacion lastAutoEvaluacion;
           
            var maxId = (from aev in dbEntity.autoevaluacion
                         select aev.idautoevaluacion).Max();

            lastAutoEvaluacion = dbEntity.autoevaluacion.Single(q => q.idautoevaluacion == maxId);

            return lastAutoEvaluacion;
        }

        #endregion

        #region Consultas

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult VerAutoevaluaciones()
        {
            ViewBag.optionmenu = 1;            
            docente docente = (docente)SessionValue("docente");
            int currentper = (int)SessionValue("currentAcadPeriod");          
            ViewBag.reporte = crearReporteAutoevalaucion(docente.idusuario);
            return View();
        }
      
        public AutoevaluacionDocente crearReporteAutoevalaucion(int idUsuario)
        {
            departamento departamento = (departamento)SessionValue("depto");
            int currentper = (int)SessionValue("currentAcadPeriod");           
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
            docente docenteactual = (docente)SessionValue("docente");
            int idjefe = docenteactual.idusuario;
            usuario jefe = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idjefe);
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();

            AutoevaluacionDocenteReporte = new AutoevaluacionDocente() { nombredocente = user.nombres + " " + user.apellidos, nombrejefe = jefe.nombres + " " + jefe.apellidos, fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };

            List<ResAutoEvaluacionLabor> labores = (from u in dbEntity.usuario
                                                    join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                    join p in dbEntity.participa on d.iddocente equals p.iddocente
                                                    join aev in dbEntity.autoevaluacion on p.idautoevaluacion equals aev.idautoevaluacion
                                                    join pr in dbEntity.problema on aev.idautoevaluacion equals pr.idautoevaluacion
                                                    join re in dbEntity.resultado on aev.idautoevaluacion equals re.idautoevaluacion
                                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
                                                    join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                                    where l.idperiodo == currentper && u.idusuario == idUsuario && aev.calificacion != -1                                                   
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
                    continue;
                }
                social = dbEntity.social.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (social != null)
                {
                    labor.tipolabor = "Social";
                    labor.tipolaborcorto = "SOC";
                    labor.descripcion = social.nombreproyecto;                   
                    continue;
                }
                investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (investigacion != null)
                {
                    labor.tipolabor = "Investigación";
                    labor.tipolaborcorto = "INV";
                    labor.descripcion = investigacion.nombreproyecto;                    
                    continue;
                }
                trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (trabajoDeGrado != null)
                {
                    labor.tipolabor = "Trabajo de Grado";
                    labor.tipolaborcorto = "TDG";
                    labor.descripcion = trabajoDeGrado.titulotrabajo;                    
                    continue;
                }
                trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (trabajoDeGradoInvestigacion != null)
                {
                    labor.tipolabor = "Trabajo de Grado Investigación";
                    labor.tipolaborcorto = "TDGI";
                    labor.descripcion = trabajoDeGradoInvestigacion.titulotrabajo;                   
                    continue;
                }
                desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (desarrolloProfesoral != null)
                {
                    labor.tipolabor = "Desarrollo Profesoral";
                    labor.tipolaborcorto = "DP";
                    labor.descripcion = desarrolloProfesoral.nombreactividad;                  
                    continue;
                }
                docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (docencia != null)
                {
                    materia materia = dbEntity.materia.SingleOrDefault(g => g.idmateria == docencia.idmateria);
                    labor.tipolabor = "Docencia Directa";
                    labor.tipolaborcorto = "DD";
                    labor.descripcion = materia.nombremateria;                    
                    continue;
                }
                otra = dbEntity.otras.SingleOrDefault(g => g.idlabor == labor.idlabor);
                if (otra != null)
                {
                    labor.tipolabor = "Otra";
                    labor.tipolaborcorto = "OTR";
                    labor.descripcion = otra.descripcion;                    
                    continue;
                }
            }
            return lista;
        }

        #endregion

        #region Reporte

        public ActionResult ExportToExcelAutoevaluacion()
        {
            SpreadsheetModelAE mySpreadsheetAE = new SpreadsheetModelAE();
            int tam = AutoevaluacionDocenteReporte.autoevaluacioneslabores.Count();
            String[,] datos = new String[tam, 9];          

            mySpreadsheetAE.fechaevaluacion = AutoevaluacionDocenteReporte.fechaevaluacion;
            mySpreadsheetAE.periodo = "" + AutoevaluacionDocenteReporte.periodonum + " - " + AutoevaluacionDocenteReporte.periodoanio;
            mySpreadsheetAE.nombredocente = AutoevaluacionDocenteReporte.nombredocente;
            mySpreadsheetAE.nombrejefe = AutoevaluacionDocenteReporte.nombrejefe;

            int i = 0;

            foreach (ResAutoEvaluacionLabor labor in AutoevaluacionDocenteReporte.autoevaluacioneslabores)
            {
                datos[i, 0] = ""+ labor.idlabor;
                datos[i, 1] = ""+ labor.tipolaborcorto;
                datos[i, 2] = ""+ labor.tipolabor;
                datos[i, 3] = ""+ labor.descripcion;
                datos[i, 4] = "" + labor.nota;
                datos[i, 5] = "" + labor.problemadescripcion;
                datos[i, 6] = "" + labor.problemasolucion;
                datos[i, 7] = "" + labor.resultadodescripcion;
                datos[i, 8] = "" + labor.resultadosolucion;
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

        #region Evaluación

        public evaluacion GetLastEvaluacion()
        {
            evaluacion lastEvaluacion;

            var maxId = (from ev in dbEntity.evaluacion
                         select ev.idevaluacion).Max();

            lastEvaluacion = dbEntity.evaluacion.Single(q => q.idevaluacion == maxId);

            return lastEvaluacion;
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult saveEval(int idL, int idD, int val)
        {
            participa eliminarParticipa = dbEntity.participa.SingleOrDefault(p => p.idlabor == idL && p.iddocente == idD);

            if (eliminarParticipa != null)
            {
                evaluacion auxEvaluacion = dbEntity.evaluacion.SingleOrDefault(e => e.idevaluacion == eliminarParticipa.idevaluacion);
                auxEvaluacion.evaluacionautoevaluacion = val;
                dbEntity.SaveChanges();
                var tarea = new { respuesta = 1 };
                return Json(tarea, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #endregion

        // FIN ADICIONADO POR CLARA

       
        [Authorize]
        public ActionResult performSelfEvaluation()
        {
            docente docente = (docente)SessionValue("docente");
            return View(docente);
        }
               
       
    }
    // NUEVO 29
    public class DocenteLabor
    {
        public int iddocente;
        public int idlabor;
        public string detalle;
        public string nombres;
        public string apellidos;
        public string rol;
        public string tipoLabor;
        public string descripcion;
        public int evaluacionAutoEvaluacion;
        public int evaluacionestudiante;
        public int evaluacionauto;
        public int evaluacionjefe;
        public int totalhoras;
        public double calificacion;
        public int horasPorLabor;

    }
    // FIN NUEVO 29
    public class Horas
    {
        public int Horassemanales;
        public int semanaslaborales;
    }   
}
