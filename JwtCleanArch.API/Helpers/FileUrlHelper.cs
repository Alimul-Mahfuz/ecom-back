namespace JwtCleanArch.API.Helpers
{
    public class FileUrlHelper
    {
        /// <summary>
        /// Generates a fully-qualified public URL for a file.
        /// </summary>
        /// <param name="baseUrl">The base URL of your API (e.g., https://localhost:7096)</param>
        /// <param name="relativePath">The relative file path inside Storage folder (e.g., UserProfiles/file.jpg)</param>
        /// <returns>Full URL accessible via browser</returns>
        public static string GeneratePublicUrl(string relativePath, string baseUrl = "https://localhost:7096")
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Relative path cannot be null or empty.", nameof(relativePath));

            // Normalize URL slashes
            var urlPath = relativePath.Replace("\\", "/").TrimStart('/');

            // Ensure baseUrl has no trailing slash
            var trimmedBaseUrl = baseUrl.TrimEnd('/');

            return $"{trimmedBaseUrl}/files/{urlPath}";
        }
    }
}
