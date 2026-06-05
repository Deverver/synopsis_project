using UnityEngine;

public class CombatFeedbackUI : MonoBehaviour
{
    public PlayerController player;
    public Enemy enemy;

    private GUIStyle richTextStyle;

    private void OnGUI()
    {
        if (richTextStyle == null)
        {
            richTextStyle = new GUIStyle(GUI.skin.label);
            richTextStyle.richText = true;
            richTextStyle.fontSize = 20;
        }

        if (player == null || player.StateMachine == null || player.StateMachine.CurrentState == null) return;

        var state = player.StateMachine.CurrentState;
        var move = state.Move;
        var window = state.GetCurrentFrameWindow();

        GUILayout.BeginArea(new Rect(20, 20, 400, 400));
        GUILayout.Label($"<b>Current Move:</b> {(move != null ? move.moveName : "None")}", richTextStyle);
        GUILayout.Label($"<b>Normalized Time:</b> {state.NormalizedTime:F2}", richTextStyle);
        
        string windowStr = window != null ? window.windowType.ToString() : "None";
        if (window != null && window.windowType == FrameWindowType.Combo)
        {
            windowStr = "<color=green><b>COMBO AVAILABLE!</b></color>";
        }
        else if (window != null && window.windowType == FrameWindowType.Active)
        {
            windowStr = "<color=red><b>ACTIVE (Hitbox ON)</b></color>";
        }

        GUILayout.Label($"<b>Frame Window:</b> {windowStr}", richTextStyle);

        if (enemy != null)
        {
            GUILayout.Label($"<b>Enemy State:</b> {enemy.CurrentState}", richTextStyle);
        }

        GUILayout.EndArea();
    }
}
