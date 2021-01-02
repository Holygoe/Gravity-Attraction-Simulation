using Cinemachine;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private const float MIN_CAMERA_POSITION = 3;
    private const float MAX_CAMERA_POSITION = 30;
    
    [SerializeField] private CinemachineVirtualCamera[] cameras;
    [SerializeField] private GameObject menu;
    [SerializeField] private float cameraSpeed = 1;

    private int _currentCamera;

    private void Start()
    {
        SetCurrentCamera(0);
    }

    // ReSharper disable once UnusedMember.Local
    private void OnNextCamera()
    {
        SetCurrentCamera(_currentCamera + 1);
    }
    
    // ReSharper disable once UnusedMember.Local
    private void OnPreviousCamera()
    {
        SetCurrentCamera(_currentCamera - 1);
    }

    // ReSharper disable once UnusedMember.Local
    private void OnEscape()
    {
        menu.SetActive(!menu.activeSelf);
    }

    // ReSharper disable once UnusedMember.Local
    private void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetCurrentCamera(int value)
    {
        _currentCamera = Mathf.Clamp(value, 0, cameras.Length - 1);
        
        for (var i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = i == _currentCamera ? 5 : 0;
        }
    }
}
