using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using Microsoft.EntityFrameworkCore.Infrastructure;
using PractitionerService.Data;

namespace PractitionerService.Migrations
{
    [Migration("20250813162000_UpdateDoctorDirectoryView_UserSchema")]
    [DbContext(typeof(PractitionerDbContext))]
    public partial class UpdateDoctorDirectoryViewUserSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate referencing dbo for rollback
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
    LEFT JOIN dbo.[User_Profile] up ON up.User_Id = d.UserId;



            ");
        }
    }
}
