using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Combat Moves")]
    [SerializeField] private MoveData idleMove;

    [Header("Combat Components")]
    [SerializeField] private Weapon equippedWeapon;
    [SerializeField] private Hurtbox hurtbox;
    [SerializeField] private Animator animator;

    public MoveData IdleMove => idleMove;
    public StateMachine StateMachine { get; private set; }
    public InputBuffer InputBuffer { get; private set; }

    private void Awake()
    {
        InputBuffer = new InputBuffer();
        if (equippedWeapon == null) equippedWeapon = GetComponentInChildren<Weapon>();
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
        InputType currentInput = InputType.None;

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) currentInput |= InputType.LightAttack;
            if (Mouse.current.rightButton.wasPressedThisFrame) currentInput |= InputType.HeavyAttack;
        }
        else
        {
            // Fallback for old input system
            try
            {
                if (Input.GetMouseButtonDown(0)) currentInput |= InputType.LightAttack;
                if (Input.GetMouseButtonDown(1)) currentInput |= InputType.HeavyAttack;
            }
            catch { }
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.jKey.wasPressedThisFrame) currentInput |= InputType.LightAttack;
            if (Keyboard.current.kKey.wasPressedThisFrame) currentInput |= InputType.HeavyAttack;
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonWest.wasPressedThisFrame) currentInput |= InputType.LightAttack;
            if (Gamepad.current.buttonNorth.wasPressedThisFrame) currentInput |= InputType.HeavyAttack;
        }

        if (currentInput != InputType.None)
        {
            InputBuffer.AddInput(currentInput);
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
            if (equippedWeapon != null && equippedWeapon.hitbox != null)
            {
                equippedWeapon.hitbox.SetHitboxActive(currentWindow.hitboxActive);
            }
        }
        else
        {
            // Default state outside of specific frame windows
            if (hurtbox != null)
            {
                hurtbox.SetHurtState(HurtState.Vulnerable);
            }
            if (equippedWeapon != null && equippedWeapon.hitbox != null)
            {
                equippedWeapon.hitbox.SetHitboxActive(false);
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
