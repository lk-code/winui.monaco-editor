using System.Text.Json.Serialization;

namespace Monaco
{
    /// <summary>
    /// Represents information about a code language.
    /// </summary>
    public record CodeLanguage
    {
        /// <summary>
        /// The unique identifier of the code language.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        /// <summary>
        /// The file extensions associated with this code language.
        /// </summary>
        [JsonPropertyName("extensions")]
        public string[]? Extensions { get; init; }

        /// <summary>
        /// The filenames associated with this code language.
        /// </summary>
        [JsonPropertyName("filenames")]
        public string[]? Filenames { get; init; }

        /// <summary>
        /// The filename patterns associated with this code language.
        /// </summary>
        [JsonPropertyName("filenamePatterns")]
        public string[]? FilenamePatterns { get; init; }

        /// <summary>
        /// The first line of code in this code language.
        /// </summary>
        [JsonPropertyName("firstLine")]
        public string? FirstLine { get; init; }

        /// <summary>
        /// The alternative names for this code language.
        /// </summary>
        [JsonPropertyName("aliases")]
        public string[]? Aliases { get; init; }

        /// <summary>
        /// The MIME types associated with this code language.
        /// </summary>
        [JsonPropertyName("mimetypes")]
        public string[]? MimeTypes { get; init; }

        /// <summary>
        /// The configuration settings for this code language.
        /// </summary>
        [JsonPropertyName("configuration")]
        public string? Configuration { get; init; }
    }
}
