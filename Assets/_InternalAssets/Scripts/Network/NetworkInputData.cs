using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    private Vector2 _movementInput;
    private Vector3 _aimForwardVector;
    private NetworkBool _isJumpPressed;
    private NetworkBool _isFireButtonPressed;
    private NetworkBool _isCrouching;

    public Vector2 MovementInput { get => _movementInput; set => _movementInput = value; }
    public Vector3 AimForwardVector { get => _aimForwardVector; set => _aimForwardVector = value; }
    public NetworkBool IsJumpPressed { get => _isJumpPressed; set => _isJumpPressed = value; }
    public NetworkBool IsFireButtonPressed { get => _isFireButtonPressed; set => _isFireButtonPressed = value; }
    public NetworkBool IsCrouching { get => _isCrouching; set => _isCrouching = value; }
}
