using IMS.Models;
using System.Data.Entity;
using System.Linq;

namespace IMS.Data
{
    public class DbInitializer : CreateDatabaseIfNotExists<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            EnsureAdmin(context);
            base.Seed(context);
        }
        public static void Initialize()
        {
            using (var context = new AppDbContext())
            {
                context.Database.Initialize(force: true);
                EnsureAdmin(context);
            }
        }

        private static void EnsureAdmin(AppDbContext context)
        {
            const string adminUsername = "admin";
            const string adminPassword = "admin123";
            if (context.Set<Admin>().Any(a => a.Username == adminUsername))
                return;

            var adminEntity = new Admin();
            var adminType = typeof(Admin);
            var hasPasswordHash = adminType.GetProperty("PasswordHash") != null;
            var hasPassword = adminType.GetProperty("Password") != null;

            if (hasPasswordHash)
            {
                adminEntity.GetType().GetProperty("Username").SetValue(adminEntity, adminUsername);
                adminEntity.GetType().GetProperty("PasswordHash").SetValue(adminEntity, adminPassword);
            }
            else if (hasPassword)
            {
                adminEntity.GetType().GetProperty("Username").SetValue(adminEntity, adminUsername);
                adminEntity.GetType().GetProperty("Password").SetValue(adminEntity, adminPassword);
            }
            else
            {
                throw new System.InvalidOperationException("Admin model does not contain 'Password' or 'PasswordHash' properties. Update the Admin class to include one of these.");
            }

            context.Set<Admin>().Add(adminEntity);
            context.SaveChanges();
        }
    }
}