using System.Text.Json;
using Interviu.Core.DTOs;
using Interviu.Service.Helpers;
using Interviu.Service.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;

namespace Interviu.Service.Services;

public class GeminiService : IGeminiService
{
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private readonly string _modelId;

    public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey configuration is missing");
        _modelId = configuration["Gemini:ModelId"] ?? "gemini-2.5-flash";
    }

    public async Task<List<GeneratedQuestionDto>> GenerateInterviewQuestionsAsync(string cvText, string position, int questionCount = 10)
    {
        try
        {
            _logger.LogInformation("Gemini AI ile {Position} pozisyonu için {QuestionCount} soru oluşturuluyor...", position, questionCount);

            // Prompt oluştur
            string prompt = GeminiPromptHelper.CreateInterviewQuestionsPrompt(cvText, position, questionCount);

            // Gemini API client oluştur
            var googleAI = new GoogleAI(_apiKey);
            var model = googleAI.GenerativeModel(model: _modelId);

            // Generation config
            var generationConfig = new GenerationConfig
            {
                Temperature = 0.7f,
                MaxOutputTokens = 8192,
                TopP = 0.95f
            };

            // API çağrısı
            var response = await model.GenerateContent(
                prompt,
                generationConfig: generationConfig
            );

            // Response'u kontrol et
            if (response == null || string.IsNullOrEmpty(response.Text))
            {
                _logger.LogError("Gemini API'den boş yanıt alındı");
                throw new Exception("Gemini API'den soru üretilemedi");
            }

            var responseText = response.Text;
            _logger.LogInformation("Gemini yanıtı alındı: {Response}", responseText);

            // JSON parse et
            var questions = ParseGeminiResponse(responseText);

            _logger.LogInformation("{Count} adet soru başarıyla oluşturuldu", questions.Count);
            return questions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini AI ile soru oluşturulurken hata oluştu");
            throw;
        }
    }

    private List<GeneratedQuestionDto> ParseGeminiResponse(string responseText)
    {
        try
        {
            // JSON'u temizle (eğer markdown code block içindeyse)
            var cleanJson = responseText.Trim();
            if (cleanJson.StartsWith("```json"))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson.Substring(3);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            // JSON parse et
            var jsonDocument = JsonDocument.Parse(cleanJson);
            var questionsArray = jsonDocument.RootElement.GetProperty("questions");

            var questions = new List<GeneratedQuestionDto>();
            foreach (var questionElement in questionsArray.EnumerateArray())
            {
                questions.Add(new GeneratedQuestionDto
                {
                    QuestionText = questionElement.GetProperty("questionText").GetString() ?? "",
                    Difficulty = questionElement.GetProperty("difficulty").GetString() ?? "Medium",
                    Category = questionElement.GetProperty("category").GetString() ?? "General"
                });
            }

            return questions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini yanıtı parse edilirken hata oluştu. Yanıt: {Response}", responseText);
            throw new Exception("Gemini yanıtı işlenemedi", ex);
        }
    }
    
    public async Task<string> GenerateContentAsync(string prompt)
    {
        try
        {
            _logger.LogInformation("Gemini AI'ya prompt gönderiliyor...");

            // Gemini API client oluştur
            var googleAI = new GoogleAI(_apiKey);
            var model = googleAI.GenerativeModel(model: _modelId);

            // Generation config
            var generationConfig = new GenerationConfig
            {
                Temperature = 0.7f,
                MaxOutputTokens = 8192,
                TopP = 0.95f
            };

            // API çağrısı
            var response = await model.GenerateContent(
                prompt,
                generationConfig: generationConfig
            );

            // Response'u kontrol et
            if (response == null || string.IsNullOrEmpty(response.Text))
            {
                _logger.LogError("Gemini API'den boş yanıt alındı");
                throw new Exception("Gemini API'den yanıt alınamadı");
            }

            _logger.LogInformation("Gemini yanıtı başarıyla alındı");
            return response.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini AI ile içerik oluşturulurken hata oluştu");
            throw;
        }
    }
}

