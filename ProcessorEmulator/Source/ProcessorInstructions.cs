namespace ProcessorEmulator.Source;

public partial class Processor
{
    // CPU control instructions
    private void NoOperation()
    {
        _instructionEnd = true;
    }

    private void Halt()
    {
        _halt = true;
        _instructionEnd = true;
    }

    // Data transfer instructions
    private void Move(ref ByteRegister to, ref ByteRegister from)
    {
        to.Value = from.Value;
        _instructionEnd = true;
    }

    private void IndirectWriteOrRead(ref ByteRegister register, bool write)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_hRegister.Value, _lRegister.Value).Value;
                break;
            case 4:
                if (write)
                {
                    _memoryDataRegister.Value = register.Value;
                    WriteDataToMemory();
                }
                else
                {
                    ReadDataFromMemory();
                    register.Value = _memoryDataRegister.Value;
                }
                break;
        }

        _instructionEnd = true;
    }

    private void MoveImmediate(ref ByteRegister register)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                register.Value = _memoryDataRegister.Value;
                _instructionEnd = true;
                break;
        }
    }

    private void IndirectWriteImmediate()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                ReadDataFromMemory();
                break;
            case 4:
                _programCounter.Value++;
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_hRegister.Value, _lRegister.Value).Value;
                WriteDataToMemory();
                _instructionEnd = true;
                break;
        }
    }

    private void LoadA()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 6:
                _programCounter.Value++;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 7:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                break;
            case 8:
                ReadDataFromMemory();
                _alu.Accumulator.Value = _memoryDataRegister.Value;
                _instructionEnd = true;
                break;
        }
    }

    private void StoreA()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 6:
                _programCounter.Value++;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 7:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                break;
            case 8:
                _memoryDataRegister.Value = _alu.Accumulator.Value;
                WriteDataToMemory();
                _instructionEnd = true;
                break;
        }
    }

    private void LoadExtendedImmediate(ref ByteRegister highReg, ref ByteRegister lowReg)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                lowReg.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 6:
                _programCounter.Value++;
                ReadDataFromMemory();
                highReg.Value = _memoryDataRegister.Value;
                _instructionEnd = true;
                break;
        }
    }
    
    private void LoadAExtended(ref ByteRegister highReg, ref ByteRegister lowReg)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(highReg.Value, lowReg.Value).Value;
                break;
            case 4:
                ReadDataFromMemory();
                _alu.Accumulator.Value = _memoryDataRegister.Value;
                _instructionEnd = true;
                break;
        }
    }
    
    private void StoreAExtended(ref ByteRegister highReg, ref ByteRegister lowReg)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(highReg.Value, lowReg.Value).Value;
                break;
            case 4:
                _memoryDataRegister.Value = _alu.Accumulator.Value;
                WriteDataToMemory();
                _instructionEnd = true;
                break;
        }
    }

    private void LoadHl()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 6:
                _programCounter.Value++;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 7:
                _memoryAddressRegister.Value++;
                break;
            case 8:
                ReadDataFromMemory();
                _lRegister.Value = _memoryDataRegister.Value;
                break;
            case 9:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value++;
                break;
            case 10:
                ReadDataFromMemory();
                _hRegister.Value = _memoryDataRegister.Value;
                _instructionEnd = true;
                break;
        }
    }
    
    private void StoreHl()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 6:
                _programCounter.Value++;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 7:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                break;
            case 8:
                _memoryDataRegister.Value = _lRegister.Value;
                WriteDataToMemory();
                break;
            case 9:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value++;
                break;
            case 10:
                _memoryDataRegister.Value = _hRegister.Value;
                WriteDataToMemory();
                _instructionEnd = true;
                break;
        }
    }

    private void ExchangeDeHl()
    {
        switch (_tState)
        {
            case 3:
                _wRegister.Value = _hRegister.Value;
                _zRegister.Value = _lRegister.Value;
                break;
            case 4:
                _hRegister.Value = _dRegister.Value;
                _lRegister.Value = _eRegister.Value;
                break;
            case 5:
                _dRegister.Value = _wRegister.Value;
                _eRegister.Value = _zRegister.Value;
                break;
        }
    }

    // Flag instructions
    private void SetCarryFlag()
    {
        ArithmeticLogicUnit.Flags carryFlag = ArithmeticLogicUnit.Flags.CarryFlag;
        _alu.FlagsRegister.SetBit((int)carryFlag, true);
        _instructionEnd = true;
    }

    private void ComplementCarryFlag()
    {
        ArithmeticLogicUnit.Flags carryFlag = ArithmeticLogicUnit.Flags.CarryFlag;
        _alu.FlagsRegister.Value ^= (byte)Math.Pow(2, (double)carryFlag);
        _instructionEnd = true;
    }

    // Arithmetic instructions
    private void ArithmeticOperation(ref ByteRegister register, ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _alu.TmpRegister.Value = register.Value;
                break;
            case 4:
                _alu.Accumulator.Value = _alu.ArithmeticOperation(operation);
                _instructionEnd = true;
                break;
        }
    }

    private void IndirectArithmeticOperation(ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_hRegister.Value, _lRegister.Value).Value;
                ReadDataFromMemory();
                _alu.TmpRegister.Value = _memoryDataRegister.Value;
                break;
            case 4:
                _alu.Accumulator.Value = _alu.ArithmeticOperation(operation);
                _instructionEnd = true;
                break;
        }
    }

    private void ArithmeticOperationImmediate(ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                ReadDataFromMemory();
                _alu.TmpRegister.Value = _memoryDataRegister.Value;
                break;
            case 4:
                _programCounter.Value++;
                _alu.Accumulator.Value = _alu.ArithmeticOperation(operation);
                _instructionEnd = true;
                break;
        }
    }

    private void DoubleAdd(ref ByteRegister highReg, ref ByteRegister lowReg)
    {
        switch (_tState)
        {
            case 3:
                _alu.Accumulator.Value = lowReg.Value;
                break;
            case 4:
                _alu.TmpRegister.Value = _lRegister.Value;
                break;
            case 5:
                _lRegister.Value = _alu.DoubleAdd(1);
                break;
            case 6:
                _alu.Accumulator.Value = highReg.Value;
                break;
            case 7:
                _alu.TmpRegister.Value = _hRegister.Value;
                break;
            case 8:
                _hRegister.Value = _alu.DoubleAdd(2);
                _instructionEnd = true;
                break;
        }
    }

    // Logical instructions
    private void LogicOperation(ref ByteRegister register, ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _alu.TmpRegister.Value = register.Value;
                break;
            case 4:
                _alu.Accumulator.Value = _alu.LogicOperation(operation);
                _instructionEnd = true;
                break;
        }
    }

    private void IndirectLogicOperation(ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_hRegister.Value, _lRegister.Value).Value;
                ReadDataFromMemory();
                _alu.TmpRegister.Value = _memoryDataRegister.Value;
                break;
            case 4:
                _alu.Accumulator.Value = _alu.LogicOperation(operation);
                _instructionEnd = true;
                break;
        }
    }

    private void LogicOperationImmediate(ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                ReadDataFromMemory();
                _alu.TmpRegister.Value = _memoryDataRegister.Value;
                break;
            case 4:
                _programCounter.Value++;
                _alu.Accumulator.Value = _alu.LogicOperation(operation);
                _instructionEnd = true;
                break;
        }
    }

    private void ComplementA()
    {
        _alu.Accumulator.Value = _alu.LogicOperation(ArithmeticLogicUnit.Operation.Cmt);
        _instructionEnd = true;
    }
    
    // Rotate and shift instructions
    private void RotateOperation(ArithmeticLogicUnit.Operation operation)
    {
        _alu.RotateOperation(operation);
        _instructionEnd = true;
    }
    
    // Increment and decrement instructions
    private void IncrementDecrement(ref ByteRegister register, ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _alu.TmpRegister.Value = register.Value;
                break;
            case 4:
                _alu.IncrementDecrement(operation);
                register.Value = _alu.TmpRegister.Value;
                _instructionEnd = true;
                break;
        }
    }

    private void IndirectIncrementDecrement(ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = WordRegister.MergeRegisters(_hRegister.Value, _lRegister.Value).Value;
                ReadDataFromMemory();
                _alu.TmpRegister.Value = _memoryDataRegister.Value;
                break;
            case 4:
                _alu.IncrementDecrement(operation);
                _memoryDataRegister.Value = _alu.TmpRegister.Value;
                break;
            case 5:
                WriteDataToMemory();
                _instructionEnd = true;
                break;
        }
    }

    private void IncrementDecrementExtended(ArithmeticLogicUnit.Operation operation, ref ByteRegister highReg,
        ref ByteRegister lowReg)
    {
        switch (_tState)
        {
            case 3:
                _alu.TmpRegister.Value = lowReg.Value;
                break;
            case 4:
                _alu.ExtendedIncrementDecrement(operation, 1);
                lowReg.Value = _alu.TmpRegister.Value;
                break;
            case 5:
                _alu.TmpRegister.Value = highReg.Value;
                break;
            case 6:
                _alu.ExtendedIncrementDecrement(operation, 2);
                highReg.Value = _alu.TmpRegister.Value;
                _instructionEnd = true;
                break;
        }
    }
    
    // Compare instructions
    private void Compare(ref ByteRegister register)
    {
        _alu.TmpRegister.Value = register.Value;
        _alu.ArithmeticOperation(ArithmeticLogicUnit.Operation.Sub);
        _instructionEnd = true;
    }
    
    private void IndirectCompare()
    {
        _memoryAddressRegister.Value = WordRegister.MergeRegisters(_hRegister.Value, _lRegister.Value).Value;
        ReadDataFromMemory();
        _alu.TmpRegister.Value = _memoryDataRegister.Value;
        _alu.ArithmeticOperation(ArithmeticLogicUnit.Operation.Sub);
        _instructionEnd = true;
    }

    private void CompareImmediate()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                ReadDataFromMemory();
                _alu.TmpRegister.Value = _memoryDataRegister.Value;
                _alu.ArithmeticOperation(ArithmeticLogicUnit.Operation.Sub);
                break;
            case 4:
                _programCounter.Value++;
                _instructionEnd = true;
                break;
        }
    }

    // Unconditional jump instructions

    private void Jump()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 6:
                _programCounter.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                _instructionEnd = true;
                break;
        }
    }

    private void JumpToHl()
    {
        _programCounter.Value = WordRegister.MergeRegisters(_hRegister.Value, _lRegister.Value).Value;
        _instructionEnd = true;
    }
    
    // Conditional jump instructions
    private void JumpIf(Condition condition)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 6:
                _programCounter.Value++;
                _instructionEnd = condition switch
                {
                    Condition.NotZero => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.ZeroFlag),
                    Condition.Zero => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.ZeroFlag),
                    Condition.NotCarry => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Carry => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Odd => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Even => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Plus => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Minus => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    _ => _instructionEnd
                };
                break;
            case 7:
                _programCounter.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                _instructionEnd = true;
                break;
        }
    }
    
    // Subroutine instructions
    private void Call()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                _stackPointer.Value--;
                break;
            case 6:
                _programCounter.Value++;
                _memoryAddressRegister.Value = _stackPointer.Value;
                break;
            case 7:
                _stackPointer.Value--;
                _memoryDataRegister.Value = _programCounter.HighByte;
                WriteDataToMemory();
                break;
            case 8:
                _memoryAddressRegister.Value = _stackPointer.Value;
                _memoryDataRegister.Value = _programCounter.LowByte;
                WriteDataToMemory();
                break;
            case 9:
                _programCounter.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                _instructionEnd = true;
                break;
        }
    }
    
    private void CallIf(Condition condition)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 6:
                _programCounter.Value++;
                _instructionEnd = condition switch
                {
                    Condition.NotZero => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.ZeroFlag),
                    Condition.Zero => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.ZeroFlag),
                    Condition.NotCarry => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Carry => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Odd => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Even => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Plus => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Minus => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    _ => _instructionEnd
                };
                break;
            case 7:
                _stackPointer.Value--;
                _memoryAddressRegister.Value = _stackPointer.Value;
                break;
            case 8:
                _stackPointer.Value--;
                _memoryDataRegister.Value = _programCounter.HighByte;
                WriteDataToMemory();
                break;
            case 9:
                _memoryAddressRegister.Value = _stackPointer.Value;
                _memoryDataRegister.Value = _programCounter.LowByte;
                WriteDataToMemory();
                break;
            case 10:
                _programCounter.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                _instructionEnd = true;
                break;
        }
    }

    private void Return()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _stackPointer.Value;
                break;
            case 4:
                _stackPointer.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _stackPointer.Value;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 6:
                _programCounter.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                _stackPointer.Value++;
                _instructionEnd = true;
                break;
        }
    }
    
    private void ReturnIf(Condition condition)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _stackPointer.Value;
                _instructionEnd = condition switch
                {
                    Condition.NotZero => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.ZeroFlag),
                    Condition.Zero => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.ZeroFlag),
                    Condition.NotCarry => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Carry => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Odd => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Even => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Plus => _alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    Condition.Minus => !_alu.FlagsRegister.GetBit((int)ArithmeticLogicUnit.Flags.CarryFlag),
                    _ => _instructionEnd
                };
                break;
            case 4:
                _stackPointer.Value++;
                ReadDataFromMemory();
                _zRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _stackPointer.Value;
                ReadDataFromMemory();
                _wRegister.Value = _memoryDataRegister.Value;
                break;
            case 6:
                _programCounter.Value = WordRegister.MergeRegisters(_wRegister.Value, _zRegister.Value).Value;
                _stackPointer.Value++;
                _instructionEnd = true;
                break;
        }
    }

    // Stack instructions
    private void LoadImmediateStackPointer()
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 4:
                _programCounter.Value++;
                ReadDataFromMemory();
                _stackPointer.LowByte = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryAddressRegister.Value = _programCounter.Value;
                break;
            case 6:
                _programCounter.Value++;
                ReadDataFromMemory();
                _stackPointer.HighByte = _memoryDataRegister.Value;
                _instructionEnd = true;
                break;
        }
    }

    private void DoubleAddStackPointer()
    {
        switch (_tState)
        {
            case 3:
                _alu.Accumulator.Value = _stackPointer.LowByte;
                break;
            case 4:
                _alu.TmpRegister.Value = _lRegister.Value;
                break;
            case 5:
                _lRegister.Value = _alu.DoubleAdd(1);
                break;
            case 6:
                _alu.Accumulator.Value = _stackPointer.HighByte;
                break;
            case 7:
                _alu.TmpRegister.Value = _hRegister.Value;
                break;
            case 8:
                _hRegister.Value = _alu.DoubleAdd(2);
                _instructionEnd = true;
                break;
        }
    }
    
    private void IncrementDecrementStackPointer(ArithmeticLogicUnit.Operation operation)
    {
        switch (_tState)
        {
            case 3:
                _alu.TmpRegister.Value = _stackPointer.LowByte;
                break;
            case 4:
                _alu.ExtendedIncrementDecrement(operation, 1);
                _stackPointer.LowByte = _alu.TmpRegister.Value;
                break;
            case 5:
                _alu.TmpRegister.Value = _stackPointer.HighByte;
                break;
            case 6:
                _alu.ExtendedIncrementDecrement(operation, 2);
                _stackPointer.HighByte = _alu.TmpRegister.Value;
                _instructionEnd = true;
                break;
        }
    }

    private void Push(ref ByteRegister highReg, ref ByteRegister lowReg)
    {
        switch (_tState)
        {
            case 3:
                _stackPointer.Value--;
                _memoryAddressRegister.Value = _stackPointer.Value;
                break;
            case 4:
                _memoryDataRegister.Value = highReg.Value;
                WriteDataToMemory();
                break;
            case 5:
                _stackPointer.Value--;
                _memoryAddressRegister.Value = _stackPointer.Value;
                break;
            case 6:
                _memoryDataRegister.Value = lowReg.Value;
                WriteDataToMemory();
                _instructionEnd = true;
                break;
        } 
    }

    private void Pop(ref ByteRegister highReg, ref ByteRegister lowReg)
    {
        switch (_tState)
        {
            case 3:
                _memoryAddressRegister.Value = _stackPointer.Value;
                ReadDataFromMemory();
                lowReg.Value = _memoryDataRegister.Value;
                break;
            case 4:
                _stackPointer.Value++;
                _memoryAddressRegister.Value = _stackPointer.Value;
                ReadDataFromMemory();
                highReg.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _stackPointer.Value++;
                _instructionEnd = true;
                break;
        }
    }

    private void ExchangeStackWithHl()
    {
        switch (_tState)
        {
            case 3:
                _wRegister.Value = _hRegister.Value;
                _zRegister.Value = _lRegister.Value;
                break;
            case 4:
                _memoryAddressRegister.Value = _stackPointer.Value;
                ReadDataFromMemory();
                _lRegister.Value = _memoryDataRegister.Value;
                break;
            case 5:
                _memoryDataRegister.Value = _zRegister.Value;
                WriteDataToMemory();
                break;
            case 6:
                _memoryAddressRegister.Value++;
                ReadDataFromMemory();
                _hRegister.Value = _memoryDataRegister.Value;
                break;
            case 7:
                _memoryDataRegister.Value = _wRegister.Value;
                WriteDataToMemory();
                _instructionEnd = true;
                break;
        }
    }

    private void HlToSp()
    {
        _stackPointer.Value = WordRegister.MergeRegisters(_hRegister.Value, _lRegister.Value).Value;
        _instructionEnd = true;
    }
}