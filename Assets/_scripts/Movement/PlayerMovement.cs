using UnityEngine;
using UnityEngine.InputSystem;

/*
Type: MonoBehaviour
Lives on: Player GameObject

Role:
    Standalone third-person movement controller.
    Reads WASD from the keyboard, projects input onto the camera's XZ plane,
    and applies force to the Rigidbody. The Rigidbody's linearDamping (drag)
    provides natural friction/deceleration so the player never feels floaty.

    The player body rotates toward the movement direction using Slerp, which
    creates a small, visible turn radius rather than snapping instantly.

    This script is intentionally decoupled from the combat state machine,
    InputBuffer, StateMachine, and CombatResolver. It reads raw keyboard state
    directly and does not share any data with those systems.
*/
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveForce = 40f;
    [SerializeField] private float maxSpeed = 7f;

    [Header("Rotation")]
    [Tooltip("Higher values = tighter/faster turn. Tune this in the Inspector.")]
    [SerializeField] private float turnSpeed = 10f;

    // ── Private references ─────────────────────────────────────────────────
    private Rigidbody rb;
    private Camera mainCam;

    // ── Lock-on integration (read-only, no coupling) ───────────────────────
    // PlayerMovement reads IsLockedOn to decide whether to strafe vs. face
    // movement direction, but does not call any methods on LockOnController.
    private LockOnController lockOnController;

    // ── Unity lifecycle ────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lockOnController = GetComponent<LockOnController>();
    }

    private void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null)
            Debug.LogWarning("[PlayerMovement] No Camera tagged 'MainCamera' found.");
    }

    private void FixedUpdate()
    {
        Vector2 rawInput = ReadMoveInput();
        Vector3 moveDir = ProjectOntoGroundPlane(rawInput);

        ApplyMovementForce(moveDir);
        CapHorizontalSpeed();
        RotateTowardMovement(moveDir);
    }

    // ── Input ──────────────────────────────────────────────────────────────
    private Vector2 ReadMoveInput()
    {
        if (Keyboard.current == null) return Vector2.zero;

        float x = 0f, z = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  z -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    z += 1f;

        Vector2 raw = new Vector2(x, z);
        if (raw.sqrMagnitude > 1f) raw.Normalize();
        return raw;
    }

    // ── Movement ───────────────────────────────────────────────────────────

    /// <summary>
    /// Projects the 2-axis input onto the camera's forward/right axes,
    /// then flattens the result onto the XZ ground plane.
    /// </summary>
    private Vector3 ProjectOntoGroundPlane(Vector2 input)
    {
        if (mainCam == null || input.sqrMagnitude < 0.01f) return Vector3.zero;

        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight   = mainCam.transform.right;

        // Flatten to ground plane
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return camForward * input.y + camRight * input.x;
    }

    private void ApplyMovementForce(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.01f) return;
        rb.AddForce(moveDir * moveForce, ForceMode.Force);
    }

    /// <summary>
    /// Clamps horizontal velocity to maxSpeed without affecting vertical
    /// (gravity) velocity so the player doesn't fly off ledges weirdly.
    /// </summary>
    private void CapHorizontalSpeed()
    {
        Vector3 horizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontal.magnitude > maxSpeed)
        {
            Vector3 capped = horizontal.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(capped.x, rb.linearVelocity.y, capped.z);
        }
    }

    // ── Rotation ───────────────────────────────────────────────────────────
    private void RotateTowardMovement(Vector3 moveDir)
    {
        bool isLockedOn = lockOnController != null && lockOnController.IsLockedOn;

        if (isLockedOn)
        {
            // While locked on, face the enemy target rather than the movement direction.
            if (lockOnController.EnemyCameraTarget != null)
            {
                Vector3 toEnemy = lockOnController.EnemyCameraTarget.position - transform.position;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toEnemy);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
                }
            }
        }
        else
        {
            // Normal mode: rotate toward the velocity direction for a grounded feel.
            Vector3 horizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (horizontal.magnitude > 0.2f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontal);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
