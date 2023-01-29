using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChange))]
    private byte _hP { get; set; }

    [Networked(OnChanged = nameof(OnStateChanged))]
    public bool IsDead { get; set; }

    private bool _isInitialized;

    [SerializeField]
    private Color _uiOnHitColor;
    [SerializeField]
    private Image _uiOnHitImage;
    [SerializeField]
    private MeshRenderer _bodyMeshRenderer;
    private Color _defaultMeshBodyColor;

    [SerializeField]
    private GameObject _playerModel;
    [SerializeField]
    private GameObject _deathGameObjectPrefab;
    [SerializeField]
    private TMP_Text _healthText;

    private HitboxRoot _hitboxRoot;
    private CharacterMovementHandler _characterMovementHandler;

    private const byte STARTING_HP = 5;

    private void Awake()
    {
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();
        _hitboxRoot = GetComponentInChildren<HitboxRoot>();
    }

    private void Start()
    {
        _hP = STARTING_HP;
        IsDead = false;

        _defaultMeshBodyColor = _bodyMeshRenderer.material.color;

        _isInitialized = true;
    }

    IEnumerator OnHitCO()
    {
        _bodyMeshRenderer.material.color = Color.white;

        if (Object.HasInputAuthority)
            _uiOnHitImage.color = _uiOnHitColor;

        yield return new WaitForSeconds(0.2f);

        _bodyMeshRenderer.material.color = _defaultMeshBodyColor;

        if (Object.HasInputAuthority && !IsDead)
            _uiOnHitImage.color = new Color(0, 0, 0, 0);
    }

    IEnumerator ServerReviveCO()
    {
        yield return new WaitForSeconds(2);
        _characterMovementHandler.RequestRespawn();
    }

    public void OnTakeDamage()
    {
        if (IsDead)
            return;

        _hP -= 1;

        Debug.Log($"{Time.time} {transform.name} took damage got {_hP} left ");

        if (_hP <= 0)
        {
            Debug.Log($"{Time.time} {transform.name} died");

            StartCoroutine(ServerReviveCO());

            IsDead = true;
        }
    }

    private static void OnHPChange(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour._hP}");
        changed.Behaviour.UpdateHealthText();

        //changed.Behaviour._healthText.text = "HP: " + changed.Behaviour._hP.ToString();

        byte newHP = changed.Behaviour._hP;

        changed.LoadOld();

        byte oldHP = changed.Behaviour._hP;

        if (newHP < oldHP)
            changed.Behaviour.OnHPReduced();

    }

    private void UpdateHealthText()
    {
        if (Object.HasInputAuthority)
        {
            _healthText.text = "HP: " + _hP.ToString();
        }
    }

    private void OnHPReduced()
    {
        if (!_isInitialized)
            return;

        StartCoroutine(OnHitCO());
    }

    private static void OnStateChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged IsDead {changed.Behaviour.IsDead}");

        bool isDeadCurrent = changed.Behaviour.IsDead;

        changed.LoadOld();

        bool isDeadOld = changed.Behaviour.IsDead;

        if (isDeadCurrent)
            changed.Behaviour.OnDeath();
        else if (!isDeadCurrent && isDeadOld)
            changed.Behaviour.OnRevive();
    }

    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");

        _playerModel.gameObject.SetActive(false);
        _hitboxRoot.HitboxRootActive = false;
        _characterMovementHandler.SetCharacterControllerEnabled(false);

        Instantiate(_deathGameObjectPrefab, transform.position, Quaternion.identity);
    }

    private void OnRevive()
    {
        Debug.Log($"{Time.time} OnRevive");

        if (Object.HasInputAuthority)
            _uiOnHitImage.color = new Color(0, 0, 0, 0);

        _playerModel.gameObject.SetActive(true);
        _hitboxRoot.HitboxRootActive = true;
        _characterMovementHandler.SetCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        _hP = STARTING_HP;
        IsDead = false;
    }
}
