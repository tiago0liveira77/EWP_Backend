using EWP_API_WEB_APP.Models.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EWP_API_WEB_APP.Data
{
    public class ApplicationDbContext : IdentityDbContext<Users>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
            //****************************************************
            //Criação das tabelas da BD
            //****************************************************
            public DbSet<Events> Events { get; set; }
            public DbSet<Tickets> Tickets { get; set; }
            public DbSet<Users> Users { get; set; }
            public DbSet<Wristbands> Wristbands { get; set; }
            public DbSet<TicketSales> TicketSales { get; set; }
            public DbSet<EventsType> EventsType { get; set; }
    }
}