using System.Text.RegularExpressions;

namespace InkVault.Services
{
    public static class BrowserDetectionService
    {
        public static string GetBrowserName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown Browser";

            userAgent = userAgent.ToLower();

            // Check for specific browsers in order of specificity
            if (userAgent.Contains("edg/") || userAgent.Contains("edge/"))
                return "Microsoft Edge";
            
            if (userAgent.Contains("chrome/") && !userAgent.Contains("chromium"))
                return "Google Chrome";
            
            if (userAgent.Contains("safari/") && !userAgent.Contains("chrome"))
                return "Safari";
            
            if (userAgent.Contains("firefox/"))
                return "Mozilla Firefox";
            
            if (userAgent.Contains("opera/") || userAgent.Contains("opr/"))
                return "Opera";
            
            if (userAgent.Contains("trident/"))
                return "Internet Explorer";
            
            if (userAgent.Contains("ucbrowser"))
                return "UC Browser";
            
            if (userAgent.Contains("samsung"))
                return "Samsung Internet";

            // Extract from common pattern: BrowserName/Version
            var match = Regex.Match(userAgent, @"(\w+)/[\d.]+");
            if (match.Success)
                return match.Groups[1].Value;

            return "Unknown Browser";
        }
    }
}
