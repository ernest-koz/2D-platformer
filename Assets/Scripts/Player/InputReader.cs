using UnityEngine;

public class InputReader : MonoBehaviour
{
    [Header("Keys")]
    [SerializeField] private KeyCode _leftKey = KeyCode.A;
    [SerializeField] private KeyCode _rightKey = KeyCode.D;
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode _restartKey = KeyCode.R;

    public float Direction { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool IsRestartPressed { get; private set; }

    private void Update()
    {
        float direction = 0f;

        if (Input.GetKey(_rightKey))
        {
            direction += 1f;
        }

        if (Input.GetKey(_leftKey))
        {
            direction -= 1f;
        }

        Direction = direction;
        IsJumpPressed = Input.GetKeyDown(_jumpKey);
        IsJumpHeld = Input.GetKey(_jumpKey);
        IsRestartPressed = Input.GetKeyDown(_restartKey);
    }
}
