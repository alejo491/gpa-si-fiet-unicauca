using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcSEDOC.Models;
using System.Data.Objects;
using System.Web.Security;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace MvcSEDOC.Controllers
{
    public class UserController : SEDOCController
    {

        public ActionResult Index()
        {
            return RedirectToAction("Login", "User"); // asi redirijo
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(usuario unUsuario)
        {
            // inicio cambio edv
            //string contrasena = "";
            // fin cambio edv
            
            if (string.IsNullOrEmpty(unUsuario.emailinstitucional) || string.IsNullOrEmpty(unUsuario.password))
            {
                ViewBag.Error = "Login o password no pueden estar vacíos";
                ViewBag.UserName = "Anónimo";
                return View(unUsuario);
            }
            else
            { //determinar si el usuario existe o no
                if (!unUsuario.emailinstitucional.EndsWith("@unicauca.edu.co"))
                {
                    unUsuario.emailinstitucional += "@unicauca.edu.co";
                }
                if (!Membership.ValidateUser(unUsuario.emailinstitucional, unUsuario.password))
                {
                    ViewBag.Error = "Login o password incorrectos";
                    ViewBag.UserName = "Anónimo";
                    return View(unUsuario);
                }
                try
                {

                    // inicio cambio edv
                    unUsuario.password = AdminController.md5(unUsuario.password);
                    // fin cambio edv

                    usuario loginusuario = dbEntity.usuario.Single(u => u.emailinstitucional == unUsuario.emailinstitucional && u.password == unUsuario.password);
                    ViewBag.UserName = unUsuario.emailinstitucional;
                    FormsAuthentication.SetAuthCookie(unUsuario.emailinstitucional, false);
                    periodo_academico ultimoPeriodo = GetLastAcademicPeriod();

                    Session["logedUser"] = loginusuario;
                    Session["currentAcadPeriod"] = ultimoPeriodo.idperiodo;

                    if (loginusuario.rol.Trim().Equals("Estudiante"))
                    {
                        Session["periodoActual"] = 0;

                        ArrayList lista_roles = new ArrayList();
                        lista_roles.Add("Estudiante");
                        Session["roles"] = lista_roles;
                        return RedirectToAction("Index", "Student");
                    }
                    else if (loginusuario.rol.Trim().Equals("Docente"))
                    {

                        docente docente = dbEntity.docente.Single(d => d.idusuario == loginusuario.idusuario);
                        esjefe unjefe = dbEntity.esjefe.FirstOrDefault(es => es.iddocente == docente.iddocente && es.idperiodo == ultimoPeriodo.idperiodo);
                        // modificado 4 febrero 
                        dirige docdirige = null;
                        var docenteDocencia = (from d in dbEntity.dirige
                                               join lab in dbEntity.labor on d.idlabor equals lab.idlabor
                                               where lab.idperiodo.Equals(ultimoPeriodo.idperiodo)
                                               where d.idusuario.Equals(docente.idusuario)
                                               select new docenciaSIGELA { nombremateria = "m", grupo = "m", codigomateria = "m" }).Distinct().ToList();

                        if (docenteDocencia.Count() > 0)
                        {
                            docdirige = dbEntity.dirige.FirstOrDefault(l => l.idusuario == docente.idusuario);
                        }


                        departamento depto = dbEntity.departamento.Single(d => d.iddepartamento == docente.iddepartamento);
                        ArrayList lista_roles = new ArrayList();
                        string redireccion;

                        Session["depto"] = depto;
                        Session["periodoActual"] = 0;
                        Session["docente"] = docente;
                        Session["docenteId"] = docente.idusuario;
                        redireccion = "Teacher";

                        lista_roles.Add("Teacher");

                        if (docdirige != null)
                        {
                            //verificar si es jefe de labor
                            lista_roles.Add("WorkChief");
                            redireccion = "WorkChief";
                        }
                        if (unjefe != null)
                        {

                            Session["docente"] = docente;
                            lista_roles.Add("DepartmentChief");
                            redireccion = "DepartmentChief";
                        }

                        //INICIO ADICIONADO POR CLARA
                        adminfacultad adminfac = dbEntity.adminfacultad.FirstOrDefault(q => q.idusuario == loginusuario.idusuario);
                        Session["adminfac"] = adminfac;

                        if (adminfac != null)
                        {
                            //verificar si es administrador de facultad
                            facultad facultad = dbEntity.facultad.Single(q => q.idfacultad == adminfac.idfacultad);
                            Session["fac"] = facultad;
                            lista_roles.Add("AdminFac");
                            redireccion = "AdminFac";
                        }
                        //FIN ADICONADO POR CLARA 

                        lista_roles.Reverse();
                        Session["roles"] = lista_roles;
                        return RedirectToAction("Index", redireccion);
                    }
                    else if (loginusuario.rol.Trim().Equals("Coordinador"))
                    {
                        Session["periodoActual"] = 0;

                        usuario docente = dbEntity.usuario.Single(d => d.idusuario == loginusuario.idusuario);
                        dirige docdirige = dbEntity.dirige.FirstOrDefault(l => l.idusuario == docente.idusuario);
                        Session["docenteId"] = docente.idusuario;
                        Session["docente"] = docente;
                        string redireccion = "Teacher";
                        ArrayList lista_roles = new ArrayList();
                        lista_roles.Add("Coordinador");

                        lista_roles.Add("WorkChief");
                        redireccion = "WorkChief";

                        lista_roles.Reverse();
                        Session["roles"] = lista_roles;
                        return RedirectToAction("Index1", redireccion);
                    }// FIN NUEVO 28
                    else if (loginusuario.rol.Trim().Equals("Administrador"))
                    {
                        Session["periodoActual"] = 0;

                        ArrayList lista_roles = new ArrayList();
                        lista_roles.Add("Admin");
                        Session["roles"] = lista_roles;
                        return RedirectToAction("Index", "Admin");
                    }
                    else //Externo
                    {
                        Session["periodoActual"] = 0;
                        dirige usudirige = dbEntity.dirige.SingleOrDefault(l => l.idusuario == unUsuario.idusuario);
                        if (usudirige != null)
                        {

                            string redireccion = "WorkChief";
                            ArrayList lista_roles = new ArrayList();
                            lista_roles.Add("WorkChief");
                            lista_roles.Reverse();
                            Session["roles"] = lista_roles;
                            return RedirectToAction("Index", redireccion);
                        }
                    }
                    return RedirectToAction("Login", "User");
                }
                catch (Exception ex)
                {
                    Utilities.ManageException(ex, Server.MapPath(@"..\generalLog.txt"));
                    ViewBag.Error = "Login o password incorrectos";
                }
            }

            return View(unUsuario);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }


        [Authorize]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetAcademicPeriods()
        {
            var periods = dbEntity.periodo_academico.Select(q => new { idperiodo = q.idperiodo, anio = q.anio, numeroperiodo = q.numeroperiodo }).OrderBy(q => q.idperiodo).ToList();
            return Json(periods, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangeCurrentAcademicPeriod(int current_academic_period, string controller_name, string view_name)
        {
            periodo_academico ultimoPeriodo = GetLastAcademicPeriod();
            Session["currentAcadPeriod"] = current_academic_period;
            int periodoSeleccionado = (int)Session["currentAcadPeriod"];
            if (ultimoPeriodo.idperiodo == periodoSeleccionado)
            {
                Session["periodoActual"] = 0;
            }
            else
            {
                Session["periodoActual"] = 1;
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

    }
}
