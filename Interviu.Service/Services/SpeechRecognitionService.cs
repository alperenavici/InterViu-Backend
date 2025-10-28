using System.Net.Http.Json; 
using Interviu.Service.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Interviu.Service.Services
{
    public class SpeechRecognitionService : ISpeechRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SpeechRecognitionService> _logger;
        private readonly string _serviceUrl;

        public SpeechRecognitionService(HttpClient httpClient, IConfiguration configuration, ILogger<SpeechRecognitionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _serviceUrl = configuration["SpeechRecognitionService:Url"];

            if (string.IsNullOrEmpty(_serviceUrl))
            {
                throw new InvalidOperationException("Konuşma tanıma mikroservis adresi (SpeechRecognitionService:Url) yapılandırılmamış.");
            }
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
        {
            _logger.LogInformation("Konuşma tanıma mikroservisine istek gönderiliyor. Dosya: {FileName}", fileName);

            var content = new MultipartFormDataContent();
            
            audioStream.Position = 0;
            
            content.Add(new StreamContent(audioStream), "audio_file", fileName);

            try
            {
                var fullUrl = $"{_serviceUrl}/transcribe/";
                var response = await _httpClient.PostAsync(fullUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Konuşma tanıma mikroservisi başarısız bir yanıt döndü. Durum Kodu: {StatusCode}, Hata: {Error}", response.StatusCode, error);
                    throw new Exception("Ses metne çevrilirken bir sunucu hatası oluştu.");
                }

                var result = await response.Content.ReadFromJsonAsync<SpeechResponse>();
                return result?.Text;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Konuşma tanıma mikroservisine bağlanılamadı. Servisin çalıştığından emin olun. URL: {Url}", _serviceUrl);
                throw new Exception("Ses analiz servisine ulaşılamıyor.", ex);
            }
        }

        private class SpeechResponse
        {
            public string Text { get; set; }
        }
    }
}