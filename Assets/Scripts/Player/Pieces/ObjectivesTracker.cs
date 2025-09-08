using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivesTracker : MonoBehaviour, IObjectivesTracker
{
    private struct Entry { public string title; public bool done; }
    private readonly List<Entry> _entries = new();

    public event Action OnObjectivesChanged;
    public int CompletedCount { get; private set; }
    public int TotalCount     => _entries.Count;

    public void SetObjectives(string[] titles)
    {
        _entries.Clear();
        CompletedCount = 0;
        if (titles != null)
            foreach (var t in titles) _entries.Add(new Entry{ title=t, done=false });
        OnObjectivesChanged?.Invoke();
    }

    public void MarkCompleted(int index)
    {
        if (index < 0 || index >= _entries.Count) return;
        if (_entries[index].done) return;
        var e = _entries[index]; e.done = true; _entries[index] = e;
        CompletedCount++;
        OnObjectivesChanged?.Invoke();
    }

    public string[] GetFormattedObjectives()
    {
        var arr = new string[_entries.Count];
        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            arr[i] = e.done ? $"<s>{e.title}</s>" : e.title;
        }
        return arr;
    }
}