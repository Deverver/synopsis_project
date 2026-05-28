using System;
using UnityEngine;

/*
Type: Plain C# class using UnityEngine

Role defines:
    - required input 
    - timing window 
    - next move 
*/
[Serializable]
public class Transition
{
    [Tooltip("None if automatic")]
    public InputType requiredInput;
    [Tooltip("0 to 1 normalized time")]
    public float windowStart;
    [Tooltip("0 to 1 normalized time")]
    public float windowEnd;
    public MoveData nextMove;
}