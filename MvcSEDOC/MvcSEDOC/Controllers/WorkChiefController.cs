using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MvcSEDOC.Models;
using System.IO;
using System.Data.OleDb;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace MvcSEDOC.Controllers
{
    public class WorkChiefController : SEDOCController
    {
        //variable para almacenar el reporte
        public static int opcionError;
        public static List<DocenteReporte> listaReporte = new List<DocenteReporte>();
        public static List<ConsolidadoDocente> consolidadoDocente = new List<ConsolidadoDocente>();
        public static List<ConsolidadoDocente> consolidadoDocenteReporte;       
        public static List<AutoevaluacionDocente> AutoevaluacionDocenteReporte;

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }


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
        public ActionResult ChangePass()
        {
            
            int periodoActualSelec = 0;
            periodoActualSelec = (int)SessionValue("periodoActual");
            if (periodoActualSelec == 1)
            {
                return RedirectPermanent("/WorkChief/Index1");
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
                return RedirectPermanent("/WorkChief/Index1");
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
                int usuarioAux = (int)SessionValue("docenteId");
                usuario NuevaContrasena = dbEntity.usuario.Single(u => u.idusuario == usuarioAux && u.password == claveA);
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


        [Authorize]
        public ActionResult Index1()
        {
            return View();
        }

        [Authorize]
        public ActionResult AssessLaborChief()
        {
            int id = (int)SessionValue("docenteId");
            ViewBag.optionmenu = 3;

            int currentper = (int)SessionValue("currentAcadPeriod");

            var users = (from u in dbEntity.usuario
                         join d in dbEntity.dirige on u.idusuario equals d.idusuario
                         join l in dbEntity.labor on d.idlabor equals l.idlabor
                         join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                         join ev in dbEntity.evaluacion on d.idevaluacion equals ev.idevaluacion
                         where l.idperiodo == currentper
                         where u.idusuario == id
                         select new JefeLabor { idusuario = u.idusuario, idlabor = l.idlabor, idevaluacion = ev.idevaluacion, nombres = u.nombres, apellidos = u.apellidos, rol = u.rol, evaluacionJefe = (int)ev.evaluacionjefe }).OrderBy(p => p.apellidos).ToList();

            if (users.Count == 0) {
                ViewBag.Error = "No tiene labores asignadas para para este periodo";
            }

            users = setLaborChief(users);
            ViewBag.lista = users;



            return View();
        }

        public List<JefeLabor> setLaborChief(List<JefeLabor> lista)
        {
            gestion gestion;
            social social;
            investigacion investigacion;
            trabajodegrado trabajoDeGrado;
            trabajodegradoinvestigacion trabajoDeGradoInvestigacion;
            desarrolloprofesoral desarrolloProfesoral;
            docencia docencia;
            otras otrasVar;
            foreach (JefeLabor jefe in lista)
            {
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
                    materia materia = dbEntity.materia.SingleOrDefault(g => g.idmateria == docencia.idmateria);
                    jefe.tipoLabor = "docencia";
                    jefe.descripcion = materia.nombremateria;
                    continue;
                }

                otrasVar = dbEntity.otras.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (otrasVar != null)
                {                 
                    jefe.tipoLabor = "Otras";
                    jefe.descripcion = otrasVar.descripcion;
                    continue;
                }
            }
            return lista;
        }


        public ActionResult evaluateSubordinates2()
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
        public ActionResult evaluateSubordinates1()
        {
            string laborEntra = Request.Form["labor"];
            string usuarioEntra1 = Request.Form["usuario"];
            string evaluacionEntra = Request.Form["evaluacion"];

            int labor = Convert.ToInt16(laborEntra);
            int doce = Convert.ToInt16(usuarioEntra1);
            int idE = Convert.ToInt16(evaluacionEntra);

            int idlabor = labor;
            int iddoce = doce;
            int idEva = idE;           

            int salir = 0;
            participa regparticipa = dbEntity.participa.SingleOrDefault(c => c.iddocente == iddoce && c.idlabor == idlabor);

            if (regparticipa != null)
            {
                evaluacion regevaluacion = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == regparticipa.idevaluacion);
                if (regevaluacion.evaluacionjefe != -1)
                {
                    salir = 1;
                }
            }
            
            if (salir == 0)
            {
              
                ViewBag.SubByWork2 = idlabor;
                ViewBag.SubByWork3 = iddoce;
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
                    // Obtener Cuestionariodocen
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

                    docente docenteEntra = dbEntity.docente.Single(c => c.iddocente == iddoce);
                    usuario usuarioEntra = dbEntity.usuario.Single(c => c.idusuario == docenteEntra.idusuario);
                    ViewBag.SubByWork1 = usuarioEntra.nombres + " " + usuarioEntra.apellidos;
                    ViewBag.WorkDescription = TeacherController.getLaborDescripcion(idlabor);
                    ViewBag.CompQuest = compQuest;

                    return View();
                }
                else
                {
                    opcionError = 0;
                    return RedirectPermanent("/sedoc/WorkChief/evaluateSubordinates2");
                }                
            }
            opcionError = 1;
            return RedirectPermanent("/sedoc/WorkChief/evaluateSubordinates2");           
        }

        public void GeneratePdfFile(ChiefEval oEval, string MapPath, double totalScore, string sEvalType = "chief", string sObservations = "")
        {
            Models.Utilities oUtil = new Models.Utilities();
            docente oDocente = dbEntity.docente.SingleOrDefault(q => q.iddocente == oEval.iddocente);
            labor oLabor = dbEntity.labor.SingleOrDefault(q => q.idlabor == oEval.idlabor);
            periodo_academico oPeriodo = dbEntity.periodo_academico.SingleOrDefault(p => p.idperiodo == oLabor.idperiodo);
            string sDescription = TeacherController.getLaborDescripcion(oEval.idlabor);
            usuario oUsuario = dbEntity.usuario.SingleOrDefault(q => q.idusuario == oDocente.idusuario);

            Document oDocument = oUtil.StartPdfWriter(oDocente.idusuario.ToString(), sEvalType,
                 oPeriodo.anio.ToString() + "-" + oPeriodo.numeroperiodo.ToString(), sDescription, MapPath);
            Font oContentFont = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL);
            System.Text.StringBuilder oBuilder = new System.Text.StringBuilder();

            oBuilder.Append("\n\nDocente: " + oUsuario.nombres + " " + oUsuario.apellidos);
            oBuilder.Append("\nPeriodo: " + oPeriodo.anio.ToString() + "-" + oPeriodo.numeroperiodo.ToString());
            oBuilder.Append("\nNombre de Labor: " + sDescription);
            if (sEvalType.Equals("chief"))
                oBuilder.Append("\nTipo de Evaluación: Jefe");
            else if (sEvalType.Equals("student"))
                oBuilder.Append("\nTipo de Evaluación: Estudiante");            
            oBuilder.Append("\nFecha y hora: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            oBuilder.Append("\nCALIFICACIÓN: " + totalScore.ToString() + "\n\n");
            Paragraph oContent = new Paragraph(oBuilder.ToString());
            oContent.Font = oContentFont;
            oContent.Alignment = Element.ALIGN_JUSTIFIED;
            oDocument.Add(oContent);

            int idGrupo = -1;
            oEval.calificaciones.Sort();
            PdfPTable oTable = new PdfPTable(2);
            oTable.WidthPercentage = 100;
            Rectangle rect = new Rectangle(100, 1000);
            oTable.SetWidthPercentage(new float[] { 95, 5 }, rect);            
            foreach (CE_Question oPregunta in oEval.calificaciones)
            {   
                if (idGrupo != oPregunta.idgrupo)
                {                    
                    grupo oGrupo = dbEntity.grupo.SingleOrDefault(g => g.idgrupo == oPregunta.idgrupo);
                    idGrupo = oPregunta.idgrupo;
                    PdfPCell oCell = new PdfPCell(new Phrase("\n" + oGrupo.nombre.ToUpper() + " (" + oGrupo.porcentaje + "%)"));
                    oCell.Colspan = 2;
                    oCell.BorderWidth = 0;
                    oTable.AddCell(oCell);
                }
                
                pregunta oPreg = dbEntity.pregunta.SingleOrDefault(p => p.idpregunta == oPregunta.idpregunta);
                PdfPCell oCellQ = new PdfPCell(new Phrase(oPreg.pregunta1));
                oCellQ.BorderWidth = 0;
                PdfPCell oCellC = new PdfPCell(new Phrase(oPregunta.calificacion.ToString()));
                oCellC.BorderWidth = 0;
                oTable.AddCell(oCellQ);
                oTable.AddCell(oCellC);
            }
            oDocument.Add(oTable);
            sObservations = sObservations.Trim();
            if (sObservations != "")
            {
                Paragraph oContent1 = new Paragraph("\nOBSERVACIONES: " + sObservations);
                oContent1.Font = oContentFont;
                oContent1.Alignment = Element.ALIGN_JUSTIFIED;
                oDocument.Add(oContent1);
            }
            
            oDocument.Close();
        }

        [Authorize]
        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult UpdateEval(StudentEval myEval)
        {
            try
            {
                var averages = myEval.calificaciones.GroupBy(l => l.idgrupo).Select(l => new { idgroup = l.Key, count = l.Count(), average = l.Average(r => r.calificacion) });

                var percents = averages
                    .Join(dbEntity.grupo,
                    aver => aver.idgroup,
                    grup => grup.idgrupo,
                    (aver, grup) => new { grupo = grup, averages = aver })
                    .Select(l => new { idgrupo = l.grupo.idgrupo, promedio = l.averages.average, porcentaje = l.grupo.porcentaje, score = (l.averages.average * (l.grupo.porcentaje / 100)) });

                var totalscore = percents.Sum(c => c.score);
                totalscore = System.Math.Round(totalscore, 0);

                //verificamos la participacion y q existe la evaluación
                participa regparticipa = dbEntity.participa.SingleOrDefault(c => c.iddocente == myEval.iddocente && c.idlabor == myEval.idlabor);

                if (regparticipa != null)
                {
                    evaluacion regevaluacion = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == regparticipa.idevaluacion);
                    regevaluacion.evaluacionjefe = (int)totalscore; // aqui se supone que el total me da con decimales, pero tengo que redondear
                    //regevaluacion.evaluacionestudiante = (int)myEval.EvalEst;       /***COMENTADO POR EDINSON ****/
                    //regevaluacion.evaluacionautoevaluacion = (int)myEval.EvalAut;   /***COMENTADO POR EDINSON ****/
                    dbEntity.SaveChanges();
                    GeneratePdfFile(myEval, Server.MapPath("~/pdfsupport/"), totalscore, sObservations: myEval.observaciones);
                    return Json((int)totalscore, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(-1, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }

        //MODIFICADO POR EDINSON
        [Authorize]
        public ActionResult evaluateSubordinates()
        {
            usuario usu = (usuario)SessionValue("logedUser");
            int idUsr = usu.idusuario;
            int currentper = (int)SessionValue("currentAcadPeriod");
            List<SubordinatesByWork> mySubordinates = new List<SubordinatesByWork>();

            var dir_ids = dbEntity.dirige
                .Join(dbEntity.labor,
                dir => dir.idlabor,
                lab => lab.idlabor,
                (dir, lab) => new { dirige = dir, labor = lab })
                .Where(dir_lab => dir_lab.dirige.idusuario == idUsr && dir_lab.labor.idperiodo == currentper);

            var part_idds = dbEntity.participa
                .Join(dir_ids,
                part => part.idlabor,
                d_ids => d_ids.labor.idlabor,
                (part, d_ids) => new { participa = part, dirige = d_ids });

            var datos_doc = dbEntity.docente
                .Join(part_idds,
                doc => doc.iddocente,
                part_ids => part_ids.participa.iddocente,
                (doc, part_ids) => new { docente = doc, part_idds = part_ids });

            var datos_usr = dbEntity.usuario
                .Join(datos_doc,
                usr => usr.idusuario,
                dat_d => dat_d.docente.idusuario,
                (usr, dat_d) => new { persona = usr, dato_doc = dat_d })
                .Select(c => new { idDocente = c.dato_doc.docente.iddocente, idLabor = c.dato_doc.part_idds.participa.idlabor, idEvaluacion = c.dato_doc.part_idds.participa.idevaluacion, nombres = c.persona.nombres, apellidos = c.persona.apellidos }).ToList();

            var gestion_works = dbEntity.gestion
                .Join(dir_ids,
                gest => gest.idlabor,
                d_ids => d_ids.labor.idlabor,
                (gest, d_ids) => new { gestion = gest, dir_ids = d_ids })
                .Select(c => new { txtLabor = c.gestion.nombrecargo, idLabor = c.gestion.idlabor })
                .OrderBy(e => e.txtLabor)
                .ToList();

            if (gestion_works.Count() > 0)
            {

                for (int i = 0; i < gestion_works.Count(); i++)
                {
                    var temp_ls = datos_usr.Where(du => du.idLabor == gestion_works.ElementAt(i).idLabor).ToList();
                    List<Subordinate> l_sub = new List<Subordinate>();
                    for (int j = 0; j < temp_ls.Count(); j++)
                    {
                        int ideval = (int)temp_ls.ElementAt(j).idEvaluacion;
                        var puntaje = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == ideval);
                        if (puntaje.evaluacionjefe <= 1)
                        {
                            l_sub.Add(new Subordinate((int)temp_ls.ElementAt(j).idDocente, (int)temp_ls.ElementAt(j).idLabor, (int)temp_ls.ElementAt(j).idEvaluacion, (int)puntaje.evaluacionjefe, (int)puntaje.evaluacionestudiante, (int)puntaje.evaluacionautoevaluacion, (string)temp_ls.ElementAt(j).nombres, (string)temp_ls.ElementAt(j).apellidos));
                        }                        
                    }
                    if (l_sub.Count > 0)
                    {
                        mySubordinates.Add(new SubordinatesByWork(gestion_works.ElementAt(i).idLabor, gestion_works.ElementAt(i).txtLabor, "Gestion", l_sub));
                    }                    
                }
            }

            var social_works = dbEntity.social
                .Join(dir_ids,
                soc => soc.idlabor,
                d_ids => d_ids.labor.idlabor,
                (soc, d_ids) => new { social = soc, dir_ids = d_ids })
                .Select(c => new { txtLabor = c.social.nombreproyecto, idLabor = c.social.idlabor })
                .OrderBy(e => e.txtLabor)
                .ToList();

            if (social_works.Count() > 0)
            {

                for (int i = 0; i < social_works.Count(); i++)
                {
                    var temp_ls = datos_usr.Where(du => du.idLabor == social_works.ElementAt(i).idLabor).ToList();
                    List<Subordinate> l_sub = new List<Subordinate>();
                    for (int j = 0; j < temp_ls.Count(); j++)
                    {
                        int ideval = (int)temp_ls.ElementAt(j).idEvaluacion;
                        var puntaje = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == ideval);
                        if (puntaje.evaluacionjefe <= 1)
                            l_sub.Add(new Subordinate((int)temp_ls.ElementAt(j).idDocente, (int)temp_ls.ElementAt(j).idLabor, (int)temp_ls.ElementAt(j).idEvaluacion, (int)puntaje.evaluacionjefe, (int)puntaje.evaluacionestudiante, (int)puntaje.evaluacionautoevaluacion, (string)temp_ls.ElementAt(j).nombres, (string)temp_ls.ElementAt(j).apellidos));
                        
                    }
                    if (l_sub.Count > 0)
                        mySubordinates.Add(new SubordinatesByWork(social_works.ElementAt(i).idLabor, social_works.ElementAt(i).txtLabor, "Social", l_sub));
                }
            }

            var investigacion_works = dbEntity.investigacion
                .Join(dir_ids,
                inv => inv.idlabor,
                d_ids => d_ids.labor.idlabor,
                (inv, d_ids) => new { investigacion = inv, dir_ids = d_ids })
                .Select(c => new { txtLabor = c.investigacion.nombreproyecto, idLabor = c.investigacion.idlabor })
                .OrderBy(e => e.txtLabor)
                .ToList();

            if (investigacion_works.Count() > 0)
            {

                for (int i = 0; i < investigacion_works.Count(); i++)
                {
                    var temp_ls = datos_usr.Where(du => du.idLabor == investigacion_works.ElementAt(i).idLabor).ToList();
                    List<Subordinate> l_sub = new List<Subordinate>();
                    for (int j = 0; j < temp_ls.Count(); j++)
                    {
                        int ideval = (int)temp_ls.ElementAt(j).idEvaluacion;
                        var puntaje = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == ideval);
                        if (puntaje.evaluacionjefe <= 1)
                            l_sub.Add(new Subordinate((int)temp_ls.ElementAt(j).idDocente, (int)temp_ls.ElementAt(j).idLabor, (int)temp_ls.ElementAt(j).idEvaluacion, (int)puntaje.evaluacionjefe, (int)puntaje.evaluacionestudiante, (int)puntaje.evaluacionautoevaluacion, (string)temp_ls.ElementAt(j).nombres, (string)temp_ls.ElementAt(j).apellidos));
                    }
                    if (l_sub.Count > 0)
                        mySubordinates.Add(new SubordinatesByWork(investigacion_works.ElementAt(i).idLabor, investigacion_works.ElementAt(i).txtLabor, "Investigacion", l_sub));
                }
            }

            var trabajodegrado_works = dbEntity.trabajodegrado
                .Join(dir_ids,
                tdg => tdg.idlabor,
                d_ids => d_ids.labor.idlabor,
                (tdg, d_ids) => new { trabajodegrado = tdg, dir_ids = d_ids })
                .Select(c => new { txtLabor = c.trabajodegrado.titulotrabajo, idLabor = c.trabajodegrado.idlabor })
                .OrderBy(e => e.txtLabor)
                .ToList();

            if (trabajodegrado_works.Count() > 0)
            {

                for (int i = 0; i < trabajodegrado_works.Count(); i++)
                {
                    var temp_ls = datos_usr.Where(du => du.idLabor == trabajodegrado_works.ElementAt(i).idLabor).ToList();
                    List<Subordinate> l_sub = new List<Subordinate>();
                    for (int j = 0; j < temp_ls.Count(); j++)
                    {
                        int ideval = (int)temp_ls.ElementAt(j).idEvaluacion;
                        var puntaje = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == ideval);
                        if (puntaje.evaluacionjefe <= 1)
                            l_sub.Add(new Subordinate((int)temp_ls.ElementAt(j).idDocente, (int)temp_ls.ElementAt(j).idLabor, (int)temp_ls.ElementAt(j).idEvaluacion, (int)puntaje.evaluacionjefe, (int)puntaje.evaluacionestudiante, (int)puntaje.evaluacionautoevaluacion, (string)temp_ls.ElementAt(j).nombres, (string)temp_ls.ElementAt(j).apellidos));
                    }
                    if (l_sub.Count > 0)
                        mySubordinates.Add(new SubordinatesByWork(trabajodegrado_works.ElementAt(i).idLabor, trabajodegrado_works.ElementAt(i).txtLabor, "Trabajo de grado", l_sub));
                }
            }

            var trabajodegradoinv_works = dbEntity.trabajodegradoinvestigacion
                .Join(dir_ids,
                tdgi => tdgi.idlabor,
                d_ids => d_ids.labor.idlabor,
                (tdgi, d_ids) => new { trabajodegradoinvestigacion = tdgi, dir_ids = d_ids })
                .Select(c => new { txtLabor = c.trabajodegradoinvestigacion.titulotrabajo, idLabor = c.trabajodegradoinvestigacion.idlabor })
                .OrderBy(e => e.txtLabor)
                .ToList();

            if (trabajodegradoinv_works.Count() > 0)
            {

                for (int i = 0; i < trabajodegradoinv_works.Count(); i++)
                {
                    var temp_ls = datos_usr.Where(du => du.idLabor == trabajodegradoinv_works.ElementAt(i).idLabor).ToList();
                    List<Subordinate> l_sub = new List<Subordinate>();
                    for (int j = 0; j < temp_ls.Count(); j++)
                    {
                        int ideval = (int)temp_ls.ElementAt(j).idEvaluacion;
                        var puntaje = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == ideval);
                        if (puntaje.evaluacionjefe <= 1)
                            l_sub.Add(new Subordinate((int)temp_ls.ElementAt(j).idDocente, (int)temp_ls.ElementAt(j).idLabor, (int)temp_ls.ElementAt(j).idEvaluacion, (int)puntaje.evaluacionjefe, (int)puntaje.evaluacionestudiante, (int)puntaje.evaluacionautoevaluacion, (string)temp_ls.ElementAt(j).nombres, (string)temp_ls.ElementAt(j).apellidos));
                    }
                    if (l_sub.Count > 0)
                        mySubordinates.Add(new SubordinatesByWork(trabajodegradoinv_works.ElementAt(i).idLabor, trabajodegradoinv_works.ElementAt(i).txtLabor, "Trabajo de grado Inv", l_sub));
                }
            }

            var desarrolloprof_works = dbEntity.desarrolloprofesoral
                .Join(dir_ids,
                dprof => dprof.idlabor,
                d_ids => d_ids.labor.idlabor,
                (dprof, d_ids) => new { desarrolloprofesoral = dprof, dir_ids = d_ids })
                .Select(c => new { txtLabor = c.desarrolloprofesoral.nombreactividad, idLabor = c.desarrolloprofesoral.idlabor })
                .OrderBy(e => e.txtLabor)
                .ToList();

            if (desarrolloprof_works.Count() > 0)
            {

                for (int i = 0; i < desarrolloprof_works.Count(); i++)
                {
                    var temp_ls = datos_usr.Where(du => du.idLabor == desarrolloprof_works.ElementAt(i).idLabor).ToList();
                    List<Subordinate> l_sub = new List<Subordinate>();
                    for (int j = 0; j < temp_ls.Count(); j++)
                    {
                        int ideval = (int)temp_ls.ElementAt(j).idEvaluacion;
                        var puntaje = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == ideval);
                        if (puntaje.evaluacionjefe <= 1)
                            l_sub.Add(new Subordinate((int)temp_ls.ElementAt(j).idDocente, (int)temp_ls.ElementAt(j).idLabor, (int)temp_ls.ElementAt(j).idEvaluacion, (int)puntaje.evaluacionjefe, (int)puntaje.evaluacionestudiante, (int)puntaje.evaluacionautoevaluacion, (string)temp_ls.ElementAt(j).nombres, (string)temp_ls.ElementAt(j).apellidos));
                    }
                    if (l_sub.Count > 0)
                        mySubordinates.Add(new SubordinatesByWork(desarrolloprof_works.ElementAt(i).idLabor, desarrolloprof_works.ElementAt(i).txtLabor, "Desarrollo profesional", l_sub));
                }
            }

            var docencias = dbEntity.docencia
                .Join(dir_ids,
                docs => docs.idlabor,
                d_ids => d_ids.labor.idlabor,
                (docs, d_ids) => new { docencia = docs, dir_ids = d_ids });

            var docencia_works = docencias
                .Join(dbEntity.materia,
                doc => doc.docencia.idmateria,
                mat => mat.idmateria,
                (doc, mat) => new { docencia = doc, materia = mat })
                .Select(c => new { txtLabor = c.materia.nombremateria, idLabor = c.docencia.docencia.idlabor }).OrderBy(e => e.txtLabor).ToList();
            
            if (docencia_works.Count() > 0)
            {
                string strTitle = "";
                List<Subordinate> l_sub = new List<Subordinate>();

                for (int i = 0; i < docencia_works.Count(); i++)
                {
                    if(strTitle != docencia_works.ElementAt(i).txtLabor && l_sub.Count > 0 )
                    {
                        mySubordinates.Add(new SubordinatesByWork(docencia_works.ElementAt(i).idLabor, strTitle, "Docencia", l_sub));
                        l_sub = new List<Subordinate>();                        
                    }
                    strTitle = docencia_works.ElementAt(i).txtLabor;
                    var temp_ls = datos_usr.Where(du => du.idLabor == docencia_works.ElementAt(i).idLabor).ToList();

                    for (int j = 0; j < temp_ls.Count(); j++)
                    {
                        int ideval = (int)temp_ls.ElementAt(j).idEvaluacion;
                        var puntaje = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == ideval);
                        if (puntaje.evaluacionjefe <= 1)
                            l_sub.Add(new Subordinate((int)temp_ls.ElementAt(j).idDocente, (int)temp_ls.ElementAt(j).idLabor, (int)temp_ls.ElementAt(j).idEvaluacion, (int)puntaje.evaluacionjefe, (int)puntaje.evaluacionestudiante, (int)puntaje.evaluacionautoevaluacion, (string)temp_ls.ElementAt(j).nombres, (string)temp_ls.ElementAt(j).apellidos));
                    }
                    //mySubordinates.Add(new SubordinatesByWork(docencia_works.ElementAt(i).idLabor, docencia_works.ElementAt(i).txtLabor, "Docencia", l_sub));
                }
                if (l_sub.Count > 0)
                {
                    mySubordinates.Add(new SubordinatesByWork(docencia_works.ElementAt(docencia_works.Count() - 1).idLabor, strTitle, "Docencia", l_sub));
                }
            }

            // NUEVO 29 NOCHE

            var otras_works = dbEntity.otras
               .Join(dir_ids,
               soc => soc.idlabor,
               d_ids => d_ids.labor.idlabor,
               (soc, d_ids) => new { social = soc, dir_ids = d_ids })
               .Select(c => new { txtLabor = c.social.descripcion, idLabor = c.social.idlabor })
               .OrderBy(e => e.txtLabor)
                .ToList();

            if (otras_works.Count() > 0)
            {

                for (int i = 0; i < otras_works.Count(); i++)
                {
                    var temp_ls = datos_usr.Where(du => du.idLabor == otras_works.ElementAt(i).idLabor).ToList();
                    List<Subordinate> l_sub = new List<Subordinate>();
                    for (int j = 0; j < temp_ls.Count(); j++)
                    {
                        int ideval = (int)temp_ls.ElementAt(j).idEvaluacion;
                        var puntaje = dbEntity.evaluacion.SingleOrDefault(c => c.idevaluacion == ideval);
                        if (puntaje.evaluacionjefe <= 1)
                            l_sub.Add(new Subordinate((int)temp_ls.ElementAt(j).idDocente, (int)temp_ls.ElementAt(j).idLabor, (int)temp_ls.ElementAt(j).idEvaluacion, (int)puntaje.evaluacionjefe, (int)puntaje.evaluacionestudiante, (int)puntaje.evaluacionautoevaluacion, (string)temp_ls.ElementAt(j).nombres, (string)temp_ls.ElementAt(j).apellidos));
                    }
                    if (l_sub.Count > 0)
                        mySubordinates.Add(new SubordinatesByWork(otras_works.ElementAt(i).idLabor, otras_works.ElementAt(i).txtLabor, "Otras", l_sub));
                }
            }

            // FIN NUEVO 29 


            if (mySubordinates.Count == 0)
            {
                ViewBag.Message = "No le han sido asignados docentes para este periodo ";
            }

            ViewBag.SubByWork = mySubordinates;


            return View();
        }

        //INICIO ADICIONADO POR CLARA       

        #region Adicionado Por Clara

        #region Docentes
        
        // NUEVO 30
       
        [Authorize]
        public ActionResult CreateQuery()
        {
            

            int tipo = 0;
            var lista_roles = (System.Collections.ArrayList)SessionValue("roles");
            int currentper = (int)SessionValue("currentAcadPeriod");


            for (int i = 0; i < lista_roles.Count; i++)
            {

                if (lista_roles[i].Equals("Decano"))
                {
                    tipo = 1;   
                }
                if (lista_roles[i].Equals("Coordinador"))
                {
                    tipo = 2;
                }                
            }

            if (tipo == 0)
            {

                ViewBag.optionmenu = 1;
                docente docente = (docente)SessionValue("docente");
                departamento departamento = (departamento)SessionValue("depto");
               
                usuario user = (usuario)SessionValue("logedUser");
                // NUEVO 28
                try
                {
                    var docentes = (from u in dbEntity.usuario
                                    join d in dbEntity.docente on u.idusuario equals d.idusuario
                                    join p in dbEntity.participa on d.iddocente equals p.iddocente
                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
                                    join di in dbEntity.dirige on p.idlabor equals di.idlabor
                                    where l.idperiodo == currentper && di.idusuario == docente.idusuario && d.iddepartamento == departamento.iddepartamento
                                    select u).Distinct().OrderBy(u => u.apellidos).ToList();

                    ViewBag.lista = docentes;
                   
                    if (docentes.Count > 0)
                        ViewBag.datos = 1;
                    else
                        ViewBag.datos = 0;

                    if (departamento != null)
                    {
                        ViewBag.departamento = departamento.nombre;
                    }
                    return View();
                }
                catch
                {
                    ViewBag.Error = "No hay ";
                    return View();
                }
                // FIN NUEVO 28
            }
            if (tipo == 2 || tipo == 1)
            {
                int idCoordinador = (int)SessionValue("docenteId");
                decanoCoordinador FacCoordinador = dbEntity.decanoCoordinador.Single(d => d.idusuario == idCoordinador);
                                
                ViewBag.optionmenu = 1;
                //docente docente = (docente)SessionValue("docente"];
                departamento departamento = (departamento)SessionValue("depto");
                
                usuario user = (usuario)SessionValue("logedUser");
                // NUEVO 28
                try
                {
                    var docentes = (from u in dbEntity.usuario
                                    join d in dbEntity.docente on u.idusuario equals d.idusuario
                                    join p in dbEntity.participa on d.iddocente equals p.iddocente
                                    join l in dbEntity.labor on p.idlabor equals l.idlabor
                                    join di in dbEntity.dirige on p.idlabor equals di.idlabor
                                    where l.idperiodo == currentper && di.idusuario == idCoordinador 
                                    select u).Distinct().OrderBy(u => u.apellidos).ToList();

                    ViewBag.lista = docentes;
                    if (departamento != null)
                    {
                        ViewBag.departamento = "";
                    }
                    return View();
                }
                catch
                {
                    ViewBag.Error = "No le han sido asignados docentes para evaluar";
                    return View();
                }
                // FIN NUEVO 28
            }
            return View();
        }

        // FIN NUEVO 30

        #endregion

        #region Evaluación

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

        public List<ResEvaluacionLabor> calcularPonderados(List<ResEvaluacionLabor> lista)
        {
            Double totalHoras = lista.Sum(q => q.horasxsemana);
            foreach (ResEvaluacionLabor labor in lista)
            {
                Double porc = (labor.horasxsemana * 100) / totalHoras;
                labor.porcentaje = (int)System.Math.Round(porc, 0);
                labor.nota = (int)calculaNotaPromedio(labor.evalest, labor.evalauto, labor.evaljefe);
                Double acum = System.Math.Round((porc * labor.nota) / 100, 2);
                labor.acumula = (int)acum;
            }
            return lista;
        }

        public Double calculaNotaPromedio(Double n1, Double n2, Double n3)
        {
            if (n1 == -1 && n2 == -1 && n3 == -1)
            {
                return 0;
            }

            int divisor = 3;

            if (n1 == -1) { n1 = 0; divisor--; }
            if (n2 == -1) { n2 = 0; divisor--; }
            if (n3 == -1) { n3 = 0; divisor--; }

            return System.Math.Round(((n1 + n2 + n3) / divisor), 0);
        }
       //edinsonV
        public ActionResult CreateReport()
        {
            int tipo = 0;
            var lista_roles = (System.Collections.ArrayList)SessionValue("roles");
            for (int i = 0; i < lista_roles.Count; i++)
            {

                if (lista_roles[i].Equals("Decano"))
                {
                    tipo = 1;
                }
                if (lista_roles[i].Equals("Coordinador"))
                {
                    tipo = 2;
                }
            }

            if (tipo == 0)
            {
                ViewBag.optionmenu = 3;
                usuario user = (usuario)SessionValue("logedUser");
                departamento departamento = (departamento)SessionValue("depto");
                int currentper = (int)SessionValue("currentAcadPeriod");

                consolidadoDocenteReporte = new List<ConsolidadoDocente>();
                // NUEVO 28
                try
                {
                    List<DocenteReporte> docentes = (from u in dbEntity.usuario
                                                     join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                     join p in dbEntity.participa on d.iddocente equals p.iddocente
                                                     join l in dbEntity.labor on p.idlabor equals l.idlabor
                                                     join di in dbEntity.dirige on p.idlabor equals di.idlabor
                                                     where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && di.idusuario == user.idusuario
                                                     select new DocenteReporte { iddocente = d.iddocente, idusuario = d.idusuario }).Distinct().ToList();

                    foreach (DocenteReporte doc in docentes)
                    {
                        consolidadoDocenteReporte.Add(crearReporte(doc.idusuario));
                    }


                    ViewBag.lista = consolidadoDocenteReporte;

                    if (consolidadoDocenteReporte.Count > 0)
                        ViewBag.datos = 1;
                    else
                        ViewBag.datos = 0;

                    if (departamento != null)
                    {
                        ViewBag.departamento = departamento.nombre;
                    }

                    return View();
                }
                catch
                {
                    ViewBag.Error = "No hay";
                    return View();
                }
                // FIN NUEVO 28
            }
            if (tipo == 2 || tipo == 1)
            {
                

                int idCoordinador = (int)SessionValue("docenteId");
                decanoCoordinador FacCoordinador = dbEntity.decanoCoordinador.Single(d => d.idusuario == idCoordinador);
                
                departamento departamento = (departamento)SessionValue("depto");
                int currentper = (int)SessionValue("currentAcadPeriod");
                usuario user = (usuario)SessionValue("logedUser");
                
                ViewBag.optionmenu = 3;
                consolidadoDocenteReporte = new List<ConsolidadoDocente>();
                // NUEVO 28
                try
                {
                    List<DocenteReporte> docentes = (from u in dbEntity.usuario
                                                     join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                     join p in dbEntity.participa on d.iddocente equals p.iddocente
                                                     join l in dbEntity.labor on p.idlabor equals l.idlabor
                                                     join di in dbEntity.dirige on p.idlabor equals di.idlabor
                                                     where l.idperiodo == currentper && di.idusuario == user.idusuario
                                                     select new DocenteReporte { iddocente = d.iddocente, idusuario = d.idusuario }).Distinct().ToList();

                    foreach (DocenteReporte doc in docentes)
                    {
                        consolidadoDocenteReporte.Add(crearReporte(doc.idusuario));
                    }


                    ViewBag.lista = consolidadoDocenteReporte;

                    if (consolidadoDocenteReporte.Count > 0)
                        ViewBag.datos = 1;
                    else
                        ViewBag.datos = 0;

                    if (departamento != null)
                    {
                        ViewBag.departamento = departamento.nombre;
                    }

                    return View();
                }
                catch
                {
                    ViewBag.Error = "No hay";
                    return View();
                }
                // FIN NUEVO 28
            }
            return View();
        }
        
        public int obtenerFilas()
        {
            int filasReporte = 0;

            foreach (ConsolidadoDocente filas in consolidadoDocenteReporte)
            {
                filasReporte = filasReporte + filas.evaluacioneslabores.Count();
            }

            return filasReporte;
        }      
        // NUEVO 31 EDINSON
        public ActionResult ExportToExcel()
        {
            SpreadsheetModel mySpreadsheet = new SpreadsheetModel();
            int tam = obtenerFilas();
            String[,] datos = new String[tam + 1, 11];
            datos[0, 0] = "Nombre Docente";
            datos[0, 1] = "Nombre Jefe";
            datos[0, 2] = "Fecha Evaluación";
            datos[0, 3] = "Labor";
            datos[0, 4] = "Tipo";
            datos[0, 5] = "Periodo";
            datos[0, 6] = "H/S";
            //datos[0, 7] = "%";
            datos[0, 7] = "Est";
            datos[0, 8] = "AutoEv";
            datos[0, 9] = "JefeNota";
            //datos[0, 11] = "Acumulado";
            datos[0, 10] = "Total";

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
                    //datos[i, 7] = "" + labor.evaluacioneslabores.ElementAt(j).porcentaje;
                    datos[i, 7] = "" + labor.evaluacioneslabores.ElementAt(j).evalest;
                    datos[i, 8] = "" + labor.evaluacioneslabores.ElementAt(j).evalauto;
                    datos[i, 9] = "" + labor.evaluacioneslabores.ElementAt(j).evaljefe;
                    //datos[i, 11] = "" + labor.evaluacioneslabores.ElementAt(j).acumula;
                    datos[i, 10] = "" + labor.evaluacioneslabores.ElementAt(j).nota;
                    //i++;
                }
                i++;
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
        // FIN NUEVO 31 EDINSON
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
        
        public ConsolidadoDocente crearReporte(int idUsuario)
        {
            departamento departamento = (departamento)SessionValue("depto");
            int currentper = (int)SessionValue("currentAcadPeriod");
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);           
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);           
            usuario usuarioActual = (usuario)SessionValue("logedUser");
            int idjefe = usuarioActual.idusuario;
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
                                                join di in dbEntity.dirige on p.idlabor equals di.idlabor
                                                join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                                where l.idperiodo == currentper && u.idusuario == idUsuario && di.idusuario == usuarioActual.idusuario
                                                select new ResEvaluacionLabor { idlabor = l.idlabor, evalest = (int)ev.evaluacionestudiante, evalauto = (int)ev.evaluacionautoevaluacion, evaljefe = (int)ev.evaluacionjefe }).ToList();
                                                
            labores = setLabor(labores);
            reporte.totalhorassemana = (Double)labores.Sum(q => q.horasxsemana);
            reporte.evaluacioneslabores = calcularPonderados(labores);
            reporte.totalporcentajes = (int)reporte.evaluacioneslabores.Sum(q => q.porcentaje);
            reporte.notafinal = (int)reporte.evaluacioneslabores.Sum(q => q.acumula);
            
            return reporte;
        }

        public List<ResEvaluacionLabor> setLabor(List<ResEvaluacionLabor> lista)
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
                    labor.tipolabor = "Gestion";
                    labor.tipolaborcorto = "GES";
                    labor.descripcion = gestion.nombrecargo;
                    labor.horasxsemana = (int)gestion.horassemana;
                    continue;
                }
               
                social = dbEntity.social.SingleOrDefault(g => g.idlabor == labor.idlabor);
               
                if (social != null)
                {
                    labor.tipolabor = "Social";
                    labor.tipolaborcorto = "SOC";
                    labor.descripcion = social.nombreproyecto;
                    labor.horasxsemana = (int)social.horassemana;
                    continue;
                }
               
                investigacion = dbEntity.investigacion.SingleOrDefault(g => g.idlabor == labor.idlabor);
                
                if (investigacion != null)
                {
                    labor.tipolabor = "Investigación";
                    labor.tipolaborcorto = "INV";
                    labor.descripcion = investigacion.nombreproyecto;
                    labor.horasxsemana = (int)investigacion.horassemana;
                    continue;
                }
                
                trabajoDeGrado = dbEntity.trabajodegrado.SingleOrDefault(g => g.idlabor == labor.idlabor);
                
                if (trabajoDeGrado != null)
                {
                    labor.tipolabor = "Trabajo de Grado";
                    labor.tipolaborcorto = "TDG";
                    labor.descripcion = trabajoDeGrado.titulotrabajo;
                    labor.horasxsemana = (int)trabajoDeGrado.horassemana;
                    continue;
                }
               
                trabajoDeGradoInvestigacion = dbEntity.trabajodegradoinvestigacion.SingleOrDefault(g => g.idlabor == labor.idlabor);
               
                if (trabajoDeGradoInvestigacion != null)
                {
                    labor.tipolabor = "Trabajo de Grado Investigación";
                    labor.tipolaborcorto = "TDGI";
                    labor.descripcion = trabajoDeGradoInvestigacion.titulotrabajo;
                    labor.horasxsemana = (int)trabajoDeGradoInvestigacion.horassemana;
                    continue;
                }
               
                desarrolloProfesoral = dbEntity.desarrolloprofesoral.SingleOrDefault(g => g.idlabor == labor.idlabor);
               
                if (desarrolloProfesoral != null)
                {
                    labor.tipolabor = "Desarrollo Profesoral";
                    labor.tipolaborcorto = "DP";
                    labor.descripcion = desarrolloProfesoral.nombreactividad;
                    labor.horasxsemana = (int)desarrolloProfesoral.horassemana;
                    continue;
                }
               
                docencia = dbEntity.docencia.SingleOrDefault(g => g.idlabor == labor.idlabor);
                
                if (docencia != null)
                {
                    materia materia = dbEntity.materia.SingleOrDefault(g => g.idmateria == docencia.idmateria);
                    labor.tipolabor = "Docencia Directa";
                    labor.tipolaborcorto = "DD";
                    labor.descripcion = materia.nombremateria;
                    labor.horasxsemana = (int)docencia.horassemana;
                    continue;
                }

                otra = dbEntity.otras.SingleOrDefault(g => g.idlabor == labor.idlabor);

                if (otra != null)
                {                    
                    labor.tipolabor = "Otra";
                    labor.tipolaborcorto = "OT";
                    labor.descripcion = otra.descripcion;
                    labor.horasxsemana = (int)otra.horassemana;
                    continue;
                }

            }
            return lista;
        }

        #endregion

        #region Autoevaluación

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult VerAutoevaluaciones()
        {
            ViewBag.optionmenu = 1;           
            usuario user = (usuario)SessionValue("logedUser");
            int currentper = (int)SessionValue("currentAcadPeriod");
            ViewBag.reporte = crearReporteAutoevaluacion(user.idusuario);
            return View();
        }

        public AutoevaluacionDocente crearReporteAutoevaluacion(int idUsuario)
        {
            departamento departamento = (departamento)SessionValue("depto");
            int currentper = (int)SessionValue("currentAcadPeriod");
            periodo_academico PeriodoSeleccionado = dbEntity.periodo_academico.Single(q => q.idperiodo == currentper);           
            usuario user = dbEntity.usuario.SingleOrDefault(q => q.idusuario == idUsuario);
            usuario usuarioActual = (usuario)SessionValue("logedUser");
            int idjefe = usuarioActual.idusuario;
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
                                                    join di in dbEntity.dirige on p.idlabor equals di.idlabor
                                                    join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                                                    where l.idperiodo == currentper && u.idusuario == idUsuario && di.idusuario == usuarioActual.idusuario
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
                    labor.tipolabor = "Gestión";
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

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ShowAutoScores(int idusuario)
        {
            ViewBag.optionmenu = 1;
            ViewBag.reporte = crearReporteAutoevaluacion(idusuario);
            if (ViewBag.reporte != null)
                ViewBag.datos = 1;
            else
                ViewBag.datos = 0;

            return View();
        }
        // NUEVO 31 EDINSONb
        public ActionResult CreateReportAE()
        {
            int tipo = 0;
            var lista_roles = (System.Collections.ArrayList)SessionValue("roles");
            for (int i = 0; i < lista_roles.Count; i++)
            {

                if (lista_roles[i].Equals("Decano"))
                {
                    tipo = 1;
                }
                if (lista_roles[i].Equals("Coordinador"))
                {
                    tipo = 2;
                }
            }

            if (tipo == 0)
            {
                ViewBag.optionmenu = 3;
                departamento departamento = (departamento)SessionValue("depto");
                usuario user = (usuario)SessionValue("logedUser");
                int currentper = (int)SessionValue("currentAcadPeriod");
                AutoevaluacionDocenteReporte = new List<AutoevaluacionDocente>();
                try
                {
                    List<DocenteReporte> docentes = (from u in dbEntity.usuario
                                                     join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                     join p in dbEntity.participa on d.iddocente equals p.iddocente
                                                     join l in dbEntity.labor on p.idlabor equals l.idlabor
                                                     join di in dbEntity.dirige on p.idlabor equals di.idlabor
                                                     where l.idperiodo == currentper && d.iddepartamento == departamento.iddepartamento && di.idusuario == user.idusuario
                                                     select new DocenteReporte { iddocente = d.iddocente, idusuario = d.idusuario }).Distinct().ToList();

                    foreach (DocenteReporte doc in docentes)
                    {
                        AutoevaluacionDocenteReporte.Add(crearReporteAutoevaluacion(doc.idusuario));
                    }

                    ViewBag.lista = AutoevaluacionDocenteReporte;
                   
                    if (AutoevaluacionDocenteReporte.Count > 0)
                        ViewBag.datos = 1;
                    else
                        ViewBag.datos = 0;

                    if (departamento != null)
                    {
                        ViewBag.departamento = departamento.nombre;
                    }
                    return View();
                }
                catch
                {
                    ViewBag.Error = "No tiene docentes asignados ";
                    return View();
                }
            }
            if (tipo == 2 || tipo == 1)
            {
                int idCoordinador = (int)SessionValue("docenteId");
                decanoCoordinador FacCoordinador = dbEntity.decanoCoordinador.Single(d => d.idusuario == idCoordinador);
                departamento departamento = (departamento)SessionValue("depto");
                int currentper = (int)SessionValue("currentAcadPeriod");                
                ViewBag.optionmenu = 3;                
                usuario user = (usuario)SessionValue("logedUser");                
                AutoevaluacionDocenteReporte = new List<AutoevaluacionDocente>();
                try
                {
                    List<DocenteReporte> docentes = (from u in dbEntity.usuario
                                                     join d in dbEntity.docente on u.idusuario equals d.idusuario
                                                     join p in dbEntity.participa on d.iddocente equals p.iddocente
                                                     join l in dbEntity.labor on p.idlabor equals l.idlabor
                                                     join di in dbEntity.dirige on p.idlabor equals di.idlabor
                                                     where l.idperiodo == currentper && di.idusuario == user.idusuario
                                                     select new DocenteReporte { iddocente = d.iddocente, idusuario = d.idusuario }).Distinct().ToList();

                    foreach (DocenteReporte doc in docentes)
                    {
                        AutoevaluacionDocenteReporte.Add(crearReporteAutoevaluacion(doc.idusuario));
                    }

                    ViewBag.lista = AutoevaluacionDocenteReporte;
                   
                    if (AutoevaluacionDocenteReporte.Count > 0)
                        ViewBag.datos = 1;
                    else
                        ViewBag.datos = 0;

                    if (departamento != null)
                    {
                        ViewBag.departamento = departamento.nombre;
                    }
                    return View();
                }
                catch
                {
                    ViewBag.Error = "No tiene docentes asignados";
                    return View();
                }
            }
            return View();
        }
        // FIN NUEVO 31 EDINSON
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

            datos[0, 0] = "Nombre Docente";
            datos[0, 1] = "ID Labor";
            datos[0, 2] = "Tipo Corto";
            datos[0, 3] = "Tipo";
            datos[0, 4] = "Labor";
            datos[0, 5] = "Nota";
            datos[0, 6] = "Descripcion Problema";
            datos[0, 7] = "Respuesta Problema";
            datos[0, 8] = "Descripcion Solucion";
            datos[0, 9] = "Respuesta Solucion";

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

        #region Labores

        public class Labor
        {
            public int idlabor;
            public String tipoLabor;
            public String descripcion;
        }

        [Authorize]
        public ActionResult GetLaborType()
        {
            ViewBag.optionmenu = 1;            
            int currentper = (int)SessionValue("currentAcadPeriod");
            departamento departamento = (departamento)SessionValue("depto");
            usuario user = (usuario)SessionValue("logedUser");

            var GenreLst = new List<int>();

            var NameLabor = new List<String>();

            var tipolabor = from p in dbEntity.participa                            
                            join aev in dbEntity.autoevaluacion on p.idautoevaluacion equals aev.idautoevaluacion
                            join l in dbEntity.labor on p.idlabor equals l.idlabor
                            join d in dbEntity.dirige on l.idlabor equals d.idlabor
                            join pa in dbEntity.periodo_academico on l.idperiodo equals pa.idperiodo
                            where l.idperiodo == currentper && d.idusuario == user.idusuario          
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
            ViewBag.nombre = user.nombres + " " + user.apellidos;
            return View();
        }
        
        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchTipoLabor(string term)
        {

            int currentper = (int)SessionValue("currentAcadPeriod");
            departamento departamento = (departamento)SessionValue("depto");
            usuario user = (usuario)SessionValue("logedUser");

            var labores = new List<Labor>();
            term = (term).ToLower();

            if (term == "gestión")
            {
                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join g in dbEntity.gestion on p.idlabor equals g.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.dirige on l.idlabor equals d.idlabor
                           where l.idperiodo == currentper && d.idusuario == user.idusuario
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = g.nombrecargo }).Distinct().ToList();

            }

            if (term == "social")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join s in dbEntity.social on p.idlabor equals s.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.dirige on l.idlabor equals d.idlabor
                           where l.idperiodo == currentper && d.idusuario == user.idusuario
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = s.nombreproyecto }).Distinct().ToList();

            }


            if (term == "investigación")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join i in dbEntity.investigacion on p.idlabor equals i.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.dirige on l.idlabor equals d.idlabor
                           where l.idperiodo == currentper && d.idusuario == user.idusuario
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = i.nombreproyecto }).Distinct().ToList();

            }

            if (term == "trabajo de grado")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join t in dbEntity.trabajodegrado on p.idlabor equals t.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.dirige on l.idlabor equals d.idlabor
                           where l.idperiodo ==currentper && d.idusuario == user.idusuario
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = t.titulotrabajo }).Distinct().ToList();

            }

            if (term == "trabajo de grado investigacion")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join t in dbEntity.trabajodegradoinvestigacion on p.idlabor equals t.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.dirige on l.idlabor equals d.idlabor
                           where l.idperiodo == currentper && d.idusuario == user.idusuario
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = t.titulotrabajo }).Distinct().ToList();

            }

            if (term == "desarrollo profesoral")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join dp in dbEntity.desarrolloprofesoral on p.idlabor equals dp.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.dirige on l.idlabor equals d.idlabor
                           where l.idperiodo == currentper && d.idusuario == user.idusuario
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
                           join d in dbEntity.dirige on l.idlabor equals d.idlabor
                           where l.idperiodo == currentper && d.idusuario == user.idusuario
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = m.nombremateria }).Distinct().ToList();

            }


            if (term == "otra")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join o in dbEntity.otras on p.idlabor equals o.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join d in dbEntity.dirige on l.idlabor equals d.idlabor
                           where l.idperiodo == currentper && d.idusuario == user.idusuario
                           orderby p.idlabor
                           select new Labor { idlabor = p.idlabor, descripcion = o.descripcion }).Distinct().ToList();

            }


            return Json(labores, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult ProblemasLabor(int id)
        {
            ViewBag.optionmenu = 1;
            int currentper = (int)SessionValue("currentAcadPeriod");
            labor lab = dbEntity.labor.SingleOrDefault(q => q.idlabor == id);
           
            if (lab.idperiodo == currentper)
            {
                if (ModelState.IsValid)
                {
                    var lista = new List<Labor>();
                    lista.Add(new Labor { idlabor = id });
                    var response = getLabor(lista);
                    ViewBag.datos = response;
                    ViewBag.mensaje = 1;
                    return View();
                }                
            }
            else
            {
                ViewBag.mensaje = 0;
            }

            return View();
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
            otras otrasVar;
            materia materia;
            foreach (Labor jefe in lista)
            {
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
                // NUEVO 30 ENERO
                otrasVar = dbEntity.otras.SingleOrDefault(g => g.idlabor == jefe.idlabor);
                if (otrasVar != null)
                {
                    jefe.tipoLabor = "Otras";
                    jefe.descripcion = otrasVar.descripcion;
                    continue;
                }
                // FIN NUEVO 30 ENERO
                

            }
            return lista;
        }

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

        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult SearchProblemasLabor(int id, string term)
        {           
            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();
            departamento departamento = (departamento)SessionValue("depto");
            var labores = new List<DepartamentoLabor>();
            usuario user = (usuario)SessionValue("logedUser");

            term = (term).ToLower();

            if (term == "gestión")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join g in dbEntity.gestion on p.idlabor equals g.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join di in dbEntity.dirige on p.idlabor equals di.idlabor
                           join d in dbEntity.docente on p.iddocente equals d.iddocente 
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == ultimoPeriodo.idperiodo && l.idlabor == id && di.idusuario == user.idusuario
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
                           join di in dbEntity.dirige on p.idlabor equals di.idlabor
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == ultimoPeriodo.idperiodo && l.idlabor == id && di.idusuario == user.idusuario
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = s.nombreproyecto, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }


            if (term == "investigación")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join i in dbEntity.investigacion on p.idlabor equals i.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join di in dbEntity.dirige on p.idlabor equals di.idlabor
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == ultimoPeriodo.idperiodo && l.idlabor == id && di.idusuario == user.idusuario
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
                           join di in dbEntity.dirige on p.idlabor equals di.idlabor
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == ultimoPeriodo.idperiodo && l.idlabor == id && di.idusuario == user.idusuario
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
                           join di in dbEntity.dirige on p.idlabor equals di.idlabor
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == ultimoPeriodo.idperiodo && l.idlabor == id && di.idusuario == user.idusuario
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
                           join di in dbEntity.dirige on p.idlabor equals di.idlabor
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == ultimoPeriodo.idperiodo && l.idlabor == id && di.idusuario == user.idusuario
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
                           join di in dbEntity.dirige on p.idlabor equals di.idlabor
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == ultimoPeriodo.idperiodo && l.idlabor == id && di.idusuario == user.idusuario
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = m.nombremateria, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }


            if (term == "otra")
            {

                labores = (from p in dbEntity.participa
                           join l in dbEntity.labor on p.idlabor equals l.idlabor
                           join o in dbEntity.otras on p.idlabor equals o.idlabor
                           join e in dbEntity.autoevaluacion on p.idautoevaluacion equals e.idautoevaluacion
                           join pr in dbEntity.problema on e.idautoevaluacion equals pr.idautoevaluacion
                           join sol in dbEntity.resultado on e.idautoevaluacion equals sol.idautoevaluacion
                           join di in dbEntity.dirige on p.idlabor equals di.idlabor
                           join d in dbEntity.docente on p.iddocente equals d.iddocente
                           join u in dbEntity.usuario on d.idusuario equals u.idusuario
                           where l.idperiodo == ultimoPeriodo.idperiodo && l.idlabor == id && di.idusuario == user.idusuario
                           orderby p.idlabor
                           select new DepartamentoLabor { idlabor = p.idlabor, docente = u.nombres + " " + u.apellidos, descripcion = o.descripcion, problemadescripcion = pr.descripcion, problemarespuesta = pr.solucion, soluciondescripcion = sol.descripcion, solucionrespuesta = sol.ubicacion }).Distinct().ToList();

            }


            return Json(labores, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion 
       
        //FIN ADICIONADO POR CLARA  



        
    }
}
