using System.Diagnostics;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Async vs Sync Demo ===\n");

        Console.WriteLine("Running synchronous blocking demo...");
        ThreadDemo();

        Console.WriteLine("\nRunning async sequential demo...");
        await TaskDemoAsync();

        Console.WriteLine("\nRunning async parallel demo...");
        await TaskParallelDemoAsync();
    }

    // ---------------------
    // 1. Synchronous Blocking Code
    // ---------------------
    static void ThreadDemo()
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

            var response = _client.GetAsync(url).Result; // blocking
            var content = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"done. ({content.Length} chars)");
        }

        stopwatch.Stop();
        Console.WriteLine($"\nTotal time (Sync): {stopwatch.ElapsedMilliseconds}ms");
    }

    // ---------------------
    // 2. Async Sequential Code
    // ---------------------
    static async Task TaskDemoAsync()
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

            string content = await _client.GetStringAsync(url); // async await

            var threadAfter = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"done. ({content.Length} chars) [Thread {threadAfter}]");
        }

        stopwatch.Stop();
        Console.WriteLine($"\nTotal time (Async Sequential): {stopwatch.ElapsedMilliseconds}ms");
    }

    // ---------------------
    // 3. Async Parallel Code
    // ---------------------
    static async Task TaskParallelDemoAsync()
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
        Console.WriteLine($"\nTotal time (Async Parallel): {stopwatch.ElapsedMilliseconds}ms");
    }
}