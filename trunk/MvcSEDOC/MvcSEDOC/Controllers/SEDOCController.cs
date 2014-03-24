using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcSEDOC.Models;

namespace MvcSEDOC.Controllers
{
    public class SEDOCController : Controller
    {
        // Entidad de acceso a datos (Modelos)
        protected SEDOCEntities dbEntity = new SEDOCEntities();

        [Authorize]
        public ActionResult JsonGetLastAcademicPeriod()
        {
            var maxYear = (from pa in dbEntity.periodo_academico
                           select pa.anio).Max();

            var maxPeriodNumber = (from pa in dbEntity.periodo_academico
                                   where pa.anio == maxYear
                                   select pa.numeroperiodo).Max();

            var response = dbEntity.periodo_academico.Single(q => q.anio == maxYear && q.numeroperiodo == maxPeriodNumber);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public periodo_academico GetLastAcademicPeriod()
        {
            var maxYear = (from pa in dbEntity.periodo_academico
                           select pa.anio).Max();

            var maxPeriodNumber = (from pa in dbEntity.periodo_academico
                                   where pa.anio == maxYear
                                   select pa.numeroperiodo).Max();

            periodo_academico lastper = dbEntity.periodo_academico.Single(q => q.anio == maxYear && q.numeroperiodo == maxPeriodNumber);
            return lastper;
        }

        //ADICIONADO
        public periodo_academico GetAcademicPeriod(int id)
        {           
            periodo_academico lastper = dbEntity.periodo_academico.Single(q => q.idperiodo == id );
            return lastper;
        }
       
        public int get_id_from_identification(string identification) {
            var response = dbEntity.usuario.SingleOrDefault(l => l.identificacion == identification);
            if (response != null) {
                return response.idusuario;
            }
            else {
                return 0;
            }
        }

        public int GetIdDocenteByIdentification(string identification) {
            int idusu = get_id_from_identification(identification);
            docente undocente = dbEntity.docente.SingleOrDefault(q => q.idusuario == idusu);
            return undocente.iddocente;
        }

        public usuario get_user_by_email(string email)
        {
            usuario response = dbEntity.usuario.SingleOrDefault(l => l.emailinstitucional == email);
            return response;
        }

        // TODO: retorna el id del departamento segun el nombre, sino existe crea uno nuevo
        public int getIdDeptoByName(string p)
        {
            departamento undepto = dbEntity.departamento.SingleOrDefault(q => q.nombre.ToLower() == p.ToLower());
            if (undepto == null) {
                undepto = new departamento() { nombre = p };
                dbEntity.departamento.AddObject(undepto);
                dbEntity.SaveChanges();
                return undepto.iddepartamento;
            }
            else {
                return undepto.iddepartamento;
            }
        }

        // TODO: retorna el id del programa segun el nombre, sino existe crea uno nuevo
        public int GetIdProgramaByName(string nomprog)
        {
            programa unprograma = dbEntity.programa.SingleOrDefault(q => q.nombreprograma.ToLower() == nomprog.ToLower());
            if (unprograma == null) {
                unprograma = new programa() { nombreprograma = nomprog };
                dbEntity.programa.AddObject(unprograma);
                dbEntity.SaveChanges();
                return unprograma.idprograma;
            }
            else {
                return unprograma.idprograma;
            }
        }
    }
}
