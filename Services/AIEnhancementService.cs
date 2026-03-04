using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InkVault.Services
{
    public interface IAIEnhancementService
    {
        Task<string?> EnhanceContentAsync(string content);
    }

    public class AIEnhancementService : IAIEnhancementService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIEnhancementService> _logger;
        private readonly HttpClient _httpClient;
        private static readonly Dictionary<string, (string result, DateTime timestamp)> _cache = new();
        private static readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(10); // Cache for 10 minutes

        public AIEnhancementService(IConfiguration configuration, ILogger<AIEnhancementService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string?> EnhanceContentAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("Content cannot be empty");

            // Check minimum word count (4+ words)
            var wordCount = content.Trim().Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount < 4)
            {
                // Return original text unchanged if less than 4 words
                _logger.LogInformation($"Content has only {wordCount} words (minimum 4 required). Returning unchanged.");
                return content.Trim();
            }

            // Check cache first to avoid unnecessary API calls
            var cacheKey = content.Trim().ToLowerInvariant();
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                if (DateTime.Now - cached.timestamp < _cacheExpiry)
                {
                    Console.WriteLine($"[CACHE] ✅ Using cached result (age: {(DateTime.Now - cached.timestamp).TotalMinutes:F1}m)");
                    return cached.result;
                }
                else
                {
                    _cache.Remove(cacheKey); // Remove expired cache
                }
            }

            // Get API key
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            }

            Console.WriteLine($"[AI] API Key Status: {(string.IsNullOrEmpty(apiKey) ? "NOT SET ❌" : "SET ✅")}");
            Console.WriteLine($"[AI] Word Count: {wordCount}");
            Console.WriteLine($"[AI] Content Preview: {content.Substring(0, Math.Min(50, content.Length))}...");

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            Console.WriteLine($"[AI] Environment: {environment}");

            string result;

            // ALWAYS try Gemini API first for comprehensive grammar checking
            if (!string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("[AI] Using Gemini API for comprehensive grammar enhancement...");

                try
                {
                    var enhancedContent = await EnhanceWithGeminiAsync(content, apiKey, isRetry: false);

                    // Validate that output is different and meaningful
                    if (IsSimilarContent(content, enhancedContent))
                    {
                        _logger.LogWarning("Gemini returned similar text to input. Retrying with stronger instruction...");
                        enhancedContent = await EnhanceWithGeminiAsync(content, apiKey, isRetry: true);

                        // If still similar after retry, apply fallback as additional layer
                        if (IsSimilarContent(content, enhancedContent))
                        {
                            _logger.LogWarning("Gemini retry still returned similar text. Using fallback enhancement.");
                            result = await FallbackEnhanceAsync(content);
                        }
                        else
                        {
                            result = enhancedContent;
                        }
                    }
                    else
                    {
                        result = enhancedContent;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AI] Gemini API Error: {ex.Message}");

                    // Check if it's a rate limit error
                    if (ex.Message.Contains("429") || ex.Message.Contains("RESOURCE_EXHAUSTED"))
                    {
                        Console.WriteLine("[AI] ⚠️ Rate limit detected. Using fallback enhancement for now.");
                        Console.WriteLine("[AI] 💡 Tip: Wait 1-5 minutes between tests or check your daily quota at https://aistudio.google.com/");
                        result = await FallbackEnhanceAsync(content);
                    }
                    else
                    {
                        // Continue to fallback for other errors
                        Console.WriteLine("[AI] Falling back to programmatic enhancement due to API error");
                        result = await FallbackEnhanceAsync(content);
                    }
                }
            }
            else
            {
                Console.WriteLine("[AI] WARNING: Gemini API key not configured. Using fallback programmatic enhancement.");
                Console.WriteLine("[AI] For comprehensive grammar checking, please set GEMINI_API_KEY environment variable.");
                result = await FallbackEnhanceAsync(content);
            }

            // Cache the result
            if (!string.IsNullOrEmpty(result))
            {
                _cache[cacheKey] = (result, DateTime.Now);
                Console.WriteLine($"[CACHE] 💾 Result cached for future use");
            }

            return result;
        }

        /// <summary>
        /// Check if two texts are too similar (minimal changes)
        /// More lenient for fallback enhancement
        /// </summary>
        private bool IsSimilarContent(string original, string? enhanced)
        {
            if (string.IsNullOrWhiteSpace(enhanced))
            {
                _logger.LogWarning("Enhanced content is empty or whitespace");
                return true;
            }

            var orig = original.Trim().ToLowerInvariant();
            var enh = enhanced.Trim().ToLowerInvariant();

            // Exact match
            if (orig == enh)
            {
                _logger.LogWarning("Enhanced content is identical to original");
                return true;
            }

            // Check if output is suspiciously shorter (likely truncated or error)
            if (enhanced.Length < original.Length * 0.5)
            {
                _logger.LogWarning($"Enhanced content is too short ({enhanced.Length} vs {original.Length} chars)");
                return true;
            }

            // Calculate character-level similarity (more lenient approach)
            var levenshteinDistance = CalculateLevenshteinDistance(orig, enh);
            var maxLength = Math.Max(orig.Length, enh.Length);
            var similarity = 1.0 - (double)levenshteinDistance / maxLength;

            _logger.LogInformation($"Content similarity: {similarity:P1} (distance: {levenshteinDistance}/{maxLength})");

            // Consider it too similar only if 95% or more similar (very lenient for fallback)
            return similarity >= 0.95;
        }

        /// <summary>
        /// Calculate Levenshtein distance for better similarity measurement
        /// </summary>
        private int CalculateLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target.Length;
            if (string.IsNullOrEmpty(target)) return source.Length;

            var distance = new int[source.Length + 1, target.Length + 1];

            // Initialize first column and row
            for (int i = 0; i <= source.Length; i++)
                distance[i, 0] = i;
            for (int j = 0; j <= target.Length; j++)
                distance[0, j] = j;

            // Calculate distance
            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[source.Length, target.Length];
        }

        /// <summary>
        /// Fallback enhancement using simplified, reliable word replacement and grammar fixes
        /// Now ensures meaningful changes are always applied
        /// </summary>
        private async Task<string> FallbackEnhanceAsync(string content)
        {
            await Task.Delay(100);

            Console.WriteLine($"[FALLBACK] Starting fallback enhancement for: '{content.Substring(0, Math.Min(50, content.Length))}...'");
            _logger.LogInformation("Applying fallback programmatic enhancement");

            var enhanced = content.Trim();
            var originalLength = enhanced.Length;
            var changesApplied = 0;

            // STEP 1: Basic grammar fixes (most reliable)
            var beforeGrammar = enhanced;
            enhanced = ApplyBasicGrammarFixes(enhanced);
            if (beforeGrammar != enhanced) changesApplied++;
            Console.WriteLine($"[FALLBACK] After grammar fixes: '{enhanced.Substring(0, Math.Min(50, enhanced.Length))}...'");

            // STEP 2: Simple vocabulary improvements
            var beforeVocab = enhanced;
            enhanced = ApplySimpleVocabularyImprovements(enhanced);
            if (beforeVocab != enhanced) changesApplied++;
            Console.WriteLine($"[FALLBACK] After vocab improvements: '{enhanced.Substring(0, Math.Min(50, enhanced.Length))}...'");

            // STEP 3: Fix capitalization
            var beforeCaps = enhanced;
            enhanced = FixBasicCapitalization(enhanced);
            if (beforeCaps != enhanced) changesApplied++;
            Console.WriteLine($"[FALLBACK] After capitalization: '{enhanced.Substring(0, Math.Min(50, enhanced.Length))}...'");

            // STEP 4: If very few changes were made, apply additional safe enhancements
            if (changesApplied < 2)
            {
                Console.WriteLine($"[FALLBACK] Minimal changes detected ({changesApplied}), applying additional enhancements...");
                enhanced = ApplySafeAdditionalEnhancements(enhanced);
            }

            var changePercent = Math.Abs(enhanced.Length - originalLength) / (double)originalLength * 100;
            Console.WriteLine($"[FALLBACK] ✅ Fallback complete. Changes applied: {changesApplied}, Length change: {changePercent:F1}%");

            _logger.LogInformation("Fallback enhancement complete.");

            return enhanced;
        }

        /// <summary>
        /// Aggressive fallback with comprehensive changes (kept for future use but not auto-triggered)
        /// </summary>
        private async Task<string> AggressiveFallbackEnhanceAsync(string content)
        {
            await Task.Delay(100);

            Console.WriteLine($"[AGGRESSIVE] Starting aggressive fallback for: '{content.Substring(0, Math.Min(50, content.Length))}...'");
            _logger.LogInformation("Applying AGGRESSIVE fallback enhancement");

            var enhanced = content.Trim();

            // Apply all standard enhancements first
            enhanced = ApplyBasicGrammarFixes(enhanced);
            enhanced = ApplySimpleVocabularyImprovements(enhanced);
            enhanced = ApplySafeAdditionalEnhancements(enhanced);

            // More aggressive vocabulary changes
            enhanced = ApplyAggressiveVocabularyChanges(enhanced);
            Console.WriteLine($"[AGGRESSIVE] After aggressive vocab: '{enhanced.Substring(0, Math.Min(50, enhanced.Length))}...'");

            // Fix capitalization
            enhanced = FixBasicCapitalization(enhanced);

            Console.WriteLine($"[AGGRESSIVE] ✅ Aggressive fallback complete");

            return enhanced;
        }

        /// <summary>
        /// Apply only the most reliable grammar fixes with better accuracy
        /// </summary>
        private string ApplyBasicGrammarFixes(string text)
        {
            // Fix "I" capitalization
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bi\b", "I");

            // Fix basic article errors (a/an) - more careful approach
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\ba\s+([aeiouAEIOU]\w*)", "an $1");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\ban\s+([bcdfghjklmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ]\w*)", "a $1");

            // Fix common contractions (only missing apostrophes)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bcant\b", "can't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bdont\b", "don't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bwont\b", "won't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bdidnt\b", "didn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bcouldnt\b", "couldn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bwouldnt\b", "wouldn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bisnt\b", "isn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\barent\b", "aren't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bwasnt\b", "wasn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bwerent\b", "weren't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bhavent\b", "haven't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bhasnt\b", "hasn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Basic subject-verb agreement (only the most obvious cases)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(I|you|we|they)\s+was\b", "$1 were", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(he|she|it)\s+were\b", "$1 was", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Fix obvious have/has errors
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(I|you|we|they)\s+has\b", "$1 have", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(he|she|it)\s+have\b", "$1 has", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Fix common its/it's confusion (conservative approach)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bits\s+(a|an|the|going|been|time|very|really|not)\b", "it's $1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bit's\s+(own|tail|color|size|name|purpose)\b", "its $1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Fix there/their/they're (only obvious cases)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\btheir\s+(is|are|was|were)\b", "there $1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bthere\s+(car|house|dog|cat|book|phone|computer)\b", "their $1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Fix your/you're
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\byour\s+(going|coming|being|having)\b", "you're $1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\byoure\b", "you're", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return text;
        }

        /// <summary>
        /// Apply simple, safe vocabulary improvements - only clear upgrades
        /// </summary>
        private string ApplySimpleVocabularyImprovements(string text)
        {
            var improvements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Only very safe, clear improvements
                { "very good", "excellent" },
                { "really good", "wonderful" },
                { "very bad", "terrible" },
                { "really bad", "awful" },
                { "a lot of", "many" },
                { "lots of", "numerous" },
                { "kind of", "somewhat" },
                { "sort of", "rather" },
                { "maybe", "perhaps" },
                { "okay", "fine" },
                { "alright", "fine" },
                { "went to", "visited" },
                { "came to", "arrived at" },
                { "got", "received" }
            };

            foreach (var kvp in improvements)
            {
                text = System.Text.RegularExpressions.Regex.Replace(
                    text,
                    $@"\b{System.Text.RegularExpressions.Regex.Escape(kvp.Key)}\b",
                    kvp.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return text;
        }

        /// <summary>
        /// Apply more vocabulary changes without breaking grammar
        /// </summary>
        private string ApplyAggressiveVocabularyChanges(string text)
        {
            var aggressiveChanges = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // More vocabulary improvements, but still safe
                { "want to", "wish to" },
                { "need to", "must" },
                { "I guess", "I believe" },
                { "I think maybe", "I believe" },
                { "pretty good", "quite good" },
                { "pretty bad", "quite poor" },
                { "big", "large" },
                { "small", "little" },
                { "nice", "pleasant" },
                { "cool", "interesting" },
                { "awesome", "wonderful" },
                { "weird", "unusual" },
                { "stuff", "things" },
                { "guy", "person" }
            };

            foreach (var kvp in aggressiveChanges)
            {
                text = System.Text.RegularExpressions.Regex.Replace(
                    text,
                    $@"\b{System.Text.RegularExpressions.Regex.Escape(kvp.Key)}\b",
                    kvp.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return text;
        }

        /// <summary>
        /// Apply safe additional enhancements when minimal changes were detected
        /// </summary>
        private string ApplySafeAdditionalEnhancements(string text)
        {
            Console.WriteLine("[FALLBACK] Applying additional safe enhancements...");

            // Additional vocabulary improvements (more comprehensive but still safe)
            var additionalImprovements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "good", "excellent" },
                { "bad", "poor" },
                { "big", "large" },
                { "small", "tiny" },
                { "fast", "quick" },
                { "slow", "gradual" },
                { "happy", "delighted" },
                { "sad", "disappointed" },
                { "easy", "simple" },
                { "hard", "difficult" },
                { "old", "vintage" },
                { "new", "fresh" },
                { "clean", "spotless" },
                { "dirty", "soiled" },
                { "hot", "warm" },
                { "cold", "cool" },
                { " and ", " and also " },
                { "but", "however" },
                { "so", "therefore" },
                { "because", "since" },
                { "also", "additionally" },
                { "then", "subsequently" },
                { "now", "currently" },
                { "here", "at this location" },
                { "there", "at that location" }
            };

            var changesMade = 0;
            foreach (var kvp in additionalImprovements)
            {
                var before = text;
                text = System.Text.RegularExpressions.Regex.Replace(
                    text,
                    $@"\b{System.Text.RegularExpressions.Regex.Escape(kvp.Key)}\b",
                    kvp.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                if (before != text)
                {
                    changesMade++;
                    // Limit changes to avoid over-enhancement
                    if (changesMade >= 3) break;
                }
            }

            // Add transitional phrases to improve flow (if text is long enough)
            if (text.Length > 100 && !text.Contains("Additionally") && !text.Contains("Furthermore"))
            {
                // Find sentences and add transitions
                if (text.Contains(". ") && System.Text.RegularExpressions.Regex.Matches(text, @"\.").Count >= 2)
                {
                    text = System.Text.RegularExpressions.Regex.Replace(
                        text,
                        @"(\.\s+)([A-Z]\w+\s+\w+)",
                        "$1Additionally, $2",
                        System.Text.RegularExpressions.RegexOptions.None,
                        TimeSpan.FromMilliseconds(100)
                    );
                }
            }

            Console.WriteLine($"[FALLBACK] Applied {changesMade} additional enhancements");
            return text;
        }

        /// <summary>
        /// Fix basic capitalization issues without breaking existing proper capitalization
        /// </summary>
        private string FixBasicCapitalization(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Capitalize first letter if it's lowercase
            if (char.IsLower(text[0]))
            {
                text = char.ToUpper(text[0]) + text.Substring(1);
            }

            // Capitalize after sentence endings (but be careful not to break abbreviations)
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"([.!?])\s+([a-z])",
                m => m.Groups[1].Value + " " + char.ToUpper(m.Groups[2].Value[0])
            );

            // Add ending punctuation if missing and text doesn't end with punctuation
            if (!System.Text.RegularExpressions.Regex.IsMatch(text, @"[.!?]\s*$"))
            {
                text = text.TrimEnd() + ".";
            }

            // Fix spacing around punctuation
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+([.!?])", "$1");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"([.!?])([A-Za-z])", "$1 $2");

            // Capitalize common abbreviations (but only if they're clearly abbreviations)
            var techAbbrevs = new[] { "api", "url", "html", "css", "js", "json", "http", "https", "ai", "ui", "ux" };
            foreach (var abbr in techAbbrevs)
            {
                text = System.Text.RegularExpressions.Regex.Replace(
                    text,
                    $@"\b{abbr}\b",
                    abbr.ToUpper(),
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return text;
        }

        private string ApplyGrammarFixes(string text)
        {
            // STEP 1: Fix article usage (a/an) FIRST - before any capitalization
            // Fix "a" + vowel sound → "an" + vowel sound
            text = System.Text.RegularExpressions.Regex.Replace(
                text, 
                @"\ba\s+([aeiouAEIOU])", 
                "an $1", 
                System.Text.RegularExpressions.RegexOptions.None
            );

            // Fix "an" + consonant sound → "a" + consonant sound
            // But exclude words starting with silent H
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"\ban\s+(?![aeiouAEIOU]|hour|honest|honor|heir)(\w)",
                "a $1",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // STEP 2: Fix possessive "its" vs contraction "it's" (SPECIFIC patterns only)
            // Only replace "its" when it's clearly a contraction context
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"\bits\s+(a|an|the|very|really|so|not|been|my|your|his|her|our|their)\s+",
                "it's $1 ",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // STEP 3: Subject-verb agreement
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(I|you|we|they)\s+was\b", "$1 were", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(he|she|it)\s+were\b", "$1 was", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Fix have/has
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(I|you|we|they)\s+has\b", "$1 have", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(he|she|it)\s+have\b", "$1 has", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // STEP 4: Fix "I" capitalization
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bi\b", "I");

            // STEP 5: Fix common contractions (missing apostrophes)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bcant\b", "can't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bdont\b", "don't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bwont\b", "won't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bdidnt\b", "didn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bcouldnt\b", "couldn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bwouldnt\b", "wouldn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bshouldnt\b", "shouldn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bisnt\b", "isn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\barent\b", "aren't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bwasnt\b", "wasn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bwerent\b", "weren't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bhavent\b", "haven't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bhasnt\b", "hasn't", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // STEP 6: Fix their/there/they're confusion
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\btheir\s+(is|are|was|were)\b", "there $1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\bthere\s+(cat|dog|book|car|house|friend)\b", "their $1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // STEP 7: Fix to/too confusion
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\b(me|us|him|her|them)\s+to\s*([.!?])", "$1 too$2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return text;
        }

        private string CapitalizeAbbreviations(string text)
        {
            var abbreviations = new[] {
                "api", "url", "html", "css", "js", "sql", "xml", "json", "http", "https",
                "ui", "ux", "ai", "ml", "iot", "rest", "soap", "crud", "ajax",
                "sdk", "ide", "gui", "cli", "os", "cpu", "gpu", "ram", "ssd"
            };

            foreach (var abbr in abbreviations)
            {
                text = System.Text.RegularExpressions.Regex.Replace(
                    text, 
                    $@"\b{abbr}\b", 
                    abbr.ToUpper(), 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return text;
        }

        private string FixSentenceStructure(string text)
        {
            // Capitalize first letter of sentences
            if (!string.IsNullOrEmpty(text) && char.IsLower(text[0]))
            {
                text = char.ToUpper(text[0]) + text.Substring(1);
            }

            // Capitalize after periods
            text = System.Text.RegularExpressions.Regex.Replace(
                text, 
                @"([.!?])\s+([a-z])", 
                m => m.Groups[1].Value + " " + char.ToUpper(m.Groups[2].Value[0])
            );

            // Fix spacing around punctuation
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+([.,!?;:])", "$1");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"([.!?;:])\s*([A-Z])", "$1 $2");

            // Ensure proper ending punctuation
            if (!text.EndsWith(".") && !text.EndsWith("!") && !text.EndsWith("?"))
            {
                text += ".";
            }

            return text;
        }

        /// <summary>
        /// Advanced demo enhancement for development - comprehensive grammar and style improvements
        /// </summary>
        private async Task<string> DemoEnhanceAsync(string content)
        {
            await Task.Delay(500); // Simulate API call delay

            if (string.IsNullOrWhiteSpace(content))
                return content;

            var enhanced = content.Trim();

            // STEP 1: Fix major grammatical errors

            // Subject-verb agreement fixes
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\b(cats|dogs|birds|people|they|connections|issues|problems)\s+is\b", "$1 are", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\b(cat|dog|bird|he|she|it|connection|issue|problem|database|server|system)\s+are\b", "$1 is", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bthere\s+was\s+(many|several|lots|multiple)", "there were $1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Fix "have/has" agreement
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\b(I|you|we|they|cats|dogs|people|systems|connections|issues|problems)\s+has\b", "$1 have", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\b(he|she|it|connection|database|server|system|api|endpoint|code|bug)\s+have\b", "$1 has", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Fix "love/loves" agreement
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\b(I|you|we|they|cats|dogs|people)\s+loves\b", "$1 love", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\b(he|she|it|my\s+\w+)\s+love\b", "$1 loves", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // STEP 2: Fix common word confusions

            // Fix "to/too" confusion at end of sentences
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bto\s*([.!?])\s*$", "too$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bissues\s+to\s*([.!?])", "issues too$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bproblems\s+to\s*([.!?])", "problems too$1", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Fix "their/there" confusion
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\btheir\s+is\b", "there is", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\btheir\s+are\b", "there are", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\btheir\s+was\b", "there was", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\btheir\s+were\b", "there were", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // STEP 3: Fix tense consistency
            // Convert past tense mixups
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\byesterday\s+i\s+(\w+)\b", match => {
                var verb = match.Groups[1].Value.ToLower();
                return verb switch {
                    "go" => "yesterday I went",
                    "wake" => "yesterday I woke",
                    "eat" => "yesterday I ate",
                    "see" => "yesterday I saw",
                    "come" => "yesterday I came",
                    _ => match.Value
                };
            }, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // STEP 4: Add missing articles and prepositions
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bwent\s+to\s+store\b", "went to the store", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bin\s+park\b", "in the park", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bweather\s+was\b", "the weather was", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bteacher\s+gave\b", "the teacher gave", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // STEP 5: Apply comprehensive grammar fixes (includes article, its/it's, contractions)
            enhanced = ApplyGrammarFixes(enhanced);

            // STEP 6: Fix common abbreviations (make them uppercase)
            enhanced = CapitalizeAbbreviations(enhanced);

            // STEP 7: Vocabulary improvements
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bokay\b", "acceptable", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bi guess\b", "I believe", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bmaybe\b", "perhaps", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\breally good\b", "excellent", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\ba lot\b", "many", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bwent to\b", "visited", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bgot\b", "received", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bgood\b", "well", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // STEP 8: Improve sentence structure
            // Join short related sentences
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\.\s+Nothing\s+special\s+happened", ", and nothing special happened");
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\.\s+Feeling\s+tired", ". I've been feeling tired");
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\.\s+Need\s+to\b", ". I need to");

            // Break up potential run-on sentences (simple heuristic)
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\s+and\s+(\w+\s+){8,}", match => {
                var trimmed = match.Value.Trim();
                if (trimmed.Length > 4)
                {
                    return ". " + char.ToUpper(trimmed[4]) + trimmed.Substring(5);
                }
                return match.Value;
            });

            // STEP 9: Fix sentence structure and capitalization
            enhanced = FixSentenceStructure(enhanced);

            // STEP 10: Final improvements for specific patterns
            enhanced = System.Text.RegularExpressions.Regex.Replace(enhanced, @"\bneither\s+(\w+)\s+nor\s+(\w+)\s+are\b", "neither $1 nor $2 is", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            Console.WriteLine($"[AI DEMO] Original: {content}");
            Console.WriteLine($"[AI DEMO] Enhanced: {enhanced}");
            return enhanced;
        }

        private async Task<string?> EnhanceWithGeminiAsync(string content, string apiKey, bool isRetry = false)
        {
            try
            {
                Console.WriteLine($"[GEMINI] Starting API call - Content: {content.Substring(0, Math.Min(100, content.Length))}...");
                Console.WriteLine($"[GEMINI] API Key available: {!string.IsNullOrEmpty(apiKey)}");

                // Use stronger prompt for retry
                var prompt = isRetry ? GetStrongEnhancementPrompt(content) : GetStandardEnhancementPrompt(content);

                var requestBody = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                    generationConfig = new { 
                        temperature = isRetry ? 0.7 : 0.4, // Adjusted temperature for better results
                        maxOutputTokens = 2048,
                        topP = 0.8,
                        topK = 40
                    }
                };

                // Updated to use Gemini 2.5 Flash (latest and most compatible)
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

                var jsonContent = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"[GEMINI] Request JSON length: {jsonContent.Length}");

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                // Add headers
                request.Headers.Add("User-Agent", "InkVault/1.0");

                Console.WriteLine($"[GEMINI] Sending {(isRetry ? "RETRY" : "INITIAL")} request...");

                using var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[GEMINI] Status: {response.StatusCode}");
                Console.WriteLine($"[GEMINI] Response length: {responseText.Length}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[GEMINI] ❌ ERROR Response: {responseText}");

                    // Enhanced error parsing
                    try
                    {
                        using var errorDoc = JsonDocument.Parse(responseText);
                        if (errorDoc.RootElement.TryGetProperty("error", out var error))
                        {
                            var code = error.TryGetProperty("code", out var codeEl) ? codeEl.GetInt32() : (int)response.StatusCode;
                            var message = error.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "Unknown error";

                            Console.WriteLine($"[GEMINI] Error Code: {code}, Message: {message}");

                            // Handle specific errors
                            if (code == 429 || message?.Contains("quota", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                throw new Exception("API quota exceeded. Please try again later or check your API key limits.");
                            }
                            if (code == 403 || code == 401)
                            {
                                throw new Exception("API key authentication failed. Please check your Gemini API key.");
                            }
                            if (message?.Contains("SAFETY", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                throw new Exception("Content blocked by safety filters. Text appears safe, this might be a temporary issue.");
                            }

                            throw new Exception($"Gemini API error: {message} (Code: {code})");
                        }
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("[GEMINI] Could not parse error response as JSON");
                    }

                    throw new Exception($"API request failed with status {response.StatusCode}: {responseText}");
                }

                Console.WriteLine("[GEMINI] ✅ Success! Parsing response...");

                try
                {
                    using var doc = JsonDocument.Parse(responseText);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];

                        // Check if content was blocked
                        if (candidate.TryGetProperty("finishReason", out var finishReason))
                        {
                            var reason = finishReason.GetString();
                            if (reason == "SAFETY")
                            {
                                Console.WriteLine("[GEMINI] ⚠️ Content blocked by safety filters");
                                throw new Exception("Content was blocked by safety filters. This is likely a temporary issue.");
                            }
                        }

                        if (candidate.TryGetProperty("content", out var contentObj))
                        {
                            if (contentObj.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                            {
                                if (parts[0].TryGetProperty("text", out var textProp))
                                {
                                    var enhanced = textProp.GetString()?.Trim();
                                    if (!string.IsNullOrEmpty(enhanced))
                                    {
                                        Console.WriteLine($"[GEMINI] ✅ Enhanced text received! Length: {enhanced.Length}");
                                        Console.WriteLine($"[GEMINI] Preview: {enhanced.Substring(0, Math.Min(100, enhanced.Length))}...");
                                        return enhanced;
                                    }
                                }
                            }
                        }
                    }

                    Console.WriteLine("[GEMINI] ❌ No valid text found in response structure");
                    Console.WriteLine($"[GEMINI] Full response: {responseText}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[GEMINI] ❌ JSON parsing error: {ex.Message}");
                    Console.WriteLine($"[GEMINI] Raw response: {responseText}");
                    throw new Exception($"Invalid JSON response from API: {ex.Message}");
                }

                throw new Exception("No enhanced text found in API response");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GEMINI] ❌ EXCEPTION: {ex.Message}");
                _logger.LogError(ex, "Gemini API call failed");
                throw;
            }
        }

        private string GetStandardEnhancementPrompt(string content)
        {
            return $@"Fix all grammar errors and improve readability in this text. Make natural improvements but keep the original tone and meaning.

REQUIREMENTS:
1. Fix ALL grammar errors (subject-verb agreement, articles, tenses, contractions)
2. Improve vocabulary naturally (very good → excellent, a lot → many)  
3. Keep the original meaning and conversational tone
4. Return ONLY the corrected text, no explanations

Text: {content}";
        }

        private string GetStrongEnhancementPrompt(string content)
        {
            return $@"IMPORTANT: The previous attempt was too similar. Make MORE significant improvements while fixing all errors.

MANDATORY FIXES:
- Grammar: Fix ALL errors (articles, subject-verb, contractions, tenses)
- Vocabulary: Upgrade weak words (very good→excellent, maybe→perhaps, got→received)
- Style: Improve sentence flow and readability
- Keep the original meaning but make it sound more polished

Original text: {content}

Return ONLY the enhanced text:";
        }
    }
}
