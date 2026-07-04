using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.18f;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);
    [SerializeField] private bool lockX = false;
    [SerializeField] private bool lockY = false;

    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) target = go.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + (Vector3)offset;
        desired.z = transform.position.z; // keep camera Z

        if (lockX) desired.x = transform.position.x;
        if (lockY) desired.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref _velocity, smoothTime);
    }

    public void SetTarget(Transform t) => target = t;
}
