namespace ProcessorEmulator.Source;

public class ByteRegister
{
    public byte Value { get; set; }

    public string HexValue
    {
        get => Value.ToString("X2");
        set => Value = Convert.ToByte(value, 16);
    }

    public string BinValue
    {
        get => Value.ToString("B8");
        set => Value = Convert.ToByte(value, 2);
    }

    public bool GetBit(int index)
    {
        return (Value & (byte)Math.Pow(2, index)) != 0;
    }

    public void SetBit(int index, bool value)
    {
        if (value)
        {
            Value |= (byte)Math.Pow(2, index);
        }
        else
        {
            Value &= (byte)~(int)Math.Pow(2, index);
        }
    }
}