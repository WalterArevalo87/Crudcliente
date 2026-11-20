using System.ComponentModel.DataAnnotations;

namespace Cliente.Models
{
    public class ClienteModel
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Nombre Requerido")]
        [MinLength(3, ErrorMessage = "El Nombre debe tener Minimo 3 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Apellido Requerido")]
        [MinLength(3, ErrorMessage = "El Apellido debe tener Minimo 3 caracteres")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "Direccion Requerida")]
        [MinLength(3, ErrorMessage = "La Direccion debe tener Minimo 3 caracteres")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "Estado Civil Requerido")]
        [Display(Name = "Estado Civil")] // nombre que sale en la pagina
        public string EstadoCivil { get; set; }

        [Required(ErrorMessage = "Mail Requerido")]
        [EmailAddress(ErrorMessage = "El Mail no tiene un formato valido")]
        public string Mail { get; set; }

        [Required(ErrorMessage = "Sexo Requerido")]
        public string Sexo { get; set; }

        [Required(ErrorMessage = "Nacionalidad Requerida")]
        [MinLength(3, ErrorMessage = "La Nacionalidad debe tener Minimo 3 caracteres")]
        public string Nacionalidad { get; set; }

        [Required(ErrorMessage = "Educacion Requerida")]
        [MinLength(3, ErrorMessage = "La Educacion debe tener Minimo 3 caracteres")]
        public string Educacion { get; set; }

        [Required(ErrorMessage = "Tipo de Sangre Requerido")]
        [Display(Name = "Tipo de Sangre")] // nombre que sale en la pagina
        public string TipoSangre { get; set; }
    }
}
