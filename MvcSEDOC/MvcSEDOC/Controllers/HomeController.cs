using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MvcSEDOC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!Roles.RoleExists("Admin"))
            {
                ViewBag.StartScriptResult = FirstStartScript();
            }
            else
            {
                ViewBag.StartScriptResult = "";
            }
            ViewBag.menuOption = "Index";
            return View();
        }

        public string FirstStartScript()
        {
            string sResult = "";
            try
            {
                // Se adicionan los roles necesarios para la aplicacion
                if (!Roles.RoleExists("Admin")) Roles.CreateRole("Admin");
                if (!Roles.RoleExists("AdminFac")) Roles.CreateRole("AdminFac");
                if (!Roles.RoleExists("DepartmentChief")) Roles.CreateRole("DepartmentChief");
                if (!Roles.RoleExists("Student")) Roles.CreateRole("Student");
                if (!Roles.RoleExists("Teacher")) Roles.CreateRole("Teacher");
                if (!Roles.RoleExists("WorkChief")) Roles.CreateRole("WorkChief");

                // Se adicionan los usuarios por defecto para la gestion de la aplicacion
                Membership.CreateUser("adminsedoc@unicauca.edu.co", "admin");
                Membership.CreateUser("adminsedocfiet@unicauca.edu.co", "admin");
                Membership.CreateUser("adminsedocfic@unicauca.edu.co", "admin");

                // Se adicionan los roles a los usuarios por defecto
                Roles.AddUserToRole("adminsedoc@unicauca.edu.co", "Admin");
                Roles.AddUserToRole("adminsedocfiet@unicauca.edu.co", "AdminFac");
                Roles.AddUserToRole("adminsedocfic@unicauca.edu.co", "AdminFac");
                Roles.AddUserToRole("adminsedocfiet@unicauca.edu.co", "Teacher");
                Roles.AddUserToRole("adminsedocfic@unicauca.edu.co", "Teacher");
            }
            catch (Exception ex)
            {
                sResult = "¡Error durante la ejecución de las acciones de configuración iniciales, mensaje del error: \n" + ex.Message;
            }

            if (sResult == "")
            {
                sResult = "¡Las acciones de configuración iniales se ejecutaron correctamente!";
            }
            return sResult;
        }

        public ActionResult ActionDenied()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.menuOption = "About";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.menuOption = "Contact";
            return View();
        }

        public ActionResult Enlaces()
        {
            ViewBag.menuOption = "Enlaces";
            return View();
        }
    }
}
