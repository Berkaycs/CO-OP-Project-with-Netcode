using UnityEngine;
using System.Collections.Generic;

public class PlayerVehicleVisualController : MonoBehaviour
{
    [SerializeField] private PlayerVehicleController _playerVehicleController;
    [SerializeField] private Transform _frontLeftWheel,_frontRightWheel,_backLeftWheel,_backRightWheel;
    [SerializeField] private float _wheelsSpinSpeed, _wheelYWhenSpringMin, _wheelYWhenSpringMax;

    private Quaternion _wheelFrontLeftRoll;
    private Quaternion _wheelFrontRightRoll;

    private float _springRestLength;
    private float _forwardSpeed;
    private float _steerInput;
    private float _steerAngle;

    private Dictionary<WheelType, float> _springsCurrentLength = new ()
    {
        { WheelType.FrontLeft, 0f },
        { WheelType.FrontRight, 0f },
        { WheelType.BackLeft, 0f },
        { WheelType.BackRight, 0f },
    };

    private void Start()
    {
        _wheelFrontLeftRoll = _frontLeftWheel.localRotation;
        _wheelFrontRightRoll = _frontRightWheel.localRotation;

        _springRestLength = _playerVehicleController.VehicleSettings.SpringRestLength;
        _steerAngle = _playerVehicleController.VehicleSettings.SteerAngle;
    }

    private void Update()
    {
        // Update Visual States
        UpdateVisualStates();
        // Rotate Wheels
        UpdateWheelsRotation();
        // Set Suspension
        UpdateSuspension();
    }

    private void UpdateVisualStates()
    {
        _steerInput = Input.GetAxis("Horizontal");

        _forwardSpeed = Vector3.Dot(_playerVehicleController.Forward, _playerVehicleController.Velocity);

        _springsCurrentLength[WheelType.FrontLeft] = _playerVehicleController.GetSpringLength(WheelType.FrontLeft);
        _springsCurrentLength[WheelType.FrontRight] = _playerVehicleController.GetSpringLength(WheelType.FrontRight);
        _springsCurrentLength[WheelType.BackLeft] = _playerVehicleController.GetSpringLength(WheelType.BackLeft);
        _springsCurrentLength[WheelType.BackRight] = _playerVehicleController.GetSpringLength(WheelType.BackRight);
    }

    private void UpdateWheelsRotation()
    {
        if (_springsCurrentLength[WheelType.FrontLeft] < _springRestLength)
        {
            _wheelFrontLeftRoll *= Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);
        }

        if (_springsCurrentLength[WheelType.FrontRight] < _springRestLength)
        {
            _wheelFrontRightRoll *= Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);
        }

        if (_springsCurrentLength[WheelType.BackLeft] < _springRestLength)
        {
            _backLeftWheel.localRotation *= Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);
        }

        if (_springsCurrentLength[WheelType.BackRight] < _springRestLength)
        {
            _backRightWheel.localRotation *= Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);
        }

        _frontLeftWheel.localRotation = Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontLeftRoll;
        _frontRightWheel.localRotation = Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontRightRoll;
    }

    private void UpdateSuspension()
    {
        float springFrontLeftRatio = _springsCurrentLength[WheelType.FrontLeft] / _springRestLength;
        float springFrontRightRatio = _springsCurrentLength[WheelType.FrontRight] / _springRestLength;
        float springBackLeftRatio = _springsCurrentLength[WheelType.BackLeft] / _springRestLength;
        float springBackRightRatio = _springsCurrentLength[WheelType.BackRight] / _springRestLength;

        _frontLeftWheel.localPosition = new Vector3(_frontLeftWheel.localPosition.x, _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springFrontLeftRatio, _frontLeftWheel.localPosition.z);
        _frontRightWheel.localPosition = new Vector3(_frontRightWheel.localPosition.x, _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springFrontRightRatio, _frontRightWheel.localPosition.z);
        _backLeftWheel.localPosition = new Vector3(_backLeftWheel.localPosition.x, _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springBackLeftRatio, _backLeftWheel.localPosition.z);
        _backRightWheel.localPosition = new Vector3(_backRightWheel.localPosition.x, _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springBackRightRatio, _backRightWheel.localPosition.z);
    }
}
