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

        // Scenario 2: Trigger Attack1
        Debug.Log("[TestRunner] Simulating Attack input...");
        player.InputBuffer.AddInput(InputType.Attack);
        
        // Wait a frame for input buffer to process in Update and transition to Attack1
        yield return null;
        Debug.Log($"[TestRunner] Post-Attack Input: Move={player.StateMachine.CurrentState.Move.moveName}");
        if (player.StateMachine.CurrentState.Move.moveName != "Attack1")
        {
            Debug.LogError("[TestRunner] FAILED: Did not transition to Attack1!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 3: Verify Hitbox is inactive before active frame window (before 0.3 * 1.5s = 0.45s)
        yield return new WaitForSeconds(0.2f);
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

        // Scenario 4: Verify Hitbox is active during frame window (0.3 to 0.5, i.e., 0.45s to 0.75s)
        // Let's wait until elapsedTime is ~0.6s
        while (player.StateMachine.CurrentState.ElapsedTime < 0.6f)
        {
            yield return null;
        }
        bool hitboxActiveDuring = player.StateMachine.CurrentState.GetCurrentFrameWindow() != null && player.StateMachine.CurrentState.GetCurrentFrameWindow().hitboxActive;
        Debug.Log($"[TestRunner] Time={player.StateMachine.CurrentState.ElapsedTime:F2}s, NormalizedTime={player.StateMachine.CurrentState.NormalizedTime:F2}: HitboxActive={hitboxActiveDuring}");
        if (!hitboxActiveDuring)
        {
            Debug.LogError("[TestRunner] FAILED: Hitbox not active during window [0.3 - 0.5]!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 5: Simulate Attack input within the active transition window (0.3 to 0.7, i.e., 0.45s to 1.05s)
        // elapsedTime is 0.6s, which is 0.4 normalized time (valid transition window).
        Debug.Log("[TestRunner] Simulating second Attack input for combo transition...");
        player.InputBuffer.AddInput(InputType.Attack);

        // Scenario 6: Verify transition to Attack2 occurs
        // The transition is checked in StateMachine.Update.
        // Wait a frame for transition to be processed.
        yield return null;
        Debug.Log($"[TestRunner] Post-Combo Input: Move={player.StateMachine.CurrentState.Move.moveName}");
        if (player.StateMachine.CurrentState.Move.moveName != "Attack2")
        {
            Debug.LogError("[TestRunner] FAILED: Did not transition to Attack2 on combo input!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 7: Verify HurtState in Attack2 starts as Invincible (window 0.0 to 0.4)
        yield return new WaitForSeconds(0.2f);
        var currentWindow = player.StateMachine.CurrentState.GetCurrentFrameWindow();
        HurtState hurtStateBefore = currentWindow != null ? currentWindow.hurtState : HurtState.Vulnerable;
        Debug.Log($"[TestRunner] Time={player.StateMachine.CurrentState.ElapsedTime:F2}s, NormalizedTime={player.StateMachine.CurrentState.NormalizedTime:F2}: HurtState={hurtStateBefore}");
        if (hurtStateBefore != HurtState.Invincible)
        {
            Debug.LogError($"[TestRunner] FAILED: HurtState during Attack2 initial window is not Invincible! Found: {hurtStateBefore}");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(1);
#endif
            yield break;
        }

        // Scenario 8: Wait for Attack2 to complete and verify it returns to Idle
        Debug.Log("[TestRunner] Waiting for Attack2 to complete...");
        while (player.StateMachine.CurrentState.Move.moveName == "Attack2")
        {
            yield return null;
        }
        Debug.Log($"[TestRunner] Post-Attack2: Move={player.StateMachine.CurrentState.Move.moveName}");
        if (player.StateMachine.CurrentState.Move.moveName != "Idle")
        {
            Debug.LogError("[TestRunner] FAILED: Did not return to Idle after Attack2 completed!");
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
