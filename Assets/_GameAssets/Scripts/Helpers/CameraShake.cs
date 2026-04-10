using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin _cinemachineBasicMultiChannelPerlin;

    private float _shakeTimer;
    private float _shakeTimerTotal;
    private float _startingIntensity;

    private void Awake()
    {
        _cinemachineBasicMultiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
        _startingIntensity = _cinemachineBasicMultiChannelPerlin.AmplitudeGain;
    }

    private void Update()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;

            if (_shakeTimer <= 0f)
            {
                _cinemachineBasicMultiChannelPerlin.AmplitudeGain
                    = Mathf.Lerp(_startingIntensity, 0f, 1 - (_shakeTimer / _shakeTimerTotal));
            }
        }
    }

    public void ShakeCamera(float intensity, float time, float delay = 0f)
    {
        StartCoroutine(ShakeCameraCoroutine(intensity, time, delay));
    }

    private IEnumerator ShakeCameraCoroutine(float intensity, float time, float delay)
    {
        yield return new WaitForSeconds(delay);

        _cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;
        _shakeTimer = time;
        _shakeTimerTotal = time;
        _startingIntensity = intensity;
    }
}
