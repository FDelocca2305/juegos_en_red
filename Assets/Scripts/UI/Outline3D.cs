using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class Outline3D : MonoBehaviour
{
    [Header("Material de Outline (URP_InvertedHullOutline)")]
    [SerializeField] private Material outlineMaterial;
    [ColorUsage(true,true)] [SerializeField] private Color color = Color.cyan;
    [SerializeField] private float thickness = 0.02f;

    private readonly List<GameObject> _clones = new();
    private readonly List<Material>   _instances = new();

    void Awake()
    {
        if (!outlineMaterial) { Debug.LogWarning("[Outline3D] no material."); return; }
        BuildClones();
        SetEnabled(false);
    }

    void OnDestroy()
    {
        foreach (var m in _instances) if (m) Destroy(m);
    }

    public void SetEnabled(bool on)
    {
        foreach (var go in _clones) if (go) go.SetActive(on);
    }

    private void BuildClones()
    {
        foreach (var mf in GetComponentsInChildren<MeshFilter>(true))
        {
            if (!mf.sharedMesh) continue;

            var child = new GameObject($"__Outline__{mf.name}");
            child.transform.SetParent(mf.transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale    = Vector3.one;

            var cmf = child.AddComponent<MeshFilter>();
            cmf.sharedMesh = mf.sharedMesh;

            var cmr = child.AddComponent<MeshRenderer>();
            var inst = new Material(outlineMaterial);
            inst.SetColor("_Color", color);
            inst.SetFloat("_Thickness", thickness);
            cmr.sharedMaterial = inst;
            
            cmr.shadowCastingMode = ShadowCastingMode.Off;
            cmr.receiveShadows = false;
            cmr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            cmr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            cmr.allowOcclusionWhenDynamic = false;

            _clones.Add(child);
            _instances.Add(inst);
        }
    }
}
