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
