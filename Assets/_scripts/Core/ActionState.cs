using UnityEngine;

public class ActionState
{
    public MoveData Move { get; private set; }
    public float ElapsedTime { get; private set; }

    public float NormalizedTime
    {
        get
        {
            if (Move == null || Move.duration <= 0f) return 0f;
            return Mathf.Clamp01(ElapsedTime / Move.duration);
        }
    }

    public ActionState(MoveData move)
    {
        Move = move;
        ElapsedTime = 0f;
    }

    public void Update(float deltaTime)
    {
        ElapsedTime += deltaTime;
    }

    public FrameWindow GetCurrentFrameWindow()
    {
        if (Move == null || Move.frameWindows == null) return null;

        float t = NormalizedTime;
        foreach (var window in Move.frameWindows)
        {
            if (t >= window.start && t <= window.end)
            {
                return window;
            }
        }
        return null;
    }
}
