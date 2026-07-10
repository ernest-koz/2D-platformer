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
        transform.rotation = Quaternion.Euler(0f, Time.time * _spinSpeed, 0f);

        float verticalOffset = _startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobAmount;
        Vector3 currentPosition = transform.position;
        currentPosition.y = verticalOffset;
        transform.position = currentPosition;
    }
}
