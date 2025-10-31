using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.Mime;
using System.Text.Json;
using AutoMapper;
using Google.Rpc;
using Interviu.Core.DTOs;
using Interviu.Data.IRepositories;
using Interviu.Data.UnitOfWork;
using Interviu.Entity.Entities;
using Interviu.Entity.Enums;
using Interviu.Service.Exceptions;
using Interviu.Service.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Interviu.Service.Services;

public class InterviewService: IInterviewService
{   
    private readonly IInterviewRepository _interviewRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<InterviewService> _logger;
    private readonly IQuestionRepository _questionRepository;
    private readonly ICVRepository _cvRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICvService _cvService;
    private readonly IGeminiService _geminiService;
    private readonly ISpeechRecognitionService _speechRecognitionService;

    public InterviewService(IMapper mapper,ICvService cvService,ISpeechRecognitionService speechRecognition, IUnitOfWork unitOfWork,IInterviewRepository interviewRepository, IQuestionRepository questionRepository, ICVRepository cvRepository, ILogger<InterviewService> logger, IGeminiService geminiService)
    {
        _mapper = mapper;
        _interviewRepository = interviewRepository;
        _logger = logger;
        _questionRepository = questionRepository;
        _cvRepository = cvRepository;
        _unitOfWork = unitOfWork;
        _cvService = cvService;
        _geminiService = geminiService;
        _speechRecognitionService = speechRecognition;
    }

    public async Task<InterviewDto> StartInterviewCvAsync(IFormFile cvFile, StartInterviewDto dto)
    {
        try
        {
            _logger.LogInformation("CV tabanlı mülakat başlatılıyor. Pozisyon: {Position}, UserId: {UserId}", dto.Position, dto.UserId);

            // 1. CV metnini çıkart
            string rawCvText = await _cvService.ExtractTextAsync(cvFile);
            _logger.LogInformation("CV metni çıkarıldı. Metin uzunluğu: {Length} karakter", rawCvText.Length);

            // 2. Gemini AI ile sorular üret
            var generatedQuestions = await _geminiService.GenerateInterviewQuestionsAsync(
                rawCvText, 
                dto.Position, 
                dto.QuestionCount);

            if (!generatedQuestions.Any())
            {
                throw new Exception("Gemini AI'dan soru üretilemedi");
            }

            _logger.LogInformation("{Count} adet soru Gemini AI tarafından üretildi", generatedQuestions.Count);

            // 3. Üretilen soruları Question entity'lerine dönüştür
            var questions = generatedQuestions.Select(gq => new Question
            {
                Id = Guid.NewGuid(),
                QuestionText = gq.QuestionText,
                Category = gq.Category,
                Difficulty = ParseDifficulty(gq.Difficulty),
                CreatedAt = DateTime.UtcNow
            }).ToList();

            // 4. Soruları veritabanına kaydet
            foreach (var question in questions)
            {
                await _questionRepository.AddAsync(question);
            }
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Üretilen sorular veritabanına kaydedildi");

            // 5. Mülakatı başlat
            var newInterview = new Interview
            {
                Id = Guid.NewGuid(),
                Position = dto.Position,
                UserId = dto.UserId,
                CvId = dto.CvId,
                Status = InterviewStatus.ONGOING,
                StartedAt = DateTime.UtcNow,
                InterviewQuestions = questions.Select(q => new InterviewQuestion
                {
                    QuestionId = q.Id,
                    AnswerText = null
                }).ToList()
            };

            await _interviewRepository.AddAsync(newInterview);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Mülakat başarıyla başlatıldı. InterviewId: {InterviewId}", newInterview.Id);

            // 6. Detayları getir ve DTO'ya dönüştür
            var createdInterviewWithDetails = await _interviewRepository.GetInterviewWithDetailAsync(newInterview.Id);
            return _mapper.Map<InterviewDto>(createdInterviewWithDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CV tabanlı mülakat başlatılırken hata oluştu");
            throw;
        }
    }

    public async Task<string> SubmitAudioAnswerAsync(Guid interviewId, Guid questionId, IFormFile audioFile)
    {
        _logger.LogInformation("Sesli cevap işleniyor. Mülakat:{InterviewId}, QuestionId: {QuestionId}", interviewId, questionId);
        string transcribedText = null;
        using(var audioStream=audioFile.OpenReadStream())
        {
            transcribedText = await _speechRecognitionService.TranscribeAudioAsync(audioStream, audioFile.FileName);
        }

        if (string.IsNullOrWhiteSpace(transcribedText))
        {
            throw new ValidationException("Seste konuşma algılanamadı");
        }
        _logger.LogInformation("Sesli cevap başarıyla metne çevrilecek");
        var interview = await _unitOfWork.Interviews.GetInterviewWithDetailAsync(interviewId);
        if(interview==null)throw new EntityNotFoundException("interview",interviewId);
        if (interview.Status != InterviewStatus.ONGOING) throw new ValidationException("Soru bu mülakatta bulunamadı");

        var questionToUpdate = interview.InterviewQuestions.FirstOrDefault(iq=>iq.QuestionId == questionId);
        if (questionToUpdate == null) throw new ValidationException("Soru bu mülakatta mevcut değil");
        questionToUpdate.AnswerText = transcribedText;
        _unitOfWork.Interviews.Update(interview);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Metne çevrilen cevap veritabanına kaydedildi");
        return transcribedText;
    }

    public async Task<InterviewAnalysisDto> AnalyzeAndCompleteInterviewAsync(Guid interviewId)
    {   
        _logger.LogInformation("Metne çevirilen ses gönderiliyor");
        var interview = await _interviewRepository.GetInterviewWithDetailAsync(interviewId);
        if (interview == null) throw new EntityNotFoundException("interview", interviewId);
        if (interview.Status != InterviewStatus.ONGOING) throw new ValidationException("Devam eden mülakat bulunamadı");
        string analysisPrompt = CreateGeminiAnalysisPrompt(interview);
        
        _logger.LogInformation("Mülakat analizi için geminiApi'sine istek gönderildi");
        string geminiResponseJson = await _geminiService.GenerateContentAsync(analysisPrompt);
        geminiResponseJson = geminiResponseJson.Trim().Replace("```json", "").Replace("```", "").Trim();
        
        var analysisResult = JsonSerializer.Deserialize<GeminiAnalysisResponse>(geminiResponseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (analysisResult == null) throw new Exception("Geminiden geçerli bir analiz sonucu alınamadı");
        _logger.LogInformation("Gemini analizi sonucu alındı.Puan:{Score}");

        interview.OverallScore = analysisResult.OverallScore;
        interview.OverallFeedback = analysisResult.OverallFeedback;
        interview.Status = InterviewStatus.COMPLETED;
        interview.CompletedAt=DateTime.UtcNow;
        
        _unitOfWork.Interviews.Update(interview);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Mülakat analizi veritabanına kaydedildi ve mülakat tamamlandı");

        return new InterviewAnalysisDto
        {
            OverallScore = analysisResult.OverallScore,
            OverallFeedback = analysisResult.OverallFeedback
        };
    }

    private string CreateGeminiAnalysisPrompt(Interview interview)
    {
        var qaText = string.Join("\n\n", interview.InterviewQuestions.Select(iq => $"Soru: {iq.Question.Text}\nCevap: {iq.AnswerText}"));

        return $@"
        Sen, bir adayın mülakat performansını değerlendiren uzman bir İK yöneticisisin. Sana bir mülakattaki sorular ve adayın verdiği cevaplar verilecek.
        
        Hedef Pozisyon: {interview.Position}
        
        Sorular ve Cevaplar:
        ---
        {qaText}
        ---

        Görevin:
        Yukarıdaki soru-cevapları analiz ederek adayın performansını değerlendir.
        1. Adayın cevaplarının kalitesini, teknik doğruluğunu ve iletişim becerisini göz önünde bulundur.
        2. Adaya 10 üzerinden bir genel puan ver (`overallScore`).
        3. Adayın güçlü yönlerini ve geliştirmesi gereken alanları belirten, yapıcı ve detaylı bir genel geri bildirim metni (`overallFeedback`) yaz.
        
        Çıktın, aşağıdaki JSON yapısına %100 uymalıdır. Sadece geçerli bir JSON objesi döndür.
        
        İstenen JSON Yapısı:
        ```json
        {{
          ""overallScore"": 8.5,
          ""overallFeedback"": ""Aday, teknik sorulara kendinden emin ve doğru cevaplar verdi. Özellikle X konusundaki bilgisi etkileyiciydi. Ancak, davranışsal sorularda daha somut örnekler (STAR metodu) kullanabilirdi. İletişim becerileri genel olarak güçlü.""
        }}
        ```
    ";
    }

    private Difficulty ParseDifficulty(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "easy" or "kolay" => Difficulty.EASY,
            "hard" or "zor" => Difficulty.HARD,
            _ => Difficulty.MEDIUM
        };
    }

    

    public async Task<InterviewDto> StartInterviewAsync(StartInterviewDto dto)
    { 
        // CV validation - eğer CvId verilmişse, CV'nin var olduğunu kontrol et
        if (dto.CvId.HasValue)
        {
            var cv = await _cvRepository.GetByIdAsync(dto.CvId.Value);
            if (cv == null)
            {
                throw new Exception($"CV with ID {dto.CvId.Value} not found");
            }
        }

        var randomQuestions=await _questionRepository.GetRandomQuestionsAsync(dto.QuestionCount,dto.Category);
        if (!randomQuestions.Any())
        {
            throw new Exception("No questions found");
        }

        var newInterview = new Interview
        {
            Id = Guid.NewGuid(),
            Position = dto.Position,
            UserId = dto.UserId,
            CvId = dto.CvId,
            Status = InterviewStatus.ONGOING,
            StartedAt = DateTime.UtcNow,
            InterviewQuestions = randomQuestions.Select(q => new InterviewQuestion
            {
                QuestionId = q.Id,
                AnswerText = null 
            }).ToList()

        };
        await _interviewRepository.AddAsync(newInterview);
        await _unitOfWork.SaveChangesAsync();
        var createdInterviewWithDetails = await _interviewRepository.GetInterviewWithDetailAsync(newInterview.Id);
        return _mapper.Map<InterviewDto>(createdInterviewWithDetails);
    }

    public async Task SubmitAnswerAsync(SubmitAnswerDto dto)
    {
        var interview=await _interviewRepository.GetInterviewWithDetailAsync(dto.InterviewId);
        if (interview == null)
        {
            throw new Exception("Interview not found");
        }
        if (interview.Status != InterviewStatus.ONGOING)
        {
            throw new Exception("Interview not in ongoing");
        }
        var questionToAnswer=interview.InterviewQuestions.FirstOrDefault(i=>i.QuestionId==dto.QuestionId);
        if (questionToAnswer == null)
        {
            throw new Exception("Question not found");
        }
        questionToAnswer.AnswerText = dto.AnswerText;
        _interviewRepository.Update(interview);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CompleteInterviewAsync(Guid interviewId, float overallScore, string overallFeedback)
    {
        var interview = await _interviewRepository.GetByIdAsync(interviewId);
        if (interview == null) throw new EntityNotFoundException("Interview", interviewId);

        interview.Status = InterviewStatus.COMPLETED;
        interview.CompletedAt = DateTime.UtcNow;
        interview.OverallScore = overallScore;
        interview.OverallFeedback = overallFeedback;
        await _interviewRepository.SaveChangesAsync();
    }

    public async Task<InterviewDto> GetInterviewWithDetailsAsync(Guid interviewId)
    {
        var interview = await _interviewRepository.GetInterviewWithDetailAsync(interviewId);
        if (interview == null) throw new EntityNotFoundException("Interview", interviewId);
        return _mapper.Map<InterviewDto>(interview);
    }

    public async Task<IEnumerable<InterviewSummaryDto>> GetInterviewsForUserAsync(string userId)
    {
        var interviews = await _interviewRepository.GetInterviewsByUserIdAsync(userId);
        return interviews.Select(i => new InterviewSummaryDto
        {
            Id = i.Id,
            Position = i.Position,
            Status = i.Status,
            StartedAt = i.StartedAt,
            OverallScore = i.OverallScore
        });
    }
}