using IMS.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace IMS.Data
{
    public class DbInitializer : CreateDatabaseIfNotExists<IMSDbContext>
    {
        protected override void Seed(IMSDbContext context)
        {
            // ✅ Seed default admin
            if (!context.User.Any())
            {
                context.User.Add(new User
                {
                    Username = "admin",
                    Password = "admin123"
                });
            }

            // ✅ Optionally seed sample quiz and designation
            if (!context.Designations.Any())
            {
                context.Designations.Add(new Designation { Title = ".NET Developer" });
            }

            context.SaveChanges();
            base.Seed(context);
        }
    }
}