using UnityEngine;

/*
Type: MonoBehaviour
lives on: Enemy GameObject

Role:
    Minimal test target
    Has Hurtbox
*/

public class Enemy : MonoBehaviour
{
    public Hurtbox[] hurtboxes { get; private set; }
    public EnemyState CurrentState { get; private set; }

    private float hitstunTimer = 0f;

    private void Awake()
    {
        // Find all hurtboxes on this object and any children
        hurtboxes = GetComponentsInChildren<Hurtbox>();
        CurrentState = EnemyState.Vulnerable;
    }

    private void Update()
    {
        if (CurrentState == EnemyState.Hitstun)
        {
            hitstunTimer -= Time.deltaTime;
            if (hitstunTimer <= 0f)
            {
                CurrentState = EnemyState.Vulnerable;
                SetAllHurtboxesState(HurtState.Vulnerable);
            }
        }
    }

    public void EnterHitstun(float duration)
    {
        CurrentState = EnemyState.Hitstun;
        hitstunTimer = duration;
        SetAllHurtboxesState(HurtState.Invincible); // Prevent multi-hits from the same attack
    }

    private void SetAllHurtboxesState(HurtState state)
    {
        foreach (var hb in hurtboxes)
        {
            if (hb != null) hb.SetHurtState(state);
        }
    }
}