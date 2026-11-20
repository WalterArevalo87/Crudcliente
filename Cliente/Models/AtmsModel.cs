using System.ComponentModel.DataAnnotations;

namespace Cliente.Models
{
    public class AtmsModel
    {
        public int id  { get; set; }
        [Required(ErrorMessage = "Nombre Requerido")]
        [MinLength(3, ErrorMessage = "El Nombre de la agencia debe tener Minimo 3 caracteres")]
        [Display(Name = "Nombre de la Agencia")] 
        public string NombreAgencia { get; set; }
        // Relaciones entre dos tablas
        [Display(Name = "Cliente")] // nombre que sale en la pagina
        public int ClienteModelid { get; set; }
        public ClienteModel ClienteModel { get; set; }

    }
}
