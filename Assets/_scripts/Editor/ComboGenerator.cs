using UnityEngine;
using UnityEditor;

public static class ComboGenerator
{
    [MenuItem("Combat System/Generate Combo Data")]
    public static void GenerateComboData()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_moves"))
        {
            AssetDatabase.CreateFolder("Assets", "_moves");
        }

        // Create instances
        MoveData idle = CreateMove("Idle", MoveType.Idle, 1f);
        MoveData light1 = CreateMove("LightSlash1", MoveType.LightSlash1, 0.8f);
        MoveData light2 = CreateMove("LightSlash2", MoveType.LightSlash2, 0.8f);
        MoveData light3 = CreateMove("LightSlash3", MoveType.LightSlash3, 1.2f);
        MoveData heavy1 = CreateMove("HeavySlash1", MoveType.HeavySlash1, 1.5f);
        MoveData rising = CreateMove("RisingSlash", MoveType.RisingSlash, 1.3f);

        // Define windows
        light1.frameWindows = new FrameWindow[] {
            new FrameWindow { windowType = FrameWindowType.Startup, start = 0f, end = 0.2f, hitboxActive = false, hurtState = HurtState.Vulnerable },
            new FrameWindow { windowType = FrameWindowType.Active, start = 0.21f, end = 0.4f, hitboxActive = true, hurtState = HurtState.Vulnerable },
            new FrameWindow { windowType = FrameWindowType.Combo, start = 0.41f, end = 0.8f, hitboxActive = false, hurtState = HurtState.Vulnerable }
        };
        
        light2.frameWindows = light1.frameWindows;
        light3.frameWindows = light1.frameWindows;
        
        heavy1.frameWindows = new FrameWindow[] {
            new FrameWindow { windowType = FrameWindowType.Startup, start = 0f, end = 0.5f, hitboxActive = false, hurtState = HurtState.Invincible },
            new FrameWindow { windowType = FrameWindowType.Active, start = 0.51f, end = 0.8f, hitboxActive = true, hurtState = HurtState.Vulnerable },
            new FrameWindow { windowType = FrameWindowType.Combo, start = 0.81f, end = 1.0f, hitboxActive = false, hurtState = HurtState.Vulnerable }
        };

        rising.frameWindows = heavy1.frameWindows;

        // Define transitions
        idle.transitions = new Transition[] {
            new Transition { requiredInput = InputType.LightAttack | InputType.HeavyAttack, windowStart = 0f, windowEnd = 1f, nextMove = rising },
            new Transition { requiredInput = InputType.HeavyAttack, windowStart = 0f, windowEnd = 1f, nextMove = heavy1 },
            new Transition { requiredInput = InputType.LightAttack, windowStart = 0f, windowEnd = 1f, nextMove = light1 }
        };

        light1.transitions = new Transition[] {
            new Transition { requiredInput = InputType.LightAttack | InputType.HeavyAttack, windowStart = 0.41f, windowEnd = 0.8f, nextMove = rising },
            new Transition { requiredInput = InputType.HeavyAttack, windowStart = 0.41f, windowEnd = 0.8f, nextMove = heavy1 },
            new Transition { requiredInput = InputType.LightAttack, windowStart = 0.41f, windowEnd = 0.8f, nextMove = light2 }
        };

        light2.transitions = new Transition[] {
            new Transition { requiredInput = InputType.HeavyAttack, windowStart = 0.41f, windowEnd = 0.8f, nextMove = heavy1 },
            new Transition { requiredInput = InputType.LightAttack, windowStart = 0.41f, windowEnd = 0.8f, nextMove = light3 }
        };

        // Light3, Heavy1, Rising return to idle implicitly at the end of duration (handled by StateMachine)

        // Save changes
        EditorUtility.SetDirty(idle);
        EditorUtility.SetDirty(light1);
        EditorUtility.SetDirty(light2);
        EditorUtility.SetDirty(light3);
        EditorUtility.SetDirty(heavy1);
        EditorUtility.SetDirty(rising);
        AssetDatabase.SaveAssets();

        Debug.Log("Combo MoveData generated successfully.");
    }

    private static MoveData CreateMove(string name, MoveType type, float duration)
    {
        string path = $"Assets/_moves/{name}.asset";
        MoveData move = AssetDatabase.LoadAssetAtPath<MoveData>(path);
        if (move == null)
        {
            move = ScriptableObject.CreateInstance<MoveData>();
            AssetDatabase.CreateAsset(move, path);
        }
        move.moveName = name;
        move.moveType = type;
        move.duration = duration;
        return move;
    }
}
