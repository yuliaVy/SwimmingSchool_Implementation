namespace SwimmingSchool_Implementation.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SwimmingSchool_Implementation.Models.SwimSchoolDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "SwimmingSchool_Implementation.Models.SwimSchoolDbContext";
        }

        protected override void Seed(SwimmingSchool_Implementation.Models.SwimSchoolDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
