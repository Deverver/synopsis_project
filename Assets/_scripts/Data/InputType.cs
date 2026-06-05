using System;

[Flags]
public enum InputType
{
    None = 0,
    LightAttack = 1 << 0,
    HeavyAttack = 1 << 1,
    Dodge = 1 << 2,
    Guard = 1 << 3
}
