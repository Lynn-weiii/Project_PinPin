using Microsoft.EntityFrameworkCore;

namespace IdentityJWT.Models;

    public partial class PinPinRegisterContext : DbContext
{

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot Configuration = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json")
                        .Build();
                optionsBuilder.UseSqlServer(
                    Configuration.GetConnectionString("PinPinSQL")
                    );
            }
        }

    }


