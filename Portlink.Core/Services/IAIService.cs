namespace PortlinkApp.Core.Services;

public interface IAIService
{
    Task<string> GetBerthRecommendation(int vesselId, string context);
    Task<string> AnswerQuestion(string question, string context);
    Task<string> GenerateRealisticPortCallScenario();
    Task<bool> IsAvailable();
}
