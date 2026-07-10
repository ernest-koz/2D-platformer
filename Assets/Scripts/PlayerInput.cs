using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float HorizontalInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool RestartPressed { get; private set; }

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

        JumpPressed = Input.GetKeyDown(KeyCode.Space)
                   || Input.GetKeyDown(KeyCode.W)
                   || Input.GetKeyDown(KeyCode.UpArrow);

        JumpHeld = Input.GetKey(KeyCode.Space)
                || Input.GetKey(KeyCode.W)
                || Input.GetKey(KeyCode.UpArrow);

        AttackPressed = Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0);
        RestartPressed = Input.GetKeyDown(KeyCode.R);
    }
}
