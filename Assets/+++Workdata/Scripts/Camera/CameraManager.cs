using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera startCamera;
    [SerializeField] private CinemachineCamera playerCamera;

    private CinemachineCamera _currentCamera;
    
    private void Start()
    {
        SetNewCamera(startCamera);
    }

    public void SetNewCamera(CinemachineCamera newCamera)
    {
        _currentCamera = newCamera;
        _currentCamera.Prioritize();
    }

    public void ResetCameraToPlayer()
    {
        SetNewCamera(playerCamera);
    }
}
