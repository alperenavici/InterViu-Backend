using AutoMapper;
using Interviu.Core.DTOs;
using Interviu.Data.IRepositories;
using Interviu.Entity.Entities;
using Interviu.Entity.Enums;
using Interviu.Service.Exceptions;
using Interviu.Service.IServices;
using Microsoft.Extensions.Logging;

namespace Interviu.Service.Services;

public class InterviewService: IInterviewService
{   
    private readonly IInterviewRepository _interviewRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<InterviewService> _logger;
    private readonly IQuestionRepository _questionRepository;

    public InterviewService(IMapper mapper, IInterviewRepository interviewRepository, IQuestionRepository questionRepository,ILogger<InterviewService> logger)
    {
        _mapper = mapper;
        _interviewRepository = interviewRepository;
        _logger = logger;
        _questionRepository = questionRepository;
    }


    public async Task<InterviewDto> StartInterviewAsync(StartInterviewDto dto)
    { 
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
                AnswerText = "" 
            }).ToList()

        };
        await _interviewRepository.AddAsync(newInterview);
        await _interviewRepository.SaveChangesAsync();
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
        await _interviewRepository.SaveChangesAsync();
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