using Microsoft.EntityFrameworkCore;

namespace PinPinServer.Models;

    public partial class PinPinContext : DbContext
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


