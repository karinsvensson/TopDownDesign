using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraController : Singleton<CameraController>
{
    [SerializeField] Transform target;

    [Space]

    [SerializeField] Vector3 cameraOffset = new Vector3(0, 10, -10);

    [Space]

    [SerializeField] int targetFrameRate = 60;

    private Vector3 targetPosition;

    Coroutine currentCameraShakeRoutine;

    private void OnValidate()
    {
        LookAtTarget();
    }

    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;

        MoveCamera();
        LookAtTarget();
    }

    public void LookAtTarget()
    {
        transform.LookAt(target);
    }

    private void LateUpdate()
    {
        if (currentCameraShakeRoutine != null) { return; }

        MoveCamera();
    }

    public void MoveCamera()
    {
        targetPosition = CalculateTargetPosition();
        transform.position = targetPosition;
    }

    private Vector3 CalculateTargetPosition()
    {
        return target.transform.position + cameraOffset;
    }

    #region Freeze Frames
    public void FreezeFrames(int numberOfFrames)
    {
        StartCoroutine(FreezeFrameRoutine(numberOfFrames));
    }

    private IEnumerator FreezeFrameRoutine(int numberOfFrames)
    {
        Time.timeScale = 0;
        for (int i = 0; i < numberOfFrames; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = 1;
    }
    #endregion

    #region Camera Shake
    public void StartCameraShake(float magnitude, float duration)
    {
        if (currentCameraShakeRoutine != null) { StopCoroutine(currentCameraShakeRoutine); }

        currentCameraShakeRoutine = StartCoroutine(CameraShakeRoutine(magnitude, duration));
    }

    private IEnumerator CameraShakeRoutine(float magnitude, float duration)
    {
        float shakeDuration = duration;
        float shakeMagnitude = magnitude / 10f;

        while(shakeDuration > 0f)
        {
            transform.position = CalculateTargetPosition() + Random.insideUnitSphere * shakeMagnitude;

            shakeDuration -= Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        currentCameraShakeRoutine = null;
    }
    #endregion
}
