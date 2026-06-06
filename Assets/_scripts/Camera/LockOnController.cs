using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

/*
Type: MonoBehaviour
Lives on: Player GameObject

Role:
    Prototype lock-on controller. Pressing T toggles lock-on to the enemy.
    
    This script is intentionally decoupled from the combat state machine,
    InputBuffer, StateMachine, and CombatResolver. It only manages camera
    priority and a single bool flag.
*/
public class LockOnController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;

    [Header("Targets")]
    [SerializeField] private Transform enemyCameraTarget;

    [Header("Priorities")]  // kept for potential future use
    private int priorityActive   = 20;
    private int priorityInactive = 0;

    public bool IsLockedOn { get; private set; }
    public Transform EnemyCameraTarget => enemyCameraTarget;

    private void Start()  { SetLockOn(false); }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            Toggle();
    }

    private void Toggle() { SetLockOn(!IsLockedOn); }

    private void SetLockOn(bool active)
    {
        IsLockedOn = active;

        if (thirdPersonCamera != null)
            thirdPersonCamera.SetLockOn(active, active ? enemyCameraTarget : null);

        Debug.Log($"[LockOnController] Lock-on {(active ? "ENABLED" : "DISABLED")}");
    }

    // Called by setup script
    public void SetReferences(ThirdPersonCamera cam, Transform enemyTarget)
    {
        thirdPersonCamera  = cam;
        enemyCameraTarget  = enemyTarget;
    }
}