using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[SubCategories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SubCategories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [CategoryId] int NOT NULL,
        CONSTRAINT [PK_SubCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]) ON DELETE CASCADE
    );
END;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_SubCategories_CategoryId'
      AND object_id = OBJECT_ID(N'[dbo].[SubCategories]')
)
BEGIN
    CREATE INDEX [IX_SubCategories_CategoryId] ON [dbo].[SubCategories] ([CategoryId]);
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[SubCategories]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[SubCategories];
END;
");
        }
    }
}
