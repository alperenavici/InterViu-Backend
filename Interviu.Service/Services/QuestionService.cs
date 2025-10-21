using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Interviu.Core.DTOs;
using Interviu.Data.IRepositories;
using Interviu.Entity.Entities;
using Interviu.Service.IServices;
using Microsoft.Extensions.Logging;

namespace Interviu.Service.Services;

public class QuestionService: IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<QuestionService> _logger;

    public QuestionService(IMapper mapper, IQuestionRepository questionRepository, ILogger<QuestionService> logger)
    {
        _mapper = mapper;
        _questionRepository = questionRepository;
        _logger = logger;
    }


    public async Task<QuestionDto> GetQuestionByIdAsync(Guid id)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null)
        {
            throw new EntryPointNotFoundException($"Question with id {id} was not found");
        }
        return _mapper.Map<QuestionDto>(question);
    }

    public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync()
    {
        var question=await _questionRepository.GetAllAsync();
        if (question == null)
        {
            throw new EntryPointNotFoundException($"Questions were not found");
        }
        return _mapper.Map<IEnumerable<QuestionDto>>(question);
    }

    public async Task<QuestionDto> CreateQuestionAsync(CreateQuestionDto dto)
    {
        var existingQuestions = await _questionRepository.FindAsync(q => q.Text.ToLower() == dto.Text.ToLower());
        if (existingQuestions != null && existingQuestions.Any())
        {
            throw new ValidationException("Soru zaten mevcut");
        }

        var newQuestion = new Question
        {
            Id = Guid.NewGuid(),
            Text = dto.Text,
            Difficulty = dto.Difficulty,
            Category = dto.Category,
        };
        
        await _questionRepository.AddAsync(newQuestion);
        await _questionRepository.SaveChangesAsync();
        return _mapper.Map<QuestionDto>(newQuestion);


    }

    public  async Task UpdateQuestionAsync(Guid id, UpdateQuestionDto dto)
    {
        var questionToUpdate = await _questionRepository.GetByIdAsync(id);
        if (questionToUpdate == null)
        {
            throw new EntryPointNotFoundException($"Question with id {id} was not found");
        }
        if (!string.IsNullOrEmpty(dto.Text))
        {
            questionToUpdate.Text = dto.Text;
        }
        if (!string.IsNullOrEmpty(dto.Category))
        {
            questionToUpdate.Category = dto.Category;
        }
        if (dto.Difficulty.HasValue)
        {
            questionToUpdate.Difficulty = dto.Difficulty.Value;
        }

        _questionRepository.Update(questionToUpdate);
        await _questionRepository.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(Guid id)
    {
        var questionToDelete=await _questionRepository.GetByIdAsync(id);
        if (questionToDelete == null)
        {
            throw new EntryPointNotFoundException($"Question with id {id} was not found");
        }
        _questionRepository.Remove(questionToDelete);
        await _questionRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<QuestionDto>> GetRandomQuestionsAsync(int count, string? category = null)
    {
        var randomQuestions=await _questionRepository.GetRandomQuestionsAsync(count, category);
        return _mapper.Map<IEnumerable<QuestionDto>>(randomQuestions);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var allQuestions = await _questionRepository.GetAllAsync();
        return allQuestions.Select(q => q.Category).Distinct();
    }
}