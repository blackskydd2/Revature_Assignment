namespace EFLinq
{
    public class Department
    {
        public int DeptNo { get; set; }
        public string DName { get; set; } = null!;
        public string Loc { get; set; } = null!;

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
    }
}
