using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PractitionerService.Data;

#nullable disable

namespace PractitionerService.Migrations
{
    [Migration("20250818170000_AddSpecializationServiceMapping")]
    [DbContext(typeof(PractitionerDbContext))]
    public partial class AddSpecializationServiceMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specialization_Service",
                schema: "practitioner",
                columns: table => new
                {
                    SpecializationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ServiceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialization_Service", x => new { x.SpecializationId, x.ServiceId });
                });

            // Seed a simple mapping by names where both exist
            migrationBuilder.Sql(@"
DECLARE @gpSpec nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='General Practitioner');
DECLARE @cardSpec nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='Cardiologist');
DECLARE @dermSpec nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Specialization WHERE Name='Dermatologist');
DECLARE @gpSvc nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='General Consultation');
DECLARE @cardSvc nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Cardiology Review');
DECLARE @dermSvc nvarchar(36) = (SELECT TOP 1 Id FROM practitioner.Service WHERE Name='Dermatology Check');
IF @gpSpec IS NOT NULL AND @gpSvc IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Specialization_Service WHERE SpecializationId=@gpSpec AND ServiceId=@gpSvc)
    INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@gpSpec, @gpSvc);
IF @cardSpec IS NOT NULL AND @cardSvc IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Specialization_Service WHERE SpecializationId=@cardSpec AND ServiceId=@cardSvc)
    INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@cardSpec, @cardSvc);
IF @dermSpec IS NOT NULL AND @dermSvc IS NOT NULL AND NOT EXISTS (SELECT 1 FROM practitioner.Specialization_Service WHERE SpecializationId=@dermSpec AND ServiceId=@dermSvc)
    INSERT INTO practitioner.Specialization_Service (SpecializationId, ServiceId) VALUES (@dermSpec, @dermSvc);
IF OBJECT_ID('practitioner.DoctorDirectory','V') IS NOT NULL EXEC sp_refreshview 'practitioner.DoctorDirectory';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Specialization_Service", schema: "practitioner");
        }
    }
}
