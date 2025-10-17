public interface ISabotageable
{
    bool CanSabotage();
    void Sabotage();
    string GetSabotagePrompt();
}