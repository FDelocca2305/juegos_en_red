using UnityEngine;

namespace UI.Gameplay
{
    public interface IGameplayUI
    {
        string DeathText { set; }
        bool DeathScreenActivate { set; }
        void ShowHint(string text, float seconds = 1.25f);
        
        void ShowObjectives(string[] lines, bool visible);
        
        void ShowRadarTargets(Transform[] targets);
        void ShowBlind(bool visible, string message = null);
        void BlindFor(float seconds, string message = null);
    }
}