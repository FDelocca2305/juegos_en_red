using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomLightGroup : MonoBehaviour
{
    [Serializable]
    public class LightEntry
    {
        [Header("Light")]
        public Light light;

        [Header("Emissive (opcional)")]
        public Renderer emissiveRenderer;
        public int emissiveMaterialIndex = 0;
        [ColorUsage(true, true)] public Color onEmission = Color.white;
        [ColorUsage(true, true)] public Color offEmission = Color.black;
        public bool affectRealtimeGI = false;

        [NonSerialized] public MaterialPropertyBlock mpb;
        [NonSerialized] public bool initialized;
    }

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private List<LightEntry> entries = new();
    [SerializeField] private bool startOn = true;

    public bool IsOn { get; private set; }
    public event Action<bool> StateChanged;

    void Awake()
    {
        IsOn = startOn;
        foreach (var e in entries)
        {
            if (e.emissiveRenderer)
            {
                var mats = e.emissiveRenderer.sharedMaterials;
                if (e.emissiveMaterialIndex >= 0 && e.emissiveMaterialIndex < mats.Length && mats[e.emissiveMaterialIndex])
                {
                    if (e.onEmission == default)
                        e.onEmission = mats[e.emissiveMaterialIndex].GetColor(EmissionColor);
                }
            }
        }
        ApplyState(IsOn);
    }

    public void Toggle() => Set(!IsOn);

    public void Set(bool on)
    {
        if (IsOn == on) return;
        IsOn = on;
        ApplyState(IsOn);
        StateChanged?.Invoke(IsOn);
    }

    private void ApplyState(bool on)
    {
        foreach (var e in entries)
        {
            if (e.light) e.light.enabled = on;

            if (e.emissiveRenderer)
            {
                if (e.mpb == null) e.mpb = new MaterialPropertyBlock();

                var color = on ? e.onEmission : e.offEmission;
                e.emissiveRenderer.GetPropertyBlock(e.mpb, e.emissiveMaterialIndex);
                e.mpb.SetColor(EmissionColor, color);
                e.emissiveRenderer.SetPropertyBlock(e.mpb, e.emissiveMaterialIndex);
                
                if (e.affectRealtimeGI)
                    DynamicGI.SetEmissive(e.emissiveRenderer, color);
            }
        }
    }
}
