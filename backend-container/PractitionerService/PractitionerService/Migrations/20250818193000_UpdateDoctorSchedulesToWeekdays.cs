using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PractitionerService.Data;

#nullable disable

namespace PractitionerService.Migrations
{
    [Migration("20250818193000_UpdateDoctorSchedulesToWeekdays")]
    [DbContext(typeof(PractitionerDbContext))]
    public partial class UpdateDoctorSchedulesToWeekdays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Ensure each doctor has a Mon–Fri 09:00–17:00 schedule if missing/incomplete
DECLARE cur CURSOR FAST_FORWARD FOR SELECT Id FROM practitioner.Doctor;
DECLARE @doc nvarchar(36);
OPEN cur;
FETCH NEXT FROM cur INTO @doc;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @cnt INT = (SELECT COUNT(*) FROM practitioner.Doctor_Schedule WHERE DoctorId=@doc);
    IF @cnt < 5
    BEGIN
        DELETE FROM practitioner.Doctor_Schedule WHERE DoctorId=@doc;
        INSERT INTO practitioner.Doctor_Schedule (DoctorId, DayOfWeek, StartTime, EndTime) VALUES
            (@doc,1,'09:00','17:00'),
            (@doc,2,'09:00','17:00'),
            (@doc,3,'09:00','17:00'),
            (@doc,4,'09:00','17:00'),
            (@doc,5,'09:00','17:00');
    END
    FETCH NEXT FROM cur INTO @doc;
END
CLOSE cur; DEALLOCATE cur;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op rollback
        }
    }
}
