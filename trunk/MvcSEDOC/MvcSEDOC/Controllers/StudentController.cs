using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcSEDOC.Models;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Data.OleDb;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace MvcSEDOC.Controllers
{
    public class StudentController : Controller
    {
        // Entidad de acceso a datos (Modelos)
        protected SEDOCEntities dbEntity = new SEDOCEntities();

        //
        // GET: /Student/

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
        public ActionResult listEvaluations()
        {
            ViewBag.optionmenu = 0;
            var usuario = dbEntity.usuario.SingleOrDefault(q => q.emailinstitucional == User.Identity.Name);
            int iNumeroEvals = 0;
            if (usuario != null)
            {
                long idUsuario = long.Parse(usuario.identificacion);
                int currentper = (int)SessionValue("currentAcadPeriod");
                var lstDocentes = (from tr in dbEntity.trabajodegradoinvestigacion
                                   join lab in dbEntity.labor on tr.idlabor equals lab.idlabor
                                   join par in dbEntity.participa on lab.idlabor equals par.idlabor
                                   join eval in dbEntity.evaluacion on par.idevaluacion equals eval.idevaluacion
                                   join doc in dbEntity.docente on par.iddocente equals doc.iddocente
                                   join us in dbEntity.usuario on doc.idusuario equals us.idusuario
                                   where tr.codigoest == idUsuario && lab.idperiodo == currentper
                                   select new DocenteDirector
                                   {
                                       iddocente = doc.iddocente,
                                       idevaluacion = eval.idevaluacion,
                                       idusuario = us.idusuario,
                                       nombres = us.nombres,
                                       apellidos = us.apellidos,
                                       idlabor = lab.idlabor
                                   }).ToList();
                ViewBag.Docentes = lstDocentes;
                iNumeroEvals = lstDocentes.Count;
            }
            ViewBag.NumeroEvals = iNumeroEvals;
            return View();
        }

        [Authorize]
        public ActionResult ChangePass()
        {
            int periodoActualSelec = 0;
            periodoActualSelec = (int)SessionValue("periodoActual");
            if (periodoActualSelec == 1)
            {
                return RedirectPermanent("/Student/Index");
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
                return RedirectPermanent("/Student/Index");
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
                usuario NuevaContrasena = dbEntity.usuario.Single(u => u.emailinstitucional == User.Identity.Name && u.password == claveA);
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

        public static int opcionError;

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

                var evalDirector = dbEntity.cuestionario.SingleOrDefault(q => q.tipocuestionario == "EvaluacionDirector");

                if (evalDirector != null)
                {
                    ViewBag.SubByWork5 = evalDirector.idcuestionario;
                    // Obtener Cuestionariodocen
                    var grup_data = dbEntity.grupo
                        .Join(dbEntity.cuestionario,
                        grup => grup.idcuestionario,
                        cues => cues.idcuestionario,
                        (grup, cues) => new { grupo = grup, cuestionario = cues })
                        .OrderBy(grup_cues => grup_cues.grupo.orden)
                        .Where(grup_cues => grup_cues.cuestionario.idcuestionario == evalDirector.idcuestionario)
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
                    ViewBag.CompQuest = compQuest;
                    ViewBag.WorkDescription = TeacherController.getLaborDescripcion(idlabor);
                    return View();
                }
                else
                {
                    opcionError = 0;
                    return RedirectPermanent("/sedoc/Student/evaluateSubordinates2");
                }
            }
            opcionError = 1;
            return RedirectPermanent("/sedoc/Student/evaluateSubordinates2");
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
                    regevaluacion.evaluacionestudiante = (int)totalscore; // aqui se supone que el total me da con decimales, pero tengo que redondear
                    regevaluacion.observaciones += myEval.observaciones + "\n";
                    dbEntity.SaveChanges();
                    WorkChiefController oController = new WorkChiefController();
                    oController.GeneratePdfFile(myEval, Server.MapPath("~/pdfsupport/"), (double)totalscore, "student", myEval.observaciones);
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


    }



    public class DocenteDirector
    {
        public string nombres;
        public string apellidos;
        public long idusuario;
        public long iddocente;
        public long idevaluacion;
        public long idlabor;
    }
}
