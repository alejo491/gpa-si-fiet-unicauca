using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using MvcSEDOC.Models;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Data.OleDb;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace MvcSEDOC.Controllers
{   

    public class AdminController : SEDOCController
    {
        public static int opcionError;
        public ImportDataSheets data_imports = new ImportDataSheets();
        public static ConsolidadoLabores ReporteLabores = new ConsolidadoLabores();
        List<ConsolidadoDocencia> ReporteMaterias = new List<ConsolidadoDocencia>();
        List<ConsolidadoLabores> ReporteLabores1 = new List<ConsolidadoLabores>();

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        #region Privilegios

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

        [Authorize]
        public ActionResult SetPrivileges(int id)
        {

            if (ModelState.IsValid)
            {
                Debug.WriteLine("depto 1" + id);
                var dep = dbEntity.departamento.SingleOrDefault(q => q.iddepartamento == id);
                var response = GetTeachersFromDepartment(id);
                ViewBag.datos = response;
                ViewBag.departamento = dep.nombre;
                return View();
            }
            return View();
        }

        #endregion

        #region Cuestionario

        [Authorize]
        public ActionResult CreateQuestionnaire()
        {
            ViewBag.optionmenu = 2;
            return View();
        }

        //MODIFICADO POR EDINSON
        [Authorize]
        [HttpPost]
        public ActionResult CreateQuestionnaire(cuestionario miCuestionario)
        {
            ViewBag.optionmenu = 2;
            int esta = 0;

            if (ModelState.IsValid)
            {
                string nombre = miCuestionario.tipocuestionario;
                if (nombre == null)
                {
                    ViewBag.Error = "Debe ingresar un nombre para el cuestionario";
                }
                else
                {
                    esta = yaEsta(nombre);
                    if (esta == 1)
                    {
                        ViewBag.Error = "Ya existe un cuestionario con el nombre ingresado";
                    }
                    else
                    {
                        dbEntity.cuestionario.AddObject(miCuestionario);
                        dbEntity.SaveChanges();
                        ViewBag.Message = "El cuestionario se creo correctamente";
                    }
                }
                return View();
            }

            return View(miCuestionario);
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult UpdateQuestionnaire()
        {
            ViewBag.optionmenu = 2;
            return View(dbEntity.cuestionario.ToList());
        }

        [Authorize]
        [HttpPost]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult EditQuestionnaire(cuestionario micuestionario)
        {
            if (ModelState.IsValid)
            {
                dbEntity.cuestionario.Attach(micuestionario);
                dbEntity.ObjectStateManager.ChangeObjectState(micuestionario, EntityState.Modified);
                dbEntity.SaveChanges();
                return Json(micuestionario, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        //MODIFICADO POR EDINSON
        [Authorize]
        [HttpPost]
        public ActionResult DeleteQuestionnaire(int id)
        {
            int id_borrar = id;

            var asignaciones = dbEntity.asignarCuestionario.Where(q => q.idcuestionario == id);

            foreach (asignarCuestionario aux in asignaciones)
            {
                dbEntity.asignarCuestionario.DeleteObject(aux);
            }

            var eliminar_grupos = dbEntity.grupo.Where(q => q.idcuestionario == id);

            foreach (grupo aux1 in eliminar_grupos)
            {
                var eliminar_preguntas = dbEntity.pregunta.Where(q => q.idgrupo == aux1.idgrupo);
                foreach (pregunta aux2 in eliminar_preguntas)
                {
                    dbEntity.pregunta.DeleteObject(aux2);

                }
                dbEntity.grupo.DeleteObject(aux1);
            }

            cuestionario cuestionario = dbEntity.cuestionario.Single(c => c.idcuestionario == id);
            dbEntity.cuestionario.DeleteObject(cuestionario);
            dbEntity.SaveChanges();

            return Json(cuestionario, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchQuestionnaire(string term)
        {
            var response = dbEntity.cuestionario.Select(q => new { label = q.tipocuestionario, id = q.idcuestionario }).Where(q => q.label.ToUpper().Contains(term.ToUpper())).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //MODIFICADO POR EDINSON
        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult AjaxSetQuestionnaire(cuestionario miCuestionario)
        {
            int esta = 0;

            if (ModelState.IsValid)
            {
                string nombre = miCuestionario.tipocuestionario;
                esta = yaEsta(nombre);

                if (esta == 1)
                {
                    miCuestionario.idcuestionario = 0;
                }
                else
                {
                    dbEntity.cuestionario.AddObject(miCuestionario);
                    dbEntity.SaveChanges();
                }

                return Json(miCuestionario, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion


        // inicio cambio contraeña        

        public static string md5(string password)
        {
            // para probar pq hace falta
            MD5 md5 = MD5CryptoServiceProvider.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            string br = "";
            stream = md5.ComputeHash(encoding.GetBytes(password));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);

            br = sb.ToString();
            return br;
        }

        [Authorize]
        public ActionResult ChangePass()
        {
            int periodoActualSelec = 0;
            
            periodoActualSelec = (int)SessionValue("periodoActual");;
            if (periodoActualSelec == 1)
            {
                return RedirectPermanent("/Admin/Index");
            }

            ViewBag.optionmenu = 1;
            return View();
        }


        [Authorize]
        public ActionResult ChangePass1()
        {
            int periodoActualSelec = 0;
            periodoActualSelec = (int)SessionValue("periodoActual");;
            if (periodoActualSelec == 1)
            {
                return RedirectPermanent("/Admin/Index");
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

            claveA = md5(anterior);

            try
            {
                usuario NuevaContrasena = dbEntity.usuario.Single(u => u.emailinstitucional == "adminsedoc@unicauca.edu.co" && u.password == claveA);
                if (nueva != repite)
                {
                    ViewBag.Error = "No coinciden la nueva contraseña con la anterior";
                    return View();
                }
                else
                {
                    if (!ActualizarContrasenaUsuarioActual(nueva, anterior))
                    {
                        throw new Exception("Invalid Password");
                    }
                    nueva = md5(nueva);
                    NuevaContrasena.password = nueva;

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
        // fin cambios contraseña



        #region Grupo

        //MODIFICADO POR EDINSON
        [Authorize]
        public ActionResult CreateGroup()
        {
            ViewBag.optionmenu = 2;
            ViewBag.Value1 = null;
            ViewBag.Value2 = null;
            ViewBag.Value3 = null;
            return View();
        }

        //MODIFICADO POR EDINSON 
        [Authorize]
        [HttpPost]
        public ActionResult CreateGroupo()
        {
            string cuestionario = "";
            string nombre = "";
            string auxporcentaje = "";
            double porcentaje = 0;
            int campo1 = 0;
            int campo2 = 0;
            int campo3 = 0;
            ViewBag.Value1 = "";
            ViewBag.Value2 = "";
            ViewBag.Value3 = "";

            cuestionario = Request.Form["cuestionario"];
            if (cuestionario == "")
            {
                campo1 = 1;
                ViewBag.Error1 = "entro";
            }
            else
            {
                ViewBag.Value1 = cuestionario;
            }

            nombre = Request.Form["nombre"];
            if (nombre == "")
            {
                campo2 = 1;
                ViewBag.Error2 = "entro";
            }
            else
            {
                ViewBag.Value2 = nombre;
            }

            auxporcentaje = Request.Form["porcentaje"];
            if (auxporcentaje == "")
            {
                campo3 = 1;
                ViewBag.Error3 = "entro";
            }
            else
            {
                ViewBag.Value3 = auxporcentaje;
            }


            ViewBag.optionmenu = 1;
            if (campo1 == 1 || campo2 == 1 || campo3 == 1)
            {
                ViewBag.Error = "Los campos con asterisco son obligatorios";
                return View();
            }

            try
            {
                cuestionario existecuestionario = dbEntity.cuestionario.Single(q => q.tipocuestionario == cuestionario);
                try
                {
                    porcentaje = Convert.ToDouble(auxporcentaje);
                }
                catch
                {
                    ViewBag.Error = "El campo porcentaje debe ser un numero";
                    return View();
                }

                int grupoExiste = yaEstaGrupo(nombre, (int)existecuestionario.idcuestionario);

                if (grupoExiste == 1)
                {
                    ViewBag.Error = "Ya existe un grupo de preguntas con el nombre " + nombre;
                    return View();
                }

                double porcentajes = 0;
                double total_final = 0;
                double aux_final = 0;

                try
                {
                    porcentajes = (double)dbEntity.grupo.Where(q => q.idcuestionario == existecuestionario.idcuestionario).Sum(q => q.porcentaje);
                }
                catch
                {
                    porcentajes = 0;
                }

                total_final = porcentaje + porcentajes;

                if (total_final > 100)
                {
                    aux_final = 100 - porcentajes;
                    if (aux_final == 0)
                    {
                        ViewBag.Error = "No puedes agregar más grupos, la suma de los porcentajes es 100%  ";
                        return View();
                    }
                    else
                    {
                        ViewBag.Error = "El máximo valor en la casilla porcentaje es: " + aux_final;
                        return View();
                    }
                }

                int maxorder;

                try
                {
                    maxorder = dbEntity.grupo.Where(q => q.idcuestionario == existecuestionario.idcuestionario).Max(q => q.orden);
                }
                catch (Exception)
                {
                    maxorder = 0;
                }
                grupo miGrupo = new grupo();
                ViewBag.optionmenu = 2;
                miGrupo.orden = maxorder + 1;
                miGrupo.idcuestionario = existecuestionario.idcuestionario;
                miGrupo.nombre = nombre;
                miGrupo.porcentaje = porcentaje;

                dbEntity.grupo.AddObject(miGrupo);
                dbEntity.SaveChanges();
                ViewBag.Message = "El Grupo se creo correctamente";
                return View();

            }
            catch
            {
                ViewBag.Error = "El campo tipo de cuestionario esta vacio o el cuestionario escrito no existe";
                return View();
            }
        }

        [Authorize]
        public ActionResult UpdateGroup()
        {
            ViewBag.optionmenu = 2;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditGroup(grupo miGrupo)
        {
            if (ModelState.IsValid)
            {
                dbEntity.grupo.Attach(miGrupo);
                dbEntity.ObjectStateManager.ChangeObjectState(miGrupo, EntityState.Modified);
                dbEntity.SaveChanges();
                return Json(miGrupo, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult DeleteGroup(int id)
        {
            grupo grupo = dbEntity.grupo.Single(c => c.idgrupo == id);
            dbEntity.grupo.DeleteObject(grupo);
            dbEntity.SaveChanges();
            return Json(grupo, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult SearchGroup(string idQuestionnarie, string term)
        {
            var response = dbEntity.grupo.Select(q => new { label = q.nombre, orden = q.orden, idq = q.idcuestionario, porcentaje = q.porcentaje, nombre = q.nombre, idgrupo = q.idgrupo }).Where(q => q.label.ToUpper().Contains(term.ToUpper())).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SortGroups(string stridq, intListing[] list_ids)
        {
            int idq = int.Parse(stridq);
            int idg;
            try
            {
                for (int i = 1; i <= list_ids.Count(); i++)
                {
                    idg = list_ids[(i - 1)].id;
                    grupo miGrupo = dbEntity.grupo.Single(q => q.idgrupo == idg);
                    miGrupo.orden = i;
                    dbEntity.ObjectStateManager.ChangeObjectState(miGrupo, EntityState.Modified);
                    dbEntity.SaveChanges();
                }
                var response = dbEntity.grupo.Select(q => new { label = q.nombre, id = q.idgrupo, pos = q.orden, idc = q.idcuestionario, porc = q.porcentaje }).Where(q => q.idc == idq).OrderBy(q => q.label).ToList();
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetQuestionnaireGroups(string term)
        {
            int idq = int.Parse(term);
            var response = dbEntity.grupo.Select(q => new { label = q.nombre, id = q.idgrupo, pos = q.orden, idc = q.idcuestionario, porc = q.porcentaje }).Where(q => q.idc == idq).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Pregunta

        [Authorize]
        public ActionResult CreateQuestion()
        {
            ViewBag.optionmenu = 2;
            return View();
        }

        [Authorize]
        public ActionResult UpdateQuestion()
        {
            ViewBag.optionmenu = 2;
            return View();
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult EditQuestion(pregunta pregunta)
        {
            if (ModelState.IsValid)
            {
                dbEntity.pregunta.Attach(pregunta);
                dbEntity.ObjectStateManager.ChangeObjectState(pregunta, EntityState.Modified);
                dbEntity.SaveChanges();
                return Json(pregunta, JsonRequestBehavior.AllowGet);
            }
            return View(pregunta);
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult DeleteQuestion(int id)
        {
            pregunta pregunta = dbEntity.pregunta.Single(c => c.idpregunta == id);
            dbEntity.pregunta.DeleteObject(pregunta);
            dbEntity.SaveChanges();
            return Json(pregunta, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetQuestionsFromGroup(string term)
        {
            int idgroup = int.Parse(term);
            var response = dbEntity.pregunta.Select(q => new { label = q.pregunta1, id = q.idpregunta, idg = q.idgrupo }).Where(q => q.idg == idgroup).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult AjaxSetQuestion(pregunta miPregunta)
        {
            if (ModelState.IsValid)
            {
                dbEntity.pregunta.AddObject(miPregunta);
                dbEntity.SaveChanges();
                return Json(miPregunta, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Periodo Académico

        [Authorize]
        public ActionResult AcademicPeriods()
        {
            ViewBag.optionmenu = 7;
            return View(dbEntity.periodo_academico.ToList());
        }

        [Authorize]
        public ConsolidadoLabores crearReporte(int idUsuario)
        {            
            int currentper = (int)SessionValue("currentAcadPeriod");
            var docjefe = new docente();
            var usujefe = new usuario();
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();

            var docente = dbEntity.docente.SingleOrDefault(q => q.idusuario == idUsuario);
            var jefe = dbEntity.esjefe.SingleOrDefault(q => q.iddepartamento == docente.iddepartamento && q.idperiodo == currentper);
            if (jefe != null)
                docjefe = dbEntity.docente.SingleOrDefault(q => q.iddocente == jefe.iddocente);
            if (docjefe != null)
                usujefe = dbEntity.usuario.SingleOrDefault(q => q.idusuario == docjefe.idusuario);

            ConsolidadoLabores reporte = new ConsolidadoLabores();

            List<int> labores = (from u in dbEntity.usuario
                                 join d in dbEntity.docente on u.idusuario equals d.idusuario
                                 join p in dbEntity.participa on d.iddocente equals p.iddocente
                                 join l in dbEntity.labor on p.idlabor equals l.idlabor
                                 join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                 where l.idperiodo == currentper && u.idusuario == idUsuario
                                 select l.idlabor).ToList();

            reporte = setLabor(labores);
            reporte.nombredocente = user.nombres + " " + user.apellidos;
            if (usujefe != null)
            {
                reporte.nombrejefe = usujefe.nombres + " " + usujefe.apellidos;
            }
            else
            {
                reporte.nombrejefe = "SIN JEFE ASIGNADO";
            }
            reporte.periodoanio = (int)PeriodoSeleccionado.anio;
            reporte.periodonum = (int)PeriodoSeleccionado.numeroperiodo;
            reporte.fechaevaluacion = fecha_actual;
            //reporte.totalhorassemana = (Double)labores.Sum(q => q.horasxsemana);

            ReporteLabores = reporte;
            return reporte;
        }


        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult CreateAcademicPeriod(periodo_academico periodo)
        {
            if (ModelState.IsValid)
            {
                dbEntity.periodo_academico.AddObject(periodo);
                dbEntity.SaveChanges();
                periodo_academico ultimoPeriodo = GetLastAcademicPeriod();
                Session["currentAcadPeriod"] = ultimoPeriodo.idperiodo;

                int currentper = (int)SessionValue("currentAcadPeriod");
                return Json(periodo, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult EditAcademicPeriod(periodo_academico periodo)
        {
            if (ModelState.IsValid)
            {
                dbEntity.periodo_academico.Attach(periodo);
                dbEntity.ObjectStateManager.ChangeObjectState(periodo, EntityState.Modified);
                dbEntity.SaveChanges();
                return Json(periodo, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult DeleteAcademicPeriod(int id)
        {
            if (dbEntity.labor.Count(q => q.idperiodo == id) == 0 && dbEntity.esjefe.Count(q => q.idperiodo == id) == 0)
            {
                periodo_academico periodo = dbEntity.periodo_academico.Single(c => c.idperiodo == id);
                dbEntity.periodo_academico.DeleteObject(periodo);
                dbEntity.SaveChanges();
                return Json(periodo, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Jefe Departamento

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetCurrentDepartmentChief(int iddepartamento)
        {
            int currentper = (int)SessionValue("currentAcadPeriod");
            esjefe response = null;

            response = dbEntity.esjefe.SingleOrDefault(q => q.iddepartamento == iddepartamento && q.idperiodo == currentper);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Jefe Labor

        //MODIFICADO POR EDINSON
        [Authorize]
        public ActionResult AssessLaborChief()
        {
            ViewBag.optionmenu = 6;

            int currentper = (int)SessionValue("currentAcadPeriod");

            var users = (from u in dbEntity.usuario
                         join d in dbEntity.dirige on u.idusuario equals d.idusuario
                         join l in dbEntity.labor on d.idlabor equals l.idlabor
                         join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                         join ev in dbEntity.evaluacion on d.idevaluacion equals ev.idevaluacion
                         where l.idperiodo == currentper
                         //select new JefeLabor { idusuario = u.idusuario,  idlabor = l.idlabor, idevaluacion = ev.idevaluacion, nombres = u.nombres, apellidos = u.apellidos, rol = u.rol, evaluacionJefe = (int)ev.evaluacionjefe }).Where(p => p.evaluacionJefe == -1).OrderBy(p => p.apellidos).ToList();
                         select new JefeLabor { idusuario = u.idusuario, idlabor = l.idlabor, idevaluacion = ev.idevaluacion, nombres = u.nombres, apellidos = u.apellidos, rol = u.rol, evaluacionJefe = (int)ev.evaluacionjefe }).OrderBy(p => p.apellidos).ToList();

            users = setLaborChief(users);
            ViewBag.lista = users;
            return View();
        }

        /// <summary>
        /// Actualiza el jefe actual para un departamento especifico
        /// </summary>
        /// <param name="idDepartamento"></param>
        /// <param name="idDocentAct"></param>
        /// <param name="idDocentNue"></param>
        /// <param name="laboresCalc"></param>
        /// <param name="currentper"></param>
        /// <returns></returns>
        public esjefe ProcessUpdateCurrentChief(int idDepartamento, int idDocentAct, int idDocentNue, int laboresCalc, int currentper)
        {
            
            if (idDocentAct != -1)
            {
                esjefe eliminar = dbEntity.esjefe.Single(q => q.iddepartamento == idDepartamento && q.iddocente == idDocentAct && q.idperiodo == currentper);
                dbEntity.esjefe.DeleteObject(eliminar);
                dbEntity.SaveChanges();
                docente oDocente = dbEntity.docente.Single(q => q.iddocente == idDocentAct);
                usuario oUsuario = dbEntity.usuario.Single(q => q.idusuario == oDocente.idusuario);
                RemoverUsuarioDeRol(oUsuario.emailinstitucional, "DepartmentChief");
            }

            esjefe nuevo = new esjefe();
            nuevo.idperiodo = currentper;
            nuevo.iddepartamento = idDepartamento;
            nuevo.iddocente = idDocentNue;

            dbEntity.esjefe.AddObject(nuevo);
            dbEntity.SaveChanges();

            docente oNDoc = dbEntity.docente.Single(q => q.iddocente == idDocentNue);
            usuario oNUser = dbEntity.usuario.Single(q => q.idusuario == oNDoc.idusuario);
            RegistrarUsuarioEnRol(oNUser.emailinstitucional, "DepartmentChief");

            if (laboresCalc > 1)
            {
                var value = dbEntity.docente.SingleOrDefault(q => q.iddocente == idDocentNue);
                DepartmentChiefController oController = new DepartmentChiefController();
                /// Docentes 
                if ((laboresCalc % 3) == 0)
                {
                    var labores = (from p in dbEntity.labor
                                   join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                   join doc in dbEntity.docencia on p.idlabor equals doc.idlabor
                                   select p.idlabor).ToList();
                    foreach (int idLabor in labores)
                    {
                        oController.SaveChiefAux(value.idusuario, idLabor, 1);
                    }
                }
                /// Desarrollo profesoral 
                if ((laboresCalc % 5) == 0)
                {
                    var labores = (from p in dbEntity.labor
                                   join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                   join doc in dbEntity.desarrolloprofesoral on p.idlabor equals doc.idlabor
                                   select p.idlabor).ToList();
                    foreach (int idLabor in labores)
                    {
                        oController.SaveChiefAux(value.idusuario, idLabor, 1);
                    }
                }
                /// Trabajo de grado de investigacion
                if ((laboresCalc % 7) == 0)
                {
                    var labores = (from p in dbEntity.labor
                                   join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                   join doc in dbEntity.trabajodegradoinvestigacion on p.idlabor equals doc.idlabor
                                   select p.idlabor).ToList();
                    foreach (int idLabor in labores)
                    {
                        oController.SaveChiefAux(value.idusuario, idLabor, 1);
                    }
                }
                /// Trabajo de grado
                if ((laboresCalc % 11) == 0)
                {
                    var labores = (from p in dbEntity.labor
                                   join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                   join doc in dbEntity.trabajodegrado on p.idlabor equals doc.idlabor
                                   select p.idlabor).ToList();
                    foreach (int idLabor in labores)
                    {
                        oController.SaveChiefAux(value.idusuario, idLabor, 1);
                    }
                }
                /// Investigacion
                if ((laboresCalc % 13) == 0)
                {
                    var labores = (from p in dbEntity.labor
                                   join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                   join doc in dbEntity.investigacion on p.idlabor equals doc.idlabor
                                   select p.idlabor).ToList();
                    foreach (int idLabor in labores)
                    {
                        oController.SaveChiefAux(value.idusuario, idLabor, 1);
                    }
                }
                /// Gestion
                if ((laboresCalc % 17) == 0)
                {
                    var labores = (from p in dbEntity.labor
                                   join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                   join doc in dbEntity.gestion on p.idlabor equals doc.idlabor
                                   select p.idlabor).ToList();
                    foreach (int idLabor in labores)
                    {
                        oController.SaveChiefAux(value.idusuario, idLabor, 1);
                    }
                }
                /// social
                if ((laboresCalc % 19) == 0)
                {
                    var labores = (from p in dbEntity.labor
                                   join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                   join doc in dbEntity.social on p.idlabor equals doc.idlabor
                                   select p.idlabor).ToList();
                    foreach (int idLabor in labores)
                    {
                        oController.SaveChiefAux(value.idusuario, idLabor, 1);
                    }
                }
                /// otras
                if ((laboresCalc % 23) == 0)
                {
                    var labores = (from p in dbEntity.labor
                                   join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                   join doc in dbEntity.otras on p.idlabor equals doc.idlabor
                                   select p.idlabor).ToList();
                    foreach (int idLabor in labores)
                    {
                        oController.SaveChiefAux(value.idusuario, idLabor, 1);
                    }
                }
            }
            return nuevo;
        }
        
        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult UpdateCurrentChief(int idDepartamento, int idDocentAct, int idDocentNue, int laboresCalc)
        {
            int currentper = (int)SessionValue("currentAcadPeriod");
            return Json(ProcessUpdateCurrentChief(idDepartamento, idDocentAct, idDocentNue, laboresCalc, currentper), JsonRequestBehavior.AllowGet);
        }

        //MODIFICADO POR CLARA
        public List<JefeLabor> setLaborChief(List<JefeLabor> lista)
        {
            gestion gestion;
            social social;
            investigacion investigacion;
            trabajodegrado trabajoDeGrado;
            trabajodegradoinvestigacion trabajoDeGradoInvestigacion;
            desarrolloprofesoral desarrolloProfesoral;
            docencia docencia;
            otras otra;

            foreach (JefeLabor jefe in lista)
            {

                gestion = dbEntity.gestion.SingleOrDefault(g => g.idlabor == jefe.idlabor);

                if (gestion != null)
                {
                    jefe.tipoLabor = "Gestiónn";
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
                    materia materia = dbEntity.materia.SingleOrDefault(g => g.idmateria == docencia.idmateria);
                    jefe.tipoLabor = "Docencia";
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

        #endregion

        #region Evaluación

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult UpdateEvaluacion(int idEvaluacion, int calificacion)
        {
            evaluacion eval = dbEntity.evaluacion.Single(q => q.idevaluacion == idEvaluacion);
            eval.evaluacionjefe = calificacion;
            dbEntity.SaveChanges();
            return null;
        }

        #endregion

        /****** INICIO ADICIONADO POR CLARA*******/

        #region ADICIONADO POR CLARA

        #region Facultad

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchFacultad()
        {
            var response = dbEntity.facultad.Select(q => new { label = q.fac_nombre, id = q.idfacultad }).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ObtenerFacultadPrograma()
        {

            List<FacultadPrograma> Facultades = new List<FacultadPrograma>();

            foreach (var fac in dbEntity.facultad)
                Facultades.Add(
                        new FacultadPrograma()
                        {
                            codfacultad = fac.idfacultad,
                            nomfacultad = fac.fac_nombre.ToString()
                        });

            ViewBag.Facultades = Facultades;
            return View(ViewBag.Facultades);
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchFacultad(string term)
        {
            var response = dbEntity.facultad.Select(q => new { label = q.fac_nombre, id = q.idfacultad }).Where(q => q.label.ToUpper().Contains(term.ToUpper())).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetFacultad(string term)
        {
            int idp = int.Parse(term);
            var dp = dbEntity.departamento.SingleOrDefault(q => q.iddepartamento == idp);
            var response = dbEntity.facultad.SingleOrDefault(q => q.idfacultad == dp.idfacultad);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult AjaxSetFacultad(facultad miFacultad)
        {
            if (ModelState.IsValid)
            {
                dbEntity.facultad.AddObject(miFacultad);
                dbEntity.SaveChanges();
                return Json(miFacultad, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Departamento

        [Authorize]
        public ActionResult ListProgramas()
        {
            ViewBag.facultades = dbEntity.facultad.ToList();
            return View();
        }

        [Authorize]
        public ActionResult SearchPrograma(string idFacultad, string term)
        {
            var response = dbEntity.departamento.Select(q => new { label = q.nombre, idf = q.idfacultad, nombre = q.nombre, idp = q.iddepartamento }).Where(q => q.label.ToUpper().Contains(term.ToUpper())).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchDepartment(string term)
        {
            var response = dbEntity.departamento.Select(q => new { label = q.nombre, id = q.iddepartamento }).Where(q => q.label.ToUpper().Contains(term.ToUpper())).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SortProgramas(string stridf, intListing[] list_ids)
        {
            int idf = int.Parse(stridf);
            int idp;
            try
            {
                for (int i = 1; i <= list_ids.Count(); i++)
                {
                    idp = list_ids[(i - 1)].id;
                    departamento miPrograma = dbEntity.departamento.Single(q => q.iddepartamento == idp);
                    dbEntity.ObjectStateManager.ChangeObjectState(miPrograma, EntityState.Modified);
                    dbEntity.SaveChanges();
                }
                var response = dbEntity.departamento.Select(q => new { label = q.nombre, id = q.iddepartamento, idfa = q.idfacultad }).Where(q => q.idfa == idf).OrderBy(q => q.label).ToList();
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetFacultadPrograma(string term)
        {
            int idf = int.Parse(term);
            var response = dbEntity.departamento.Select(q => new { label = q.nombre, id = q.iddepartamento, idfa = q.idfacultad }).Where(q => q.idfa == idf).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Docente

        [Authorize]
        public ActionResult ListDocente()
        {
            ViewBag.optionmenu = 1;
            ViewBag.facultades = dbEntity.facultad.ToList();
            return View();
        }

        [Authorize]
        public ActionResult VerDocentes(int id)
        {
            ViewBag.optionmenu = 1;
            if (ModelState.IsValid)
            {
                var dep = dbEntity.departamento.SingleOrDefault(q => q.iddepartamento == id);
                var response = GetTeachersFromDepartment(id);
                ViewBag.datos = response;
                ViewBag.departamento = dep.nombre;
                return View();
            }
            return View();
        }

        [Authorize]
        public ActionResult VerMDocentes(int id)
        {
            ViewBag.optionmenu = 1;
            if (ModelState.IsValid)
            {
                var dep = dbEntity.departamento.SingleOrDefault(q => q.iddepartamento == id);
                var response = GetTeachersFromDepartment(id);

                ViewBag.datos = response;
                ViewBag.departamento = dep.nombre;
                return View();
            }
            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public List<DocenteDepto> GetTeachersFromDepartment(int iddepartment)
        {
            int currentper = (int)SessionValue("currentAcadPeriod");

            var doc_depto = dbEntity.usuario
                .Join(dbEntity.docente,
                usu => usu.idusuario,
                doc => doc.idusuario,
                (usu, doc) => new { usuario = usu, docente = doc })
                .Where(usu_doc => usu_doc.docente.iddepartamento == iddepartment && usu_doc.docente.estado == "activo");

            var response = doc_depto.Select(q => new DocenteDepto { idususaio = q.usuario.idusuario, iddetp = q.docente.iddepartamento, iddocente = q.docente.iddocente, nombre = q.usuario.nombres, apellido = q.usuario.apellidos, estado = q.docente.estado }).OrderBy(q => q.apellido).ToList();

            esjefe jefe = dbEntity.esjefe.SingleOrDefault(q => q.iddepartamento == iddepartment && q.idperiodo == currentper);

            foreach (var d in response)
                try
                {
                    if (jefe != null && d.iddocente == jefe.iddocente)
                        d.jefe = "checked";
                    else
                        d.jefe = "";
                }
                catch { d.jefe = ""; }

            return (response);
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetDepartamentoProfesores(string term)
        {
            int idd = int.Parse(term);
            var response = dbEntity.usuario.Select(q => new { label = q.nombres, id = q.idusuario }).OrderBy(q => q.label).ToList();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Materias

        public ActionResult ListSemestre(int id)
        {
            ViewBag.optionmenu = 1;
            if (ModelState.IsValid)
            {
                var dep = dbEntity.departamento.SingleOrDefault(q => q.iddepartamento == id);
                ViewBag.iddepa = dep.iddepartamento;
                ViewBag.nombredepa = dep.nombre;
                return View();
            }
            return View();
        }

        [Authorize]
        public ActionResult ListMateria()
        {
            ViewBag.optionmenu = 1;
            ViewBag.facultades = dbEntity.facultad.ToList();
            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowMaterias(int idusuario)
        {
            ViewBag.optionmenu = 1;
            ViewBag.reporte = crearReporteMateria(idusuario);
            return View();
        }

        public ConsolidadoDocencia crearReporteMateria(int idUsuario)
        {
            int currentper = (int)SessionValue("currentAcadPeriod");
            var docjefe = new docente();
            var usujefe = new usuario();
            ConsolidadoDocencia reporte;

            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();

            var docente = dbEntity.docente.SingleOrDefault(q => q.idusuario == idUsuario);
            var jefe = dbEntity.esjefe.SingleOrDefault(q => q.iddepartamento == docente.iddepartamento && q.idperiodo == currentper);

            if (jefe != null)
                docjefe = dbEntity.docente.SingleOrDefault(q => q.iddocente == jefe.iddocente);
            if (docjefe != null)
                usujefe = dbEntity.usuario.SingleOrDefault(q => q.idusuario == docjefe.idusuario);

            if (usujefe != null)
                reporte = new ConsolidadoDocencia() { nombredocente = user.nombres + " " + user.apellidos, nombrejefe = usujefe.nombres + " " + usujefe.apellidos, fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };
            else
                reporte = new ConsolidadoDocencia() { nombredocente = user.nombres + " " + user.apellidos, nombrejefe = " ", fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };

            List<DetalleDocencia> labores = (from p in dbEntity.participa
                                             join l in dbEntity.labor on p.idlabor equals l.idlabor
                                             join ld in dbEntity.docencia on l.idlabor equals ld.idlabor
                                             join m in dbEntity.materia on ld.idmateria equals m.idmateria
                                             //join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                                             join ae in dbEntity.autoevaluacion on p.idautoevaluacion equals ae.idautoevaluacion
                                             //join pg in dbEntity.programa on m.idprograma equals pg.idprograma
                                             join pr in dbEntity.problema on ae.idautoevaluacion equals pr.idautoevaluacion
                                             join s in dbEntity.resultado on ae.idautoevaluacion equals s.idautoevaluacion
                                             where l.idperiodo == currentper && p.iddocente == docente.iddocente
                                             select new DetalleDocencia
                                             {
                                                 idLabDoc = l.idlabor,
                                                 idmateria = m.idmateria,
                                                 nombremateria = m.nombremateria,//grupo = m.grupo,tipo = m.tipo,idprograma= pg.idprograma, nombreprograma= pg.nombreprograma,
                                                 //horassemana = (int)ld.horassemana, semanaslaborales = (int)ld.semanaslaborales, creditos = (int)m.creditos, semestre = (int)m.semestre,
                                                 //evaluacionjefe = (int)e.evaluacionjefe, evaluacionestudiante = (int)e.evaluacionestudiante,
                                                 evaluacionautoevaluacion = (int)ae.calificacion,
                                                 problemadescripcion = pr.descripcion,
                                                 problemasolucion = pr.solucion,
                                                 resultadodescripcion = s.descripcion,
                                                 resultadosolucion = s.ubicacion
                                             }).ToList();

            reporte.labDocencia = labores;
            //reporte.nombredocente = user.nombres + " " + user.apellidos;
            //reporte.nombrejefe = usujefe.nombres + " " + usujefe.apellidos;
            //reporte.periodoanio = (int)PeriodoSeleccionado.anio;
            //reporte.periodonum = (int)PeriodoSeleccionado.numeroperiodo;
            //reporte.fechaevaluacion = fecha_actual;
            //reporte.totalhorassemana = (Double)labores.Sum(q => q.horasxsemana);

            Debug.WriteLine("# labores" + labores.Count);

            foreach (var l in labores)
            {
                Debug.WriteLine("lab " + l.idLabDoc + "mat  " + l.idmateria);
            }
            //Debug.WriteLine("user 1 " + reporte.nombredocente);
            //Debug.WriteLine("periodo 1 " + reporte.periodoanio);
            return reporte;
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetMateriasPrograma(string term, string depa)
        {
            int sem = int.Parse(term);
            int dep = int.Parse(depa);
            int currentper = (int)SessionValue("currentAcadPeriod");

            var response = (from l in dbEntity.labor
                            join d in dbEntity.docencia on l.idlabor equals d.idlabor
                            join m in dbEntity.materia on d.idmateria equals m.idmateria
                            where m.semestre == sem && m.idprograma == dep && l.idperiodo == currentper
                            select new Asignatura { id = m.idmateria, nombre = m.nombremateria }).Distinct().ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowAutoScoresMateria(string depa, string tipo)
        {
            ViewBag.optionmenu = 1;
            int idD = int.Parse(depa);
            int id = int.Parse(tipo);
            ViewBag.reporte = reporteEvaluacionesMateria(idD, id);
            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowScoresMateria(string depa, string tipo)
        {
            ViewBag.optionmenu = 1;
            int idD = int.Parse(depa);
            int id = int.Parse(tipo);
            Debug.WriteLine("idl 1 " + id);
            Debug.WriteLine("idl 2 " + idD);
            ViewBag.reporte = reporteEvaluacionesMateria(idD, id);
            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public List<ConsolidadoDocencia> reporteEvaluacionesMateria(int idD, int idM)
        {
            ViewBag.optionmenu = 1;
            int currentper = (int)SessionValue("currentAcadPeriod");
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);

            List<ConsolidadoDocencia> MateriaEvaluada = new List<ConsolidadoDocencia>();

            List<int> labores = (from l in dbEntity.labor
                                 join d in dbEntity.docencia on l.idlabor equals d.idlabor
                                 where d.idmateria == idM && l.idperiodo == currentper
                                 select d.idlabor).Distinct().ToList();


            foreach (var lab in labores)
            {

                List<usuDoc> docentes = (from u in dbEntity.usuario
                                         join d in dbEntity.docente on u.idusuario equals d.idusuario
                                         join p in dbEntity.participa on d.iddocente equals p.iddocente
                                         where p.idlabor == lab && d.iddepartamento == idD
                                         select new usuDoc { id = p.iddocente, nombre = u.nombres + " " + u.apellidos, idlab = p.idlabor }).Distinct().ToList();


                materia mat = dbEntity.materia.SingleOrDefault(q => q.idmateria == idM);
                docencia labdoc = dbEntity.docencia.SingleOrDefault(q => q.idlabor == lab);


                foreach (var doc in docentes)
                {

                    ConsolidadoDocencia datos = new ConsolidadoDocencia() { nombredocente = doc.nombre, fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };

                    datos.labDocencia = new List<DetalleDocencia>();

                    List<DetalleDocencia> lista = (from p in dbEntity.participa
                                                   join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                                                   join aev in dbEntity.autoevaluacion on p.idautoevaluacion equals aev.idautoevaluacion
                                                   join pr in dbEntity.problema on aev.idautoevaluacion equals pr.idautoevaluacion
                                                   join re in dbEntity.resultado on aev.idautoevaluacion equals re.idautoevaluacion
                                                   where p.idlabor == doc.idlab && p.iddocente == doc.id
                                                   select new DetalleDocencia
                                                   {
                                                       nombremateria = mat.nombremateria,
                                                       grupo = mat.grupo,
                                                       codmateria = mat.codigomateria,
                                                       creditos = (int)mat.creditos,
                                                       horassemana = (int)labdoc.horassemana,
                                                       semanaslaborales = (int)labdoc.semanaslaborales,
                                                       idLabDoc = lab,
                                                       evaluacionautoevaluacion = (int)e.evaluacionautoevaluacion,
                                                       evaluacionestudiante = (int)e.evaluacionestudiante,
                                                       evaluacionjefe = (int)e.evaluacionjefe,
                                                       problemadescripcion = pr.descripcion,
                                                       problemasolucion = pr.solucion,
                                                       resultadodescripcion = re.descripcion,
                                                       resultadosolucion = re.ubicacion
                                                   }).Distinct().ToList();


                    datos.labDocencia.AddRange(lista);
                    MateriaEvaluada.Add(datos);
                }
            }

            return MateriaEvaluada;
        }

        #endregion

        #region Autoevaluacion

        [Authorize]
        public ActionResult Autoevaluacion()
        {
            ViewBag.optionmenu = 1;
            ViewBag.facultades = dbEntity.facultad.ToList();
            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowAutoDocentes(int idusuario)
        {
            ViewBag.optionmenu = 1;

            var response = crearReporteAutoevaluacion(idusuario);
            ViewBag.reporte = response;
            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowAutoScores(string depa, string tipo)
        {
            ViewBag.optionmenu = 1;
            departamento dep = dbEntity.departamento.SingleOrDefault(q => q.nombre == depa);
            var response = GetAutoFromDepartment(dep.iddepartamento);

            ViewBag.reporte = response;
            ViewBag.tipo = tipo;
            ViewBag.dep = dep.nombre;

            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public List<AutoevaluacionDocente> GetAutoFromDepartment(int iddepartment)
        {
            int currentper = (int)SessionValue("currentAcadPeriod");
            var docjefe = new docente();

            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();
            var jefe = dbEntity.esjefe.SingleOrDefault(q => q.iddepartamento == iddepartment && q.idperiodo == currentper);

            if (jefe != null)
                docjefe = dbEntity.docente.SingleOrDefault(q => q.iddocente == jefe.iddocente);


            List<AutoevaluacionDocente> ReporteAutoLabores = new List<AutoevaluacionDocente>();

            var docentes = GetTeachersFromDepartment(iddepartment);

            foreach (var doc in docentes)
            {
                ReporteAutoLabores.Add(crearReporteAutoevaluacion(doc.idususaio));
            }

            return (ReporteAutoLabores);
        }
        #endregion

        #region Labores

        [Authorize]
        public ActionResult VerLabores(int id)
        {
            ViewBag.optionmenu = 1;
            if (ModelState.IsValid)
            {

                var dep = dbEntity.departamento.SingleOrDefault(q => q.iddepartamento == id);
                var response = GetWorksFromDepartment(id);
                List<string> lab = new List<string>();

                foreach (var l in response)
                {
                    foreach (var a in l.detalleslabores)
                    {
                        if (!lab.Contains(a))
                            lab.Add(a);
                    }
                }
                ViewBag.labores = lab;
                ViewBag.datos = response;
                ViewBag.departamento = dep.nombre;
                return View();
            }
            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowDetallesLabores(string depa, string tipo)
        {
            ViewBag.optionmenu = 1;
            departamento dep = dbEntity.departamento.SingleOrDefault(q => q.nombre == depa);
            var response = GetWorksFromDepartment(dep.iddepartamento);

            ViewBag.reporte = response;
            ViewBag.tipo = tipo;
            ViewBag.dep = dep.nombre;

            return View();
        }

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public List<ConsolidadoLabores> GetWorksFromDepartment(int iddepartment)
        {
            int currentper = (int)SessionValue("currentAcadPeriod");
            var docjefe = new docente();

            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();

            var jefe = dbEntity.esjefe.SingleOrDefault(q => q.iddepartamento == iddepartment && q.idperiodo == currentper);

            var docentes = GetTeachersFromDepartment(iddepartment);

            if (jefe != null)
                docjefe = dbEntity.docente.SingleOrDefault(q => q.iddocente == jefe.iddocente);

            foreach (var doc in docentes)
            {
                ReporteLabores1.Add(crearReporte(doc.idususaio));
            }

            return (ReporteLabores1);
        }

        public AutoevaluacionDocente crearReporteAutoevaluacion(int idUsuario)
        {
            departamento departamento = (departamento)Session["depto"];
            AutoevaluacionDocente AutoevaluacionDocenteReporte;
            var docjefe = new docente();
            var usujefe = new usuario();

            int currentper = (int)SessionValue("currentAcadPeriod");
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
            DateTime Hoy = DateTime.Today;
            string fecha_actual = Hoy.ToString("MMMM dd") + " de " + Hoy.ToString("yyyy");
            fecha_actual = fecha_actual.ToUpper();

            var docente = dbEntity.docente.SingleOrDefault(q => q.idusuario == idUsuario);
            var jefe = dbEntity.esjefe.SingleOrDefault(q => q.iddepartamento == docente.iddepartamento && q.idperiodo == currentper);

            if (jefe != null)
                docjefe = dbEntity.docente.SingleOrDefault(q => q.iddocente == jefe.iddocente);

            if (docjefe != null)
                usujefe = dbEntity.usuario.SingleOrDefault(q => q.idusuario == docjefe.idusuario);

            if (usujefe != null)
            {
                AutoevaluacionDocenteReporte = new AutoevaluacionDocente() { nombredocente = user.nombres + " " + user.apellidos, nombrejefe = usujefe.nombres + " " + usujefe.apellidos, fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };
            }
            else
            {
                AutoevaluacionDocenteReporte = new AutoevaluacionDocente() { nombredocente = user.nombres + " " + user.apellidos, nombrejefe = "SIN JEFE ASIGNADO", fechaevaluacion = fecha_actual, periodoanio = (int)PeriodoSeleccionado.anio, periodonum = (int)PeriodoSeleccionado.numeroperiodo };
            }

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

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowLabores(int idusuario)
        {
            ViewBag.optionmenu = 1;
            ViewBag.reporte = crearReporte(idusuario);
            return View();
        }

        public ConsolidadoLabores setLabor(List<int> lista)
        {
            gestion gestion;
            social social;
            investigacion investigacion;
            trabajodegrado trabajoDeGrado;
            trabajodegradoinvestigacion trabajoDeGradoInvestigacion;
            desarrolloprofesoral desarrolloProfesoral;
            docencia docencia;
            otras otra;
            ConsolidadoLabores labores = new ConsolidadoLabores();

            labores.detalleslabores = new List<String>();
            labores.labSocial = new List<social>();
            labores.labGestion = new List<gestion>();
            labores.labDocencia = new List<DocenciaMateria>();
            labores.labInvestigacion = new List<investigacion>();
            labores.labTrabajoDeGrado = new List<trabajodegrado>();
            labores.labTrabajoDegradoInvestigacion = new List<trabajodegradoinvestigacion>();
            labores.labDesarrolloProfesoral = new List<desarrolloprofesoral>();
            labores.labOtras = new List<otras>();

            for (var i = 0; i < lista.Count; i++)
            {
                int id = lista[i];

                gestion = dbEntity.gestion.SingleOrDefault(g => g.idlabor == id);

                if (gestion != null)
                {
                    labores.labGestion.Add(gestion);
                    if (!labores.detalleslabores.Contains("Gestion"))
                        labores.detalleslabores.Add("Gestion");
                    continue;
                }

                social = dbEntity.social.SingleOrDefault(g => g.idlabor == id);

                if (social != null)
                {
                    labores.labSocial.Add(social);
                    if (!labores.detalleslabores.Contains("Social"))
                        labores.detalleslabores.Add("Social");
                    continue;
                }

                investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == id);

                if (investigacion != null)
                {
                    labores.labInvestigacion.Add(investigacion);
                    if (!labores.detalleslabores.Contains("Investigación"))
                        labores.detalleslabores.Add("Investigación");
                    continue;
                }

                trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == id);

                if (trabajoDeGrado != null)
                {
                    labores.labTrabajoDeGrado.Add(trabajoDeGrado);
                    if (!labores.detalleslabores.Contains("Trabajo de Grado"))
                        labores.detalleslabores.Add("Trabajo de Grado");
                    continue;
                }

                trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == id);

                if (trabajoDeGradoInvestigacion != null)
                {
                    labores.labTrabajoDegradoInvestigacion.Add(trabajoDeGradoInvestigacion);
                    if (!labores.detalleslabores.Contains("Trabajo de Grado Investigación"))
                        labores.detalleslabores.Add("Trabajo de Grado Investigación");
                    continue;
                }

                desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == id);

                if (desarrolloProfesoral != null)
                {
                    labores.labDesarrolloProfesoral.Add(desarrolloProfesoral);
                    if (!labores.detalleslabores.Contains("Desarrollo Profesoral"))
                        labores.detalleslabores.Add("Desarrollo Profesoral");
                    continue;
                }

                docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == id);

                if (docencia != null)
                {
                    DocenciaMateria aux = new DocenciaMateria();
                    aux.DocMateria = dbEntity.materia.SingleOrDefault(g => g.idmateria == docencia.idmateria);
                    aux.MatPrograma = dbEntity.programa.SingleOrDefault(p => p.idprograma == aux.DocMateria.idprograma);
                    aux.labDoc = docencia;
                    labores.labDocencia.Add(aux);
                    if (!labores.detalleslabores.Contains("Docencia Directa"))
                        labores.detalleslabores.Add("Docencia Directa");

                    continue;
                }

                otra = dbEntity.otras.SingleOrDefault(g => g.idlabor == id);

                if (otra != null)
                {
                    labores.labOtras.Add(otra);
                    if (!labores.detalleslabores.Contains("Otra"))
                        labores.detalleslabores.Add("Otra");
                    continue;
                }
            }

            return labores;
        }

        public ActionResult ExportToExcelLabores()
        {
            SpreadsheetModelLabores mySpreadsheetAE = new SpreadsheetModelLabores();
            String[] datos = new String[32];

            datos[0] = "Docente";
            datos[1] = "ID Labor";
            datos[2] = "Tipo Corto";
            datos[3] = "Tipo";
            datos[4] = "Labor";
            datos[5] = "Nota";
            datos[6] = "Descripción Problema";
            datos[7] = "Solución Problema";
            datos[8] = "Descripción Solucion";
            datos[9] = "Ubicación Solucion";
            //Gestion
            datos[10] = "Nombre Cargo";
            datos[11] = "Horas Semana";
            datos[12] = "Semanas Laborales";
            datos[13] = "Unidad";
            //Social
            datos[14] = "Resolución";
            datos[15] = "Nombre Proyecto";
            datos[16] = "Fecha Inicio";
            datos[17] = "Fecha Fin";
            //Desarrollo Profesoral
            datos[18] = "Nombre Actividad";
            //Docencia
            datos[19] = "Grupo";
            datos[20] = "ID Materia";
            //Materia
            datos[21] = "Código Materia";
            datos[22] = "Nombre Materia";
            datos[23] = "Créditos";
            datos[24] = "Semestre";
            datos[25] = "Intensidad Horaria";
            datos[26] = "Tipo";
            datos[27] = "Programa";
            //Investigación
            datos[28] = "Código VRI";
            //Trabajo Grado
            datos[29] = "Título De Trabajo";
            datos[30] = "Código Estudiante";
            //Otra
            datos[31] = "Descripcion";

            mySpreadsheetAE.fechaevaluacion = ReporteLabores.fechaevaluacion;
            mySpreadsheetAE.periodo = "" + ReporteLabores.periodonum + " - " + ReporteLabores.periodoanio;
            mySpreadsheetAE.nombrejefe = ReporteLabores.nombrejefe;
            mySpreadsheetAE.datos = datos;
            mySpreadsheetAE.detalleslabores = new List<string>();
            mySpreadsheetAE.detalleslabores.AddRange(ReporteLabores.detalleslabores);
            mySpreadsheetAE.labSocial = new List<social>();
            mySpreadsheetAE.labDesarrolloProfesoral = new List<desarrolloprofesoral>();
            mySpreadsheetAE.labGestion = new List<gestion>();
            mySpreadsheetAE.labDocencia = new List<DocenciaMateria>();
            mySpreadsheetAE.labInvestigacion = new List<investigacion>();
            mySpreadsheetAE.labTrabajoDeGrado = new List<trabajodegrado>();
            mySpreadsheetAE.labTrabajoDegradoInvestigacion = new List<trabajodegradoinvestigacion>();
            mySpreadsheetAE.labOtras = new List<otras>();

            foreach (string tipo in ReporteLabores.detalleslabores)
            {
                switch (tipo)
                {

                    case "Gestion":
                        for (int j = 0; j < ReporteLabores.labGestion.Count(); j++)
                        {
                            gestion aux = new gestion();

                            aux.idlabor = ReporteLabores.labGestion.ElementAt(j).idlabor;
                            aux.nombrecargo = ReporteLabores.labGestion.ElementAt(j).nombrecargo;
                            aux.horassemana = ReporteLabores.labGestion.ElementAt(j).horassemana;
                            aux.semanaslaborales = ReporteLabores.labGestion.ElementAt(j).semanaslaborales;
                            aux.unidad = ReporteLabores.labGestion.ElementAt(j).unidad;
                            mySpreadsheetAE.labGestion.Add(aux);
                        }
                        break;

                    case "Social":
                        for (int j = 0; j < ReporteLabores.labSocial.Count(); j++)
                        {
                            social aux = new social();

                            aux.idlabor = ReporteLabores.labSocial.ElementAt(j).idlabor;
                            aux.nombreproyecto = ReporteLabores.labSocial.ElementAt(j).nombreproyecto;
                            aux.resolucion = ReporteLabores.labSocial.ElementAt(j).resolucion;
                            aux.horassemana = ReporteLabores.labSocial.ElementAt(j).horassemana;
                            aux.unidad = ReporteLabores.labSocial.ElementAt(j).unidad;
                            aux.semanaslaborales = ReporteLabores.labSocial.ElementAt(j).semanaslaborales;
                            aux.fechafin = ReporteLabores.labSocial.ElementAt(j).fechafin;
                            aux.fechainicio = ReporteLabores.labSocial.ElementAt(j).fechainicio;
                            mySpreadsheetAE.labSocial.Add(aux);
                        }
                        break;

                    case "Desarrollo Profesoral":
                        for (int j = 0; j < ReporteLabores.labDesarrolloProfesoral.Count(); j++)
                        {
                            desarrolloprofesoral aux = new desarrolloprofesoral();

                            aux.idlabor = ReporteLabores.labDesarrolloProfesoral.ElementAt(j).idlabor;
                            aux.nombreactividad = ReporteLabores.labDesarrolloProfesoral.ElementAt(j).nombreactividad;
                            aux.resolucion = ReporteLabores.labDesarrolloProfesoral.ElementAt(j).resolucion;
                            aux.horassemana = ReporteLabores.labDesarrolloProfesoral.ElementAt(j).horassemana;
                            aux.semanaslaborales = ReporteLabores.labDesarrolloProfesoral.ElementAt(j).semanaslaborales;
                            mySpreadsheetAE.labDesarrolloProfesoral.Add(aux);
                        }
                        break;

                    case "Docencia Directa":
                        for (int j = 0; j < ReporteLabores.labDocencia.Count(); j++)
                        {
                            DocenciaMateria aux = new DocenciaMateria();
                            aux.DocMateria = new materia();
                            aux.labDoc = new docencia();
                            aux.MatPrograma = new programa();

                            aux.labDoc.idlabor = ReporteLabores.labDocencia.ElementAt(j).labDoc.idlabor;
                            aux.labDoc.idmateria = ReporteLabores.labDocencia.ElementAt(j).labDoc.idmateria;
                            aux.labDoc.grupo = ReporteLabores.labDocencia.ElementAt(j).labDoc.grupo;
                            aux.labDoc.horassemana = ReporteLabores.labDocencia.ElementAt(j).labDoc.horassemana;
                            aux.labDoc.semanaslaborales = ReporteLabores.labDocencia.ElementAt(j).labDoc.semanaslaborales;
                            aux.MatPrograma.idprograma = ReporteLabores.labDocencia.ElementAt(j).MatPrograma.idprograma;
                            aux.MatPrograma.nombreprograma = ReporteLabores.labDocencia.ElementAt(j).MatPrograma.nombreprograma;
                            aux.DocMateria.idmateria = ReporteLabores.labDocencia.ElementAt(j).DocMateria.idmateria;
                            aux.DocMateria.nombremateria = ReporteLabores.labDocencia.ElementAt(j).DocMateria.nombremateria;
                            aux.DocMateria.codigomateria = ReporteLabores.labDocencia.ElementAt(j).DocMateria.codigomateria;
                            aux.DocMateria.creditos = ReporteLabores.labDocencia.ElementAt(j).DocMateria.creditos;
                            aux.DocMateria.grupo = ReporteLabores.labDocencia.ElementAt(j).DocMateria.grupo;
                            aux.DocMateria.intensidadhoraria = ReporteLabores.labDocencia.ElementAt(j).DocMateria.intensidadhoraria;
                            aux.DocMateria.semestre = ReporteLabores.labDocencia.ElementAt(j).DocMateria.semestre;
                            aux.DocMateria.tipo = ReporteLabores.labDocencia.ElementAt(j).DocMateria.tipo;
                            mySpreadsheetAE.labDocencia.Add(aux);
                        }
                        break;

                    case "Investigacion":
                        for (int j = 0; j < ReporteLabores.labInvestigacion.Count(); j++)
                        {
                            investigacion aux = new investigacion();

                            aux.idlabor = ReporteLabores.labInvestigacion.ElementAt(j).idlabor;
                            aux.nombreproyecto = ReporteLabores.labInvestigacion.ElementAt(j).nombreproyecto;
                            aux.codigovri = ReporteLabores.labInvestigacion.ElementAt(j).codigovri;
                            aux.horassemana = ReporteLabores.labInvestigacion.ElementAt(j).horassemana;
                            aux.semanaslaborales = ReporteLabores.labInvestigacion.ElementAt(j).semanaslaborales;
                            aux.fechafin = ReporteLabores.labInvestigacion.ElementAt(j).fechafin;
                            aux.fechainicio = ReporteLabores.labInvestigacion.ElementAt(j).fechainicio;
                            mySpreadsheetAE.labInvestigacion.Add(aux);
                        }
                        break;

                    case "Trabajo de Grado":
                        for (int j = 0; j < ReporteLabores.labTrabajoDeGrado.Count(); j++)
                        {
                            trabajodegrado aux = new trabajodegrado();

                            aux.idlabor = ReporteLabores.labTrabajoDeGrado.ElementAt(j).idlabor;
                            aux.titulotrabajo = ReporteLabores.labTrabajoDeGrado.ElementAt(j).titulotrabajo;
                            aux.resolucion = ReporteLabores.labTrabajoDeGrado.ElementAt(j).resolucion;
                            aux.horassemana = ReporteLabores.labTrabajoDeGrado.ElementAt(j).horassemana;
                            aux.semanaslaborales = ReporteLabores.labTrabajoDeGrado.ElementAt(j).semanaslaborales;
                            aux.fechafin = ReporteLabores.labTrabajoDeGrado.ElementAt(j).fechafin;
                            aux.fechainicio = ReporteLabores.labTrabajoDeGrado.ElementAt(j).fechainicio;
                            aux.codigoest = ReporteLabores.labTrabajoDeGrado.ElementAt(j).codigoest;
                            mySpreadsheetAE.labTrabajoDeGrado.Add(aux);
                        }
                        break;

                    case "Trabajo de Grado Investigación":
                        for (int j = 0; j < ReporteLabores.labTrabajoDegradoInvestigacion.Count(); j++)
                        {
                            trabajodegradoinvestigacion aux = new trabajodegradoinvestigacion();

                            aux.idlabor = ReporteLabores.labTrabajoDegradoInvestigacion.ElementAt(j).idlabor;
                            aux.titulotrabajo = ReporteLabores.labTrabajoDegradoInvestigacion.ElementAt(j).titulotrabajo;
                            aux.horassemana = ReporteLabores.labTrabajoDegradoInvestigacion.ElementAt(j).horassemana;
                            aux.semanaslaborales = ReporteLabores.labTrabajoDegradoInvestigacion.ElementAt(j).semanaslaborales;
                            aux.codigoest = ReporteLabores.labTrabajoDegradoInvestigacion.ElementAt(j).codigoest;
                            mySpreadsheetAE.labTrabajoDegradoInvestigacion.Add(aux);
                        }
                        break;

                    case "Otra":
                        for (int j = 0; j < ReporteLabores.labOtras.Count(); j++)
                        {
                            otras aux = new otras();

                            aux.idlabor = ReporteLabores.labOtras.ElementAt(j).idlabor;
                            aux.descripcion = ReporteLabores.labOtras.ElementAt(j).descripcion;
                            aux.horassemana = ReporteLabores.labOtras.ElementAt(j).horassemana;
                            mySpreadsheetAE.labOtras.Add(aux);
                        }
                        break;
                }
            }

            periodo_academico lastAcademicPeriod = GetLastAcademicPeriod();
            DateTime Hora = DateTime.Now;
            DateTime Hoy = DateTime.Today;
            string hora = Hora.ToString("HH:mm");
            string hoy = Hoy.ToString("dd-MM");
            mySpreadsheetAE.fileName = "Reporte" + lastAcademicPeriod.anio + "-" + lastAcademicPeriod.idperiodo + "_" + hoy + "_" + hora + ".xls";
            return View(mySpreadsheetAE);
        }

        public int obtenerFilasRM()
        {
            int filasReporte = 0;
            foreach (ConsolidadoDocencia filas in ReporteMaterias)
            {
                filasReporte = filasReporte + filas.labDocencia.Count() + 1;
            }
            return filasReporte;
        }

        #endregion

        public class DocenteDepto
        {
            public String nombre;
            public String apellido;
            public String estado;
            public int iddetp;
            public int iddocente;
            public int idususaio;
            public String jefe;
        }

        public class Lab
        {
            public int id;
            public String tipo;
        }

        public class evaluaciones
        {
            public int idevaluacion;
            public int idautoevaluacion;
        }

        public class usuDoc
        {
            public int id;
            public string nombre;
            public int idlab;
        }

        #endregion

        /******FIN ADICIONADO POR CLARA *******/

        #region Importar Datos De SIMCA

        [Authorize]
        public ActionResult ImportDataSIMCA()
        {
            ViewBag.optionmenu = 3;
            return View(new List<DocenteMateria>());
        }

        [Authorize]
        [HttpPost]
        public ActionResult ImportDataSIMCA(HttpPostedFileBase excelfile)
        {
            ViewBag.optionmenu = 3;
            var guardados = new List<DocenteMateria>();
            var noguardados = new List<DocenteMateria>();
            var vistaDocenteMateria = new List<DocenteMateria>();
            var tablaDatosSIMCA = new List<DocenteMateria>();
            if (excelfile != null)
            {
                String extension = Path.GetExtension(excelfile.FileName);
                String filename = Path.GetFileName(excelfile.FileName);
                String filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(excelfile.FileName));

                if (extension == ".xls" || extension == ".xlsx")
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    excelfile.SaveAs(filePath);
                    try
                    {
                        string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Password=\"\";User ID=Admin;Data Source=" + filePath.ToString() + ";Mode=Share Deny Write;Extended Properties=\"HDR=YES;\";Jet OLEDB:Engine Type=37";
                        System.Data.OleDb.OleDbConnection oconn = new System.Data.OleDb.OleDbConnection(connectionString);

                        oconn.Open();
                        string sInvalidas;
                        bool valido = ExcelValidation(oconn, out sInvalidas, 1);

                        if (valido)
                        {
                            OleDbDataAdapter dataAdapter = new OleDbDataAdapter("SELECT * FROM [Docentes$]", oconn);
                            DataSet myDataSet = new DataSet();
                            dataAdapter.Fill(myDataSet, "Docentes");
                            DataTable dataTable = myDataSet.Tables["Docentes"];
                            var rows = from p in dataTable.AsEnumerable()

                                       select new
                                       {
                                           codMateria = p[0],
                                           nombreMateria = p[1],
                                           grupo = p[2],
                                           identificacion = p[3],
                                           total = p[4],
                                       };

                            var id = new List<int>();

                            // modificado febrero 4

                            int currentper = (int)SessionValue("currentAcadPeriod");

                            vistaDocenteMateria = (from u in dbEntity.usuario
                                                   join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                   join p in dbEntity.participa on d.iddocente equals p.iddocente
                                                   join lab in dbEntity.labor on p.idlabor equals lab.idlabor
                                                   join e in dbEntity.evaluacion on p.idevaluacion equals e.idevaluacion
                                                   join doc in dbEntity.docencia on p.idlabor equals doc.idlabor
                                                   join mat in dbEntity.materia on doc.idmateria equals mat.idmateria
                                                   where lab.idperiodo.Equals(currentper)
                                                   select new DocenteMateria { identificacion = u.identificacion, nombres = u.nombres, apellidos = u.apellidos, email = u.emailinstitucional, nombremateria = mat.nombremateria, codigomateria = mat.codigomateria, grupo = mat.grupo, ideval = e.idevaluacion }).ToList();

                            Debug.WriteLine("filas " + rows.Count());
                            foreach (var row in rows)
                            {
                                DocenteMateria dm = new DocenteMateria();
                                string identificacion = row.identificacion.ToString();
                                string codigo = row.codMateria.ToString();
                                string nombre = row.nombreMateria.ToString();
                                string grupo = row.grupo.ToString();
                                string total = row.total.ToString();

                                if (identificacion != "" && codigo != "" && nombre != "" && grupo != "" && total != "")
                                {

                                    dm.identificacion = identificacion;
                                    dm.codigomateria = codigo;
                                    dm.nombremateria = nombre;
                                    dm.grupo = grupo;
                                    try
                                    {
                                        dm.evalest = Convert.ToInt16(total);
                                    }
                                    catch(Exception ex)
                                    {
                                        Utilities.ManageException(ex, Server.MapPath(@"..\generalLog.txt"));
                                        
                                        dm.evalest = 0;
                                    }
                                    dm.guardado = 0;
                                    tablaDatosSIMCA.Add(dm);
                                }
                            }
                            int cont = 0;
                            foreach (DocenteMateria ds in tablaDatosSIMCA)
                            {
                                foreach (DocenteMateria dv in vistaDocenteMateria)
                                {
                                    try
                                    {
                                        if (ds.identificacion == dv.identificacion)
                                        {
                                            if (ds.codigomateria == dv.codigomateria && ds.grupo == dv.grupo)
                                            {
                                                evaluacion eval = dbEntity.evaluacion.Single(q => q.idevaluacion == dv.ideval);
                                                eval.evaluacionestudiante = ds.evalest;
                                                dv.evalest = ds.evalest;
                                                dbEntity.SaveChanges();
                                                ds.grupo = dv.grupo;
                                                ds.identificacion = dv.identificacion;
                                                ds.nombres = dv.nombres;
                                                ds.apellidos = dv.apellidos;
                                                ds.nombremateria = dv.nombremateria;
                                                ds.guardado = 1;
                                            }
                                            else
                                            {
                                                cont++;
                                            }
                                        }
                                    }
                                    catch { }
                                }
                                if (ds.guardado == 0)
                                {
                                    cont++;
                                }
                            }
                        }
                        else
                        {
                            ViewBag.Error += "El formato de archivo no es valido";
                        }

                        oconn.Close();
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        Utilities.ManageException(ex, Server.MapPath(@"..\generalLog.txt"));
                        ViewBag.Error = "No se pudo abrir el archivo, consulte el formato del mismo";
                    }
                }
                else
                {
                    ViewBag.Error = "Solo se admiten archivos excel .xls o .xlsx";
                }
            }
            else
            {
                ViewBag.Error = "No Recibido";
            }
            return View(tablaDatosSIMCA);
        }

        #endregion

        //#region Importar Datos de Evaluacion desde EXCEL

        //[Authorize]
        //public ActionResult ImportDataEvaluations()
        //{
        //    ViewBag.optionmenu = 5;
        //    return View(new List<savedDataSIGELA>());
        //}
        //#endregion

        #region Importar Datos De SIGELA  MODIFICADO POR AMBOS

        [Authorize]
        public ActionResult ImportDataSIGELA()
        {
            ViewBag.optionmenu = 3;
            return View(new List<savedDataSIGELA>());
        }

        public List<savedDataSIGELA> ProcessImportDataSIGELA(HttpPostedFileBase excelfile,
            int periodId,ref string sError,ref string FileValid,ref int LabsReg, ref int TeacReg)
        {
            string connectionString = "";
            System.Data.OleDb.OleDbConnection oconn = new OleDbConnection();
            List<savedDataSIGELA> savedsigela = new List<savedDataSIGELA>();

            if (excelfile != null)
            {
                String extension = Path.GetExtension(excelfile.FileName);
                String filename = Path.GetFileName(excelfile.FileName);
                String filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                if (extension == ".xls" || extension == ".xlsx")
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    excelfile.SaveAs(filePath);

                    try
                    {
                         connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Password=\"\";User ID=Admin;Data Source=" + filePath.ToString() + ";Mode=Share Deny Write;Extended Properties=\"HDR=YES;\";Jet OLEDB:Engine Type=37";

                        oconn = new System.Data.OleDb.OleDbConnection(connectionString);
                        oconn.Open();
                        string sInvalidas = "";
                        bool valido = ExcelValidation(oconn, out sInvalidas);

                        if (valido)
                        {
                            int currentper = periodId;
                            List<docenteSIGELA> docs_sig = this.data_imports.getDocentes(); //Obtener la lista de docentes desde excel edv
                            List<docenteSIGELA> saveddtes = TeacherImportSave(docs_sig);

                            savedsigela = TeachingImportSave(this.data_imports.getDocencias(), currentper);
                            savedsigela = savedsigela.Concat(TeacherProfDevImportSave(this.data_imports.getDlloProfesoral(), currentper)).ToList();
                            savedsigela = savedsigela.Concat(ResearchGradeWorkImportSave(this.data_imports.getTrabajoGdoInv(), currentper)).ToList();
                            savedsigela = savedsigela.Concat(GradeWorkImportSave(this.data_imports.getTrabajoGdo(), currentper)).ToList();
                            savedsigela = savedsigela.Concat(ResearchImportSave(this.data_imports.getInvestigacion(), currentper)).ToList();
                            savedsigela = savedsigela.Concat(SocialImportSave(this.data_imports.getSocial(), currentper)).ToList();
                            savedsigela = savedsigela.Concat(ManagementImportSave(this.data_imports.getGestion(), currentper)).ToList();

                            // nuevo febrero 3
                            savedsigela = savedsigela.Concat(OtrasImportSave(this.data_imports.getOtras(), currentper)).ToList();

                            FileValid = "El archivo " + filename + " recibido es valido";
                            LabsReg = savedsigela.Count();
                            TeacReg = docs_sig.Count();


                        }
                        else
                        {
                            sError = "Excel no valido. Revise las hojas: " + sInvalidas;
                        }

                        oconn.Close();
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        if (oconn.State != ConnectionState.Closed)
                        {
                            oconn.Close();
                        }
                        sError = "No se pudo abrir el archivo de excel " + ex;
                    }
                }
                else
                {
                    sError = "Solo se admiten archivos excel .xls o .xlsx";
                }
            }
            else
            {
                sError = "Archivo no recibido";
            }
            return savedsigela;
        }

        // modificado 3 febrero
        [Authorize]
        [HttpPost]
        public ActionResult ImportDataSIGELA(HttpPostedFileBase excelfile)
        {
            ViewBag.optionmenu = 5;
            List<savedDataSIGELA> oResult;
            string sError = "";
            string sFileValid = "";
            int LabsReg = 0;
            int TeacReg = 0;
            oResult = ProcessImportDataSIGELA(excelfile, (int)Session["currentAcadPeriod"], ref sError, ref sFileValid, ref LabsReg, ref TeacReg);
            ViewBag.FileValid = sFileValid;
            ViewBag.Labsreg = LabsReg;
            ViewBag.Teacreg = TeacReg;
            if (sError != "")
                ViewBag.Error = sError;
            return View(oResult);
        }
        // modificado febrero 3
        [Authorize]
        public List<savedDataSIGELA> ManagementImportSave(List<gestionSIGELA> list, int currentper)
        {
            List<savedDataSIGELA> savedManage = new List<savedDataSIGELA>();
            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();

            foreach (gestionSIGELA man in list)
            {
                try
                {
                    int iddoc = GetIdDocenteByIdentification(man.identificacion);
                    docente undocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == iddoc);
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == undocente.idusuario);

                    var docenteGes = (from d in dbEntity.gestion
                                      join p in dbEntity.participa on d.idlabor equals p.idlabor
                                      join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                      join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                      join usu in dbEntity.usuario on doc.idusuario equals usu.idusuario
                                      where d.nombrecargo.Equals(man.nombrecargo)
                                      where lab.idperiodo.Equals(currentper)
                                      where usu.identificacion.Equals(man.identificacion)
                                      where d.unidad.Equals(man.unidad)
                                      select new gestionSIGELA { nombrecargo = d.nombrecargo }).Distinct().ToList();


                    //Para verificar que no ingresen una labor si esta ya existe            
                    if (docenteGes.Count() == 0)
                    {
                        try
                        {
                            labor unalabor = new labor() { idperiodo = currentper };
                            dbEntity.labor.AddObject(unalabor);
                            dbEntity.SaveChanges();
                            gestion ungestion = new gestion() { idlabor = unalabor.idlabor, horassemana = man.horassemana, semanaslaborales = man.semanaslaborales, unidad = man.unidad, nombrecargo = man.nombrecargo };
                            dbEntity.gestion.AddObject(ungestion);
                            dbEntity.SaveChanges();
                            evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                            dbEntity.evaluacion.AddObject(unaevaluacion);
                            dbEntity.SaveChanges();
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
                            //FIN ADICIONADO POR CLARA
                            participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                            dbEntity.participa.AddObject(unparticipa);
                            dbEntity.SaveChanges();
                            savedManage.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Gestion", nombrelabor = man.nombrecargo });

                        }
                        catch (Exception ex) { }
                    }
                    if (docenteGes.Count() > 0)
                    {

                        var docenteGesEsta = (from d in dbEntity.gestion
                                              join p in dbEntity.participa on d.idlabor equals p.idlabor
                                              join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                              join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                              where d.nombrecargo.Equals(man.nombrecargo)
                                              where lab.idperiodo.Equals(currentper)
                                              where p.iddocente.Equals(iddoc)
                                              select new gestionSIGELA { nombrecargo = d.nombrecargo }).Distinct().ToList();

                        if (docenteGesEsta.Count() == 0)
                        {

                            try
                            {
                                //labor unalabor = new labor() { idperiodo = currentper };
                                //dbEntity.labor.AddObject(unalabor);
                                //dbEntity.SaveChanges();
                                //gestion ungestion = new gestion() { idlabor = unalabor.idlabor, horassemana = man.horassemana, semanaslaborales = man.semanaslaborales, unidad = man.unidad, nombrecargo = man.nombrecargo };
                                //dbEntity.gestion.AddObject(ungestion);
                                //dbEntity.SaveChanges();
                                evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                                dbEntity.evaluacion.AddObject(unaevaluacion);
                                dbEntity.SaveChanges();
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
                                //FIN ADICIONADO POR CLARA


                                // nuevo febrero 3 
                                var maxid = (from pa in dbEntity.gestion
                                             where pa.nombrecargo.Equals(man.nombrecargo)
                                             select pa.idlabor).Max();

                                gestion unalabor = dbEntity.gestion.FirstOrDefault(gt => gt.idlabor == maxid && gt.nombrecargo == man.nombrecargo);


                                participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                                dbEntity.participa.AddObject(unparticipa);
                                dbEntity.SaveChanges();
                                savedManage.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Gestion", nombrelabor = man.nombrecargo });

                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                catch { }
            }
            return savedManage;
        }

        // nuevo febrero 3
        [Authorize]
        public List<savedDataSIGELA> OtrasImportSave(List<otrasSIGELA> list, int currentper)
        {
            List<savedDataSIGELA> savedManage = new List<savedDataSIGELA>();
            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();
            // aqui voy 
            foreach (otrasSIGELA man in list)
            {
                try
                {
                    int iddoc = GetIdDocenteByIdentification(man.identificacion);
                    docente undocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == iddoc);
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == undocente.idusuario);

                    var otrasVar = (from d in dbEntity.otras
                                    join p in dbEntity.participa on d.idlabor equals p.idlabor
                                    join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                    join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                    join usu in dbEntity.usuario on doc.idusuario equals usu.idusuario
                                    where usu.identificacion.Equals(man.identificacion)
                                    where d.descripcion.Equals(man.descripcion)
                                    where lab.idperiodo.Equals(currentper)
                                    select new otrasSIGELA { descripcion = d.descripcion }).Distinct().ToList();

                    //Para verificar que no ingresen una labor si esta ya existe            
                    if (otrasVar.Count() == 0)
                    {
                        try
                        {
                            labor unalabor = new labor() { idperiodo = currentper };
                            dbEntity.labor.AddObject(unalabor);
                            dbEntity.SaveChanges();
                            otras unOtra = new otras() { idlabor = unalabor.idlabor, descripcion = man.descripcion, horassemana = man.horassemana };
                            dbEntity.otras.AddObject(unOtra);
                            dbEntity.SaveChanges();
                            evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                            dbEntity.evaluacion.AddObject(unaevaluacion);
                            dbEntity.SaveChanges();
                            
                            autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                            dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                            dbEntity.SaveChanges();
                            problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                            dbEntity.problema.AddObject(unproblema);
                            dbEntity.SaveChanges();
                            resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                            dbEntity.resultado.AddObject(unresultado);
                            dbEntity.SaveChanges();
                            
                            participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                            dbEntity.participa.AddObject(unparticipa);
                            dbEntity.SaveChanges();
                            savedManage.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Otras", nombrelabor = man.descripcion });

                        }
                        catch (Exception ex) { }
                    }
                    if (otrasVar.Count() > 0)
                    {

                        var otrasVarEsta = (from d in dbEntity.otras
                                            join p in dbEntity.participa on d.idlabor equals p.idlabor
                                            join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                            join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                            where d.descripcion.Equals(man.descripcion)
                                            where lab.idperiodo.Equals(currentper)
                                            where doc.iddocente.Equals(iddoc)
                                            select new otrasSIGELA { descripcion = d.descripcion }).Distinct().ToList();

                        if (otrasVarEsta.Count() == 0)
                        {

                            try
                            {

                                //labor unalabor = new labor() { idperiodo = currentper };
                                //dbEntity.labor.AddObject(unalabor);
                                //dbEntity.SaveChanges();
                                //gestion ungestion = new gestion() { idlabor = unalabor.idlabor, horassemana = man.horassemana, semanaslaborales = man.semanaslaborales, unidad = man.unidad, nombrecargo = man.nombrecargo };
                                //dbEntity.gestion.AddObject(ungestion);
                                //dbEntity.SaveChanges();
                                evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                                dbEntity.evaluacion.AddObject(unaevaluacion);
                                dbEntity.SaveChanges();
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
                                //FIN ADICIONADO POR CLARA


                                // nuevo febrero 3 
                                var maxid = (from pa in dbEntity.otras
                                             where pa.descripcion.Equals(man.descripcion)
                                             select pa.idlabor).Max();

                                otras unalabor = dbEntity.otras.FirstOrDefault(gt => gt.idlabor == maxid && gt.descripcion == man.descripcion);

                                participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                                dbEntity.participa.AddObject(unparticipa);
                                dbEntity.SaveChanges();
                                savedManage.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Otras", nombrelabor = man.descripcion });

                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                catch { }
            }
            return savedManage;
        }

        // modificado febrero 3
        [Authorize]
        public List<savedDataSIGELA> SocialImportSave(List<socialSIGELA> list, int currentper)
        {
            List<savedDataSIGELA> savedSocial = new List<savedDataSIGELA>();

            foreach (socialSIGELA soc in list)
            {
                try
                {
                    int iddoc = GetIdDocenteByIdentification(soc.identificacion);
                    docente undocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == iddoc);
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == undocente.idusuario);
                    var docenteSoc = (from d in dbEntity.social
                                      join p in dbEntity.participa on d.idlabor equals p.idlabor
                                      join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                      join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                      join usu in dbEntity.usuario on doc.idusuario equals usu.idusuario
                                      where d.nombreproyecto.Equals(soc.nombreproyecto)
                                      where lab.idperiodo.Equals(currentper)
                                      where d.resolucion.Equals(soc.resolucion)
                                      where usu.identificacion.Equals(soc.identificacion)
                                      where d.unidad.Equals(soc.unidad)
                                      select new socialSIGELA { nombreproyecto = d.nombreproyecto, resolucion = d.resolucion }).Distinct().ToList();


                    //Para verificar que no ingresen una labor si esta ya existe            
                    if (docenteSoc.Count() == 0)
                    {
                        try
                        {
                            labor unalabor = new labor() { idperiodo = currentper };
                            dbEntity.labor.AddObject(unalabor);
                            dbEntity.SaveChanges();
                            social unsocial = new social() { idlabor = unalabor.idlabor, horassemana = soc.horassemana, semanaslaborales = soc.semanaslaborales, resolucion = soc.resolucion, unidad = soc.unidad, fechainicio = DateTime.Parse(soc.fechainicio), fechafin = DateTime.Parse(soc.fechafin), nombreproyecto = soc.nombreproyecto };
                            dbEntity.social.AddObject(unsocial);
                            dbEntity.SaveChanges();
                            evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                            dbEntity.evaluacion.AddObject(unaevaluacion);
                            dbEntity.SaveChanges();
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
                            //FIN ADICIONADO POR CLARA
                            participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                            dbEntity.participa.AddObject(unparticipa);
                            dbEntity.SaveChanges();
                            savedSocial.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Social", nombrelabor = soc.nombreproyecto });
                        }
                        catch (Exception ex) { }
                    }
                    if (docenteSoc.Count() > 0)
                    {

                        var docenteSocEsta = (from d in dbEntity.social
                                              join p in dbEntity.participa on d.idlabor equals p.idlabor
                                              join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                              join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                              where d.nombreproyecto.Equals(soc.nombreproyecto)
                                              where lab.idperiodo.Equals(currentper)
                                              where p.iddocente.Equals(iddoc)
                                              select new socialSIGELA { nombreproyecto = d.nombreproyecto, resolucion = d.resolucion }).Distinct().ToList();
                        if (docenteSocEsta.Count() == 0)
                        {

                            try
                            {
                                //labor unalabor = new labor() { idperiodo = currentper };
                                //dbEntity.labor.AddObject(unalabor);
                                //dbEntity.SaveChanges();
                                //social unsocial = new social() { idlabor = unalabor.idlabor, horassemana = soc.horassemana, semanaslaborales = soc.semanaslaborales, resolucion = soc.resolucion, unidad = soc.unidad, fechainicio = DateTime.Parse(soc.fechainicio), fechafin = DateTime.Parse(soc.fechafin), nombreproyecto = soc.nombreproyecto };
                                //dbEntity.social.AddObject(unsocial);
                                //dbEntity.SaveChanges();
                                evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                                dbEntity.evaluacion.AddObject(unaevaluacion);
                                dbEntity.SaveChanges();
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
                                //FIN ADICIONADO POR CLARA


                                // nuevo febrero 3
                                var maxid = (from pa in dbEntity.social
                                             where pa.nombreproyecto.Equals(soc.nombreproyecto)
                                             select pa.idlabor).Max();

                                social unalabor = dbEntity.social.FirstOrDefault(sc => sc.idlabor == maxid && sc.nombreproyecto == soc.nombreproyecto);

                                participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                                dbEntity.participa.AddObject(unparticipa);
                                dbEntity.SaveChanges();
                                savedSocial.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Social", nombrelabor = soc.nombreproyecto });
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                catch { }
            }
            return savedSocial;
        }
        // modificado febrero 3
        [Authorize]
        public List<savedDataSIGELA> ResearchImportSave(List<investigacionSIGELA> list, int currentper)
        {
            List<savedDataSIGELA> savedResearch = new List<savedDataSIGELA>();

            foreach (investigacionSIGELA inv in list)
            {
                try
                {
                    int iddoc = GetIdDocenteByIdentification(inv.identificacion);
                    docente undocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == iddoc);
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == undocente.idusuario);
                    var docenteInv = (from d in dbEntity.investigacion
                                      join p in dbEntity.participa on d.idlabor equals p.idlabor
                                      join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                      join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                      join usu in dbEntity.usuario on doc.idusuario equals usu.idusuario
                                      where d.nombreproyecto.Equals(inv.nombreproyecto)
                                      where lab.idperiodo.Equals(currentper)
                                      where d.codigovri.Equals(inv.codigovri)
                                      where usu.identificacion.Equals(inv.identificacion)
                                      select new investigacionSIGELA { nombreproyecto = d.nombreproyecto, codigovri = d.codigovri }).Distinct().ToList();


                    //Para verificar que no ingresen una labor si esta ya existe            
                    if (docenteInv.Count() == 0)
                    {
                        try
                        {
                            labor unalabor = new labor() { idperiodo = currentper };
                            dbEntity.labor.AddObject(unalabor);
                            dbEntity.SaveChanges();
                            investigacion untrabajo = new investigacion() { idlabor = unalabor.idlabor, codigovri = inv.codigovri, horassemana = inv.horassemana, semanaslaborales = inv.semanaslaborales, fechainicio = DateTime.Parse(inv.fechainicio), fechafin = DateTime.Parse(inv.fechafin), nombreproyecto = inv.nombreproyecto };
                            dbEntity.investigacion.AddObject(untrabajo);
                            dbEntity.SaveChanges();
                            evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                            dbEntity.evaluacion.AddObject(unaevaluacion);
                            dbEntity.SaveChanges();
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
                            //FIN ADICIONADO POR CLARA
                            participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                            dbEntity.participa.AddObject(unparticipa);
                            dbEntity.SaveChanges();
                            savedResearch.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Investigación", nombrelabor = inv.nombreproyecto });
                        }
                        catch (Exception ex) { }
                    }
                    if (docenteInv.Count() > 0)
                    {

                        var docenteInvEsta = (from d in dbEntity.investigacion
                                              join p in dbEntity.participa on d.idlabor equals p.idlabor
                                              join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                              join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                              where d.nombreproyecto.Equals(inv.nombreproyecto)
                                              where lab.idperiodo.Equals(currentper)
                                              where p.iddocente.Equals(iddoc)
                                              select new investigacionSIGELA { nombreproyecto = d.nombreproyecto, codigovri = d.codigovri }).Distinct().ToList();

                        if (docenteInvEsta.Count() == 0)
                        {

                            try
                            {
                                //labor unalabor = new labor() { idperiodo = currentper };
                                //dbEntity.labor.AddObject(unalabor);
                                //dbEntity.SaveChanges();
                                //investigacion untrabajo = new investigacion() { idlabor = unalabor.idlabor, codigovri = inv.codigovri, horassemana = inv.horassemana, semanaslaborales = inv.semanaslaborales, fechainicio = DateTime.Parse(inv.fechainicio), fechafin = DateTime.Parse(inv.fechafin), nombreproyecto = inv.nombreproyecto };
                                //dbEntity.investigacion.AddObject(untrabajo);
                                //dbEntity.SaveChanges();
                                evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                                dbEntity.evaluacion.AddObject(unaevaluacion);
                                dbEntity.SaveChanges();
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
                                //FIN ADICIONADO POR CLARA


                                // nuevo febrero 3
                                var maxid = (from pa in dbEntity.investigacion
                                             where pa.nombreproyecto.Equals(inv.nombreproyecto)
                                             select pa.idlabor).Max();

                                investigacion unalabor = dbEntity.investigacion.FirstOrDefault(investig => investig.idlabor == maxid && investig.nombreproyecto == inv.nombreproyecto);

                                participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                                dbEntity.participa.AddObject(unparticipa);
                                dbEntity.SaveChanges();
                                savedResearch.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Investigación", nombrelabor = inv.nombreproyecto });
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                catch { }
            }
            return savedResearch;
        }
        // modificado febrero 3
        [Authorize]
        public List<savedDataSIGELA> GradeWorkImportSave(List<trabajoGdoSIGELA> list, int currentper)
        {
            List<savedDataSIGELA> savedGradeWork = new List<savedDataSIGELA>();
            foreach (trabajoGdoSIGELA tg in list)
            {
                try
                {
                    int iddoc = GetIdDocenteByIdentification(tg.identificacion);
                    docente undocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == iddoc);
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == undocente.idusuario);

                    var docenteTrabGrad = (from d in dbEntity.trabajodegrado
                                           join p in dbEntity.participa on d.idlabor equals p.idlabor
                                           join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                           join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                           join usu in dbEntity.usuario on doc.idusuario equals usu.idusuario
                                           where usu.identificacion.Equals(tg.identificacion)
                                           where d.titulotrabajo.Equals(tg.titulotrabajo)
                                           where lab.idperiodo.Equals(currentper)
                                           where d.resolucion.Equals(tg.resolucion)
                                           select new trabajoGdoSIGELA { titulotrabajo = d.titulotrabajo, resolucion = d.resolucion }).Distinct().ToList();


                    //Para verificar que no ingresen una labor si esta ya existe            
                    if (docenteTrabGrad.Count() == 0)
                    {
                        try
                        {
                            labor unalabor = new labor() { idperiodo = currentper };
                            dbEntity.labor.AddObject(unalabor);
                            dbEntity.SaveChanges();
                            int idprog = GetIdProgramaByName(tg.programa);
                            trabajodegrado untrabajo = new trabajodegrado() { idlabor = unalabor.idlabor, idprograma = idprog, codigoest = tg.codigoest, horassemana = tg.horassemana, semanaslaborales = tg.semanaslaborales, resolucion = tg.resolucion, titulotrabajo = tg.titulotrabajo };
                            dbEntity.trabajodegrado.AddObject(untrabajo);
                            dbEntity.SaveChanges();
                            evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                            dbEntity.evaluacion.AddObject(unaevaluacion);
                            dbEntity.SaveChanges();
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
                            //FIN ADICIONADO POR CLARA
                            participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                            dbEntity.participa.AddObject(unparticipa);
                            dbEntity.SaveChanges();
                            savedGradeWork.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Trabajo De Grado", nombrelabor = tg.titulotrabajo });

                            RegisterStudentForResearchGradeWork(untrabajo.codigoest, untrabajo.titulotrabajo);
                        }
                        catch (Exception ex) { }
                    }
                    if (docenteTrabGrad.Count() > 0)
                    {

                        var trabGradEsta = (from d in dbEntity.trabajodegrado
                                            join p in dbEntity.participa on d.idlabor equals p.idlabor
                                            join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                            join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                            where d.titulotrabajo.Equals(tg.titulotrabajo)
                                            where lab.idperiodo.Equals(currentper)
                                            where p.iddocente.Equals(iddoc)
                                            select new trabajoGdoSIGELA { titulotrabajo = d.titulotrabajo, resolucion = d.resolucion }).Distinct().ToList();

                        if (trabGradEsta.Count() == 0)
                        {
                            try
                            {
                                evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                                dbEntity.evaluacion.AddObject(unaevaluacion);
                                dbEntity.SaveChanges();
                                autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                                dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                                dbEntity.SaveChanges();
                                problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                                dbEntity.problema.AddObject(unproblema);
                                dbEntity.SaveChanges();
                                resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                                dbEntity.resultado.AddObject(unresultado);
                                dbEntity.SaveChanges();

                                var maxid = (from pa in dbEntity.trabajodegrado
                                             where pa.titulotrabajo.Equals(tg.titulotrabajo)
                                             select pa.idlabor).Max();

                                trabajodegrado unalabor = dbEntity.trabajodegrado.FirstOrDefault(tr => tr.idlabor == maxid && tr.titulotrabajo == tg.titulotrabajo);

                                participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                                dbEntity.participa.AddObject(unparticipa);
                                dbEntity.SaveChanges();
                                savedGradeWork.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Trabajo De Grado", nombrelabor = tg.titulotrabajo });

                                RegisterStudentForResearchGradeWork(unalabor.codigoest, unalabor.titulotrabajo);
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                catch { }
            }
            return savedGradeWork;
        }

        /// <summary>
        /// Obtiene nombres y apellidos de un nombre completo
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        public void SplitFullName(string fullName, ref string sNames, ref string sLastNames)
        {
            string[] lstNames = fullName.Trim().Split(' ');
            switch (lstNames.Length)
            {
                case 1:
                    sNames = lstNames[0];
                    break;
                case 2:
                    sNames = lstNames[0];
                    sLastNames = lstNames[1];
                    break;
                case 3:
                    sNames = lstNames[0];
                    sLastNames = lstNames[1] + ' ' + lstNames[2];
                    break;
                default:
                    sNames = lstNames[0] + ' ' + lstNames[1];
                    sLastNames = lstNames[2] + ' ' + lstNames[3];
                    break;
            }
        }

        /// <summary>
        /// Registra un estudiante para que sea el evaluador de la labor
        /// </summary>
        /// <param name="untrabajoinv"></param>
        private void RegisterStudentForResearchGradeWork(long codigoest, string titulotrabajo)
        {
            string sCodigoEst = codigoest.ToString();
            /// Ristrar usuarios para evaluacion de docentes
            var lstEstudiantes = (from u in dbEntity.usuario
                                  where u.identificacion.Equals(sCodigoEst)
                                  select u.idusuario).ToList();
            if (lstEstudiantes.Count == 0)
            {
                string snombres = "", sapellidos = "";
                SplitFullName(titulotrabajo, ref snombres, ref sapellidos);
                usuario oEstudiante = new usuario()
                {
                    nombres = snombres,
                    apellidos = sapellidos,
                    rol = "Estudiante",
                    emailinstitucional = (codigoest.ToString().Trim() + "@unicauca.edu.co"),
                    password = md5(codigoest.ToString().Trim()),
                    identificacion = codigoest.ToString()
                };
                dbEntity.usuario.AddObject(oEstudiante);
                dbEntity.SaveChanges();                
            }
            RegistrarUsuarioConRol(codigoest.ToString().Trim() + "@unicauca.edu.co", codigoest.ToString().Trim(), "Student");        
        }

        // nuevo febrero 3
        [Authorize]
        public List<savedDataSIGELA> ResearchGradeWorkImportSave(List<trabajoGdoInvSIGELA> list, int currentper)
        {
            List<savedDataSIGELA> savedResearchGradeWork = new List<savedDataSIGELA>();
            foreach (trabajoGdoInvSIGELA tgi in list)
            {
                try
                {
                    int iddoc = GetIdDocenteByIdentification(tgi.identificacion);
                    docente undocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == iddoc);
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == undocente.idusuario);

                    var docenteDlloProf = (from d in dbEntity.trabajodegradoinvestigacion
                                           join p in dbEntity.participa on d.idlabor equals p.idlabor
                                           join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                           join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                           join usu in dbEntity.usuario on doc.idusuario equals usu.idusuario
                                           where usu.identificacion.Equals(tgi.identificacion)
                                           where d.titulotrabajo.Equals(tgi.titulotrabajo)
                                           where lab.idperiodo.Equals(currentper)
                                           where d.codigoest.Equals(tgi.codigoest)
                                           select new trabajoGdoInvSIGELA { titulotrabajo = d.titulotrabajo, codigoest = d.codigoest }).Distinct().ToList();

                    //Para verificar que no ingresen una labor si esta ya existe            
                    if (docenteDlloProf.Count() == 0)
                    {
                        try
                        {
                            labor unalabor = new labor() { idperiodo = currentper };
                            dbEntity.labor.AddObject(unalabor);
                            dbEntity.SaveChanges();
                            int idprog = GetIdProgramaByName(tgi.programa);
                            trabajodegradoinvestigacion untrabajoinv = new trabajodegradoinvestigacion() { idlabor = unalabor.idlabor, idprograma = idprog, codigoest = tgi.codigoest, horassemana = tgi.horassemana, semanaslaborales = tgi.semanaslaborales, titulotrabajo = tgi.titulotrabajo };
                            dbEntity.trabajodegradoinvestigacion.AddObject(untrabajoinv);
                            dbEntity.SaveChanges();
                            evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                            dbEntity.evaluacion.AddObject(unaevaluacion);
                            dbEntity.SaveChanges();
                            autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                            dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                            dbEntity.SaveChanges();
                            problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                            dbEntity.problema.AddObject(unproblema);
                            dbEntity.SaveChanges();
                            resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                            dbEntity.resultado.AddObject(unresultado);
                            dbEntity.SaveChanges();
                            participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                            dbEntity.participa.AddObject(unparticipa);
                            dbEntity.SaveChanges();
                            savedResearchGradeWork.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Trabajo De Grado Investigacion", nombrelabor = tgi.titulotrabajo });
                            // Se llama al registro del estudiante
                            RegisterStudentForResearchGradeWork(untrabajoinv.codigoest, untrabajoinv.titulotrabajo);
                        }
                        catch (Exception ex)
                        { 
                        }
                    }
                    if (docenteDlloProf.Count() > 0)
                    {

                        var trabajoInvEsta = (from d in dbEntity.trabajodegradoinvestigacion
                                              join p in dbEntity.participa on d.idlabor equals p.idlabor
                                              join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                              join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                              where d.titulotrabajo.Equals(tgi.titulotrabajo)
                                              where lab.idperiodo.Equals(currentper)
                                              where p.iddocente.Equals(iddoc)
                                              select new trabajoGdoInvSIGELA { titulotrabajo = d.titulotrabajo, codigoest = d.codigoest }).Distinct().ToList();

                        if (trabajoInvEsta.Count() == 0)
                        {
                            try
                            {
                                evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                                dbEntity.evaluacion.AddObject(unaevaluacion);
                                dbEntity.SaveChanges();
                                autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                                dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                                dbEntity.SaveChanges();
                                problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                                dbEntity.problema.AddObject(unproblema);
                                dbEntity.SaveChanges();
                                resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                                dbEntity.resultado.AddObject(unresultado);
                                dbEntity.SaveChanges();

                                var maxid = (from pa in dbEntity.trabajodegradoinvestigacion
                                             where pa.titulotrabajo.Equals(tgi.titulotrabajo)
                                             select pa.idlabor).Max();

                                trabajodegradoinvestigacion unalabor = dbEntity.trabajodegradoinvestigacion.FirstOrDefault(t => t.idlabor == maxid && t.titulotrabajo == tgi.titulotrabajo);

                                participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                                dbEntity.participa.AddObject(unparticipa);
                                dbEntity.SaveChanges();
                                savedResearchGradeWork.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Trabajo De Grado Investigacion", nombrelabor = tgi.titulotrabajo });

                                // Se llama al registro del estudiante
                                RegisterStudentForResearchGradeWork(unalabor.codigoest, unalabor.titulotrabajo);
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                catch (Exception ex) { }
            }
            return savedResearchGradeWork;
        }
        // modificado febrero 3
        [Authorize]
        public List<savedDataSIGELA> TeacherProfDevImportSave(List<dlloProfesoralSIGELA> list, int currentper)
        {
            List<savedDataSIGELA> savedTeacherProfDev = new List<savedDataSIGELA>();
            foreach (dlloProfesoralSIGELA dllp in list)
            {
                try
                {
                    int iddoc = GetIdDocenteByIdentification(dllp.identificacion);
                    docente undocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == iddoc);
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == undocente.idusuario);

                    var docenteTrabGradInv = (from d in dbEntity.desarrolloprofesoral
                                              join p in dbEntity.participa on d.idlabor equals p.idlabor
                                              join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                              join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                              join usu in dbEntity.usuario on doc.idusuario equals usu.idusuario
                                              where usu.identificacion.Equals(dllp.identificacion)
                                              where d.nombreactividad.Equals(dllp.nombreactividad)
                                              where d.resolucion.Equals(dllp.resolucion)
                                              where lab.idperiodo.Equals(currentper)
                                              select new dlloProfesoralSIGELA { nombreactividad = d.nombreactividad, resolucion = d.resolucion }).Distinct().ToList();



                    //Para verificar que no ingresen una labor si esta ya existe            
                    if (docenteTrabGradInv.Count() == 0)
                    {
                        try
                        {
                            labor unalabor = new labor() { idperiodo = currentper };
                            dbEntity.labor.AddObject(unalabor);
                            dbEntity.SaveChanges();
                            desarrolloprofesoral undesarrollo = new desarrolloprofesoral() { idlabor = unalabor.idlabor, resolucion = dllp.resolucion, horassemana = dllp.horassemana, semanaslaborales = dllp.semanaslaborales, nombreactividad = dllp.nombreactividad };
                            dbEntity.desarrolloprofesoral.AddObject(undesarrollo);
                            dbEntity.SaveChanges();
                            evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                            dbEntity.evaluacion.AddObject(unaevaluacion);
                            dbEntity.SaveChanges();
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
                            //FIN ADICIONADO POR CLARA
                            participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                            dbEntity.participa.AddObject(unparticipa);
                            dbEntity.SaveChanges();
                            savedTeacherProfDev.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Desarrollo profesoral", nombrelabor = dllp.nombreactividad });
                        }
                        catch (Exception ex) { }
                    }
                    if (docenteTrabGradInv.Count() > 0)
                    {

                        var laborDesarrollo = (from d in dbEntity.desarrolloprofesoral
                                               join p in dbEntity.participa on d.idlabor equals p.idlabor
                                               join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                               join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                               where d.nombreactividad.Equals(dllp.nombreactividad)
                                               where lab.idperiodo.Equals(currentper)
                                               where p.iddocente.Equals(iddoc)
                                               select new dlloProfesoralSIGELA { nombreactividad = d.nombreactividad, resolucion = d.resolucion }).Distinct().ToList();

                        if (laborDesarrollo.Count() == 0)
                        {
                            try
                            {
                                evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                                dbEntity.evaluacion.AddObject(unaevaluacion);
                                dbEntity.SaveChanges();
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
                                //FIN ADICIONADO POR CLARA
                                // nuevo 3 febrero                            

                                var maxid = (from pa in dbEntity.desarrolloprofesoral
                                             where pa.nombreactividad.Equals(dllp.nombreactividad)
                                             where pa.resolucion.Equals(dllp.resolucion)
                                             select pa.idlabor).Max();

                                desarrolloprofesoral unalabor = dbEntity.desarrolloprofesoral.FirstOrDefault(d => d.idlabor == maxid && d.nombreactividad == dllp.nombreactividad && d.resolucion == dllp.resolucion);


                                participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                                dbEntity.participa.AddObject(unparticipa);
                                dbEntity.SaveChanges();
                                savedTeacherProfDev.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Desarrollo profesoral", nombrelabor = dllp.nombreactividad });
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex) { }
            }
            return savedTeacherProfDev;
        }
        // modificado 3 de febrero 
        [Authorize]
        public List<savedDataSIGELA> TeachingImportSave(List<docenciaSIGELA> list, int currentper)
        {
            List<savedDataSIGELA> savedTeaching = new List<savedDataSIGELA>();
            foreach (docenciaSIGELA dcia in list)
            {
                try
                {
                    int iddoc = GetIdDocenteByIdentification(dcia.identificacion);
                    docente undocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == iddoc);
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == undocente.idusuario);

                    var docenteDocencia = (from m in dbEntity.materia
                                           join d in dbEntity.docencia on m.idmateria equals d.idmateria
                                           join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                           join p in dbEntity.participa on d.idlabor equals p.idlabor
                                           join doc in dbEntity.docente on p.iddocente equals doc.iddocente
                                           join usu in dbEntity.usuario on doc.idusuario equals usu.idusuario
                                           where m.codigomateria.Equals(dcia.codigomateria)
                                           where m.grupo.Equals(dcia.grupo)
                                           where usu.identificacion.Equals(dcia.identificacion)
                                           where lab.idperiodo.Equals(currentper)
                                           select new docenciaSIGELA
                                           {
                                               nombremateria = m.nombremateria,
                                               grupo = m.grupo,
                                               codigomateria = m.codigomateria
                                           }).Distinct().ToList();

                    //Para verificar que no ingresen una labor si esta ya existe            
                    if (docenteDocencia.Count() == 0)
                    {
                        try
                        {
                            labor unalabor = new labor() { idperiodo = currentper };
                            dbEntity.labor.AddObject(unalabor);
                            dbEntity.SaveChanges();
                            int idmateria = GetMateriaByImport(dcia);
                            docencia unadocencia = new docencia() { idlabor = unalabor.idlabor, idmateria = idmateria, horassemana = dcia.horassemana, semanaslaborales = dcia.semanaslaborales, grupo = dcia.grupo };
                            dbEntity.docencia.AddObject(unadocencia);
                            dbEntity.SaveChanges();
                            evaluacion unaevaluacion = new evaluacion() { evaluacionautoevaluacion = -1, evaluacionestudiante = -1, evaluacionjefe = -1 };
                            dbEntity.evaluacion.AddObject(unaevaluacion);
                            dbEntity.SaveChanges();
                            autoevaluacion unaautoevalaucion = new autoevaluacion() { calificacion = -1, fecha = null };
                            dbEntity.autoevaluacion.AddObject(unaautoevalaucion);
                            dbEntity.SaveChanges();
                            problema unproblema = new problema() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", solucion = "Ninguna" };
                            dbEntity.problema.AddObject(unproblema);
                            dbEntity.SaveChanges();
                            resultado unresultado = new resultado() { idautoevaluacion = unaautoevalaucion.idautoevaluacion, descripcion = "Ninguna", ubicacion = "Ninguna" };
                            dbEntity.resultado.AddObject(unresultado);
                            dbEntity.SaveChanges();
                            participa unparticipa = new participa() { iddocente = iddoc, idlabor = unalabor.idlabor, idevaluacion = unaevaluacion.idevaluacion, idautoevaluacion = unaautoevalaucion.idautoevaluacion };
                            dbEntity.participa.AddObject(unparticipa);
                            dbEntity.SaveChanges();
                            savedTeaching.Add(new savedDataSIGELA() { nombre = unusuario.nombres, apellido = unusuario.apellidos, email = unusuario.emailinstitucional, identificacion = unusuario.identificacion, tipolabor = "Docencia", nombrelabor = dcia.nombremateria });

                            // Se revisa si hay un jefe para la importacion
                            var JefesListId = (from j in dbEntity.esjefe
                                             join d in dbEntity.docente on j.iddocente equals d.iddocente
                                             join u in dbEntity.usuario on d.idusuario equals u.idusuario
                                             where j.iddepartamento.Equals(undocente.iddepartamento)
                                             where j.idperiodo.Equals(currentper)
                                             select u.idusuario).ToList();
                                                        
                            int iUsuarioJefe = -1;                            
                            foreach(int iValor in JefesListId)
                            {
                                iUsuarioJefe = iValor;
                            }
                            if (iUsuarioJefe != -1)
                            {
                                var labores = (from p in dbEntity.labor
                                               join per in dbEntity.periodo_academico on p.idperiodo equals per.idperiodo
                                               join doc in dbEntity.docencia on p.idlabor equals doc.idlabor
                                               select p.idlabor).ToList();
                                DepartmentChiefController oController = new DepartmentChiefController();
                                foreach (int idLabor in labores)
                                {
                                    oController.SaveChiefAux(iUsuarioJefe, idLabor, 1);
                                }
                            }
                        }
                        catch (Exception ex) { }
                    }
                }
                catch (Exception ex) { }
            }
            return savedTeaching;
        }

        [Authorize]
        public int GetMateriaByImport(docenciaSIGELA dcia)
        {
            int idprograma = GetIdProgramaByName(dcia.programa);
            materia unamateria = dbEntity.materia.SingleOrDefault(q => q.idprograma == idprograma && q.codigomateria == dcia.codigomateria);
            if (unamateria == null)
            {
                unamateria = new materia() { nombremateria = dcia.nombremateria, codigomateria = dcia.codigomateria, idprograma = idprograma, grupo = dcia.grupo, creditos = null, intensidadhoraria = null, semestre = null, tipo = null };
                dbEntity.materia.AddObject(unamateria);
                dbEntity.SaveChanges();
                return unamateria.idmateria;
            }
            else
            {
                return unamateria.idmateria;
            }
        }
        // modificado 3 febrero
        [Authorize]
        public List<docenteSIGELA> TeacherImportSave(List<docenteSIGELA> sigela_docentes)
        {
            List<docenteSIGELA> savedTeachers = new List<docenteSIGELA>();
            string claveEn = "";

            foreach (docenteSIGELA dte in sigela_docentes)
            {
                try
                {
                    String emailinst;
                    int idusu;
                    dte.email = dte.email.Trim();
                    if (!dte.email.EndsWith("@unicauca.edu.co"))
                    {
                        dte.email += "@unicauca.edu.co";
                    }
                    emailinst = dte.email;
                    usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.emailinstitucional == emailinst && q.identificacion == dte.identificacion);
                    if (unusuario == null) //no existe
                    {

                        // para contraseña md5

                        claveEn = md5(emailinst.Replace("@unicauca.edu.co", ""));

                        //

                        //Creamos el usuario
                        unusuario = new usuario() { emailinstitucional = emailinst, password = claveEn, rol = "Docente", nombres = dte.nombres, apellidos = dte.apellidos, identificacion = dte.identificacion };
                        //guardamos el usuario, ahora en unusuario debe existir el id asignado
                        dbEntity.usuario.AddObject(unusuario);
                        dbEntity.SaveChanges();
                        idusu = unusuario.idusuario;
                        //verificar el departamento
                        int iddepto = getIdDeptoByName(dte.departamento);
                        //creamos el docente
                        docente undocente = new docente() { iddepartamento = iddepto, idusuario = idusu, estado = "activo" };
                        dbEntity.docente.AddObject(undocente);
                        dbEntity.SaveChanges();
                        savedTeachers.Add(dte);
                        RegistrarUsuarioConRol(unusuario.emailinstitucional, emailinst.Replace("@unicauca.edu.co", ""), "Teacher");
                    }
                    else
                    {//ya existe el usuario
                        idusu = unusuario.idusuario;
                        unusuario.nombres = dte.nombres;
                        unusuario.apellidos = dte.apellidos;
                        dbEntity.SaveChanges();
                        docente undocente = dbEntity.docente.SingleOrDefault(q => q.idusuario == idusu);
                        //verificar el departamento
                        int iddepto = getIdDeptoByName(dte.departamento);
                        RegistrarUsuarioConRol(unusuario.emailinstitucional, emailinst.Replace("@unicauca.edu.co", ""), "Teacher");
                        if (undocente == null)
                        {
                            //creamos el docente
                            undocente = new docente() { iddepartamento = iddepto, idusuario = idusu, estado = "activo" };
                            dbEntity.docente.AddObject(undocente);
                            dbEntity.SaveChanges();
                            savedTeachers.Add(dte);
                        }
                        else
                        {
                            undocente.estado = "activo";
                            dbEntity.SaveChanges();
                        }
                    }
                }
                catch { }
            }
            return savedTeachers;
        }
        // nuevo febrero 3
        [Authorize]
        public bool ExcelValidation(System.Data.OleDb.OleDbConnection oconn, out string oResult, int TipoImportacion = 0)
        {
            if (TipoImportacion == 0)
            {
                bool dtes_val, dcia_val, dllprof_val, trabGradInv_val, trabGrad_val, inv_val, soc_val, ges_val, otras_val;
                this.data_imports.docentesTable = SheetValidation(oconn, "Docentes", new String[] { "identificacion", "emailinstitucional", "nombres", "apellidos", "departamento", "estado" }, out dtes_val);
                this.data_imports.docenciaTable = SheetValidation(oconn, "Docencia", new String[] { "programa", "codigomateria", "nombremateria", "grupo", "identificacion", "horassemana", "semanaslaborales" }, out dcia_val);
                this.data_imports.dlloProfesoralTable = SheetValidation(oconn, "DesarrolloProfesoral", new String[] { "identificacion", "resolucion", "horassemana", "semanaslaborales", "nombreactividad" }, out dllprof_val);
                this.data_imports.trabajoGdoInvTable = SheetValidation(oconn, "TrabajoDeGradoInvestigacion", new String[] { "identificacion", "programa", "codigoest", "horassemana", "semanaslaborales", "titulotrabajo" }, out trabGradInv_val);
                this.data_imports.trabajoGdoTable = SheetValidation(oconn, "TrabajoDeGrado", new String[] { "identificacion", "programa", "codigoest", "horassemana", "semanaslaborales", "resolucion", "titulotrabajo" }, out trabGrad_val);
                this.data_imports.investigacionTable = SheetValidation(oconn, "Investigacion", new String[] { "identificacion", "codigovri", "horassemana", "fechainicio", "fechafin", "semanaslaborales", "nombreproyecto" }, out inv_val);
                this.data_imports.socialTable = SheetValidation(oconn, "Social", new String[] { "identificacion", "horassemana", "resolucion", "unidad", "fechainicio", "fechafin", "semanaslaborales", "nombreproyecto" }, out soc_val);
                this.data_imports.gestionTable = SheetValidation(oconn, "Gestion", new String[] { "identificacion", "horassemana", "unidad", "semanaslaborales", "nombrecargo" }, out ges_val);
                this.data_imports.otrasTable = SheetValidation(oconn, "Otras", new String[] { "identificacion", "horassemana", "descripcion" }, out otras_val);
                oResult = "";
                if (!dtes_val) oResult += "Docentes,";
                if (!dcia_val) oResult += "Docencia,";
                if (!dllprof_val) oResult += "DesarrolloProfesoral,";
                if (!trabGradInv_val) oResult += "TrabajoDeGradoInvestigacion,";
                if (!trabGrad_val) oResult += "TrabajoDeGrado,";
                if (!inv_val) oResult += "Investigacion,";
                if (!soc_val) oResult += "Social,";
                if (!ges_val) oResult += "Gestion,";
                if (!otras_val) oResult += "Otras,";
                if (oResult != "") oResult = oResult.Substring(0, oResult.Length - 1);

                return dtes_val && dcia_val && dllprof_val && trabGradInv_val && trabGrad_val && inv_val && soc_val && ges_val && otras_val;
            }
            else if (TipoImportacion == 1)
            {
                bool dcia_val;
                this.data_imports.docenciaTable = SheetValidation(oconn, "Docentes", new string[] { "CodigoMateria", "NombreMateria", "Grupo", "Identificacion", "Total" }, out dcia_val);
                oResult = "";
                if (!dcia_val) oResult += "Docentes,";
                if (oResult != "") oResult = oResult.Substring(0, oResult.Length - 1);
                return dcia_val;
            }
            oResult = "";
            return true;
        }

        [Authorize]
        public DataTable SheetValidation(System.Data.OleDb.OleDbConnection oconn, String sheetname, String[] columnas, out bool valid)
        {
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter("SELECT * FROM [" + sheetname + "$]", oconn);
            DataSet myDataSet = new DataSet();
            dataAdapter.Fill(myDataSet, sheetname);
            DataTable dataTab;

            dataTab = myDataSet.Tables[sheetname];

            // Use a DataTable object's DataColumnCollection.
            DataColumnCollection columns = dataTab.Columns;

            // Print the ColumnName and DataType for each column.
            valid = false;
            if (columnas.Length == columns.Count)
            {
                int i = 0;
                valid = true;
                foreach (DataColumn column in columns)
                {
                    valid = valid && (columnas[i] == column.ColumnName);
                    i++;
                }
            }
            return dataTab;
        }

        #endregion

        //INICIO ADICIONADO POR EDINSON

        #region ADICIONADO POR EDINSON

        public int yaEsta(string nombre)
        {
            try
            {
                cuestionario cuestionario1 = dbEntity.cuestionario.Single(c => c.tipocuestionario == nombre);
            }
            catch
            {
                return 0;
            }
            return 1;
        }

        public int yaEstaGrupo(string nombre, int Cuestionario)
        {
            try
            {
                grupo grupoExiste = dbEntity.grupo.Single(c => c.idcuestionario == Cuestionario && c.nombre == nombre);
            }
            catch
            {
                return 0;
            }
            return 1;
        }

        #region Asignar Cuestionario

        [Authorize]
        public ActionResult AssignQuestionnaire()
        {
            int contador = 0;
            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();
            int idperiodoAc = ultimoPeriodo.idperiodo;

            try
            {
                var asignacionesPeriodo = dbEntity.asignarCuestionario.ToList();
                foreach (asignarCuestionario tempAsigancion in asignacionesPeriodo)
                {
                    contador = contador + 1;
                }

            }
            catch
            {

            }
            if (contador == 8)
            {
                ViewBag.Message = "Todos los cuestionarios han sido asignados";
            }

            ViewBag.optionmenu = 1;
            return View();
        }

        [Authorize]
        public ActionResult EditarAssignQuestionnaire()
        {
            ViewBag.optionmenu = 1;
            return View();
        }
        // NUEVO 28 ENERO
        [Authorize]
        public ActionResult AssignQuestionnaire1()
        {
            string socialVar = "";
            string gestionVar = "";
            string investigacionVar = "";
            string trabajoGVar = "";
            string desarrolloPVar = "";
            string trabajoIVar = "";
            string docenciaVar = "";
            string otrasVar = "";
            int vacio = 0;
            int entradas = 8;
            Boolean entro = false;
            Boolean error = false;


            socialVar = Request.Form["social"];
            if (socialVar == null)
            {
                entradas = entradas - 1;
                socialVar = "";
            }
            gestionVar = Request.Form["gestion"];
            if (gestionVar == null)
            {
                entradas = entradas - 1;
                gestionVar = "";
            }
            investigacionVar = Request.Form["investigacion"];
            if (investigacionVar == null)
            {
                entradas = entradas - 1;
                investigacionVar = "";
            }
            trabajoGVar = Request.Form["trabajoG"];
            if (trabajoGVar == null)
            {
                entradas = entradas - 1;
                trabajoGVar = "";
            }
            desarrolloPVar = Request.Form["desarrolloP"];
            if (desarrolloPVar == null)
            {
                entradas = entradas - 1;
                desarrolloPVar = "";
            }
            trabajoIVar = Request.Form["trabajoI"];
            if (trabajoIVar == null)
            {
                entradas = entradas - 1;
                trabajoIVar = "";
            }
            docenciaVar = Request.Form["docencia"];
            if (docenciaVar == null)
            {
                entradas = entradas - 1;
                docenciaVar = "";
            }
            otrasVar = Request.Form["otras"];
            if (docenciaVar == null)
            {
                entradas = entradas - 1;
                otrasVar = "";
            }

            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();


            if (socialVar.Length != 0)
            {

                try
                {
                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();

                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == socialVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;
                    nuevaAsigancion.idlabor = 1;
                    dbEntity.asignarCuestionario.AddObject(nuevaAsigancion);
                    dbEntity.SaveChanges();
                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (gestionVar.Length != 0)
            {
                try
                {
                    cuestionario cuestionarioObtenido1 = null;
                    asignarCuestionario nuevaAsigancion1 = new asignarCuestionario();
                    cuestionarioObtenido1 = dbEntity.cuestionario.Single(c => c.tipocuestionario == gestionVar);
                    nuevaAsigancion1.idcuestionario = cuestionarioObtenido1.idcuestionario;
                    nuevaAsigancion1.idlabor = 2;
                    dbEntity.asignarCuestionario.AddObject(nuevaAsigancion1);
                    dbEntity.SaveChanges();
                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }

            if (investigacionVar.Length != 0)
            {
                try
                {
                    cuestionario cuestionarioObtenido2 = null;
                    asignarCuestionario nuevaAsigancion2 = new asignarCuestionario();

                    cuestionarioObtenido2 = dbEntity.cuestionario.Single(c => c.tipocuestionario == investigacionVar);
                    nuevaAsigancion2.idcuestionario = cuestionarioObtenido2.idcuestionario;
                    nuevaAsigancion2.idlabor = 3;
                    dbEntity.asignarCuestionario.AddObject(nuevaAsigancion2);
                    dbEntity.SaveChanges();
                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (trabajoGVar.Length != 0)
            {
                try
                {
                    cuestionario cuestionarioObtenido3 = null;
                    asignarCuestionario nuevaAsigancion3 = new asignarCuestionario();

                    cuestionarioObtenido3 = dbEntity.cuestionario.Single(c => c.tipocuestionario == trabajoGVar);
                    nuevaAsigancion3.idcuestionario = cuestionarioObtenido3.idcuestionario;
                    nuevaAsigancion3.idlabor = 4;
                    dbEntity.asignarCuestionario.AddObject(nuevaAsigancion3);
                    dbEntity.SaveChanges();
                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (desarrolloPVar.Length != 0)
            {
                try
                {
                    cuestionario cuestionarioObtenido4 = null;
                    asignarCuestionario nuevaAsigancion4 = new asignarCuestionario();

                    cuestionarioObtenido4 = dbEntity.cuestionario.Single(c => c.tipocuestionario == desarrolloPVar);
                    nuevaAsigancion4.idcuestionario = cuestionarioObtenido4.idcuestionario;
                    nuevaAsigancion4.idlabor = 5;
                    dbEntity.asignarCuestionario.AddObject(nuevaAsigancion4);
                    dbEntity.SaveChanges();
                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (trabajoIVar.Length != 0)
            {
                try
                {
                    cuestionario cuestionarioObtenido5 = null;
                    asignarCuestionario nuevaAsigancion5 = new asignarCuestionario();

                    cuestionarioObtenido5 = dbEntity.cuestionario.Single(c => c.tipocuestionario == trabajoIVar);
                    nuevaAsigancion5.idcuestionario = cuestionarioObtenido5.idcuestionario;
                    nuevaAsigancion5.idlabor = 6;
                    dbEntity.asignarCuestionario.AddObject(nuevaAsigancion5);
                    dbEntity.SaveChanges();
                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (docenciaVar.Length != 0)
            {
                try
                {
                    cuestionario cuestionarioObtenido6 = null;
                    asignarCuestionario nuevaAsigancion6 = new asignarCuestionario();

                    cuestionarioObtenido6 = dbEntity.cuestionario.Single(c => c.tipocuestionario == docenciaVar);
                    nuevaAsigancion6.idcuestionario = cuestionarioObtenido6.idcuestionario;
                    nuevaAsigancion6.idlabor = 7;
                    dbEntity.asignarCuestionario.AddObject(nuevaAsigancion6);
                    dbEntity.SaveChanges();
                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (otrasVar.Length != 0)
            {
                try
                {
                    cuestionario cuestionarioObtenido6 = null;
                    asignarCuestionario nuevaAsigancion6 = new asignarCuestionario();

                    cuestionarioObtenido6 = dbEntity.cuestionario.Single(c => c.tipocuestionario == otrasVar);
                    nuevaAsigancion6.idcuestionario = cuestionarioObtenido6.idcuestionario;
                    nuevaAsigancion6.idlabor = 8;
                    dbEntity.asignarCuestionario.AddObject(nuevaAsigancion6);
                    dbEntity.SaveChanges();
                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }


            ViewBag.optionmenu = 1;
            if (entradas == 0)
            {
                ViewBag.Message = "Todos los cuestionarios han sido asignados";
            }
            if (entro == true && error == false)
            {
                ViewBag.Message = "Se ejecutaron las asignaciones solicitadas correctamente";
            }
            else
            {


                if (entro == true && error == true)
                {
                    ViewBag.Error = "Algunos cambios no se realizaron";
                }
                if (entro == false && error == true)
                {
                    ViewBag.Error = "Algunos cambios no se realizaron";
                }

            }



            return View();
        }
        // FIN  NUEVO 29 NOCHE

        // NUEVO 29 NOCHE
        [Authorize]
        public ActionResult EditarAssignQuestionnaire1()
        {
            string socialVar = "";
            string gestionVar = "";
            string investigacionVar = "";
            string trabajoGVar = "";
            string desarrolloPVar = "";
            string trabajoIVar = "";
            string docenciaVar = "";
            string otrasVar = "";


            int vacio = 0;
            int entradas = 8;
            Boolean entro = false;
            Boolean error = false;


            socialVar = Request.Form["social"];
            if (socialVar == null)
            {
                entradas = entradas - 1;
                socialVar = "";
            }
            gestionVar = Request.Form["gestion"];
            if (gestionVar == null)
            {
                entradas = entradas - 1;
                gestionVar = "";
            }
            investigacionVar = Request.Form["investigacion"];
            if (investigacionVar == null)
            {
                entradas = entradas - 1;
                investigacionVar = "";
            }
            trabajoGVar = Request.Form["trabajoG"];
            if (trabajoGVar == null)
            {
                entradas = entradas - 1;
                trabajoGVar = "";
            }
            desarrolloPVar = Request.Form["desarrolloP"];
            if (desarrolloPVar == null)
            {
                entradas = entradas - 1;
                desarrolloPVar = "";
            }
            trabajoIVar = Request.Form["trabajoI"];
            if (trabajoIVar == null)
            {
                entradas = entradas - 1;
                trabajoIVar = "";
            }
            docenciaVar = Request.Form["docencia"];
            if (docenciaVar == null)
            {
                entradas = entradas - 1;
                docenciaVar = "";
            }
            otrasVar = Request.Form["otras"];
            if (otrasVar == null)
            {
                entradas = entradas - 1;
                docenciaVar = "";
            }

            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();


            if (socialVar.Length != 0)
            {

                try
                {
                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();


                    nuevaAsigancion = dbEntity.asignarCuestionario.Single(c => c.idlabor == 1);
                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == socialVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;

                    dbEntity.ObjectStateManager.ChangeObjectState(nuevaAsigancion, EntityState.Modified);
                    dbEntity.SaveChanges();


                    entro = true;

                    entradas = entradas - 1;

                }
                catch
                {

                    vacio = 2;
                    error = true;
                }
            }
            if (gestionVar.Length != 0)
            {
                try
                {

                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();


                    nuevaAsigancion = dbEntity.asignarCuestionario.Single(c => c.idlabor == 2);
                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == gestionVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;



                    dbEntity.ObjectStateManager.ChangeObjectState(nuevaAsigancion, EntityState.Modified);
                    dbEntity.SaveChanges();




                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }

            if (investigacionVar.Length != 0)
            {
                try
                {

                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();


                    nuevaAsigancion = dbEntity.asignarCuestionario.Single(c => c.idlabor == 3);
                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == investigacionVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;

                    dbEntity.ObjectStateManager.ChangeObjectState(nuevaAsigancion, EntityState.Modified);
                    dbEntity.SaveChanges();




                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (trabajoGVar.Length != 0)
            {
                try
                {
                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();


                    nuevaAsigancion = dbEntity.asignarCuestionario.Single(c => c.idlabor == 4);
                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == trabajoGVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;

                    dbEntity.ObjectStateManager.ChangeObjectState(nuevaAsigancion, EntityState.Modified);
                    dbEntity.SaveChanges();

                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (desarrolloPVar.Length != 0)
            {
                try
                {

                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();


                    nuevaAsigancion = dbEntity.asignarCuestionario.Single(c => c.idlabor == 5);
                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == desarrolloPVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;

                    dbEntity.ObjectStateManager.ChangeObjectState(nuevaAsigancion, EntityState.Modified);
                    dbEntity.SaveChanges();



                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (trabajoIVar.Length != 0)
            {
                try
                {

                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();

                    nuevaAsigancion = dbEntity.asignarCuestionario.Single(c => c.idlabor == 6);
                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == trabajoIVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;

                    dbEntity.ObjectStateManager.ChangeObjectState(nuevaAsigancion, EntityState.Modified);
                    dbEntity.SaveChanges();


                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (docenciaVar.Length != 0)
            {
                try
                {

                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();

                    nuevaAsigancion = dbEntity.asignarCuestionario.Single(c => c.idlabor == 7);
                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == docenciaVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;


                    dbEntity.ObjectStateManager.ChangeObjectState(nuevaAsigancion, EntityState.Modified);
                    dbEntity.SaveChanges();


                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }
            if (otrasVar.Length != 0)
            {
                try
                {

                    cuestionario cuestionarioObtenido = null;
                    asignarCuestionario nuevaAsigancion = new asignarCuestionario();

                    nuevaAsigancion = dbEntity.asignarCuestionario.Single(c => c.idlabor == 8);
                    cuestionarioObtenido = dbEntity.cuestionario.Single(c => c.tipocuestionario == otrasVar);
                    nuevaAsigancion.idcuestionario = cuestionarioObtenido.idcuestionario;


                    dbEntity.ObjectStateManager.ChangeObjectState(nuevaAsigancion, EntityState.Modified);
                    dbEntity.SaveChanges();


                    entro = true;
                    entradas = entradas - 1;

                }
                catch
                {
                    vacio = 2;
                    error = true;
                }
            }



            ViewBag.optionmenu = 1;
            if (entro == true && error == false)
            {
                ViewBag.Message = "Se ejecutaron los cambios solicitados";
            }
            else
            {
                if (entro == true && error == true)
                {
                    ViewBag.Error = "Algunos cambios no se realizaron";
                }
                if (entro == false && error == true)
                {
                    ViewBag.Error = "Algunos cambios no se realizaron";
                }
            }

            return View();
        }
        // FIN NUEVO 29 NOCHE
        // NUEVO 29 NOCHE
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetAsiganacionPeriodo(string term)
        {

            int[] response;
            response = new int[8];
            response[0] = 0;
            response[1] = 0;
            response[2] = 0;
            response[3] = 0;
            response[4] = 0;
            response[5] = 0;
            response[6] = 0;
            // NUEVO 29
            response[7] = 0;
            // FIN NUEVO 29

            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();
            int currentper = (int)SessionValue("currentAcadPeriod");

            //if (currentper == ultimoPeriodo.idperiodo) { 



            int idperiodoAc = ultimoPeriodo.idperiodo;
            try
            {
                var asignacionesPeriodo = dbEntity.asignarCuestionario.ToList();
                foreach (asignarCuestionario tempAsigancion in asignacionesPeriodo)
                {
                    response[tempAsigancion.idlabor - 1] = 1;
                }
            }
            catch
            {

            }

            return Json(response, JsonRequestBehavior.AllowGet);
            // }
            // return Json(-1, JsonRequestBehavior.AllowGet);
        }
        // FIN NUEVO 29 NOCHE

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetTechingWork1(string term)
        {
            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();

            var asignados = (from p in dbEntity.asignarCuestionario
                             join l in dbEntity.cuestionario on p.idcuestionario equals l.idcuestionario
                             select new asignacionQ { custionario = l.tipocuestionario, idlabor = p.idlabor }).ToList();

            return Json(asignados, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Evaluar Jefe

        [Authorize]
        public ActionResult EvaluateSubordinates2()
        {
            if (opcionError == 0)
            {
                ViewBag.Error = "No hay cuestionario asignado para la evaluación de esta labor";
            }
            else
            {
                ViewBag.Error = "La evaluacion de este docente ya se realizo";
            }

            return View();
        }

        [Authorize]
        public ActionResult EvaluateSubordinates1()
        {

            string laborEntra = Request.Form["labor"];
            string usuarioEntra1 = Request.Form["usuario"];
            string evaluacionEntra = Request.Form["evaluacion"];

            int labor = Convert.ToInt16(laborEntra);
            int doce = Convert.ToInt16(usuarioEntra1);
            int idE = Convert.ToInt16(evaluacionEntra);


            //    docente auxDocente = dbEntity.docente.Single(c => c.idusuario == doce);

            int idlabor = labor;
            //int iddoce = auxDocente.iddocente;
            //doce = iddoce;
            int idEva = idE;
            int salir = 0;

            evaluacion regevaluacion = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == idE);

            if (regevaluacion.evaluacionjefe != -1)
            {
                salir = 1;
            }

            if (salir == 0)
            {
                ViewBag.SubByWork2 = idlabor;
                ViewBag.SubByWork3 = doce;
                ViewBag.SubByWork4 = idEva;
                int currentper2 = (int)SessionValue("currentAcadPeriod");

                asignarCuestionario realizada = null;
                try
                {
                    social socialVar = dbEntity.social.Single(c => c.idlabor == idlabor);
                    realizada = dbEntity.asignarCuestionario.Single(c => c.idlabor == 1);
                }
                catch
                {
                    try
                    {
                        docencia docenciaVar = dbEntity.docencia.Single(c => c.idlabor == idlabor);
                        realizada = dbEntity.asignarCuestionario.Single(c => c.idlabor == 7);
                    }
                    catch
                    {
                        try
                        {
                            desarrolloprofesoral desarrolloVar = dbEntity.desarrolloprofesoral.Single(c => c.idlabor == idlabor);
                            realizada = dbEntity.asignarCuestionario.Single(c => c.idlabor == 5);
                        }
                        catch
                        {
                            try
                            {
                                gestion gestionVar = dbEntity.gestion.Single(c => c.idlabor == idlabor);
                                realizada = dbEntity.asignarCuestionario.Single(c => c.idlabor == 2);
                            }
                            catch
                            {
                                try
                                {
                                    investigacion investigacionVar = dbEntity.investigacion.Single(c => c.idlabor == idlabor);
                                    realizada = dbEntity.asignarCuestionario.Single(c => c.idlabor == 3);
                                }
                                catch
                                {
                                    try
                                    {
                                        trabajodegrado trabajoGVar = dbEntity.trabajodegrado.Single(c => c.idlabor == idlabor);
                                        realizada = dbEntity.asignarCuestionario.Single(c => c.idlabor == 4);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            trabajodegradoinvestigacion trabajoIVar = dbEntity.trabajodegradoinvestigacion.Single(c => c.idlabor == idlabor);
                                            realizada = dbEntity.asignarCuestionario.Single(c => c.idlabor == 6);
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                otras otrasVar = dbEntity.otras.Single(c => c.idlabor == idlabor);
                                                realizada = dbEntity.asignarCuestionario.Single(c => c.idlabor == 8);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (realizada != null)
                {
                    ViewBag.SubByWork5 = realizada.idcuestionario;

                    var grup_data = dbEntity.grupo
                        .Join(dbEntity.cuestionario,
                        grup => grup.idcuestionario,
                        cues => cues.idcuestionario,
                        (grup, cues) => new { grupo = grup, cuestionario = cues })
                        .OrderBy(grup_cues => grup_cues.grupo.orden)
                        .Where(grup_cues => grup_cues.cuestionario.idcuestionario == realizada.idcuestionario)
                        .Select(l => new { idCuestionario = l.cuestionario.idcuestionario, idGrupo = l.grupo.idgrupo, nombreGrupo = l.grupo.nombre, porcentaje = l.grupo.porcentaje }).ToList();

                    List<QuestionnaireGroup> l_quesgroup = new List<QuestionnaireGroup>();
                    for (int i = 0; i < grup_data.Count(); i++)
                    {
                        int idG = grup_data.ElementAt(i).idGrupo;
                        var temp_p = dbEntity.pregunta.Where(l => l.idgrupo == idG).ToList();
                        List<Question> l_ques = new List<Question>();
                        for (int j = 0; j < temp_p.Count(); j++)
                        {
                            l_ques.Add(new Question((int)temp_p.ElementAt(j).idpregunta, (string)temp_p.ElementAt(j).pregunta1));
                        }
                        l_quesgroup.Add(new QuestionnaireGroup((int)grup_data.ElementAt(i).idGrupo, (string)grup_data.ElementAt(i).nombreGrupo, (double)grup_data.ElementAt(i).porcentaje, l_ques));
                    }

                    CompleteQuestionnaire compQuest = new CompleteQuestionnaire(1, l_quesgroup);

                    //   docente docenteEntra = dbEntity.docente.Single(c => c.iddocente == iddoce);
                    usuario usuarioEntra = dbEntity.usuario.Single(c => c.idusuario == doce);
                    ViewBag.SubByWork1 = usuarioEntra.nombres + " " + usuarioEntra.apellidos;
                    ViewBag.CompQuest = compQuest;
                    ViewBag.WorkDescription = TeacherController.getLaborDescripcion(idlabor);
                    return View();
                }
                else
                {
                    opcionError = 0;
                    return RedirectPermanent("/sedoc/Admin/EvaluateSubordinates2");


                }
            }
            opcionError = 1;
            return RedirectPermanent("/sedoc/Admin/EvaluateSubordinates2");
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult UpdateEval(ChiefEval myEval)
        {
            var averages = myEval.calificaciones.GroupBy(l => l.idgrupo).Select(l => new { idgroup = l.Key, count = l.Count(), average = l.Average(r => r.calificacion) });

            var percents = averages
                .Join(dbEntity.grupo,
                aver => aver.idgroup,
                grup => grup.idgrupo,
                (aver, grup) => new { grupo = grup, averages = aver })
                .Select(l => new { idgrupo = l.grupo.idgrupo, promedio = l.averages.average, porcentaje = l.grupo.porcentaje, score = (l.averages.average * (l.grupo.porcentaje / 100)) });

            var totalscore = (decimal)percents.Sum(c => c.score);
            totalscore = System.Math.Round(totalscore, 0);

            try
            {
                evaluacion regevaluacion = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == myEval.idevaluacion);
                regevaluacion.evaluacionjefe = (int)totalscore; // aqui se supone que el total me da con decimales, pero tengo que redondear
                WorkChiefController oController = new WorkChiefController();
                oController.GeneratePdfFile(myEval, Server.MapPath("~/pdfsupport/"), (double)totalscore);
                dbEntity.SaveChanges();
                return Json((int)totalscore, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion

        #endregion

        //FIN ADICIONADO POR EDINSON

        // NUEVO 28

        [Authorize]
        [HttpPost]
        public ActionResult RestorePassword(UsuarioSimple Model)
        {
            try
            {
                string sName = Request.Form["txtUserName"];
                if (!sName.EndsWith("@unicauca.edu.co"))
                {
                    sName += "@unicauca.edu.co";
                }

                MembershipUser oUSer = Membership.GetUser(sName);
                string sPassword = oUSer.ResetPassword();
                string sUsuario = sName.Replace("@unicauca.edu.co","");

                bool oResult = oUSer.ChangePassword(sPassword, sUsuario);

                usuario oUsuario = dbEntity.usuario.SingleOrDefault(u => u.emailinstitucional == sName);
                oUsuario.password = md5(sUsuario);
                dbEntity.ObjectStateManager.ChangeObjectState(oUsuario, EntityState.Modified);
                dbEntity.SaveChanges();
                return RedirectToAction("CreateUser", "Admin", new { Result = "Contraseña Restaurada! " });
            }
            catch
            {
                return RedirectToAction("CreateUser", "Admin", new { Result = "Error al actualizar la contraseña!! "});
            }
        }

        [Authorize]
        public ActionResult CreateUser(string Result = "")
        {
            ViewBag.sResult = Result;
            ViewBag.optionmenu = 5;
             List<SelectListItem> oRoles = new List<SelectListItem>();
             oRoles.Add(new SelectListItem { Selected = true, Text = "Docente Evaluador", Value = "Coordinador" });
            //oRoles.Add(new SelectListItem { Selected = true, Text = "Docente", Value = "Docente" });
            oRoles.Add(new SelectListItem { Selected = false, Text = "Estudiante Evaluador", Value = "Estudiante" });
            ViewBag.lstRoles = oRoles;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult CreateUser(UsuarioSimple Model)
        {
            string sResult = "";
            try
            {
                
                usuario oUsuario = new usuario();
                oUsuario.nombres = Request.Form["Nombres"];
                oUsuario.apellidos = Request.Form["Apellidos"];
                string sUserName = Request.Form["UserName"];
                string sSimpleUser = "";
                if (sUserName.EndsWith("@unicauca.edu.co"))
                {
                    oUsuario.emailinstitucional = sUserName;
                    sSimpleUser = sUserName.Replace("@unicauca.edu.co", "");
                    oUsuario.password = md5(sSimpleUser);
                }
                else
                {
                    sSimpleUser  = sUserName;
                    oUsuario.password = md5(sUserName);
                    oUsuario.emailinstitucional = sUserName + "@unicauca.edu.co";
                }


                
                oUsuario.identificacion = Request.Form["Identification"];
                oUsuario.rol = Request.Form["Role"];
                dbEntity.usuario.AddObject(oUsuario);
                dbEntity.SaveChanges();

                if (Membership.FindUsersByName(oUsuario.emailinstitucional).Count == 0)
                {
                    Membership.CreateUser(oUsuario.emailinstitucional, sSimpleUser);
                }
                
                if (oUsuario.rol.Equals("Coordinador") || oUsuario.rol.Equals("Docente"))
                {
                    Roles.AddUserToRole(oUsuario.emailinstitucional, "Teacher");
                }
                else if (oUsuario.rol.Equals("Estudiante"))
                {
                    Roles.AddUserToRole(oUsuario.emailinstitucional, "Student");
                }

                sResult = "Usuario " + oUsuario.nombres + " creado satisfactoriamente!";
            }
            catch(Exception ex)
            {
                Utilities.ManageException(ex, Server.MapPath(@"..\generalLog.txt"));                
                sResult = "Error al intentar crear el usuario";
            }


            return RedirectToAction("CreateUser", "Admin", new { Result = sResult });
        }


        [Authorize]
        public ActionResult ImportDataC()
        {
            ViewBag.optionmenu = 3;
            return View(new List<savedDataSIGELA>());
        }

        [Authorize]
        [HttpPost]
        public ActionResult ImportDataC(HttpPostedFileBase excelfile)
        {
            ViewBag.optionmenu = 2;
            string connectionString = "";
            System.Data.OleDb.OleDbConnection oconn = new OleDbConnection();
            List<savedDataSIGELA> savedsigela = new List<savedDataSIGELA>();

            if (excelfile != null)
            {
                String extension = Path.GetExtension(excelfile.FileName);
                String filename = Path.GetFileName(excelfile.FileName);
                String filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                if (extension == ".xls" || extension == ".xlsx")
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    excelfile.SaveAs(filePath);

                    try
                    {
                        connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Password=\"\";User ID=Admin;Data Source=" + filePath.ToString() + ";Mode=Share Deny Write;Extended Properties=\"HDR=YES;\";Jet OLEDB:Engine Type=37";
                        oconn = new System.Data.OleDb.OleDbConnection(connectionString);
                        oconn.Open();

                        bool valido = ExcelValidation1(oconn); /// edv 

                        if (valido)
                        {
                            int currentper = (int)SessionValue("currentAcadPeriod");
                            List<coordinadorSIGELA> docs_sig = this.data_imports.getDocentes1(); //Obtener la lista de docentes desde excel
                            List<coordinadorSIGELA> saveddtes = CoordinatorImportSave1(docs_sig);


                            ViewBag.FileValid = "El archivo " + filename + " recibido es valido";
                            ViewBag.Labsreg = savedsigela.Count();
                            ViewBag.Teacreg = docs_sig.Count();
                        }
                        else
                        {
                            ViewBag.Error = "Excel no valido";
                        }

                        oconn.Close();
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception)
                    {
                        if (oconn.State != ConnectionState.Closed)
                        {
                            oconn.Close();
                        }
                        ViewBag.Error = "No se pudo abrir el archivo en Excel, consulte el formato del mismo";

                    }
                }
                else
                {
                    ViewBag.Error = "Solo se admiten archivos Excel .xls o .xlsx";
                }
            }
            else
            {
                ViewBag.Error = "No Recibido";
            }
            return View(savedsigela);
        }

        [Authorize]
        public List<coordinadorSIGELA> CoordinatorImportSave1(List<coordinadorSIGELA> sigela_docentes)
        {
            List<coordinadorSIGELA> savedTeachers = new List<coordinadorSIGELA>();
            string claveN = "";
            foreach (coordinadorSIGELA dte in sigela_docentes)
            {
                claveN = "";
                String emailinst;
                int idusu;

                if (!dte.email.EndsWith("@unicauca.edu.co"))
                {
                    dte.email += "@unicauca.edu.co";
                }
                emailinst = dte.email;
                usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.emailinstitucional == emailinst && q.identificacion == dte.identificacion);
                //usuario unusuario = dbEntity.usuario.SingleOrDefault(q => q.emailinstitucional == emailinst);
                if (emailinst != "@unicauca.edu.co")
                {
                    if (unusuario == null) //no existe
                    {
                        try
                        {

                            departamento dpt = dbEntity.departamento.SingleOrDefault(dp => dp.nombre == dte.departamento);
                            if (dpt != null)
                            {
                                // para encriptar la clave 
                                claveN = md5(emailinst.Replace("@unicauca.edu.co", ""));

                                // para encriptar la clave 

                                //Creamos el usuario
                                unusuario = new usuario() { emailinstitucional = emailinst, password = claveN, rol = "Coordinador", nombres = dte.nombres, apellidos = dte.apellidos, identificacion = dte.identificacion };
                                //guardamos el usuario, ahora en unusuario debe existir el id asignado
                                dbEntity.usuario.AddObject(unusuario);
                                dbEntity.SaveChanges();

                                usuario unusuarioCrd = dbEntity.usuario.SingleOrDefault(q => q.emailinstitucional == emailinst);

                                decanoCoordinador ingresa = new decanoCoordinador() { id = 9, idusuario = unusuarioCrd.idusuario, idfacultadDepto = dpt.iddepartamento };
                                dbEntity.decanoCoordinador.AddObject(ingresa);
                                dbEntity.SaveChanges();
                            }
                        }
                        catch { }
                    }
                    else
                    {//ya existe el usuario
                        idusu = unusuario.idusuario;
                        unusuario.nombres = dte.nombres;
                        unusuario.apellidos = dte.apellidos;
                        dbEntity.SaveChanges();
                    }
                }
            }
            return savedTeachers;
        }


        public bool ExcelValidation1(System.Data.OleDb.OleDbConnection oconn)
        {
            bool dtes_val;
            this.data_imports.docentesTable = SheetValidation(oconn, "Coordinadores", new String[] { "identificacion", "emailinstitucional", "nombres", "apellidos", "departamento" }, out dtes_val);
            return dtes_val;
        }

        #region AdmnistrarMembresias
        
        public void RegistrarUsuarioEnRol(string sUserName, string sRole)
        {
            if (!Roles.IsUserInRole(sUserName, sRole))
            {
                Roles.AddUserToRole(sUserName, sRole);
            }   
        }

        public void RemoverUsuarioDeRol(string sUserName, string sRole)
        {
            if (Roles.IsUserInRole(sUserName, sRole))
            {
                Roles.RemoveUserFromRole(sUserName, sRole);
            }
        }

        public void RegistrarUsuarioConRol(string sUserName, string sPassword, string sRole)
        {
            MembershipUser oUSer;
            if (Membership.GetUser(sUserName) != null)
            {
                oUSer = Membership.GetUser(sUserName);
            }
            else
            {
                oUSer = Membership.CreateUser(sUserName, sPassword);            
            }
            RegistrarUsuarioEnRol(sUserName, sRole);                     
        }

        public bool ActualizarContrasenaUsuarioActual(string sNewPassword, string sOldPassword)
        {
            MembershipUser oUSer = Membership.GetUser();
            return oUSer.ChangePassword(sOldPassword, sNewPassword);
        }

        #endregion
    }
}

    public class JefeLabor
    {
        public int idusuario;
        public int idlabor;
        public string nombres;
        public string apellidos;
        public string rol;
        public string tipoLabor;
        public string descripcion;
        public int evaluacionJefe;
        public int idevaluacion;
    }

    public class FacultadPrograma
    {
        public int codfacultad;
        public string nomfacultad;
        public int codprograma;
        public string nomprograma;
    }

    public class Departamento

    {
        public int iddepartamento;
        public string nombre;        
    }

    public class asignacionQ
    {
        public string custionario;
        public string asignacion;
        public int idlabor;

    }

    public class Asignatura
    {
        public int id;
        public string nombre;
    }

    public class UsuarioSimple
    {       
        [Display(Name="Usuario Unicauca")]
        [Required(ErrorMessage="Campo Requerido!")]
        public string UserName;

        [Display(Name="Nombres")]
        [Required(ErrorMessage = "Campo Requerido!")]
        public string Nombres;

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "Campo Requerido!")]
        public string Apellidos;

        [Required(ErrorMessage = "Campo Requerido!")]
        [Display(Name = "Identificación")]        
        public string Identification;

        [Required(ErrorMessage = "Campo Requerido!")]
        [Display(Name = "Rol")]
        public string Role;
    }