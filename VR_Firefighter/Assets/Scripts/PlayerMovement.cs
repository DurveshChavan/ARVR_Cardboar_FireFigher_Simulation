using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f;
    private CharacterController cc;
    private float verticalVelocity = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        var gp = Gamepad.current;
        if (gp == null) return;

        float h = gp.leftStick.x.ReadValue();
        float v = gp.leftStick.y.ReadValue();

        Vector3 move = transform.right * h + transform.forward * v;
        move *= speed;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);

        if (cc.isGrounded) verticalVelocity = 0f;
    }
}
