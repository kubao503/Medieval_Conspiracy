using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Unity.Netcode;

public class MainUIController : NetworkBehaviour
{
    public static MainUIController Instance;

    public event EventHandler RespawnClicked;

    [SerializeField] private TextMeshProUGUI _deathMessage;
    [SerializeField] private GameObject _respawnButton;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _victoryText;
    [SerializeField] private TextMeshProUGUI _gameOverText;
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private Image _damageVignette;
    [SerializeField] private AnimationCurve _damageVignetteFadingCurve;
    [SerializeField] private float _damageVignetteDuration = 1f;

    public void SubscribeToLocalPlayerEvents(GameObject localPlayer)
    {
        localPlayer.GetComponent<PlayerState>().StateUpdated += StateUpdated;
        localPlayer.GetComponent<PlayerHealth>().HealthUpdated += HealthUpdated;
    }

    private void StateUpdated(object sender, StateEventArgs args)
    {
        UpdateDeathMessage(args.NewState);
        UpdateRespawnButton(args.NewState);
    }

    private void UpdateDeathMessage(PlayerState.State state)
    {
        _deathMessage.enabled = ShouldBeDeathMessageActive(state);
    }

    private bool ShouldBeDeathMessageActive(PlayerState.State state)
    {
        return state == PlayerState.State.Ragdoll
            || state == PlayerState.State.Dead;
    }

    private void UpdateRespawnButton(PlayerState.State state)
    {
        _respawnButton.SetActive(state == PlayerState.State.Dead);
    }

    private void HealthUpdated(object sender, HealthEventArgs args)
    {
        UpdateHealthText(args.NewHealth);

        if (WasDamageTaken(args))
            ShowDamageVignette();
    }

    private void UpdateHealthText(int health)
    {
        _healthText.text = "Health: " + health.ToString().PadLeft(3);
    }

    private bool WasDamageTaken(HealthEventArgs args)
    {
        return args.NewHealth < args.OldHealth;
    }

    private void ShowDamageVignette()
    {
        StartCoroutine(DamageVignetteFadingCoroutine());
    }

    private IEnumerator DamageVignetteFadingCoroutine()
    {
        for (var time = 0f; time <= 1f; time += (Time.deltaTime / _damageVignetteDuration))
        {
            SetDamageVignetteColorBasedOnTime(time);
            yield return null;
        }
    }

    private void SetDamageVignetteColorBasedOnTime(float time)
    {
        var color = _damageVignette.color;
        color.a = _damageVignetteFadingCurve.Evaluate(time);
        _damageVignette.color = color;
    }

    public void ShowGameEndText(bool victory)
    {
        _victoryText.enabled = victory;
        _gameOverText.enabled = !victory;
    }

    public void UpdateMoneyText(int money)
    {
        _moneyText.text = "Money: " + money.ToString().PadLeft(3);
    }

    private void Awake()
    {
        Instance = this;
        _respawnButton.GetComponent<Button>().onClick.AddListener(RespawnClick);
    }

    private void RespawnClick()
    {
        RespawnClicked?.Invoke(this, EventArgs.Empty);
    }
}
