/*
Type: Plain C#

Role defines:
    - Stores inputs in memory
    - Only holds inputs for 0.2 seconds
*/
[System.Serializable]
public class BufferedInput
{
    public InputType Input { get; private set; }
    public float Timestamp { get; private set; }

    public BufferedInput(InputType input, float timestamp)
    {
        Input = input;
        Timestamp = timestamp;
    }
}
