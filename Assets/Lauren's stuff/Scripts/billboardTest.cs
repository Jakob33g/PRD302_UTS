using UnityEngine;
using UnityEngine.UIElements;

/*
CODE WRITTEN BY LAUREN
*/
public class CamTest : MonoBehaviour
{
    private Camera _mainCamera;
    private void Awake()
    {
        _mainCamera = Camera.main;
    }
    private void LateUpdate()
    {
        Vector3 cameraPosition = _mainCamera.transform.position;
        cameraPosition.y = transform.position.y;
        transform.LookAt(cameraPosition);
        transform.Rotate(0f, 180f, 0f);
    }
}
