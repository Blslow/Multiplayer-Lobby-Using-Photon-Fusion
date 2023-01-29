using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    [SerializeField]
    private Transform _cameraAnchorPoint;

    private Vector2 _viewInput;

    private float _cameraRotationX = 0;
    private float _cameraRotationY = 0;

    private Camera _localCamera;
    private NetworkCharacterControllerPrototypeCustom _networkCharacterControllerPrototypeCustom;

    private void Awake()
    {
        _localCamera = GetComponent<Camera>();
        _networkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
    }

    private void Start()
    {
        if (_localCamera.enabled)
            _localCamera.transform.parent = null;
    }

    private void LateUpdate()
    {
        if (_cameraAnchorPoint == null)
            return;

        if (!_localCamera.enabled)
            return;

        _localCamera.transform.localPosition = _cameraAnchorPoint.position;

        _cameraRotationX += _viewInput.y * Time.deltaTime * _networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -90, 90);

        _cameraRotationY += _viewInput.x * Time.deltaTime * _networkCharacterControllerPrototypeCustom.rotationSpeed;

        _localCamera.transform.rotation = Quaternion.Euler(_cameraRotationX, _cameraRotationY, 0);
    }

    public void SetViewInputVetor(Vector2 viewInput)
    {
        _viewInput = viewInput;
    }
}
