using System.Diagnostics;

try
{
    await DemonstrateExceptions();
}
catch (AggregateException ex)
{
    Console.WriteLine($"Aggregate Exception: {ex.Message}");
}


async Task DemonstrateExceptions()
{
    using var _client = new HttpClient();

    var urls = new[]
    {
        "https://jsonplaceholder.typicode.com/posts/1",   // valid
        "https://this-does-not-exist.invalid/post/1",       // will fail
        "https://this-does-not-exist.invalid/post/2",       // will fail
        "https://jsonplaceholder.typicode.com/posts/3"    // valid
    };

    var tasks = urls.Select(url => _client.GetStringAsync(url)).ToList();

    try
    {
        Console.WriteLine($"Count: {tasks.Count}");
        // string[] results = await Task.WaitAll(tasks.ToArray());
        string[] results = await Task.WhenAll(tasks.ToArray());
        Console.WriteLine($"All {results.Length} succeeded.");
    }
    catch (HttpRequestException ex)
    {
        // Only the first exception is re-thrown by await
        Console.WriteLine($"At least one failed: {ex.Message}");

        // Inspect the Task directly for all exceptions
        foreach (var task in tasks.Where(t => t.IsFaulted))
        {
            Console.WriteLine($"  - {task.Exception?.InnerException?.Message}");
        }
    }

    foreach (var task in tasks)
    {

        if (task.IsCompletedSuccessfully)
        {
            Console.WriteLine($"Success: {task.Result.Length} chars");
        }
        else if (task.IsFaulted)
        {
            Console.WriteLine($"Failed: {task.Exception?.InnerException?.Message}");
        }
    }
}



async Task<int> Demos()
{

    await TaskParallelDemoAsync();
    await TaskDemoAsync();
    ThreadDemo();

    return await Task.FromResult(0);
}

async Task TaskParallelDemoAsync()
{
    using var _client = new HttpClient();

    var urls = Enumerable.Range(1, 10)
        .Select(i => $"https://jsonplaceholder.typicode.com/posts/{i}")
        .ToList();

    var stopwatch = Stopwatch.StartNew();

    var downloadTasks = urls.Select(async url =>
    {
        var threadBefore = Thread.CurrentThread.ManagedThreadId;

        string content = await _client.GetStringAsync(url);

        var threadAfter = Thread.CurrentThread.ManagedThreadId;

        Console.WriteLine($"Thread Before: {threadBefore} downloading {url}. ({content.Length} chars) [Thread After {threadAfter}]");

        return content;

    });

    string[] results = await Task.WhenAll(downloadTasks);

    stopwatch.Stop();
    Console.WriteLine($"\nTotal time: {stopwatch.ElapsedMilliseconds}ms");
}

async Task TaskDemoAsync()
{
    using var _client = new HttpClient();

    var urls = Enumerable.Range(1, 10)
        .Select(i => $"https://jsonplaceholder.typicode.com/posts/{i}")
        .ToList();

    var stopwatch = Stopwatch.StartNew();

    foreach (var url in urls)
    {
        var threadBefore = Thread.CurrentThread.ManagedThreadId;

        Console.Write($"[Thread {threadBefore}] Fetching {url}... ");

        string content = await _client.GetStringAsync(url);

        var threadAfter = Thread.CurrentThread.ManagedThreadId;

        Console.WriteLine($"done. ({content.Length} chars) [Thread {threadAfter}]");
    }

    stopwatch.Stop();
    Console.WriteLine($"\nTotal time: {stopwatch.ElapsedMilliseconds}ms");
}


void ThreadDemo()
{
    using var _client = new HttpClient();

    var urls = Enumerable.Range(1, 10)
        .Select(i => $"https://jsonplaceholder.typicode.com/posts/{i}")
        .ToList();

    var stopwatch = Stopwatch.StartNew();

    foreach (var url in urls)
    {
        int threadId = Thread.CurrentThread.ManagedThreadId;

        Console.Write($"[Thread {threadId}] Fetching {url}... ");

        var response = _client.GetAsync(url).Result;
        var content = response.Content.ReadAsStringAsync().Result;

        Console.WriteLine($"done. ({content.Length} chars)");
    }

    stopwatch.Stop();
    Console.WriteLine($"\nTotal time: {stopwatch.ElapsedMilliseconds}ms");

}



// using System.Diagnostics;

// class Program
// {
//     static async Task Main(string[] args)
//     {
//         Console.WriteLine("=== Async vs Sync Demo ===\n");

//         Console.WriteLine("Running synchronous blocking demo...");
//         ThreadDemo();

//         Console.WriteLine("\nRunning async sequential demo...");
//         await TaskDemoAsync();

//         Console.WriteLine("\nRunning async parallel demo...");
//         await TaskParallelDemoAsync();
//     }

//     // ---------------------
//     // 1. Synchronous Blocking Code
//     // ---------------------
//     static void ThreadDemo()
//     {
//         using var _client = new HttpClient();

//         var urls = Enumerable.Range(1, 10)
//             .Select(i => $"https://jsonplaceholder.typicode.com/posts/{i}")
//             .ToList();

//         var stopwatch = Stopwatch.StartNew();

//         foreach (var url in urls)
//         {
//             int threadId = Thread.CurrentThread.ManagedThreadId;
//             Console.Write($"[Thread {threadId}] Fetching {url}... ");

//             var response = _client.GetAsync(url).Result; // blocking
//             var content = response.Content.ReadAsStringAsync().Result;

//             Console.WriteLine($"done. ({content.Length} chars)");
//         }

//         stopwatch.Stop();
//         Console.WriteLine($"\nTotal time (Sync): {stopwatch.ElapsedMilliseconds}ms");
//     }

//     // ---------------------
//     // 2. Async Sequential Code
//     // ---------------------
//     static async Task TaskDemoAsync()
//     {
//         using var _client = new HttpClient();

//         var urls = Enumerable.Range(1, 10)
//             .Select(i => $"https://jsonplaceholder.typicode.com/posts/{i}")
//             .ToList();

//         var stopwatch = Stopwatch.StartNew();

//         foreach (var url in urls)
//         {
//             var threadBefore = Thread.CurrentThread.ManagedThreadId;
//             Console.Write($"[Thread {threadBefore}] Fetching {url}... ");

//             string content = await _client.GetStringAsync(url); // async await

//             var threadAfter = Thread.CurrentThread.ManagedThreadId;
//             Console.WriteLine($"done. ({content.Length} chars) [Thread {threadAfter}]");
//         }

//         stopwatch.Stop();
//         Console.WriteLine($"\nTotal time (Async Sequential): {stopwatch.ElapsedMilliseconds}ms");
//     }

//     // ---------------------
//     // 3. Async Parallel Code
//     // ---------------------
//     static async Task TaskParallelDemoAsync()
//     {
//         using var _client = new HttpClient();

//         var urls = Enumerable.Range(1, 10)
//             .Select(i => $"https://jsonplaceholder.typicode.com/posts/{i}")
//             .ToList();

//         var stopwatch = Stopwatch.StartNew();

//         var downloadTasks = urls.Select(async url =>
//         {
//             var threadBefore = Thread.CurrentThread.ManagedThreadId;

//             string content = await _client.GetStringAsync(url);

//             var threadAfter = Thread.CurrentThread.ManagedThreadId;
//             Console.WriteLine($"Thread Before: {threadBefore} downloading {url}. ({content.Length} chars) [Thread After {threadAfter}]");

//             return content;
//         });

//         string[] results = await Task.WhenAll(downloadTasks);

//         stopwatch.Stop();
//         Console.WriteLine($"\nTotal time (Async Parallel): {stopwatch.ElapsedMilliseconds}ms");
//     }
// }