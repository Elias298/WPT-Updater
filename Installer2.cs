using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class Installer
{
    private readonly HttpClient _httpClient = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<DownloadTask> _queue = new();
    private readonly ConcurrentDictionary<string, DownloadTask> _activeDownloads = new();
    public static string DownloadPath = GetDownloadPath();


    public static string GetDownloadPath()
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        string? path = ConfigurationManager.AppSettings["DownloadPath"];
        if (path == null) { return ""; }
        return path;
    }

    public static void SetDownloadPath(string path = "")
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["Downloadpath"].Value = path;
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }

    public Installer(int maxConcurrentDownloads)
    {
        _semaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
    }

    public async Task<bool> DownloadFileAsync(string url, string destination, IProgress<float>? progress = null)
    {
        var task = new DownloadTask(url, destination, progress);
        _queue.Enqueue(task);
        _ = ProcessQueueAsync();
        return await task.TaskCompletionSource.Task;
    }

    public void PauseDownload(string url)
    {
        if (_activeDownloads.TryGetValue(url, out var task))
        {
            task.Pause();
        }
    }

    public void ResumeDownload(string url)
    {
        if (_activeDownloads.TryGetValue(url, out var task))
        {
            _queue.Enqueue(task);
            _ = ProcessQueueAsync();
        }
    }

    public void CancelDownload(string url)
    {
        if (_activeDownloads.TryRemove(url, out var task))
        {
            task.Cancel();
        }
    }

    private async Task ProcessQueueAsync()
    {
        while (_queue.TryDequeue(out var task))
        {
            await _semaphore.WaitAsync();
            try
            {
                _activeDownloads[task.Url] = task;
                bool result = await task.StartDownloadAsync();
                task.TaskCompletionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading {task.Url}: {ex.Message}");
                task.TaskCompletionSource.SetException(ex);
            }
            finally
            {
                _activeDownloads.TryRemove(task.Url, out _);
                _semaphore.Release();
            }
        }
    }
}

public class DownloadTask
{
    public string Url { get; }
    public string Destination { get; }
    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new();
    private readonly IProgress<float>? _progress;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private long _downloadedBytes = 0;
    private bool _paused = false;

    public DownloadTask(string url, string destination, IProgress<float>? progress)
    {
        Url = url;
        Destination = destination;
        _progress = progress;
    }

    public async Task<bool> StartDownloadAsync()
    {
        using var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, Url);

        if (_downloadedBytes > 0)
        {
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(_downloadedBytes, null);
        }

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        if (totalBytes.HasValue)
        {
            totalBytes += _downloadedBytes;
        }

        using var fileStream = new FileStream(Destination, FileMode.Append, FileAccess.Write, FileShare.None);
        using var contentStream = await response.Content.ReadAsStreamAsync();

        byte[] buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
        {
            if (_paused)
            {
                await WaitUntilResumed();
            }

            await fileStream.WriteAsync(buffer, 0, bytesRead);
            _downloadedBytes += bytesRead;

            if (totalBytes.HasValue)
            {
                _progress?.Report((float)_downloadedBytes / totalBytes.Value);
            }
        }

        return true;
    }

    public void Pause()
    {
        _paused = true;
    }

    public void Resume()
    {
        _paused = false;
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }

    private async Task WaitUntilResumed()
    {
        while (_paused)
        {
            await Task.Delay(500);
        }
    }
}