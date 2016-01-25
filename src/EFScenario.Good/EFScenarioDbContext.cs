namespace EFScenario.Bad
{
    using EFScenario.Bad.Model;
    using System.Data.Entity;

    public class EFScenarioDbContext : DbContext
    {
        public EFScenarioDbContext()
            : base("name=EFScenarioDbContext")
        {
        }

        public EFScenarioDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Database.SetInitializer<EFScenarioDbContext>(new DropCreateDatabaseAlways<EFScenarioDbContext>());
        }

        public virtual DbSet<Category> Categories { get; set; }

        public virtual DbSet<Product> Products { get; set; }
    }
}