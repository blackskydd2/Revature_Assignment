using Microsoft.EntityFrameworkCore;

namespace EFLinq
{
    public class AppDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=.\\SQLEXPRESS;Database=EmpDeptDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasKey(e => e.EmpNo);

            modelBuilder.Entity<Department>()
                .HasKey(d => d.DeptNo);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DeptNo);

            modelBuilder.Entity<Department>().HasData(
                new Department { DeptNo = 10, DName = "ACCOUNTING", Loc = "NEW YORK" },
                new Department { DeptNo = 20, DName = "RESEARCH", Loc = "DALLAS" },
                new Department { DeptNo = 30, DName = "SALES", Loc = "CHICAGO" },
                new Department { DeptNo = 40, DName = "OPERATIONS", Loc = "BOSTON" }
            );

           modelBuilder.Entity<Employee>().HasData(
                new Employee { EmpNo = 7369, EName = "SMITH", Job = "CLERK", Mgr = 7902, HireDate = new DateTime(1980, 12, 17), Sal = 800, DeptNo = 20 },
                new Employee { EmpNo = 7499, EName = "ALLEN", Job = "SALESMAN", Mgr = 7698, HireDate = new DateTime(1981, 2, 20), Sal = 1600, Comm = 300, DeptNo = 30 },
                new Employee { EmpNo = 7521, EName = "WARD", Job = "SALESMAN", Mgr = 7698, HireDate = new DateTime(1981, 2, 22), Sal = 1250, Comm = 500, DeptNo = 30 },
                new Employee { EmpNo = 7566, EName = "JONES", Job = "MANAGER", Mgr = 7839, HireDate = new DateTime(1981, 4, 2), Sal = 2975, DeptNo = 20 },
                new Employee { EmpNo = 7654, EName = "MARTIN", Job = "SALESMAN", Mgr = 7698, HireDate = new DateTime(1981, 9, 28), Sal = 1250, Comm = 1400, DeptNo = 30 },
                new Employee { EmpNo = 7698, EName = "BLAKE", Job = "MANAGER", Mgr = 7839, HireDate = new DateTime(1981, 5, 1), Sal = 2850, DeptNo = 30 },
                new Employee { EmpNo = 7782, EName = "CLARK", Job = "MANAGER", Mgr = 7839, HireDate = new DateTime(1981, 6, 9), Sal = 2450, DeptNo = 10 },
                new Employee { EmpNo = 7788, EName = "SCOTT", Job = "ANALYST", Mgr = 7566, HireDate = new DateTime(1987, 4, 19), Sal = 3000, DeptNo = 20 },
                new Employee { EmpNo = 7839, EName = "KING", Job = "PRESIDENT", Mgr = null, HireDate = new DateTime(1981, 11, 17), Sal = 5000, DeptNo = 10 },
                new Employee { EmpNo = 7844, EName = "TURNER", Job = "SALESMAN", Mgr = 7698, HireDate = new DateTime(1981, 9, 8), Sal = 1500, Comm = 0, DeptNo = 30 },
                new Employee { EmpNo = 7876, EName = "ADAMS", Job = "CLERK", Mgr = 7788, HireDate = new DateTime(1987, 5, 23), Sal = 1100, DeptNo = 20 },
                new Employee { EmpNo = 7900, EName = "JAMES", Job = "CLERK", Mgr = 7698, HireDate = new DateTime(1981, 12, 3), Sal = 950, DeptNo = 30 },
                new Employee { EmpNo = 7902, EName = "FORD", Job = "ANALYST", Mgr = 7566, HireDate = new DateTime(1981, 12, 3), Sal = 3000, DeptNo = 20 },
                new Employee { EmpNo = 7934, EName = "MILLER", Job = "CLERK", Mgr = 7782, HireDate = new DateTime(1982, 1, 23), Sal = 1300, DeptNo = 10 }
            );

        }
    }
}
