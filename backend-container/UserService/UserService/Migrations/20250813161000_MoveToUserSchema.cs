using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    [Migration("20250813161000_MoveToUserSchema")]
    public partial class MoveToUserSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "user");

            // Transfer tables only if they currently exist in dbo and not already in user schema
            migrationBuilder.Sql(@"IF OBJECT_ID('[dbo].[Role]', 'U') IS NOT NULL AND OBJECT_ID('[user].[Role]', 'U') IS NULL
                                   ALTER SCHEMA [user] TRANSFER [dbo].[Role];");
            migrationBuilder.Sql(@"IF OBJECT_ID('[dbo].[User]', 'U') IS NOT NULL AND OBJECT_ID('[user].[User]', 'U') IS NULL
                                   ALTER SCHEMA [user] TRANSFER [dbo].[User];");
            migrationBuilder.Sql(@"IF OBJECT_ID('[dbo].[User_Profile]', 'U') IS NOT NULL AND OBJECT_ID('[user].[User_Profile]', 'U') IS NULL
                                   ALTER SCHEMA [user] TRANSFER [dbo].[User_Profile];");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Move back if needed
            migrationBuilder.Sql(@"IF OBJECT_ID('[user].[User_Profile]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[User_Profile]', 'U') IS NULL
                                   ALTER SCHEMA [dbo] TRANSFER [user].[User_Profile];");
            migrationBuilder.Sql(@"IF OBJECT_ID('[user].[User]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[User]', 'U') IS NULL
                                   ALTER SCHEMA [dbo] TRANSFER [user].[User];");
            migrationBuilder.Sql(@"IF OBJECT_ID('[user].[Role]', 'U') IS NOT NULL AND OBJECT_ID('[dbo].[Role]', 'U') IS NULL
                                   ALTER SCHEMA [dbo] TRANSFER [user].[Role];");
        }
    }
}
