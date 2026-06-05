using System;
using UnityEngine;


/*
Type: Plain C# class using UnityEngine

Role defines:
    - Frame timing
    - Hitbox Active/Inactive
    - Hurtstate (Invincible or Vulnerable)
*/

[Serializable]
public class FrameWindow
{
    public FrameWindowType windowType;
    [Tooltip("0-1 normalized time")]
    public float start;
    [Tooltip("0-1 normalized time")]
    public float end;
    public HurtState hurtState;
    public bool hitboxActive;
}
