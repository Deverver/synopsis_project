using UnityEngine;

/*
Type: MonoBehaviour

Role:
    Holds reference to hitbox
    Connects animations to system
*/

public class Weapon : MonoBehaviour
{
    public Hitbox hitbox { get; private set; }
    public Animator animator { get; private set; }

    private void Awake()
    {
        hitbox = GetComponentInChildren<Hitbox>();
        animator = GetComponent<Animator>();
    }
}
