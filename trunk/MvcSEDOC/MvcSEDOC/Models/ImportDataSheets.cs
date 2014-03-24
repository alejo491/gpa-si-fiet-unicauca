using System;
using System.Linq;
using System.Web;
using System.Data;
using System.Collections.Generic;

namespace MvcSEDOC.Models
{
    public class ImportDataSheets
    {
        public DataTable docentesTable { get; set; }
        public DataTable docenciaTable { get; set; }
        public DataTable dlloProfesoralTable { get; set; }
        public DataTable trabajoGdoInvTable { get; set; }
        public DataTable trabajoGdoTable { get; set; }
        public DataTable investigacionTable { get; set; }
        public DataTable socialTable { get; set; }
        public DataTable gestionTable { get; set; }
        public DataTable otrasTable { get; set; }
        
        // NUEVO 28
        public List<coordinadorSIGELA> getDocentes1()
        {
            List<coordinadorSIGELA> data = new List<coordinadorSIGELA>();
            try
            {
                foreach (var row in this.docentesTable.AsEnumerable())
                {

                    coordinadorSIGELA nuevo = new coordinadorSIGELA()
                       {
                           identificacion = row[0].ToString().Trim(),
                           email = row[1].ToString().Trim(),
                           nombres = row[2].ToString().Trim(),
                           apellidos = row[3].ToString().Trim(),
                           departamento = row[4].ToString().Trim(),
                       };
                    if (nuevo.identificacion != "" && nuevo.email != "" && nuevo.nombres != "" && nuevo.apellidos != "" && nuevo.departamento != "")
                    {
                        data.Add(nuevo);
                    }
                    
                }
            }
            catch 
            {
                
            }
            return data;
        }

        // FIN NUEVO 28

        public List<docenteSIGELA> getDocentes()
        {
            List<docenteSIGELA> data = new List<docenteSIGELA>();
            try {
                foreach (var row in this.docentesTable.AsEnumerable()) {
                    // MODIFICADO 1 FEB
                    docenteSIGELA docente = new docenteSIGELA()
                    {
                        identificacion = row[0].ToString().Trim(),
                        email = row[1].ToString().Trim(),
                        nombres = row[2].ToString().Trim(),
                        apellidos = row[3].ToString().Trim(),
                        departamento = row[4].ToString().Trim(),
                        estado = row[5].ToString().Trim(),
                    };
                    if (docente.identificacion != "" && docente.email != "" && docente.nombres != "" && docente.apellidos != "" && docente.departamento != "" && docente.estado != "")
                    {                                        
                        data.Add(docente);
                    }
                    // FIN MODIFICADO 1 FEB
                }
            }
            catch (Exception ex) {
            }
            return data;
        }

        public List<docenciaSIGELA> getDocencias()
        {
            List<docenciaSIGELA> data = new List<docenciaSIGELA>();
            try {
                foreach (var row in this.docenciaTable.AsEnumerable()) {
                    data.Add(
                        new docenciaSIGELA()
                        {
                            programa = row[0].ToString().Trim(),
                            codigomateria = row[1].ToString().Trim(),
                            nombremateria = row[2].ToString().Trim(),
                            grupo = row[3].ToString().Trim(),
                            identificacion = row[4].ToString().Trim(),
                            horassemana = decimal.Parse(row[5].ToString().Trim()),
                            semanaslaborales = int.Parse(row[6].ToString().Trim())
                        });
                }
            }
            catch (Exception ex) {
            }
            return data;
        }

        public List<dlloProfesoralSIGELA> getDlloProfesoral()
        {
            List<dlloProfesoralSIGELA> data = new List<dlloProfesoralSIGELA>();
            try {
                foreach (var row in this.dlloProfesoralTable.AsEnumerable()) {
                    data.Add(
                        new dlloProfesoralSIGELA()
                        {
                            identificacion = row[0].ToString().Trim(),
                            resolucion = row[1].ToString().Trim(),
                            horassemana = decimal.Parse(row[2].ToString().Trim()),
                            semanaslaborales = int.Parse(row[3].ToString().Trim()),
                            nombreactividad = row[4].ToString().Trim()
                        });
                }
            }
            catch (Exception ex)
            {
            }
            return data;
        }

        public List<trabajoGdoInvSIGELA> getTrabajoGdoInv()
        {
            List<trabajoGdoInvSIGELA> data = new List<trabajoGdoInvSIGELA>();
            try {
                foreach (var row in this.trabajoGdoInvTable.AsEnumerable()) {
                    data.Add(
                        new trabajoGdoInvSIGELA()
                        {
                            identificacion = row[0].ToString().Trim(),
                            programa = row[1].ToString().Trim(),
                            codigoest = Int64.Parse(row[2].ToString().Trim()),
                            horassemana = decimal.Parse(row[3].ToString().Trim()),
                            semanaslaborales = int.Parse(row[4].ToString().Trim()),
                            titulotrabajo = row[5].ToString().Trim()
                        });
                }
            }
            catch (Exception ex) {
            }
            return data;
        }

        public List<trabajoGdoSIGELA> getTrabajoGdo()
        {
            List<trabajoGdoSIGELA> data = new List<trabajoGdoSIGELA>();
            try {
                foreach (var row in this.trabajoGdoTable.AsEnumerable())  {
                    data.Add(
                        new trabajoGdoSIGELA() {
                            identificacion = row[0].ToString().Trim(),
                            programa = row[1].ToString().Trim(),
                            codigoest = Int64.Parse(row[2].ToString().Trim()),
                            horassemana = decimal.Parse(row[3].ToString().Trim()),
                            semanaslaborales = int.Parse(row[4].ToString().Trim()),
                            resolucion = row[5].ToString().Trim(),
                            titulotrabajo = row[6].ToString().Trim()
                        });
                }
            }
            catch (Exception ex) {
            }
            return data;
        }

        public List<investigacionSIGELA> getInvestigacion()
        {
            List<investigacionSIGELA> data = new List<investigacionSIGELA>();
            try {
                foreach (var row in this.investigacionTable.AsEnumerable()) {
                    data.Add(
                        new investigacionSIGELA() {
                            identificacion = row[0].ToString().Trim(),
                            codigovri = row[1].ToString().Trim(),
                            horassemana = decimal.Parse(row[2].ToString().Trim()),
                            fechainicio = row[3].ToString().Trim(),
                            fechafin = row[4].ToString().Trim(),
                            semanaslaborales = int.Parse(row[5].ToString().Trim()),
                            nombreproyecto = row[6].ToString().Trim()
                        });
                }
            }
            catch (Exception ex) { 
            }
            return data;
        }

        public List<socialSIGELA> getSocial()
        {
            List<socialSIGELA> data = new List<socialSIGELA>();
            try {
                foreach (var row in this.socialTable.AsEnumerable()) {
                    data.Add(
                        new socialSIGELA() {
                            identificacion = row[0].ToString().Trim(),
                            horassemana = decimal.Parse(row[1].ToString().Trim()),
                            resolucion = row[2].ToString().Trim(),
                            unidad = row[3].ToString().Trim(),
                            fechainicio = row[4].ToString().Trim(),
                            fechafin = row[5].ToString().Trim(),
                            semanaslaborales = int.Parse(row[6].ToString().Trim()),
                            nombreproyecto = row[7].ToString().Trim()
                        });
                }
            }
            catch (Exception ex) {
            }
            return data;
        }

        public List<gestionSIGELA> getGestion()
        {
            List<gestionSIGELA> data = new List<gestionSIGELA>();
            try {
                foreach (var row in this.gestionTable.AsEnumerable()) {
                    data.Add(
                        new gestionSIGELA() {
                            identificacion = row[0].ToString().Trim(),
                            horassemana = decimal.Parse(row[1].ToString().Trim()),
                            unidad = row[2].ToString().Trim(),
                            semanaslaborales = int.Parse(row[3].ToString().Trim()),
                            nombrecargo = row[4].ToString().Trim()
                        });
                }
            }
            catch (Exception ex) {
            }
            return data;
        }

        // nuevo febrero 3

        public List<otrasSIGELA> getOtras()
        {
            List<otrasSIGELA> data = new List<otrasSIGELA>();
            try
            {
                foreach (var row in this.otrasTable.AsEnumerable())
                {
                    data.Add(
                        new otrasSIGELA()
                        {
                            identificacion = row[0].ToString().Trim(),
                            horassemana = decimal.Parse(row[1].ToString().Trim()),
                            descripcion = row[2].ToString().Trim(),                            
                        });
                }
            }
            catch
            {
            }
            return data;
        }


    }

    // Clases para importar de Excel

    // NUEVO 28

    public class coordinadorSIGELA
    {
        public String identificacion { get; set; }
        public String email { get; set; }
        public String nombres { get; set; }
        public String apellidos { get; set; }
        public String departamento { get; set; } 
    }

    // FIN NUEVO 28

    public class docenteSIGELA {
        public String identificacion { get; set; }
        public String email { get; set; }
        public String nombres { get; set; }
        public String apellidos  { get; set; }
        public String departamento { get; set; }
        public String estado { get; set; }
    }

    public class docenciaSIGELA {
        public String programa { get; set; }
        public String codigomateria { get; set; }
        public String nombremateria { get; set; }
        public String grupo { get; set; }
        public String identificacion { get; set; } 
        public decimal horassemana { get; set; }
        public int semanaslaborales { get; set; }
        //public int creditos { get; set; }
        //public int semestre { get; set; }
        //public int intensidadhoraria { get; set; }
        //public String tipo { get; set; }
    }

    public class dlloProfesoralSIGELA {
        public String identificacion { get; set; } 
        public String resolucion { get; set; }
        public decimal horassemana { get; set; }
        public int semanaslaborales { get; set; }
        public String nombreactividad { get; set; }
    }

    public class trabajoGdoInvSIGELA {
        public String identificacion { get; set; } 
        public String programa { get; set; }
        public Int64 codigoest { get; set; }
        public decimal horassemana { get; set; }
        public int semanaslaborales { get; set; }
        public String titulotrabajo { get; set; }
    }

    public class trabajoGdoSIGELA {
        public String identificacion { get; set; } 
        public String programa { get; set; }
        public Int64 codigoest { get; set; }
        public decimal horassemana { get; set; }
        public int semanaslaborales { get; set; }
        public String resolucion { get; set; }
        public String titulotrabajo { get; set; }
    }

    public class investigacionSIGELA {
        public String identificacion { get; set; } 
        public String codigovri { get; set; }
        public decimal horassemana { get; set; }
        public String fechainicio { get; set; }
        public String fechafin { get; set; }
        public int semanaslaborales { get; set; }
        public String nombreproyecto { get; set; }
    }

    public class socialSIGELA {
        public String identificacion { get; set; }
        public decimal horassemana { get; set; }
        public String resolucion { get; set; }
        public String unidad { get; set; }
        public String fechainicio { get; set; }
        public String fechafin { get; set; }
        public int semanaslaborales { get; set; }
        public String nombreproyecto { get; set; }
    }

    public class gestionSIGELA {
        public String identificacion { get; set; }
        public decimal horassemana { get; set; }
        public String unidad { get; set; }
        public int semanaslaborales { get; set; }
        public String nombrecargo { get; set; }
    }

    public class savedDataSIGELA {
        public String identificacion { get; set; } 
        public String nombre { get; set; }
        public String apellido { get; set; }
        public String email { get; set; } 
        public String tipolabor { get; set; }
        public String nombrelabor { get; set; }
    }

    // nuevo feberro 3

    public class otrasSIGELA
    {
        public String identificacion { get; set; }
        public String descripcion { get; set; }
        public decimal horassemana { get; set; }        
    }

}