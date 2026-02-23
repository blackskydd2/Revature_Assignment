namespace EFLinq;

 public class Employee
{
    public int EmpNo { get; set; }
    public string EName { get; set; }
    public string Job { get; set; }
    public int? Mgr { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Sal { get; set; }
    public decimal? Comm { get; set; }

    // Foreign Key
    public int DeptNo { get; set; }
    public Department Department { get; set; }
}   

