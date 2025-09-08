using System;

public interface IObjectivesTracker
{
    event Action OnObjectivesChanged;
    void SetObjectives(string[] titles);
    void MarkCompleted(int index);
    int CompletedCount { get; }
    int TotalCount { get; }
    string[] GetFormattedObjectives();
}