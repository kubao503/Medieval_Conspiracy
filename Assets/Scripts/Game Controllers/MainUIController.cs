using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class MainUIController : MonoBehaviour
{
    public static MainUIController Instance;

    [SerializeField] private TextMeshProUGUI _joinCodeText;
    [SerializeField] private TextMeshProUGUI _deathMessage;
    //[SerializeField] private GameObject _respawnButton;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _victoryText;
    [SerializeField] private TextMeshProUGUI _gameOverText;
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private Image _damageVignette;

    [SerializeField] private AnimationCurve _damageVignetteFadingCurve;
    [SerializeField] private float _damageVignetteDuration = 1f;
    private const float _damageVignetteTimeDelta = .025f;


    private void Awake()
    {
        Instance = this;

        _joinCodeText.text = InterSceneStorage.Instance.JoinCode;
    }


    public void ShowDamageVignete() => StartCoroutine(DamageVigneteFadingCo());


    private IEnumerator DamageVigneteFadingCo()
    {
        var color = _damageVignette.color;
        var timeStep = _damageVignetteTimeDelta / _damageVignetteDuration;

        for (var time = 0f; time <= 1f; time += timeStep)
        {
            // Changing alpha component of image color
            color.a = _damageVignetteFadingCurve.Evaluate(time);
            _damageVignette.color = color;

            yield return new WaitForSeconds(_damageVignetteTimeDelta);
        }
    }


    public void ShowDeathInfo(bool active)
    {
        //if (active) Cursor.lockState = CursorLockMode.None;
        //else Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;

        _deathMessage.enabled = active;
        //_respawnButton.SetActive(active);
    }


    public void ShowGameEndText(bool victory)
    {
        _victoryText.enabled = victory;
        _gameOverText.enabled = !victory;
    }


    //public void SubscribeToRespawnClick(UnityAction action)
    //{
    //    _respawnButton.GetComponent<Button>().onClick.AddListener(action);
    //}


    public void UpdateHealthText(int health) => _healthText.text = "Health: " + health.ToString().PadLeft(3);


    public void UpdateMoneyText(int money) => _moneyText.text = "Money: " + money.ToString().PadLeft(3);
}
