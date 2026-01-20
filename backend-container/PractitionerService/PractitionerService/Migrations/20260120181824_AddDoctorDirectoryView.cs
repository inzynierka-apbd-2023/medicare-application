using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PractitionerService.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorDirectoryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW practitioner.DoctorDirectory AS
                SELECT
                    d.Id AS DoctorId,
                    d.UserId,
                    CAST(NULL AS nvarchar(100)) AS FirstName,
                    CAST(NULL AS nvarchar(100)) AS LastName,
                    CAST(NULL AS nvarchar(255)) AS Email,
                    CAST(NULL AS nvarchar(50)) AS Phone,
                    (
                        SELECT STRING_AGG(CAST(ds.SpecializationId AS nvarchar(50)), ',')
                        FROM practitioner.Doctor_Specialization ds
                        WHERE ds.DoctorId = d.Id
                    ) AS Specializations,
                    CAST(NULL AS nvarchar(max)) AS Services,
                    d.IsActive
                FROM practitioner.Doctor d
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS practitioner.DoctorDirectory");
        }
    }
}
