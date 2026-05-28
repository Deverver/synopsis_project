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
  public Hurtbox hurtbox { get; private set; }
  
  private void Awake()
  {
    hurtbox = GetComponent<Hurtbox>();
  }
}