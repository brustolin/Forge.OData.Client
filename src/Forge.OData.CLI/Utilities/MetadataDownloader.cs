namespace Forge.OData.CLI.Utilities;

public static class MetadataDownloader
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public static async Task<string> DownloadMetadata(string metadataUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(metadataUrl);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            
            // Validate that it's XML
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Received empty metadata from server");
            }

            if (!content.TrimStart().StartsWith("<?xml") && !content.TrimStart().StartsWith("<"))
            {
                throw new InvalidOperationException("Response does not appear to be valid XML metadata");
            }

            return content;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Failed to download metadata from {metadataUrl}: {ex.Message}", ex);
        }
    }
}
