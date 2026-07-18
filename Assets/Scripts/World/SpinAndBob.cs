using UnityEngine;

public class SpinAndBob : MonoBehaviour
{
    [SerializeField] private float _spinSpeed = 120f;
    [SerializeField] private float _bobAmount = 0.1f;
    [SerializeField] private float _bobSpeed = 3f;

    private Vector3 _startPosition;

    private void Awake()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        float spinAngle = Time.time * _spinSpeed;
        transform.rotation = Quaternion.Euler(0f, spinAngle, 0f);

        float verticalOffset = Mathf.Sin(Time.time * _bobSpeed) * _bobAmount;
        Vector3 position = transform.position;
        position.y = _startPosition.y + verticalOffset;
        transform.position = position;
    }
}
