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

    public bool HasInput(InputType input)
    {
        Update();
        foreach (var item in buffer)
        {
            if (item.Input == input)
            {
                return true;
            }
        }
        return false;
    }

    public void ConsumeInput(InputType input)
    {
        for (int i = 0; i < buffer.Count; i++)
        {
            if (buffer[i].Input == input)
            {
                buffer.RemoveAt(i);
                break;
            }
        }
    }

    public void Clear()
    {
        buffer.Clear();
    }
}
