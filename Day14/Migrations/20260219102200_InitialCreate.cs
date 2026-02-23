using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EFLinq.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DeptNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Loc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DeptNo);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmpNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Job = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mgr = table.Column<int>(type: "int", nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeptNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmpNo);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DeptNo",
                        column: x => x.DeptNo,
                        principalTable: "Departments",
                        principalColumn: "DeptNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "DeptNo", "DName", "Loc" },
                values: new object[,]
                {
                    { 10, "ACCOUNTING", "NEW YORK" },
                    { 20, "RESEARCH", "DALLAS" },
                    { 30, "SALES", "CHICAGO" },
                    { 40, "OPERATIONS", "BOSTON" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmpNo", "Comm", "DeptNo", "EName", "HireDate", "Job", "Mgr", "Sal" },
                values: new object[,]
                {
                    { 7369, null, 20, "SMITH", new DateTime(1980, 12, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "CLERK", 7902, 800m },
                    { 7499, 300m, 30, "ALLEN", new DateTime(1981, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "SALESMAN", 7698, 1600m },
                    { 7521, 500m, 30, "WARD", new DateTime(1981, 2, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "SALESMAN", 7698, 1250m },
                    { 7566, null, 20, "JONES", new DateTime(1981, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "MANAGER", 7839, 2975m },
                    { 7654, 1400m, 30, "MARTIN", new DateTime(1981, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "SALESMAN", 7698, 1250m },
                    { 7698, null, 30, "BLAKE", new DateTime(1981, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MANAGER", 7839, 2850m },
                    { 7782, null, 10, "CLARK", new DateTime(1981, 6, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "MANAGER", 7839, 2450m },
                    { 7788, null, 20, "SCOTT", new DateTime(1987, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "ANALYST", 7566, 3000m },
                    { 7839, null, 10, "KING", new DateTime(1981, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "PRESIDENT", null, 5000m },
                    { 7844, 0m, 30, "TURNER", new DateTime(1981, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "SALESMAN", 7698, 1500m },
                    { 7876, null, 20, "ADAMS", new DateTime(1987, 5, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "CLERK", 7788, 1100m },
                    { 7900, null, 30, "JAMES", new DateTime(1981, 12, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "CLERK", 7698, 950m },
                    { 7902, null, 20, "FORD", new DateTime(1981, 12, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "ANALYST", 7566, 3000m },
                    { 7934, null, 10, "MILLER", new DateTime(1982, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "CLERK", 7782, 1300m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DeptNo",
                table: "Employees",
                column: "DeptNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
