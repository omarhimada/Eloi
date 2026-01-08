using Eloi.Models;
using Eloi.Models.Classes;
using Ollama;

namespace Eloi.Services.Http {
    public sealed class EloiClient {
        private readonly HttpClient _http;
        private readonly EloiSettings _settings;

        public EloiClient(HttpClient http, EloiSettings settings) {
            _http = http;
            _settings = settings;
        }

        public async Task<EloiResponse?> UpsertChunkAsync(EloiRequest request, CancellationToken cancellationToken) {
            HttpResponseMessage responseMessage = await _http.PostAsJsonAsync(
                Constants.RetrievalAugmentSQLite._upsertChunkCommandText,
                request
            );

            if (!responseMessage.IsSuccessStatusCode) {
                return new EloiResponse {
                    Done = true
                };
            }

            EloiResponse? result = await responseMessage.Content.ReadFromJsonAsync<EloiResponse>(cancellationToken);

            return result;
        }

        public async Task<string> ChatAsync(string question, string contextBlock, CancellationToken ct) {
            var payload = new {
                model = Constants._eloi, // the model built from your Modelfile
                stream = false,
                messages = new[]
                {
                    new { role = "user", content = $"CONTEXT:\n{contextBlock}\n\nQUESTION:\n{question}" }
                }
            };

            using var resp = await _http.PostAsJsonAsync(Constants._localEloiApiChatUri, payload, ct);
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException("Ollama chat call failed.");

            var json = await resp.Content.ReadFromJsonAsync<OllamaChatReply>(ct);
            return json?.message?.content ?? "";
        }

        private sealed class OllamaChatReply {
            public Msg? message { get; set; }
        }

        public async Task<float[]> EmbedAsync(string text, CancellationToken ct) {
            var req = new { model = _settings.EmbedModel, input = text };
            using HttpResponseMessage resp = await _http.PostAsJsonAsync(Constants._localEloiApiEmbedUri, req, ct);
            resp.EnsureSuccessStatusCode();

            EmbedResponse? json = await resp.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken: ct);
            return json?.embedding ?? throw new InvalidOperationException("No embedding returned.");
        }

        private sealed class EmbedResponse { public float[] embedding { get; set; } = Array.Empty<float>(); }
        private sealed class Msg { public string content { get; set; } = ""; }
    }
}
