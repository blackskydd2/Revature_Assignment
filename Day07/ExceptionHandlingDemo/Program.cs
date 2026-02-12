// System.IndexOutOfRangeException

// var array = new int[] { 1, 2, 3, 4, 5 };

// var item = array[10];

// System.NullReferenceException:
// string message = null;

// Console.WriteLine(message.ToUpper());

// System.DivideByZeroException: Attempted to divide by zero.
// var numerator = 10;
// var denominator = 0;

// Console.WriteLine(numerator / denominator);

// System.IO.FileNotFoundException:
// var allText = System.IO.File.ReadAllText("No this file does not exist.txt");
// Console.WriteLine(allText);

// try
// {
//     var numerator = 10;
//     var denominator = 0;

//     var result = numerator / denominator;

//     Console.WriteLine($"Division result: {result}" );
// }
// // pefectly valid catch block
// // catch

// // catch (Exception ex) when (ex is DivideByZeroException || ex is NullReferenceException)
// catch (DivideByZeroException ex)
// {
//     Console.WriteLine("Cannot divide by zero. Please provide a non-zero denominator.");
//     Console.WriteLine($"Error details: {ex.Message}");
//     Console.WriteLine($"Stack Trace: {ex.StackTrace}");
// }

// Console.WriteLine("Hello, World!");
try
{
    First();
}
// catch
// {
// Empty catch type wont work
// }
catch (DivideByZeroException ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    // var numerator = 10;
    // var denominator = 0;

    // var result = numerator / denominator;
    // Console.WriteLine($"Division result: {result}");
}
// Generic Exceptions towards the end
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    Console.WriteLine($"Inner Exception: {ex.InnerException}");
}
finally
{
    // house keeping
    Console.WriteLine("finally.");
}

Console.WriteLine("Program continues after handling the exception.");

void First()
{
    Second();
}

void Second()
{
    try
    {
        Third();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception From Third: {ex.Message}");
        // Good rethrow
        // throw;

        // Bad rethrow
        // throw ex;
        // throw new Exception("My Exception here");
        throw new Exception("My Exception here", ex);
    }
}

void Third()
{
    var numerator = 10;
    var denominator = 0;

    var result = numerator / denominator;
}


void AcceptPayment(decimal amount, decimal balance)
{
    if (amount > balance)
    {
        throw new NotEnoughBalanceException("Not enough balance to complete the payment.");
    }

    Console.WriteLine("Payment accepted.");
}


class BankException : ApplicationException
{
    public BankException(string message) : base(message)
    {
    }

    public BankException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

class NotEnoughBalanceException : BankException
{
    public NotEnoughBalanceException(string message) : base(message)
    {
    }

    public NotEnoughBalanceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
