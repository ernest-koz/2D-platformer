using UnityEngine;

public class InputReader : MonoBehaviour
{
    [Header("Keys")]
    [SerializeField] private KeyCode _leftKey = KeyCode.A;
    [SerializeField] private KeyCode _rightKey = KeyCode.D;
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode _restartKey = KeyCode.R;
    [SerializeField] private KeyCode _vampirismKey = KeyCode.E;

    private bool _isBlocked;

    public float Direction { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool IsRestartPressed { get; private set; }
    public bool IsVampirismPressed { get; private set; }

    public bool IsBlocked => _isBlocked;

    public void Read()
    {
        IsRestartPressed = Input.GetKeyDown(_restartKey);
        IsVampirismPressed = Input.GetKeyDown(_vampirismKey);

        if (_isBlocked)
        {
            Direction = 0f;
            IsJumpPressed = false;
            IsJumpHeld = false;
            return;
        }

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
    }

    public void Block()
    {
        _isBlocked = true;
    }
}
