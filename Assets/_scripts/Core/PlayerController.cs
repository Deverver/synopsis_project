using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Combat Moves")]
    [SerializeField] private MoveData idleMove;

    [Header("Combat Components")]
    [SerializeField] private Hitbox hitbox;
    [SerializeField] private Hurtbox hurtbox;
    [SerializeField] private Animator animator;

    public MoveData IdleMove => idleMove;
    public StateMachine StateMachine { get; private set; }
    public InputBuffer InputBuffer { get; private set; }

    private void Awake()
    {
        InputBuffer = new InputBuffer();
        if (hitbox == null) hitbox = GetComponentInChildren<Hitbox>();
        if (hurtbox == null) hurtbox = GetComponentInChildren<Hurtbox>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (idleMove == null)
        {
            Debug.LogWarning("PlayerController: IdleMove is not assigned!");
        }
        StateMachine = new StateMachine(this, idleMove);
    }

    private void Update()
    {
        ReadInput();
        InputBuffer.Update();
        if (StateMachine != null)
        {
            StateMachine.Update(Time.deltaTime, InputBuffer);
        }
        UpdateCombatBoxes();
    }

    private void ReadInput()
    {
        bool attackPressed = false;

        // Support new Input System
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
        {
            attackPressed = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            attackPressed = true;
        }
        else if (Gamepad.current != null && (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            attackPressed = true;
        }

        // This is a fix: should allow support for the old input system.
        if (!attackPressed)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                {
                    attackPressed = true;
                }
            }
            catch
            {
                // This is purely for stopping console spam, by ignoring exceptions form the old input manager.
            }
        }

        if (attackPressed)
        {
            InputBuffer.AddInput(InputType.Attack);
        }
    }

    private void UpdateCombatBoxes()
    {
        if (StateMachine == null || StateMachine.CurrentState == null) return;

        var currentState = StateMachine.CurrentState;
        var currentWindow = currentState.GetCurrentFrameWindow();

        if (currentWindow != null)
        {
            if (hurtbox != null)
            {
                hurtbox.SetHurtState(currentWindow.hurtState);
            }
            if (hitbox != null)
            {
                hitbox.SetHitboxActive(currentWindow.hitboxActive);
            }
        }
        else
        {
            // Default state outside of specific frame windows
            if (hurtbox != null)
            {
                hurtbox.SetHurtState(HurtState.Vulnerable);
            }
            if (hitbox != null)
            {
                hitbox.SetHitboxActive(false);
            }
        }
    }

    public void OnStateChanged(ActionState newState)
    {
        if (newState == null || newState.Move == null) return;

        // Only play animation if assigned
        if (animator != null && newState.Move.animation != null)
        {
            animator.Play(newState.Move.animation.name, 0, 0f);
        }
    }
}
