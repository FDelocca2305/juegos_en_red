using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [Header("Group")]
    [SerializeField] private RoomLightGroup group;

    [Header("Visual Handle (optional)")]
    [SerializeField] private Transform handle;
    [SerializeField] private Vector3 onLocalEuler  = new Vector3(-20, 0, 0);
    [SerializeField] private Vector3 offLocalEuler = new Vector3( 20, 0, 0);
    [SerializeField] private float animTime = 0.08f;
    [SerializeField] private AudioSource clickSfx;

    void OnEnable()
    {
        if (group) group.StateChanged += OnGroupStateChanged;
        SnapTo(group && group.IsOn);
    }
    
    public bool CanInteract() => true;
    public string GetPrompt() => "Toggle light";

    public void Interact()
    {
        if (group) group.Toggle();
    }

    void OnDisable()
    {
        if (group) group.StateChanged -= OnGroupStateChanged;
    }

    private void OnGroupStateChanged(bool isOn)
    {
        if (clickSfx) clickSfx.Play();
        if (!handle) return;
        StopAllCoroutines();
        StartCoroutine(AnimateTo(isOn ? onLocalEuler : offLocalEuler));
    }

    private void SnapTo(bool isOn)
    {
        if (handle) handle.localEulerAngles = isOn ? onLocalEuler : offLocalEuler;
    }

    private IEnumerator AnimateTo(Vector3 targetEuler)
    {
        if (!handle || animTime <= 0f) { SnapTo(group.IsOn); yield break; }

        Quaternion start = handle.localRotation;
        Quaternion end   = Quaternion.Euler(targetEuler);
        float t = 0f;

        while (t < animTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / animTime);
            handle.localRotation = Quaternion.Slerp(start, end, a);
            yield return null;
        }
        handle.localRotation = end;
    }
}
