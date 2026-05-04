using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace SwimmingSchool_Implementation.Models
{
    public class SwimSchoolDbContext : IdentityDbContext<User>
    {
        public SwimSchoolDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DatabaseInitializer());
        }

        public static SwimSchoolDbContext Create()
        {
            return new SwimSchoolDbContext();
        }
    }
}