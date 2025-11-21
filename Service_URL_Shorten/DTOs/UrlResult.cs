namespace Service_URL_Shorten.DTOs
{
    public class UrlResult
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string Source { get; set; } = "Unknown";
        public int ClickCount { get; set; } = 0; 
    }
}