namespace ProcessorEmulator.Source;

public class ArithmeticLogicUnit
{
    public ByteRegister Accumulator = new();
    public ByteRegister TmpRegister = new();
    public ByteRegister FlagsRegister = new();
    private bool _incrementDecrementCarry = false;

    private void CheckFlags(byte value)
    {
        bool newParityFlag = value.ToString("b").Count(c => c.ToString() == "1") % 2 == 0;
        bool newZeroFlag = value == 0;
        bool newSignFlag = value >> 7 == 1;
        
        FlagsRegister.SetBit((int)Flags.ParityFlag, newParityFlag);
        FlagsRegister.SetBit((int)Flags.ZeroFlag, newZeroFlag);
        FlagsRegister.SetBit((int)Flags.SignFlag, newSignFlag);
    }

    private byte Adder(int sub)
    {
        string resultBin = "";
        int cIn = sub;
        
        for (var i = 7; i >= 0; i--)
        {
            int bitA = int.Parse(Accumulator.BinValue[i].ToString());
            int bitB = int.Parse(TmpRegister.BinValue[i].ToString());
    
            int bitQ = bitB ^ sub ^ bitA ^ cIn;
            resultBin = bitQ.ToString() + resultBin;
            int cOut = ((bitB ^ sub ^ bitA) & cIn) | ((bitB ^ sub) & bitA);
            cIn = cOut;
        }
        
        bool newCarryFlag = (cIn ^ sub) == 1;
        FlagsRegister.SetBit((int)Flags.CarryFlag, newCarryFlag);
        byte result = Convert.ToByte(resultBin, 2);

        return result;
    }

    public byte ArithmeticOperation(Operation operation)
    {
        int sub = operation is Operation.Sbb or Operation.Sub ? 1 : 0;
        
        if (operation is Operation.Adc or Operation.Sbb)
        {
            bool oldCarryFlag = FlagsRegister.GetBit((int)Flags.CarryFlag);
            TmpRegister.Value += (byte)(oldCarryFlag ? 1 : 0);
        }

        byte result = Adder(sub);
        
        CheckFlags(result);

        return result;
    }

    public byte DoubleAdd(int step)
    {
        if (step == 2)
        {
            bool oldCarryFlag = FlagsRegister.GetBit((int)Flags.CarryFlag);
            TmpRegister.Value += (byte)(oldCarryFlag ? 1 : 0);
        }

        byte result = Adder(0);

        return result;
    }

    public byte LogicOperation(Operation operation)
    {
        byte result = operation switch
        {
            Operation.And => (byte)(Accumulator.Value & TmpRegister.Value),
            Operation.Or => (byte)(Accumulator.Value | TmpRegister.Value),
            Operation.Xor => (byte)(Accumulator.Value ^ TmpRegister.Value),
            Operation.Cmt => (byte)(~Accumulator.Value),
            _ => 0
        };
        CheckFlags(result);
        return result;
    }

    public void RotateOperation(Operation operation)
    {
        bool newCarryFlag, oldCarryFlag;
        switch (operation)
        {
            case Operation.Ral:
                newCarryFlag = Accumulator.GetBit(7);
                Accumulator.Value <<= 1;
                oldCarryFlag = FlagsRegister.GetBit((int)Flags.CarryFlag);
                Accumulator.SetBit(0, oldCarryFlag);
                break;
            case Operation.Rar:
                newCarryFlag = Accumulator.GetBit(0);
                Accumulator.Value >>= 1;
                oldCarryFlag = FlagsRegister.GetBit((int)Flags.CarryFlag);
                Accumulator.SetBit(7, oldCarryFlag);
                FlagsRegister.SetBit((int)Flags.CarryFlag, newCarryFlag);
                break;
            case Operation.Rlc:
                newCarryFlag = Accumulator.GetBit(7);
                Accumulator.Value <<= 1;
                Accumulator.SetBit(0, newCarryFlag);
                FlagsRegister.SetBit((int)Flags.CarryFlag, newCarryFlag);
                break;
            case Operation.Rrc:
                newCarryFlag = Accumulator.GetBit(0);
                Accumulator.Value >>= 1;
                Accumulator.SetBit(7, newCarryFlag);
                FlagsRegister.SetBit((int)Flags.CarryFlag, newCarryFlag);
                break;
        }
    }

    public void IncrementDecrement(Operation operation)
    {
        switch (operation)
        {
            case Operation.Inr:
                TmpRegister.Value++;
                CheckFlags(TmpRegister.Value);
                break;
            
            case Operation.Dcr:
                TmpRegister.Value--;
                CheckFlags(TmpRegister.Value);
                break;
            
        }
    }

    public void ExtendedIncrementDecrement(Operation operation, int step)
    {
        switch (operation)
        {
            case Operation.Inx:
                if (step == 1)
                {
                    TmpRegister.Value++;
                    _incrementDecrementCarry = TmpRegister.Value == 0;
                }
                if (step == 2)
                {
                    if (_incrementDecrementCarry)
                    {
                        TmpRegister.Value++;
                    }
                    _incrementDecrementCarry = false;
                }
                break;
            case Operation.Dcx:
                if (step == 1)
                {
                    _incrementDecrementCarry = TmpRegister.Value == 0;
                    TmpRegister.Value--;
                }
                if (step == 2)
                {
                    if (_incrementDecrementCarry)
                    {
                        TmpRegister.Value--;
                    }

                    _incrementDecrementCarry = false;
                }
                break;
        }
    }

    public enum Flags
    {
        CarryFlag = 0,
        ParityFlag = 2,
        ZeroFlag = 6,
        SignFlag = 7
    }

    public enum Operation
    {
        Add,
        Adc,
        Sub,
        Sbb,
        And,
        Or,
        Xor,
        Cmt,
        Ral,
        Rar,
        Rlc,
        Rrc,
        Inr,
        Dcr,
        Inx,
        Dcx,
    }
}