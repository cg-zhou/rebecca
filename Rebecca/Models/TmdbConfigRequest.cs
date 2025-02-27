namespace Rebecca.Models
{
    public class TmdbConfigRequest
    {
        public string BearerToken { get; set; } = string.Empty;
        public string? BaseApiUrl { get; set; }
        public string? BaseImageUrl { get; set; }
        public string? Language { get; set; }
        public string? ApiKeyType { get; set; }
    }
}