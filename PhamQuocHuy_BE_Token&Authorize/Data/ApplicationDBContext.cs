using Microsoft.EntityFrameworkCore;
using PhamQuocHuy_BE_Token_Authorize.Model;

namespace PhamQuocHuy_BE_Token_Authorize.Data
{
    public class ApplicationDBContext: DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
    }
}
