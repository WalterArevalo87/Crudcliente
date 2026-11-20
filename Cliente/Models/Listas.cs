namespace Cliente.Models
{
    // Opciones que se muestran en las listas desplegables del formulario de Cliente
    public static class Listas
    {
        public static readonly string[] EstadosCiviles =
            { "Soltero", "Casado", "Divorciado", "Viudo", "Union Libre" };

        public static readonly string[] Sexos =
            { "Masculino", "Femenino", "Otro" };

        public static readonly string[] TiposSangre =
            { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
    }
}
