using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PractitionerService.Data;

namespace PractitionerService.Migrations
{
    [Migration("20250820130500_AddIsActiveAndUpdateDirectory")]
    [DbContext(typeof(PractitionerDbContext))]
    public partial class AddIsActiveAndUpdateDirectory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "practitioner",
                table: "Doctor",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW practitioner.DoctorDirectory AS
    SELECT d.Id AS DoctorId,
           d.UserId,
           up.FirstName,
           up.LastName,
           up.Email,
           up.Phone,
           STUFF((
               SELECT ',' + ds.SpecializationId
               FROM practitioner.Doctor_Specialization ds
               WHERE ds.DoctorId = d.Id
               FOR XML PATH(''), TYPE
           ).value('.','NVARCHAR(MAX)'), 1, 1, '') AS Specializations,
           NULL AS Services,
           d.IsActive
    FROM practitioner.Doctor d
    LEFT JOIN [user].[User_Profile] up ON up.User_Id = d.UserId;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert view
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW practitioner.DoctorDirectory AS
    SELECT d.Id AS DoctorId,
           d.UserId,
           up.FirstName,
           up.LastName,
           up.Email,
           up.Phone,
           STUFF((
               SELECT ',' + ds.SpecializationId
               FROM practitioner.Doctor_Specialization ds
               WHERE ds.DoctorId = d.Id
               FOR XML PATH(''), TYPE
           ).value('.','NVARCHAR(MAX)'), 1, 1, '') AS Specializations,
           NULL AS Services
    FROM practitioner.Doctor d
    LEFT JOIN [user].[User_Profile] up ON up.User_Id = d.UserId;
            ");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "practitioner",
                table: "Doctor");
        }
    }
}
