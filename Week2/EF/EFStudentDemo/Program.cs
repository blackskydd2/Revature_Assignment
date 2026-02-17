using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SchoolEFCoreDemo
{
    class Program
    {
        public static void Main()
        {
            using var context = new SchoolContext();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Add sample student and course
            var student = new Student { Name = "Panna Patel", Age = 22 };
            var course = new Course { Title = "Database Schema", Credits = 4 };

            context.Students.Add(student);
            context.Courses.Add(course);
            context.SaveChanges();

            // Enroll student in course
            var enrollment = new Enrollment
            {
                StudentId = student.StudentId,
                CourseId = course.CourseId,
                Grade = "A"
            };

            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            // Query and print students with enrollments
            var query1 = context.Students.Include(s => s.Enrollments);
            foreach (var s in query1)
            {
                Console.WriteLine($"{s.Name}, {s.Age}");
            }
        }
    }

    public class SchoolContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Inline connection string
            optionsBuilder.UseSqlServer(
                "Server=.\\SQLEXPRESS;Database=SchoolDBDemo;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    public class Student
    {
        [Key]
        public int StudentId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }

    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        public string Title { get; set; }
        public int Credits { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }

    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public string Grade { get; set; }

        public Student Student { get; set; }
        public Course Course { get; set; }
    }
}