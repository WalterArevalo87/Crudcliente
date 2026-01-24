namespace Cliente.Models
{
    using Microsoft.EntityFrameworkCore;
    public class clientedbcontext : DbContext
    {
        public clientedbcontext(DbContextOptions db) : base(db)
        {
            
        }

        public DbSet < AtmsModel> atms { get; set; }

        public DbSet <ClienteModel> Clientes { get; set; }
    }
}
