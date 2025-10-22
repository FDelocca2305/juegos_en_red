using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashlightIconUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private IFlashlightController _flashlight;
    
    private void Awake()
    {
        StartCoroutine(Bind());
    }

    private IEnumerator Bind()
    {
        yield return ServiceLocatorUtil.WaitFor<IFlashlightController>(svc => _flashlight = svc);
        _flashlight.OnChanged += Refresh;
        Refresh(_flashlight.IsOn);
    }

    private void OnDestroy()
    {
        if (_flashlight != null)
            _flashlight.OnChanged -= Refresh;
    }

    private void Refresh(bool on)
    {
        if (!icon) return;
        icon.sprite = on ? onSprite : offSprite;
        icon.enabled = true;
    }
}