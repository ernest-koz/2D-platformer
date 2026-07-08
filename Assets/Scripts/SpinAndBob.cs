using UnityEngine;
using UnityEngine.Serialization;

public class SpinAndBob : MonoBehaviour
{
    [FormerlySerializedAs("spinSpeed")] [SerializeField] private float _spinSpeed = 120f;
    [FormerlySerializedAs("bobAmount")] [SerializeField] private float _bobAmount = 0.1f;
    [FormerlySerializedAs("bobSpeed")] [SerializeField] private float _bobSpeed = 3f;

    private Vector3 _startPos;

    private void Awake()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, Time.time * _spinSpeed, 0f);

        float y = _startPos.y + Mathf.Sin(Time.time * _bobSpeed) * _bobAmount;
        var p = transform.position;
        p.y = y;
        transform.position = p;
    }
}
