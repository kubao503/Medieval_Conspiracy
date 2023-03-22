using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Unity.Netcode;

public class MainUIController : NetworkBehaviour
{
    public static MainUIController Instance;

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
    }

    [ClientRpc]
    public void SubscribeToLocalPlayerEventsClientRpc()
    {
        var player = PlayerController.LocalPlayer;
        player.GetComponent<PlayerState>().StateUpdated += StateUpdated;
        player.GetComponent<PlayerHealth>().HealthUpdated += HealthUpdated;
    }

    private void StateUpdated(object sender, StateEventArgs args)
    {
        var deathInfoActive = IsPlayerDead(args.NewState);
        SetDeathInfoActive(deathInfoActive);
    }

    private bool IsPlayerDead(PlayerState.State state)
    {
        return state == PlayerState.State.Dead
            || state == PlayerState.State.Ragdoll;
    }

    public void SetDeathInfoActive(bool active)
    {
        _deathMessage.enabled = active;
        //_respawnButton.SetActive(active);
    }

    private void HealthUpdated(object sender, HealthEventArgs args)
    {
        UpdateHealthText(args.NewHealth);

        if (DamageWasTaken(args))
            ShowDamageVignete();
    }

    private void UpdateHealthText(int health)
    {
        _healthText.text = "Health: " + health.ToString().PadLeft(3);
    }

    private bool DamageWasTaken(HealthEventArgs args)
    {
        return args.NewHealth < args.OldHealth;
    }

    private void ShowDamageVignete()
    {
        StartCoroutine(DamageVigneteFadingCo());
    }

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


    public void ShowGameEndText(bool victory)
    {
        _victoryText.enabled = victory;
        _gameOverText.enabled = !victory;
    }


    //public void SubscribeToRespawnClick(UnityAction action)
    //{
    //    _respawnButton.GetComponent<Button>().onClick.AddListener(action);
    //}


    public void UpdateMoneyText(int money) => _moneyText.text = "Money: " + money.ToString().PadLeft(3);
}
