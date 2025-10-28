using System.Net.Http.Json;
using System.Reflection.Metadata;
using Interviu.Service.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Interviu.Service.Services;

public class SpeechRecognitionService:ISpeechRecognitionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpeechRecognitionService> _logger;
    private readonly string _speechRecognitionServiceUrl;
    private readonly IConfiguration _configuration;

    public SpeechRecognitionService(HttpClient httpClient,IConfiguration configuration, ILogger<SpeechRecognitionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _speechRecognitionServiceUrl = configuration["SpeechRecognitionServiceUrl"];

        if (string.IsNullOrEmpty(_speechRecognitionServiceUrl))
        {
            throw new InvalidOperationException("Speech recognition service url not set");
        }
    }
    
    public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
    {   
        var content = new MultipartFormDataContent();
        audioStream.Position = 0;
        content.Add(new StreamContent(audioStream), "audio_file", fileName);

        try
        {
            var response = await _httpClient.PostAsync(_speechRecognitionServiceUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Speech mikroservis başlatılamadı");
                throw new Exception("Ses metne çevrilemiyor");
            }

            var result = await response.Content.ReadFromJsonAsync<SpeechResponse>();
            return result?.Text;

        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,"Mikroservis başlatılamadı");
            throw new Exception("Ses metne çevrilemiyor");
        }
        
    }

    private class SpeechResponse
    {
        public string Text { get; set; }
    }
}