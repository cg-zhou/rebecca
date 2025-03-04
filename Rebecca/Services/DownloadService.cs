using Microsoft.Extensions.Logging;
using Rebecca.Services.Interfaces;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;

namespace Rebecca.Services;

/// <summary>
/// 下载服务，处理网络下载操作
/// </summary>
public class DownloadService : IDownloadService, IDisposable
{
    private readonly ILogger<DownloadService> _logger;
    private readonly HttpClient _httpClient;
    private const int MaxRetries = 3;

    public DownloadService(ILogger<DownloadService> logger)
    {
        _logger = logger;

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            CheckCertificateRevocationList = false
        };
        
        _httpClient = new HttpClient(handler);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadFileAsync(string url, CancellationToken cancellationToken)
    {
        for (int retry = 0; retry < MaxRetries; retry++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is IOException)
            {
                if (retry == MaxRetries - 1)
                {
                    _logger.LogError(ex, $"下载文件失败，已重试{MaxRetries}次。URL: {url}");
                    throw;
                }
                
                _logger.LogWarning($"下载尝试{retry + 1}失败，重试中... URL: {url}");
                await Task.Delay(TimeSpan.FromSeconds(2 * (retry + 1)), cancellationToken);
            }
        }

        // 这行代码实际上不会执行，但编译器需要它
        throw new InvalidOperationException("Unexpected code path");
    }

    /// <inheritdoc />
    public async Task DownloadToFileAsync(string url, string localPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        try
        {
            // 确保目标目录存在
            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var stream = await DownloadFileAsync(url, cancellationToken);
            using var fileStream = File.Create(localPath);
            await stream.CopyToAsync(fileStream, cancellationToken);
            
            _logger.LogInformation($"成功下载文件到: {localPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"下载文件到本地路径失败: {localPath}, URL: {url}");
            throw;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}