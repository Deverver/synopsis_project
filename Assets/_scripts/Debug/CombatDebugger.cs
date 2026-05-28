using UnityEngine;

public class CombatDebugger : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private void OnGUI()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player == null || player.StateMachine == null)
        {
            GUI.Box(new Rect(10, 10, 250, 40), "Combat Debugger: Player not found!");
            return;
        }

        var state = player.StateMachine.CurrentState;
        if (state == null) return;

        // Custom GUI styles
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 13;
        boxStyle.alignment = TextAnchor.UpperLeft;
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 13;
        labelStyle.richText = true;
        labelStyle.normal.textColor = Color.white;

        // Background container
        GUI.Box(new Rect(10, 10, 400, 250), "", boxStyle);
        GUI.Label(new Rect(20, 15, 380, 25), "<color=cyan><b>COMBAT SYSTEM DEBUGGER</b></color>", labelStyle);

        // Current Move Name
        GUI.Label(new Rect(20, 40, 380, 20), $"Current Move: <b><color=yellow>{state.Move.moveName}</color></b>", labelStyle);

        // Normalized Time
        GUI.Label(new Rect(20, 65, 380, 20), $"Normalized Time: <b>{state.NormalizedTime:F2}</b> ({state.ElapsedTime:F2}s / {state.Move.duration:F2}s)", labelStyle);

        // Progress Bar
        float barWidth = 360f;
        GUI.Box(new Rect(20, 90, barWidth, 15), "");
        GUI.color = Color.cyan;
        GUI.Box(new Rect(20, 90, barWidth * state.NormalizedTime, 15), "");
        GUI.color = Color.white;

        // Current Frame Window
        var currentWindow = state.GetCurrentFrameWindow();
        string windowInfo;
        if (currentWindow != null)
        {
            string hitboxColor = currentWindow.hitboxActive ? "red" : "orange";
            windowInfo = $"[{currentWindow.start:F2} - {currentWindow.end:F2}] | Hurt: <color=yellow>{currentWindow.hurtState}</color> | Hitbox: <color={hitboxColor}>{(currentWindow.hitboxActive ? "ACTIVE" : "INACTIVE")}</color>";
        }
        else
        {
            windowInfo = "No Active Frame Window (Default)";
        }
        GUI.Label(new Rect(20, 115, 380, 20), $"Frame Window: {windowInfo}", labelStyle);

        // Transitions list
        string transitionsInfo = "Available Transitions:\n";
        bool hasTransitions = false;
        if (state.Move.transitions != null)
        {
            foreach (var trans in state.Move.transitions)
            {
                if (trans.nextMove == null) continue;
                string inputStr = trans.requiredInput == InputType.None ? "None (Auto)" : trans.requiredInput.ToString();
                
                bool windowActive = (state.NormalizedTime >= trans.windowStart && state.NormalizedTime <= trans.windowEnd);
                string activeMarker = windowActive ? "<color=green>[ACTIVE WINDOW]</color>" : "<color=grey>[LOCKED]</color>";
                
                transitionsInfo += $"- to <b>{trans.nextMove.moveName}</b> via <i>{inputStr}</i> in [{trans.windowStart:F2} - {trans.windowEnd:F2}] {activeMarker}\n";
                hasTransitions = true;
            }
        }
        if (!hasTransitions)
        {
            transitionsInfo += "- None (will return to Idle on completion)";
        }
        
        GUI.Label(new Rect(20, 140, 380, 100), transitionsInfo, labelStyle);
    }
}
