Console.WriteLine("Fetching...");
// var data = FetchData();  // App frozen for 2 seconds
var data = await FetchDataAsync();  // App frozen for 2 seconds
Console.WriteLine($"Got: {data}");

string FetchData()
{
    Thread.Sleep(5000);  // Freezes for 2 seconds!
    return "Data";
}

async Task<string> FetchDataAsync()
{
    await Task.Delay(5000);  // Does NOT freeze the app!
    return "Data";
}