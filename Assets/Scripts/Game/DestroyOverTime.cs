using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
