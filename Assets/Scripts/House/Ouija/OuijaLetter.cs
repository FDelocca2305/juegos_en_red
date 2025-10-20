using UnityEngine;

public class OuijaLetter : MonoBehaviour
{
    [SerializeField] private string symbol = "A";
    public string Symbol => symbol.ToUpper();
    private void OnValidate(){ if (string.IsNullOrEmpty(symbol)) symbol = name; }
}
