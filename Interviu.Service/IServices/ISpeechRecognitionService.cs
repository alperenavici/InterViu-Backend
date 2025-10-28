namespace Interviu.Service.IServices;

public interface ISpeechRecognitionService
{
    Task<string> TranscribeAudioAsync(Stream audioStream, string fileName);
}