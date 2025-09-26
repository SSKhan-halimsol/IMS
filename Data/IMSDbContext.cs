using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Data
{
    public class IMSDbContext : DbContext
    {
        public IMSDbContext() : base("name=IMSConnection")
        {
        }
        public DbSet<IMS.Models.Applicant> Applicants { get; set; }
    }

}
