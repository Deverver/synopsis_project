using UnityEngine;

[CreateAssetMenu(fileName = "NewMoveData", menuName = "Combat/Move Data")]
public class MoveData : ScriptableObject
{
    public string moveName;
    public MoveType moveType;
    public float duration;
    public FrameWindow[] frameWindows;
    public Transition[] transitions;
    public AnimationClip animation;
}
