
using static Eloi.Constants;

namespace Eloi.Models {
    public sealed record EloiSettings {
        public required string BaseUrl { get; set; }
        public required string ChatModel { get; set; }
        public required string EmbedModel { get; set; }
        public required string Memories { get; set; }
        public required string EloiModelfile { get; set; }
    }
}
