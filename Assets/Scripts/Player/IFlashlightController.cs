using System;

public interface IFlashlightController
{
    bool IsOn { get; }
    event Action<bool> OnChanged;
    void Toggle();
    void SetState(bool on);
}