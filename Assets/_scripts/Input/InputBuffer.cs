using System.Collections.Generic;
using UnityEngine;

public class InputBuffer
{
    private const float BUFFER_DURATION = 0.2f;
    private readonly List<BufferedInput> buffer = new List<BufferedInput>();

    public void AddInput(InputType input)
    {
        buffer.Add(new BufferedInput(input, Time.time));
    }

    /*
    Type: Method (runs every frame)
    
    Role:
        - Clears inputs that are older than buffer duration
        - Removes them from the buffer
    */
    public void Update()
    {
        float curTime = Time.time;
        buffer.RemoveAll(item => curTime - item.Timestamp > BUFFER_DURATION);
    }

    public bool HasInput(InputType requiredInput)
    {
        Update();
        InputType accumulatedInput = InputType.None;
        foreach (var item in buffer)
        {
            accumulatedInput |= item.Input;
        }
        
        return (accumulatedInput & requiredInput) == requiredInput;
    }

    public void ConsumeInput(InputType input)
    {
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            if ((buffer[i].Input & input) != InputType.None)
            {
                buffer.RemoveAt(i);
            }
        }
    }

    public void Clear()
    {
        buffer.Clear();
    }
}
