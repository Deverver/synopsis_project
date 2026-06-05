using UnityEngine;

public class CombatResolver : MonoBehaviour
{
    public static CombatResolver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public HitResults Resolve(Hitbox hitbox, Hurtbox hurtbox)
    {
        HurtState state = hurtbox.CurrentHurtState;

        if (state == HurtState.Vulnerable)
        {
            Debug.Log($"[CombatResolver] RESOLVE: Hitbox on '{hitbox.gameObject.name}' -> Hurtbox on '{hurtbox.gameObject.name}' (HurtState: {state}) => HIT CONFIRMED");
            hurtbox.ReceiveHit(hitbox);
            
            // Try to find an Enemy component to enter hitstun
            Enemy enemy = hurtbox.GetComponent<Enemy>();
            if (enemy == null) enemy = hurtbox.GetComponentInParent<Enemy>();
            
            if (enemy != null)
            {
                enemy.EnterHitstun(0.5f); // 0.5 seconds hitstun
            }

            // Simple hit feedback
            Debug.Log("<color=red><b>HIT CONFIRMED!</b></color> (Placeholder for Hitstop/Particles)");
            
            return HitResults.Hit;
        }
        else if (state == HurtState.Invincible)
        {
            Debug.Log($"[CombatResolver] RESOLVE: Hitbox on '{hitbox.gameObject.name}' -> Hurtbox on '{hurtbox.gameObject.name}' (HurtState: {state}) => HIT IGNORED");
            return HitResults.Ignored;
        }

        return HitResults.Ignored;
    }
}
