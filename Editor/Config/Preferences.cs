using System.Text.Json.Serialization;

namespace Editor.Config
{
    public sealed class Preferences
    {
        [JsonIgnore]
        public static bool IsDirty { get; set; } = false;

        [JsonPropertyName("code")]
        public required CodeConfig CodeConfig { get; set; }
    }
}
