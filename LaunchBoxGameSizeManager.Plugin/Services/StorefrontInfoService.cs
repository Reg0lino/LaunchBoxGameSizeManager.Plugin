// --- START OF FILE Services/StorefrontInfoService.cs ---
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Globalization; // For NumberStyles
using System.IO; // For Path, File
using System.Linq;
using System.Net.Http;
using System.Reflection; // For Assembly
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unbroken.LaunchBox.Plugins.Data; // For IGame
using LaunchBoxGameSizeManager.Utils; // For FileLogger and Constants

namespace LaunchBoxGameSizeManager.Services
{
    public class StorefrontInfoService
    {
        private static string RawgApiKey = null;
        public static bool IsApiKeyConfigured => !string.IsNullOrWhiteSpace(RawgApiKey);

        private static readonly HttpClient _httpClient = new HttpClient();
        private const string RawgApiBaseUrl = "https://api.rawg.io/api/";

        private class CacheEntry
        {
            public long? RequiredSpaceInBytes { get; }
            public DateTimeOffset CachedTime { get; }
            public string ErrorMessage { get; }

            public CacheEntry(long? requiredSpaceInBytes, string errorMessage = null)
            {
                RequiredSpaceInBytes = requiredSpaceInBytes;
                CachedTime = DateTimeOffset.UtcNow;
                ErrorMessage = errorMessage;
            }
        }

        private static readonly ConcurrentDictionary<string, CacheEntry> _apiCache = new ConcurrentDictionary<string, CacheEntry>();
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromDays(7);

        static StorefrontInfoService()
        {
            // This is essential info, always log
            FileLogger.Log("StorefrontInfoService: Static constructor called. Attempting to load API key...");
            LoadApiKey();
        }

        public StorefrontInfoService()
        {
            // This is essential info, always log
            FileLogger.Log("StorefrontInfoService: Instance constructor called.");
            if (!IsApiKeyConfigured)
            {
                FileLogger.Log("StorefrontInfoService: Instance created, but API Key is still not loaded/configured.");
            }
        }

        private static void LoadApiKey()
        {
            try
            {
                string executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(executingAssemblyLocation))
                {
                    // Essential error, always log
                    FileLogger.Log("StorefrontInfoService.LoadApiKey: Executing assembly location is null or empty. Cannot determine plugin directory.");
                    RawgApiKey = null;
                    return;
                }
                string pluginDirectory = Path.GetDirectoryName(executingAssemblyLocation);
                if (string.IsNullOrEmpty(pluginDirectory))
                {
                    // Essential error, always log
                    FileLogger.Log("StorefrontInfoService.LoadApiKey: Plugin directory is null or empty. Cannot determine API key file path.");
                    RawgApiKey = null;
                    return;
                }

                string apiKeyFilePath = Path.Combine(pluginDirectory, "RAWG_API_KEY.txt");
                // Info, but important for setup, always log
                FileLogger.Log($"StorefrontInfoService.LoadApiKey: Attempting to read API key from: {apiKeyFilePath}");

                if (File.Exists(apiKeyFilePath))
                {
                    string keyFromFile = File.ReadAllText(apiKeyFilePath).Trim();
                    if (!string.IsNullOrWhiteSpace(keyFromFile) && keyFromFile != "YOUR_RAWG_API_KEY_HERE")
                    {
                        RawgApiKey = keyFromFile;
                        // Essential success, always log
                        FileLogger.Log("StorefrontInfoService.LoadApiKey: RAWG API Key loaded successfully from file.");
                        return;
                    }
                    else
                    {
                        // Important warning, always log
                        FileLogger.Log("StorefrontInfoService.LoadApiKey: RAWG_API_KEY.txt found but is empty, whitespace, or placeholder.");
                        RawgApiKey = null;
                    }
                }
                else
                {
                    // Important warning, always log
                    FileLogger.Log($"StorefrontInfoService.LoadApiKey: RAWG_API_KEY.txt not found at expected location. Online features will be disabled.");
                    RawgApiKey = null;
                }
            }
            catch (Exception ex)
            {
                // Critical error, always log
                FileLogger.LogError("StorefrontInfoService.LoadApiKey: CRITICAL error loading API key from RAWG_API_KEY.txt.", ex);
                RawgApiKey = null;
            }

            if (!IsApiKeyConfigured)
            {
                // Summary status, always log
                FileLogger.Log("StorefrontInfoService.LoadApiKey: RAWG API Key is NOT configured after attempting to load. Storefront lookups will fail or be skipped.");
            }
        }

        public async Task<(long? requiredSpaceInBytes, string errorMessage)> GetEstRequiredDiskSpaceAsync(IGame game)
        {
            string gameTitleForLog = game?.Title ?? "NULL_GAME_OBJECT";
            // Basic operation log, keep for release
            FileLogger.Log($"StorefrontInfoService.GetEstRequiredDiskSpaceAsync: Called for game: '{gameTitleForLog}'.");

            if (!IsApiKeyConfigured)
            {
                // Important operational status, keep for release
                FileLogger.Log($"StorefrontInfoService.GetEstRequiredDiskSpaceAsync: RAWG API Key is not configured. Skipping API call for '{gameTitleForLog}'.");
                return (null, "API Key Not Configured");
            }

            string normalizedTitle = game.Title?.Trim();
            if (string.IsNullOrEmpty(normalizedTitle))
            {
                // Important data validation, keep for release
                FileLogger.Log("StorefrontInfoService.GetEstRequiredDiskSpaceAsync: Game title is empty or null. Cannot perform lookup.");
                return (null, "Game title is empty");
            }

            string cacheKey = $"RAWG_{normalizedTitle.ToLowerInvariant()}_pc";
            if (_apiCache.TryGetValue(cacheKey, out CacheEntry cachedEntry) && (DateTimeOffset.UtcNow - cachedEntry.CachedTime) < _cacheDuration)
            {
                // Cache hit is useful info, keep for release (not too verbose)
                FileLogger.Log($"StorefrontInfoService.GetEstRequiredDiskSpaceAsync: Cache HIT for '{normalizedTitle}'. Size: {cachedEntry.RequiredSpaceInBytes?.ToString() ?? "N/A"}, Error: {cachedEntry.ErrorMessage ?? "None"}");
                return (cachedEntry.RequiredSpaceInBytes, cachedEntry.ErrorMessage);
            }
            // Cache miss is start of an operation, keep for release
            FileLogger.Log($"StorefrontInfoService.GetEstRequiredDiskSpaceAsync: Cache MISS for '{normalizedTitle}'. Fetching from RAWG API.");

            (long? space, string error) result = await FetchFromRawgApi(normalizedTitle, "4", game);

            _apiCache[cacheKey] = new CacheEntry(result.space, result.error);
            // Result summary, keep for release
            FileLogger.Log($"StorefrontInfoService.GetEstRequiredDiskSpaceAsync: Result for '{normalizedTitle}'. Size: {result.space?.ToString() ?? "N/A"}, Error: {result.error ?? "None"}. Stored in cache.");
            return result;
        }

        private async Task<(long? requiredSpaceInBytes, string errorMessage)> FetchFromRawgApi(string gameTitle, string platformId, IGame originalGame)
        {
            // Basic operation log, keep for release
            FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Fetching '{gameTitle}' (Original LB Title: '{originalGame.Title}') for platform ID '{platformId}'.");

            string searchTitle = Regex.Replace(gameTitle, @"[^\w\s\-\:\']", "").Trim();
            searchTitle = Regex.Replace(searchTitle, @"\s+", " ");
#if DEBUG
            FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Sanitized search title: '{searchTitle}'."); // Verbose detail
#endif

            string requestUrlPrecise = $"{RawgApiBaseUrl}games?key={RawgApiKey}&search={Uri.EscapeDataString(searchTitle)}&search_precise=true&platforms={platformId}&page_size=1";

            try
            {
#if DEBUG
                FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Attempting precise search. URL: {requestUrlPrecise}"); // Verbose detail
#endif
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrlPrecise);

                if (!response.IsSuccessStatusCode)
                {
#if DEBUG
                    FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Precise search for '{searchTitle}' failed. Status: {response.StatusCode}."); // Verbose detail
#endif
                    string requestUrlBroad = $"{RawgApiBaseUrl}games?key={RawgApiKey}&search={Uri.EscapeDataString(searchTitle)}&platforms={platformId}&page_size=1";
#if DEBUG
                    FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Attempting broad search. URL: {requestUrlBroad}"); // Verbose detail
#endif
                    response = await _httpClient.GetAsync(requestUrlBroad);
                    if (!response.IsSuccessStatusCode)
                    {
                        // Error condition, always log
                        FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Broad search for '{searchTitle}' also failed. Status: {response.StatusCode}.");
                        return (null, $"API Search Error: {response.StatusCode}");
                    }
                }
#if DEBUG
                FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Search successful for '{searchTitle}'. Status: {response.StatusCode}."); // Verbose detail
#endif

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject parsedResponse = JObject.Parse(jsonResponse);

                var gameResults = parsedResponse["results"];
                if (gameResults == null || !gameResults.Any())
                {
                    // Important outcome, always log
                    FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: No game results found in API response for '{searchTitle}'.");
                    return (null, "Game Not Found in API");
                }

                JToken firstGame = gameResults.First();
                string gameSlug = firstGame["slug"]?.ToString();
                string foundGameTitle = firstGame["name"]?.ToString();
#if DEBUG
                FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Found game slug '{gameSlug}' (Title: '{foundGameTitle}') for search term '{searchTitle}'."); // Verbose detail
#endif

                if (string.IsNullOrEmpty(gameSlug))
                {
                    // Important outcome, always log
                    FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Game found for '{searchTitle}', but slug is missing from API response.");
                    return (null, "API Data Incomplete (slug)");
                }

                string detailsRequestUrl = $"{RawgApiBaseUrl}games/{gameSlug}?key={RawgApiKey}";
#if DEBUG
                FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Fetching details for slug '{gameSlug}'. URL: {detailsRequestUrl}"); // Verbose detail
#endif
                HttpResponseMessage detailsResponse = await _httpClient.GetAsync(detailsRequestUrl);

                if (!detailsResponse.IsSuccessStatusCode)
                {
                    // Error condition, always log
                    FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: RAWG API Error fetching details for '{gameSlug}'. Status: {detailsResponse.StatusCode}.");
                    return (null, $"API Details Error: {detailsResponse.StatusCode}");
                }

                string detailsJsonResponse = await detailsResponse.Content.ReadAsStringAsync();
                JObject parsedDetails = JObject.Parse(detailsJsonResponse);

                string bestRequirementsText = null;
                var platformsData = parsedDetails["platforms"] as JArray;
                if (platformsData != null)
                {
#if DEBUG
                    FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Processing platform data for '{gameSlug}'. Found {platformsData.Count} platforms."); // Verbose detail
#endif
                    foreach (var pformEntry in platformsData)
                    {
                        if (pformEntry["platform"]?["id"]?.ToString() == platformId)
                        {
#if DEBUG
                            FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Found matching platform ID '{platformId}' for '{gameSlug}'."); // Verbose detail
#endif
                            var requirements = pformEntry["requirements"];
                            if (requirements != null && requirements.Type != JTokenType.Null)
                            {
                                bestRequirementsText = requirements["minimum"]?.ToString();
                                if (string.IsNullOrWhiteSpace(bestRequirementsText))
                                {
                                    bestRequirementsText = requirements["recommended"]?.ToString();
                                }
#if DEBUG
                                FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Extracted requirements string for '{gameSlug}': '{bestRequirementsText ?? "NULL"}'."); // Verbose detail
#endif
                            }
                            else { FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: No 'requirements' object or it's null for platform '{platformId}' for '{gameSlug}'."); } // This is an important finding, keep.
                            break;
                        }
                    }
                }
                else { FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: No 'platforms' array in details for '{gameSlug}'."); } // Important finding, keep.

                long? bytes = null;
                if (!string.IsNullOrWhiteSpace(bestRequirementsText))
                {
                    bytes = ParseStorageRequirement(bestRequirementsText);
                }

                if (bytes.HasValue)
                {
                    // Success, keep for release
                    FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Successfully parsed size for '{gameTitle}' (using slug '{gameSlug}'): {bytes.Value} bytes.");
                    return (bytes, null);
                }
                else
                {
                    // Important outcome if parsing fails, keep for release
                    FileLogger.Log($"StorefrontInfoService.FetchFromRawgApi: Could not parse storage info for '{gameTitle}' (Slug: {gameSlug}) from available text fields. BestReqText was: '{bestRequirementsText ?? "NULL"}'");
                    return (null, "Size Info Not Found in API Data");
                }
            }
            catch (HttpRequestException ex)
            {
                FileLogger.LogError($"StorefrontInfoService.FetchFromRawgApi: HTTP Request Exception for '{gameTitle}'.", ex); // Always log errors
                return (null, "Network Error");
            }
            catch (JsonException ex)
            {
                FileLogger.LogError($"StorefrontInfoService.FetchFromRawgApi: JSON Parsing Exception for '{gameTitle}'.", ex); // Always log errors
                return (null, "API Response Parse Error");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"StorefrontInfoService.FetchFromRawgApi: General Exception for '{gameTitle}'.", ex); // Always log errors
                return (null, $"Unexpected API Error: {ex.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0]}");
            }
        }

        private long? ParseStorageRequirement(string requirementText)
        {
            if (string.IsNullOrWhiteSpace(requirementText)) return null;
#if DEBUG
            // Log full requirement text only in debug, as it can be large.
            string loggableRequirementText = requirementText.Replace("\n", "\\n").Replace("\r", "\\r");
            FileLogger.Log($"StorefrontInfoService.ParseStorageRequirement: Attempting to parse: '{loggableRequirementText.Substring(0, Math.Min(loggableRequirementText.Length, 200))}(...)'");
#endif

            var regex1 = new Regex(@"(\d+[\.,]?\d*)\s*(GB|MB)\s+available space", RegexOptions.IgnoreCase);
            Match match1 = regex1.Match(requirementText);
            if (match1.Success)
            {
                string valueStr = match1.Groups[1].Value.Replace(',', '.');
                string unit = match1.Groups[2].Value.ToUpper();
#if DEBUG
                FileLogger.Log($"StorefrontInfoService.ParseStorageRequirement: Pattern 1 ('available space') match. Value: '{valueStr}', Unit: '{unit}'.");
#endif
                if (decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                {
                    if (unit == "GB") return (long)(val * 1024 * 1024 * 1024);
                    if (unit == "MB") return (long)(val * 1024 * 1024);
                }
            }

            var regex2 = new Regex(@"(Storage|Disk Space|Hard Drive|HDD|SSD):\s*(\d+[\.,]?\d*)\s*(GB|MB)", RegexOptions.IgnoreCase);
            Match match2 = regex2.Match(requirementText);
            if (match2.Success)
            {
                string valueStr = match2.Groups[2].Value.Replace(',', '.');
                string unit = match2.Groups[3].Value.ToUpper();
#if DEBUG
                FileLogger.Log($"StorefrontInfoService.ParseStorageRequirement: Pattern 2 ('Storage:' label) match. Value: '{valueStr}', Unit: '{unit}'.");
#endif
                if (decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                {
                    if (unit == "GB") return (long)(val * 1024 * 1024 * 1024);
                    if (unit == "MB") return (long)(val * 1024 * 1024);
                }
            }

            var regex3 = new Regex(@"(?<!(Memory|RAM|VRAM)\s*:\s*)\b(\d+[\.,]?\d*)\s*(GB|MB)\b(?!\s*RAM)", RegexOptions.IgnoreCase);
            MatchCollection matches3 = regex3.Matches(requirementText);
            if (matches3.Count > 0)
            {
                foreach (Match m3 in matches3.Cast<Match>().OrderByDescending(m => m.Index))
                {
                    int matchStartIndex = m3.Index;
                    int searchWindowStart = Math.Max(0, matchStartIndex - 40);
                    string preContext = requirementText.Substring(searchWindowStart, matchStartIndex - searchWindowStart);
                    if (preContext.IndexOf("storage", StringComparison.OrdinalIgnoreCase) != -1 ||
                        preContext.IndexOf("disk", StringComparison.OrdinalIgnoreCase) != -1 ||
                        preContext.IndexOf("hard drive", StringComparison.OrdinalIgnoreCase) != -1 ||
                        preContext.IndexOf("hdd", StringComparison.OrdinalIgnoreCase) != -1 ||
                        preContext.IndexOf("ssd", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        string valueStr = m3.Groups[2].Value.Replace(',', '.');
                        string unit = m3.Groups[3].Value.ToUpper();
#if DEBUG
                        FileLogger.Log($"StorefrontInfoService.ParseStorageRequirement: Pattern 3 (general with context) match. Value: '{valueStr}', Unit: '{unit}'. PreContext: '{preContext}'");
#endif
                        if (decimal.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                        {
                            if (unit == "GB") return (long)(val * 1024 * 1024 * 1024);
                            if (unit == "MB") return (long)(val * 1024 * 1024);
                        }
                    }
                }
                Match firstGeneralMatch = matches3[0];
                string fValueStr = firstGeneralMatch.Groups[2].Value.Replace(',', '.');
                string fUnit = firstGeneralMatch.Groups[3].Value.ToUpper();
#if DEBUG
                FileLogger.Log($"StorefrontInfoService.ParseStorageRequirement: Pattern 3 (general, FALLBACK to first match) considered. Value: '{fValueStr}', Unit: '{fUnit}'.");
#endif
                if (decimal.TryParse(fValueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decVal))
                {
                    if (fUnit == "GB") return (long)(decVal * 1024 * 1024 * 1024);
                    if (fUnit == "MB") return (long)(decVal * 1024 * 1024);
                }
            }

            // This is an important outcome if all patterns fail, always log
            FileLogger.Log($"StorefrontInfoService.ParseStorageRequirement: No known storage pattern successfully extracted value. Input snippet: '{requirementText.Replace("\n", "\\n").Replace("\r", "\\r").Substring(0, Math.Min(requirementText.Length, 100))}(...)'.");
            return null;
        }
    }
}
// --- END OF FILE Services/StorefrontInfoService.cs ---