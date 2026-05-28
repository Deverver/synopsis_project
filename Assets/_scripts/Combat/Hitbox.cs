using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private Collider hitboxCollider;

    private void Awake()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider>();
        }

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false; // Start disabled
        }
    }

    public void SetHitboxActive(bool active)
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = active;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent hitting oneself (e.g. player hitting player's hurtbox)
        if (other.transform.IsChildOf(transform.root)) return;

        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox != null)
        {
            if (CombatResolver.Instance != null)
            {
                CombatResolver.Instance.Resolve(this, hurtbox);
            }
            else
            {
                Debug.LogWarning("CombatResolver: No singleton instance found to resolve hit!");
            }
        }
    }
}
