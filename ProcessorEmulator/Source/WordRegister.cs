namespace ProcessorEmulator.Source;

public class WordRegister
{
    public ushort Value { get; set; }

    public string HexValue
    {
        get => Value.ToString("X4");
        set => Value = Convert.ToUInt16(value, 16);
    }

    public string BinValue
    {
        get => Value.ToString("B16");
        set => Value = Convert.ToUInt16(value, 2);
    }

    public byte LowByte
    {
        get => (byte)Value;
        set => Value |= value;
    }

    public byte HighByte
    {
        get => (byte)(Value >> 8);
        set => Value |= (ushort)(value << 8);
    }

    public static WordRegister MergeRegisters(byte highByte, byte lowByte)
    {
        return new WordRegister() {LowByte = lowByte, HighByte = highByte};
    }
}