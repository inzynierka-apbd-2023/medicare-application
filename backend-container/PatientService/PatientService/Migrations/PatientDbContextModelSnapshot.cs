using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PatientService.Data;

namespace PatientService.Migrations
{
    [DbContext(typeof(PatientDbContext))]
    partial class PatientDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("PatientService.Models.Patient", b => { });
            modelBuilder.Entity("PatientService.Models.EmergencyContact", b => { });
            modelBuilder.Entity("PatientService.Models.Insurance", b => { });
            modelBuilder.Entity("PatientService.Models.PatientStatus", b => { });
            modelBuilder.Entity("PatientService.Models.PatientOverview", b => { });
#pragma warning restore 612, 618
        }
    }
}
