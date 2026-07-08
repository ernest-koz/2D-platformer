using UnityEngine;
using UnityEngine.Serialization;

public class CameraFollow : MonoBehaviour
{
    [FormerlySerializedAs("target")] [SerializeField] private Transform _target;
    [FormerlySerializedAs("smoothTime")] [SerializeField] private float _smoothTime = 0.18f;
    [FormerlySerializedAs("offset")] [SerializeField] private Vector2 _offset = new Vector2(0f, 1.5f);
    [FormerlySerializedAs("lockX")] [SerializeField] private bool _lockX = false;
    [FormerlySerializedAs("lockY")] [SerializeField] private bool _lockY = false;

    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        if (_target == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");

            if (go != null) _target = go.transform;
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desired = _target.position + (Vector3)_offset;
        desired.z = transform.position.z;

        if (_lockX) desired.x = transform.position.x;

        if (_lockY) desired.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref _velocity, _smoothTime);
    }

    public void SetTarget(Transform t) =>
        _target = t;
}
