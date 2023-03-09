using UnityEngine;

public class InterSceneStorage : MonoBehaviour
{
    public static InterSceneStorage Instance;
    private string _joinCode = "";
    private bool _codeSet = false;
    
    public string JoinCode { get => _codeSet ? "Join code:\n" + _joinCode : ""; set { _joinCode = value; _codeSet = true; } }
    
    private void Awake() => Instance = this;
}
