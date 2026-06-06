using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to the Main Camera.
/// Follows the player with mouse-controlled orbit. No Cinemachine required.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;          // Player root transform
    [SerializeField] private float heightOffset = 1.6f; // Look at player's upper body

    [Header("Distance")]
    [SerializeField] private float distance    = 5f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 10f;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float sensitivityX = 180f;
    [SerializeField] private float sensitivityY = 120f;

    [Header("Vertical Clamp")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch =  60f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.2f;

    private float _yaw;
    private float _pitch = 10f;

    private bool      _lockedOn;
    private Transform _lockTarget;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Start behind the player
        if (player != null)
            _yaw = player.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        // ── Read input ─────────────────────────────────────────────────────
        Vector2 mouse = Mouse.current?.delta.ReadValue() ?? Vector2.zero;

        if (_lockedOn && _lockTarget != null)
        {
            // Aim at the midpoint between player and enemy
            Vector3 midpoint = (player.position + _lockTarget.position) * 0.5f;
            Vector3 behindPlayer = _lockTarget.position - player.position;
            behindPlayer.y = 0f;
            _yaw = Mathf.LerpAngle(_yaw, Quaternion.LookRotation(behindPlayer).eulerAngles.y, 5f * Time.deltaTime);
            // Drive yaw toward the enemy direction, ignoring mouse X
            _yaw   = Mathf.LerpAngle(_yaw, Quaternion.LookRotation(behindPlayer).eulerAngles.y, 5f * Time.deltaTime);
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }
        else
        {
            _yaw   += mouse.x * sensitivityX * Time.deltaTime;
            _pitch -= mouse.y * sensitivityY * Time.deltaTime;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        // ── Compute desired position ───────────────────────────────────────
        Vector3 lookTarget;
        float   frameDistance = distance;

        if (_lockedOn && _lockTarget != null)
        {
            lookTarget    = (player.position + Vector3.up * heightOffset 
                        +  _lockTarget.position) * 0.5f;
            float separation  = Vector3.Distance(player.position, _lockTarget.position);
            frameDistance = Mathf.Clamp(separation * 1.4f, distance, maxDistance);
        }
        else
        {
            lookTarget    = player.position + Vector3.up * heightOffset;
            frameDistance = distance;
        }

        Quaternion rotation   = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    desiredPos = lookTarget - rotation * Vector3.forward * frameDistance;

        // ── Camera collision: pull in if geometry is in the way ────────────
        float actualDistance = frameDistance;
        if (Physics.SphereCast(lookTarget, collisionRadius,
            (desiredPos - lookTarget).normalized,
            out RaycastHit hit, frameDistance, collisionMask))
        {
            actualDistance = Mathf.Clamp(hit.distance - collisionRadius, minDistance, frameDistance);
        }

    Vector3 finalPos = lookTarget - rotation * Vector3.forward * actualDistance;

        // ── Apply ──────────────────────────────────────────────────────────
        transform.position = finalPos;
        transform.LookAt(lookTarget);
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        _yaw   = player != null ? player.eulerAngles.y : 0f;
    }
    
    public void SetLockOn(bool active, Transform target)
    {
        _lockedOn   = active;
        _lockTarget = target;
    }


}