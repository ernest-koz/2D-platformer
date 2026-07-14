using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime = 0.18f;
    [SerializeField] private Vector2 _offset = new Vector2(0f, 1.5f);
    [SerializeField] private bool _isXLocked = false;
    [SerializeField] private bool _isYLocked = false;

    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        if (_target == null)
        {
            Debug.LogError($"CameraFollow: target not assigned on {gameObject.name}. Drag the player Transform in the inspector.", gameObject);
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        Vector3 desired = _target.position + (Vector3)_offset;
        desired.z = transform.position.z;

        if (_isXLocked)
        {
            desired.x = transform.position.x;
        }

        if (_isYLocked)
        {
            desired.y = transform.position.y;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref _velocity, _smoothTime);
    }

    public void SetTarget(Transform target) =>
        _target = target;
}
