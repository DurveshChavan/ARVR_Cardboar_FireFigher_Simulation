using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CardboardPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public Transform cameraTransform;

    private CharacterController cc;
    private float verticalVelocity = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Gyroscope is handled automatically by the Cardboard XR Plugin.
        // No need to call Input.gyro (which is Old Input API and crashes).

        Debug.Log("CardboardPlayerController ready");
    }

    void Update()
    {
        var gp = Gamepad.current;
        if (gp == null) return;

        float h = gp.leftStick.x.ReadValue();
        float v = gp.leftStick.y.ReadValue();

        // Apply gravity even when not moving
        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
        {
            cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
            if (cc.isGrounded) verticalVelocity = 0f;
            return;
        }

        // Move in the direction the camera (gyroscope) is facing
        Vector3 camForward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTransform != null ? cameraTransform.right : transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDirection = (camForward * v) + (camRight * h);
        moveDirection *= moveSpeed;
        moveDirection.y = verticalVelocity;

        cc.Move(moveDirection * Time.deltaTime);
        if (cc.isGrounded) verticalVelocity = 0f;
    }
}
