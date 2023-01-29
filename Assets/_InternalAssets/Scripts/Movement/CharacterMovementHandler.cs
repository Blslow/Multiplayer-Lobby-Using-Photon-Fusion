using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    private bool _isRespawnRequested = false;

    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;
    private HPHandler _hPHandler;
    //private Camera _localCamera;

    private void Awake()
    {
        _networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        _hPHandler = GetComponent<HPHandler>();
        _networkCharacterControllerPrototypeCustom.baseMaxSpeed = _networkCharacterControllerPrototypeCustom.maxSpeed;
        //_localCamera = GetComponentInChildren<Camera>();
    }

    public void SetCrouchSpeed(bool crouch)
    {
        if (crouch)
            _networkCharacterControllerPrototypeCustom.maxSpeed /= 2;
        else
            _networkCharacterControllerPrototypeCustom.maxSpeed = _networkCharacterControllerPrototypeCustom.baseMaxSpeed;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (_isRespawnRequested)
            {
                Respawn();
                return;
            }

            if (_hPHandler.IsDead)
                return;
        }

        if (GetInput(out NetworkInputData networkInputData))
        {
            //_networkCharacterControllerPrototypeCustom.Rotate(networkInputData.AimForwardVector);

            transform.forward = networkInputData.AimForwardVector;

            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;

            Vector3 moveDirection = transform.forward * networkInputData.MovementInput.y + transform.right * networkInputData.MovementInput.x;
            moveDirection.Normalize();

            _networkCharacterControllerPrototypeCustom.Move(moveDirection);

            if (networkInputData.IsJumpPressed)
                _networkCharacterControllerPrototypeCustom.Jump();

            CheckFallRespawn();
        }
    }

    private void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            if (Object.HasStateAuthority)
            {
                Debug.Log($"{Time.time} Respawn due to fall outside of map at position {transform.position}");
                Respawn();
            }
        }
    }

    public void RequestRespawn()
    {
        _isRespawnRequested = true;
    }

    private void Respawn()
    {
        _networkCharacterControllerPrototypeCustom.TeleportToPosition(Utils.GetRandomSpawnPoint());

        _hPHandler.OnRespawned();

        _isRespawnRequested = false;
    }

    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        _networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }
}
