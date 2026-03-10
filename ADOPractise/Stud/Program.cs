using System;
using System.Data.SqlClient;


namespace Stud;

class Program
{
    static string conn = @"Data Source=DESKTOP-KLCVA4H\SQLEXPRESS;
                                Integrated Security=True;
                                Persist Security Info=False;
                                Pooling=False;
                                MultipleActiveResultSets=False;
                                Encrypt=True;
                                TrustServerCertificate=True";

    static void Main(String [] args)
    {
    }

    static void InsertStudent(string name, int age, string course)
    {
        using (SqlConnection conn = new SqlConnection(conn))
        {
            conn.Open();
            string query = "INSERT INTO STUDENTS (NAME, AGE, COURSE) VALUES (@name, @age, @course)";
            SqlCommand cmd = new SqlCommand(string query, conn);
            cmd.Parameters.AddWithValue(@Name, name);
            cmd.Parameters.AddWithValue(@Age, age);
            cmd.Parameters.AddWithValue(@Course, course);
            cmd.ExecuteNonQuery();
            System.Console.WriteLine("Student inserted Sucessfully");
        }
    }

    static void ReadStudents()
    {
        using (SqlConnection conn = new (conn))
        {
            conn.Open();
            string query = "select * from student";
            SqlCommand cmd = new (query, conn)
            SqlDataReader rader = cmd.ExecuteReader();
            System.Console.WriteLine("Records===============");
            while (PEReader.read())
            {
                
            }
        }
    }



}