using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;

public class PlayerSkillController : NetworkBehaviour
{
    public static event Action<ulong> OnTimerFinished;

    [Header("References")]
    [SerializeField] private PlayerVehicleController _playerVehicleController;
    [SerializeField] private PlayerInteractionController _playerInteractionController;
    [SerializeField] private Transform _rocketLauncherTransform;
    [SerializeField] private Transform _rocketLaunchPointTransform;

    [Header("Settings")]
    [SerializeField] private bool _hasSkillAlready;
    [SerializeField] private float _resetDelay = 1f;

    private MysteryBoxSkillsSO _currentSkill;
    private bool _isSkillUsed;
    private bool _hasTimerStarted;
    private float _timer;
    private float _timerMax;
    private int _mineAmountCounter;

    override public void OnNetworkSpawn()
    {
        _playerVehicleController.OnVehicleCrashed += PlayerVehicleController_OnVehicleCrashed;
    }

    private void PlayerVehicleController_OnVehicleCrashed()
    {
        enabled = false;
        SkillsUI.Instance.SetSkillToNone();
        _hasSkillAlready = false;
        _hasTimerStarted = false;
        SetRocketLauncherActiveRpc(false);
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!_hasSkillAlready) return;
        if (!GameManager.IsGamePlaying()) return;

        if (Input.GetKeyDown(KeyCode.Space) && !_isSkillUsed)
        {
            ActivateSkill();
            _isSkillUsed = true;
        }

        if (_hasTimerStarted)
        {
            _timer -= Time.deltaTime;
            SkillsUI.Instance.SetTimerCounterText((int)_timer);

            if (_timer <= 0)
            {
                OnTimerFinished?.Invoke(OwnerClientId);
                SkillsUI.Instance.SetSkillToNone();
                _hasTimerStarted = false;
                _hasSkillAlready = false;

                if (_currentSkill.SkillType == SkillType.Shield)
                {
                    _playerInteractionController.SetShieldActive(false);
                }

                if (_currentSkill.SkillType == SkillType.Spike)
                {
                    _playerInteractionController.SetSpikeActive(false);
                }
            }
        }
    }

    public void SetupSkill(MysteryBoxSkillsSO skill)
    {
        _currentSkill = skill;

        if (_currentSkill.SkillType == SkillType.Rocket)
        {
            SetRocketLauncherActiveRpc(true);
        }

        _hasSkillAlready = true;
        _isSkillUsed = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetRocketLauncherActiveRpc(bool active)
    {
        _rocketLauncherTransform.gameObject.SetActive(active);
    }

    private IEnumerator ResetRocketLauncher()
    {
        yield return new WaitForSeconds(_resetDelay);
        SetRocketLauncherActiveRpc(false);
    }

    public void ActivateSkill()
    {
        if (!_hasSkillAlready) return;

        SkillManager.Instance.ActivateSkill(_currentSkill.SkillType, transform, OwnerClientId);
        SetSkillToNone();

        if (_currentSkill.SkillType == SkillType.Rocket)
        {
            StartCoroutine(ResetRocketLauncher());
        }

        if (_currentSkill.SkillType == SkillType.Shield)
        {
            _playerInteractionController.SetShieldActive(true);
        }

        if (_currentSkill.SkillType == SkillType.Spike)
        {
            _playerInteractionController.SetSpikeActive(true);
        }
    }

    private void SetSkillToNone()
    {
        if (_currentSkill.SkillUsageType == SkillUsageType.None)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
        }

        if (_currentSkill.SkillUsageType == SkillUsageType.Timer)
        {
            _hasTimerStarted = true;
            _timerMax = _currentSkill.SkillData.SpawnAmountOrTimer;
            _timer = _timerMax;
        }

        if (_currentSkill.SkillType == SkillType.Mine)
        {
            _mineAmountCounter = _currentSkill.SkillData.SpawnAmountOrTimer;

            SkillManager.Instance.OnMineCountReduced += SkillManager_OnMineCountReduced;
        }
    }

    private void SkillManager_OnMineCountReduced()
    {
        _mineAmountCounter--;
        SkillsUI.Instance.SetTimerCounterText(_mineAmountCounter);

        if (_mineAmountCounter <= 0)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
            SkillManager.Instance.OnMineCountReduced -= SkillManager_OnMineCountReduced;
        }
    }

    public bool HasSkillAlready()
    {
        return _hasSkillAlready;
    }

    public Vector3 GetRocketLaunchPoint()
    {
        return _rocketLaunchPointTransform.position;
    }

    public void OnPlayerRespawned() => enabled = true;
}
