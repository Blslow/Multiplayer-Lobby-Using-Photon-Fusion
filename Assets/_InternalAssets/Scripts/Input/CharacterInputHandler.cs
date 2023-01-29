using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    private Vector2 _moveInputVector = Vector2.zero;
    private Vector2 _viewInputVector = Vector2.zero;
    private bool _isJumButtonPressed = false;
    private bool _isFireButtonPressed = false;
    private bool _shouldRecoil = false;
    private bool _isCrouching = false;

    private LocalCameraHandler _localCameraHandler;
    private CharacterMovementHandler _characterMovementHandler;

    [SerializeField]
    private float _recoilValue = 2;

    private void Awake()
    {
        _localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        _characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!_characterMovementHandler.Object.HasInputAuthority)
            return;

        _viewInputVector.x = Input.GetAxis("Mouse X");
        _viewInputVector.y = Input.GetAxis("Mouse Y") * -1;


        _moveInputVector.x = Input.GetAxis("Horizontal");
        _moveInputVector.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.C))
        {
            _isCrouching = !_isCrouching;
            _characterMovementHandler.SetCrouchSpeed(_isCrouching);
        }

        if (Input.GetButtonDown("Jump"))
            _isJumButtonPressed = true;


        if (Input.GetButtonDown("Fire1"))
        {
            _isFireButtonPressed = true;
        }

        if (_shouldRecoil)
        {
            if (_isCrouching)
                _viewInputVector.y = Input.GetAxis("Mouse Y") * -1 - _recoilValue / 3;
            else
                _viewInputVector.y = Input.GetAxis("Mouse Y") * -1 - _recoilValue;

            _shouldRecoil = false;
        }

        _localCameraHandler.SetViewInputVetor(_viewInputVector);
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new();


        networkInputData.AimForwardVector = _localCameraHandler.transform.forward;

        networkInputData.MovementInput = _moveInputVector;

        networkInputData.IsJumpPressed = _isJumButtonPressed;

        networkInputData.IsFireButtonPressed = _isFireButtonPressed;

        networkInputData.IsCrouching = _isCrouching;

        if (_isFireButtonPressed)
            _shouldRecoil = true;

        _isJumButtonPressed = false;
        _isFireButtonPressed = false;


        return networkInputData;
    }
}
