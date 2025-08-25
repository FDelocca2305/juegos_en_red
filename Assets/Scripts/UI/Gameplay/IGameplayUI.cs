using UnityEngine;

namespace UI.Gameplay
{
    public interface IGameplayUI
    {
        void ShowHint(string text, float seconds = 1.25f);
        
        void ShowObjectives(string[] lines, bool visible);
        
        void ShowRadarTargets(Transform[] targets);
    }
}