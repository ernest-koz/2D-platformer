using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float HorizontalInput { get; private set; }
    public bool IsJumpPressed { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool IsRestartPressed { get; private set; }

    private void Update()
    {
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontal += 1f;
        }

        HorizontalInput = horizontal;

        IsJumpPressed = Input.GetKeyDown(KeyCode.Space)
                   || Input.GetKeyDown(KeyCode.W)
                   || Input.GetKeyDown(KeyCode.UpArrow);

        IsJumpHeld = Input.GetKey(KeyCode.Space)
                || Input.GetKey(KeyCode.W)
                || Input.GetKey(KeyCode.UpArrow);

        IsRestartPressed = Input.GetKeyDown(KeyCode.R);
    }
}
