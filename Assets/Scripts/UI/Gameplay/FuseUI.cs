using UnityEngine;

public class FuseUI : MonoBehaviour
{
    [SerializeField] private GameObject fuseIcon;

    void Start()
    {
        if (fuseIcon) fuseIcon.SetActive(false);
        if (FuseController.Exists)
            FuseController.I.OnLocalFuseHolderChanged += has => { if (fuseIcon) fuseIcon.SetActive(has); };
    }
}