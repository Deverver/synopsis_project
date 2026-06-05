using UnityEngine;
using System.Collections;

public class CombatSystemTestRunner : MonoBehaviour
{
    private PlayerController player;
    private CombatResolver resolver;

    private void Start()
    {
#if UNITY_2023_1_OR_NEWER
        player = Object.FindFirstObjectByType<PlayerController>();
        resolver = Object.FindFirstObjectByType<CombatResolver>();
#else
        player = Object.FindObjectOfType<PlayerController>();
        resolver = Object.FindObjectOfType<CombatResolver>();
#endif
        
        if (player == null || resolver == null)
        {
            Debug.LogError("[TestRunner] Failed to find PlayerController or CombatResolver!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            return;
        }

        StartCoroutine(RunTestRoutine());
    }

    private IEnumerator RunTestRoutine()
    {
        Debug.Log("[TestRunner] Starting Timeline-Driven Combat System Test...");

        // Scenario 1: Idle state validation
        yield return new WaitForSeconds(0.1f);
        Debug.Log($"[TestRunner] Initial state: Move={player.StateMachine.CurrentState.Move.moveName}, NormalizedTime={player.StateMachine.CurrentState.NormalizedTime}");
        if (player.StateMachine.CurrentState.Move.moveName != "Idle")
        {
            Debug.LogError("[TestRunner] FAILED: Player did not start in Idle!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 2: Trigger LightAttack
        Debug.Log("[TestRunner] Simulating LightAttack input...");
        player.InputBuffer.AddInput(InputType.LightAttack);
        
        // Wait a frame for input buffer to process in Update and transition to LightSlash1
        yield return null;
        Debug.Log($"[TestRunner] Post-Attack Input: Move={player.StateMachine.CurrentState.Move.moveName}");
        if (player.StateMachine.CurrentState.Move.moveName != "LightSlash1")
        {
            Debug.LogError("[TestRunner] FAILED: Did not transition to LightSlash1!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 3: Verify Hitbox is inactive before active frame window (before 0.2 normalized, duration 0.8s = 0.16s)
        yield return new WaitForSeconds(0.1f);
        bool hitboxActiveBefore = player.StateMachine.CurrentState.GetCurrentFrameWindow() != null && player.StateMachine.CurrentState.GetCurrentFrameWindow().hitboxActive;
        Debug.Log($"[TestRunner] Time={player.StateMachine.CurrentState.ElapsedTime:F2}s, NormalizedTime={player.StateMachine.CurrentState.NormalizedTime:F2}: HitboxActive={hitboxActiveBefore}");
        if (hitboxActiveBefore)
        {
            Debug.LogError("[TestRunner] FAILED: Hitbox active too early!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 4: Verify Hitbox is active during frame window (0.21 to 0.40, duration 0.8s => 0.17s to 0.32s)
        while (player.StateMachine.CurrentState.ElapsedTime < 0.25f)
        {
            yield return null;
        }
        bool hitboxActiveDuring = player.StateMachine.CurrentState.GetCurrentFrameWindow() != null && player.StateMachine.CurrentState.GetCurrentFrameWindow().hitboxActive;
        Debug.Log($"[TestRunner] Time={player.StateMachine.CurrentState.ElapsedTime:F2}s, NormalizedTime={player.StateMachine.CurrentState.NormalizedTime:F2}: HitboxActive={hitboxActiveDuring}");
        if (!hitboxActiveDuring)
        {
            Debug.LogError("[TestRunner] FAILED: Hitbox not active during window [0.21 - 0.40]!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 5: Simulate HeavyAttack input within the combo transition window (0.41 to 0.80 => >0.33s)
        while (player.StateMachine.CurrentState.ElapsedTime < 0.45f)
        {
            yield return null;
        }
        Debug.Log("[TestRunner] Simulating HeavyAttack input for combo transition...");
        player.InputBuffer.AddInput(InputType.HeavyAttack);

        // Scenario 6: Verify transition to HeavySlash1 occurs
        yield return null;
        Debug.Log($"[TestRunner] Post-Combo Input: Move={player.StateMachine.CurrentState.Move.moveName}");
        if (player.StateMachine.CurrentState.Move.moveName != "HeavySlash1")
        {
            Debug.LogError("[TestRunner] FAILED: Did not transition to HeavySlash1 on combo input!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 7: Verify HurtState in HeavySlash1 starts as Invincible (window 0.0 to 0.5)
        yield return new WaitForSeconds(0.1f);
        var currentWindow = player.StateMachine.CurrentState.GetCurrentFrameWindow();
        HurtState hurtStateBefore = currentWindow != null ? currentWindow.hurtState : HurtState.Vulnerable;
        Debug.Log($"[TestRunner] Time={player.StateMachine.CurrentState.ElapsedTime:F2}s, NormalizedTime={player.StateMachine.CurrentState.NormalizedTime:F2}: HurtState={hurtStateBefore}");
        if (hurtStateBefore != HurtState.Invincible)
        {
            Debug.LogError($"[TestRunner] FAILED: HurtState during HeavySlash1 initial window is not Invincible! Found: {hurtStateBefore}");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 8: Wait for HeavySlash1 to complete and verify it returns to Idle
        Debug.Log("[TestRunner] Waiting for HeavySlash1 to complete...");
        while (player.StateMachine.CurrentState.Move.moveName == "HeavySlash1")
        {
            yield return null;
        }
        Debug.Log($"[TestRunner] Post-HeavySlash1: Move={player.StateMachine.CurrentState.Move.moveName}");
        if (player.StateMachine.CurrentState.Move.moveName != "Idle")
        {
            Debug.LogError("[TestRunner] FAILED: Did not return to Idle after HeavySlash1 completed!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        Debug.Log("[TestRunner] SUCCESS: All timeline-driven combat test cases passed!");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(0);
#endif
    }
}
