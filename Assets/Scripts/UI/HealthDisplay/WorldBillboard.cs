using UnityEngine;

public class WorldBillboard : MonoBehaviour
{
    private Camera _camera;

    private void LateUpdate()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera == null)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
    }
}
