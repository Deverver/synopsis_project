using UnityEngine;

/*
Type: MonoBehaviour
Lives on: Player + Enemy

Role:
    Stores current HurtState
    Receives hits
*/

public class Hurtbox : MonoBehaviour
{
  public HurtState CurrentHurtState { get; private set; } 
  
  private void Start()
  {
    CurrentHurtState = HurtState.Vulnerable;
  }

  public void SetHurtState(HurtState state)
  {
    CurrentHurtState = state;
  }

  public void ReceiveHit(Hitbox hitbox)
  {
    Debug.Log($"[Hurtbox] {gameObject.name} was HIT by hitbox on {hitbox.gameObject.name}!");
  }
}
