using UnityEngine;

public class SpinAndBob : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 120f;
    [SerializeField] private float bobAmount = 0.1f;
    [SerializeField] private float bobSpeed = 3f;

    private Vector3 _startPos;

    private void Awake()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, Time.time * spinSpeed, 0f);

        float y = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        var p = transform.position;
        p.y = y;
        transform.position = p;
    }
}
