 using UnityEngine;


 public class FpsLimit : MonoBehaviour
 {
    [SerializeField] private int _targetFrameRate = 60;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _targetFrameRate;
    }
 }
