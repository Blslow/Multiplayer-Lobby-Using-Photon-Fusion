using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponHandler : NetworkBehaviour
{
    private float _lastTimeFired = 0;
    private float _fireInterval = .15f;

    [SerializeField]
    private ParticleSystem _fireParticleSystem;
    [SerializeField]
    private Transform _aimPoint;
    [SerializeField]
    private LayerMask _collisionLayers;

    private HPHandler _hPHandler;

    [SerializeField]
    private LocalCameraHandler _localCameraHandler;

    [SerializeField]
    private GameObject _hitMarkPrefab;

    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool IsFiring { get; set; }

    private void Awake()
    {
        _hPHandler = GetComponent<HPHandler>();
    }

    public override void FixedUpdateNetwork()
    {
        if (_hPHandler.IsDead)
            return;

        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.IsFireButtonPressed)
                Fire(networkInputData.AimForwardVector);
        }
    }

    private void Fire(Vector3 aimForwardVector)
    {
        if (Time.time - _lastTimeFired < _fireInterval)
            return;


        //_localCameraHandler.SetViewInputVetor(new Vector2())

        StartCoroutine(FireEffectCO());

        Runner.LagCompensation.Raycast(_aimPoint.position, aimForwardVector, 100, Object.InputAuthority, out var hitinfo, _collisionLayers, HitOptions.IncludePhysX);

        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if (hitinfo.Distance > 0)
            hitDistance = hitinfo.Distance;

        if (hitinfo.Hitbox != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

            if (Object.HasStateAuthority)
                hitinfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage();

            isHitOtherPlayer = true;
        }
        else if (hitinfo.Collider != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitinfo.Collider.transform.name}");
            Debug.Log(hitinfo.Point);
            Instantiate(_hitMarkPrefab, hitinfo.Point, Quaternion.identity);
        }

        if (isHitOtherPlayer)
            Debug.DrawRay(_aimPoint.position, aimForwardVector * hitDistance, Color.red, 1);
        else
            Debug.DrawRay(_aimPoint.position, aimForwardVector * hitDistance, Color.green, 1);

        _lastTimeFired = Time.time;
    }

    IEnumerator FireEffectCO()
    {
        IsFiring = true;

        _fireParticleSystem.Play();

        yield return new WaitForSeconds(0.09f);

        IsFiring = false;
    }

    private static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFireChanged value {changed.Behaviour.IsFiring}");
        bool isFiringCurrent = changed.Behaviour.IsFiring;

        changed.LoadOld();

        bool isFiringOld = changed.Behaviour.IsFiring;

        if (isFiringCurrent && !isFiringOld)
            changed.Behaviour.OnFireRemote();
    }

    private void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
            _fireParticleSystem.Play();
    }
}
