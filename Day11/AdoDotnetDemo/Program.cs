// Microsoft SQL Server
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

// mySQL
// using MySql.Data.SqlClient;

// // create console application builder
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsetting.json");

// Connection
// var connectionString = builder.GetConnectionString("CrmDbConnection");
var connectionString = builder.Build().GetConnectionString("master");

// for disposing connection object
//using (var connection = new SqlConnection(connectionString))
//{
//}

using var connection = new SqlConnection(connectionString);



try
{
    connection.Open();
    Console.WriteLine("Connection opened successfully.");
    // Execute Reader
    // ExecuteReader(connection);

    // Execute NonQuery
    // ExecuteNonQuery(connection);

    // Execute Scalar
    // ExecuteScalar(connection);

    // SQL Data Adapater
    // SqlDataAdapeterDemo(connection);

    // Insert Customer Demo
    InsertCustomerDemo(connection);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return;
}
finally
{
    connection.Close();
}

void InsertCustomerDemo(SqlConnection connection)
{
    var dataSet = new DataSet();
    var selectQuery = "SELECT * FROM Customers";
    using var selectCommand = new SqlCommand(selectQuery, connection);
    using var adapter = new SqlDataAdapter(selectCommand);
    adapter.Fill(dataSet, "Customers");

    var dataTable = dataSet.Tables["Customers"];

    var newRow = dataTable.NewRow();
    newRow["Id"] = 2;
    newRow["Name"] = "New Customer";
    newRow["Age"] = 28;



    adapter.InsertCommand = new SqlCommand("INSERT INTO Customers (Id, Name, Age) VALUES (@Id, @Name, @Age)", connection);

    adapter.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 6, "Id");
    adapter.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 50, "Name");
    adapter.InsertCommand.Parameters.Add("@Age", SqlDbType.Int, 0, "Age");

    dataSet.AcceptChanges();
}


void SqlDataAdapeterDemo(SqlConnection connection)
{
    var query = "SELECT * FROM Customers";
    SqlCommand sqlCommand = new(query, connection);
    using var selectAllCustomersCommand = sqlCommand;
    using var adapter = new SqlDataAdapter(selectAllCustomersCommand);
    var customerDataTable = new DataTable();

    adapter.Fill(customerDataTable);

    foreach (DataRow row in customerDataTable.Rows)
    {
        Console.WriteLine($"Id: {row["Id"]}, Name: {row["Name"]}, Age: {row["Age"]}");
    }
}

void ExecuteScalar(SqlConnection connection)
{
    var query = "SELECT COUNT(*) FROM Customers";
    using var command = new SqlCommand(query, connection);
    var count = (int)command.ExecuteScalar();
    Console.WriteLine($"Total customers: {count}");
}

void ExecuteReader(SqlConnection connection)
{
    var query = "SELECT * FROM Customers WHERE Age > 25";
    using var command = new SqlCommand(query, connection);
    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        Console.WriteLine($"Id: {reader["Id"]}, Name: {reader["Name"]}, Age: {reader["Age"]}");
    }
}

void ExecuteNonQuery(SqlConnection connection)
{
    var query = "INSERT INTO Customers (Id, Name, Age) VALUES (1, 'Danny', 30)";
    using var command = new SqlCommand(query, connection);
    var rowsAffected = command.ExecuteNonQuery();
    Console.WriteLine($"Rows affected: {rowsAffected}");
}