using System.Text.Json.Serialization;

namespace Editor.Config
{
    public sealed class Preferences
    {
        [JsonPropertyName("code")]
        public required CodeConfig CodeConfig { get; set; }
    }
}
