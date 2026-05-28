using UnityEngine;

public class StateMachine
{
    public ActionState CurrentState { get; private set; }
    private readonly PlayerController player;

    public StateMachine(PlayerController player, MoveData startingMove)
    {
        this.player = player;
        ChangeState(startingMove);
    }

    public void ChangeState(MoveData newMove)
    {
        if (newMove == null) return;
        CurrentState = new ActionState(newMove);
        player.OnStateChanged(CurrentState);
    }

    public void Update(float deltaTime, InputBuffer inputBuffer)
    {
        if (CurrentState == null) return;

        CurrentState.Update(deltaTime);

        float t = CurrentState.NormalizedTime;
        bool transitioned = false;

        // Evaluate transitions defined in the current move
        if (CurrentState.Move.transitions != null)
        {
            foreach (var transition in CurrentState.Move.transitions)
            {
                if (transition.nextMove == null) continue;

                // Check if current normalized time is within the transition window
                if (t >= transition.windowStart && t <= transition.windowEnd)
                {
                    if (transition.requiredInput != InputType.None)
                    {
                        if (inputBuffer.HasInput(transition.requiredInput))
                        {
                            inputBuffer.ConsumeInput(transition.requiredInput);
                            ChangeState(transition.nextMove);
                            transitioned = true;
                            break;
                        }
                    }
                    else
                    {
                        // Automatic transition (no input required, e.g. end of combo or timer-based)
                        ChangeState(transition.nextMove);
                        transitioned = true;
                        break;
                    }
                }
            }
        }

        // Fallback to the default Idle move when the current move completes
        if (!transitioned && t >= 1.0f)
        {
            ChangeState(player.IdleMove);
        }
    }
}
