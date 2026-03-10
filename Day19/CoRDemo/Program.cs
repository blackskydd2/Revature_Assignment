using System.Text;

// var message = "How are you?";
// SRP
// Encrypt
// StringBuilder encryptedMessage = new StringBuilder(20);

// foreach (var ch in message)
// {
//     encryptedMessage.Append(ch + 1);
// RSA
// }

// Console.WriteLine($"Encrypted Message: {encryptedMessage.ToString()}");

// Zip

// var zippedMessage = encryptedMessage.ToString();

// Email

// Console.WriteLine($"Sending encrypted message: {zippedMessage}");

// Chain of Reponsibility

// link all the handlers
var encryption = new RsaEncryption();
var archival = new Archival();
var email = new Email();
var pdf = new Pdf();

List<IHandler> handlers = new List<IHandler> {  email };

var message = "How are you?";

// chain
foreach (var handler in handlers)
{
    try
    {
        handler.Handle(message);
    }
    catch
    {

    }
}

interface IHandler
{
    void Handle(string message);
}

class Encryption : IHandler
{
    public void Handle(string message)
    {
        StringBuilder encryptedMessage = new StringBuilder(20);

        foreach (var ch in message)
        {
            encryptedMessage.Append(ch + 1);
        }
    }
}

class RsaEncryption : IHandler
{
    public void Handle(string message)
    {
        StringBuilder encryptedMessage = new StringBuilder(20);
    }
}
class Archival : IHandler
{
    public void Handle(string message)
    {
        Console.WriteLine($"Sending encrypted message: {message}");
    }
}

class Email : IHandler
{
    public void Handle(string message)
    {
        Console.WriteLine($"Sending encrypted message: {message}");
    }
}

class Pdf : IHandler
{
    public void Handle(string message)
    {
        Console.WriteLine($"Sending encrypted message: {message}");
    }
}

class CloudSave : IHandler
{
    public void Handle(string message)
    {
        Console.WriteLine($"Sending encrypted message: {message}");
    }
}