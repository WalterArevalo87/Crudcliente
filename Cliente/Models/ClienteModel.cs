using System.ComponentModel.DataAnnotations;

namespace Cliente.Models
{
    public class ClienteModel
    {
        public int id { get; set; }
        [Required (ErrorMessage ="Nombre Reqerido")]
        [MinLength(3, ErrorMessage = "El Nombre debe tener Minimo 3 caracteres")]
        public string Nombre { get; set; } // necesito que se muestren las listas
        [Required(ErrorMessage = "Apellido Reqerido")]
        [MinLength(3, ErrorMessage = "El Apellido debe tener Minimo 3 caracteres")]
        public string Apellido { get; set; }
        [Required(ErrorMessage = "Apellido Reqerido")]
        [MinLength(3, ErrorMessage = "La Direccion debe tener Minimo 3 caracteres")]
        public string Direccion { get; set; }
        [Required(ErrorMessage = "Nombre Reqerido")]
        [Display(Name = "Estado Civil")] // nombre que sale en la pagina
        public string EstadoCivil { get; set; }
        [Required(ErrorMessage = "Mail Reqerido")]
        [MinLength(3, ErrorMessage = "La Mail debe tener Minimo 3 caracteres")]
        public string Mail { get; set; }
        [Required(ErrorMessage = "Sexo Reqerido")]
        [MinLength(3, ErrorMessage = "La Sexo debe tener Minimo 3 caracteres")]
        public string Sexo { get; set; }
        [Required(ErrorMessage = "Nacionalidad Reqerido")]
        [MinLength(3, ErrorMessage = "La Nacionalidad debe tener Minimo 3 caracteres")]
        public string Nacionalidad { get; set; }
        [Required(ErrorMessage = "Educacion Reqerido")]
        [MinLength(3, ErrorMessage = "La Eduaccion debe tener Minimo 3 caracteres")]
        public string Educacion { get; set; }
        [Required(ErrorMessage = "TipoSangres Reqerido")]
        [Display(Name = "Tipo de Sangre")] // nombre que sale en la pagina
        public string TipoSangre { get; set; }
    }
}
