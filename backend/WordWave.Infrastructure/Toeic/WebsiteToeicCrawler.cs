using System.Text.Json;
using WordWave.Application.Contracts.Toeic;
using WordWave.Application.Interfaces;

namespace WordWave.Infrastructure.Toeic;

public class WebsiteToeicCrawler : IToeicDataSourceCrawler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly HttpClient _httpClient;
    private readonly IToeicImportPackageWriter _writer;

    public WebsiteToeicCrawler(HttpClient httpClient, IToeicImportPackageWriter writer)
    {
        _httpClient = httpClient;
        _writer = writer;
    }

    public async Task<ToeicRawData> CrawlAsync(string keyword, string sourceUrl, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("sourceUrl must be an absolute HTTP or HTTPS URL.");
        }

        if (!await IsAllowedByRobotsAsync(uri, cancellationToken))
        {
            throw new InvalidOperationException("Crawler skipped this source because robots.txt disallows access.");
        }

        using var response = await _httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        return new ToeicRawData
        {
            SourceType = "website",
            SourceName = uri.Host,
            SourceUrl = sourceUrl,
            Payload = payload
        };
    }

    public Task<ToeicImportPackage> NormalizeAsync(ToeicRawData rawData, CancellationToken cancellationToken = default)
    {
        var package = JsonSerializer.Deserialize<ToeicImportPackage>(rawData.Payload, JsonOptions);
        if (package is null)
        {
            throw new InvalidDataException("The crawled source must expose TOEIC data as JSON using the WordWave import schema.");
        }

        if (string.IsNullOrWhiteSpace(package.SourceType)) package.SourceType = rawData.SourceType;
        if (string.IsNullOrWhiteSpace(package.SourceName)) package.SourceName = rawData.SourceName;
        if (string.IsNullOrWhiteSpace(package.SourceUrl)) package.SourceUrl = rawData.SourceUrl;

        return Task.FromResult(package);
    }

    public Task<ToeicImportResultDto> SaveAsync(ToeicImportPackage normalizedData, CancellationToken cancellationToken = default)
    {
        return _writer.SaveAsync(normalizedData, cancellationToken);
    }

    private async Task<bool> IsAllowedByRobotsAsync(Uri targetUri, CancellationToken cancellationToken)
    {
        var robotsUri = new Uri($"{targetUri.Scheme}://{targetUri.Host}/robots.txt");
        try
        {
            using var response = await _httpClient.GetAsync(robotsUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return true;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return IsPathAllowed(content, targetUri.AbsolutePath);
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    private static bool IsPathAllowed(string robotsText, string path)
    {
        var appliesToAll = false;
        foreach (var rawLine in robotsText.Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator < 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (key.Equals("user-agent", StringComparison.OrdinalIgnoreCase))
            {
                appliesToAll = value == "*";
                continue;
            }

            if (appliesToAll && key.Equals("disallow", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (value == "/" || path.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
