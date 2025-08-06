namespace ProcessorEmulator.Source;

using System.Diagnostics;

public partial class Processor
{
    private bool _halt;
    private bool _instructionEnd;
    private int _tState = 1;
    private int _ticks = 0;

    // Memory
    private const int MemorySize = 65536;
    private byte[] _memory = new byte[MemorySize];
    private ByteRegister _memoryDataRegister = new();
    private WordRegister _memoryAddressRegister = new();

    private ArithmeticLogicUnit _alu = new();

    // Register pair BC
    private ByteRegister _bRegister = new();
    private ByteRegister _cRegister = new();

    // Register pair DE
    private ByteRegister _dRegister = new();
    private ByteRegister _eRegister = new();

    // Register pair HL
    private ByteRegister _hRegister = new();
    private ByteRegister _lRegister = new();

    // Register pair WZ
    private ByteRegister _wRegister = new();
    private ByteRegister _zRegister = new();

    private WordRegister _programCounter = new();
    private WordRegister _stackPointer = new();

    private ByteRegister _instructionRegister = new();

    private void ReadDataFromMemory()
    {
        _memoryDataRegister.Value = _memory[_memoryAddressRegister.Value];
    }

    private void WriteDataToMemory()
    {
        _memory[_memoryAddressRegister.Value] = _memoryDataRegister.Value;
    }

    public void LoadFile(string filePath)
    {
        Console.WriteLine($"Loading file...");
        try
        {
            BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open));
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                _memory[reader.BaseStream.Position] = reader.ReadByte();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }

        Console.WriteLine("File loaded successfully.");
    }

    public void Run(string filePath, int frequency, bool debug, bool everyTick)
    {
        LoadFile(filePath);
        
        Console.WriteLine("Running...");
        var programStopwatch = new Stopwatch();
        programStopwatch.Start();
        
        while (!_halt)
        {
            Thread.Sleep(1000 / frequency);
            var operationStopwatch = new Stopwatch();
            operationStopwatch.Start();
            
            switch (_tState)
            {
                case 1:
                    _memoryAddressRegister.Value = _programCounter.Value;
                    break;
                case 2:
                    _programCounter.Value++;
                    ReadDataFromMemory();
                    _instructionRegister.Value = _memoryDataRegister.Value;
                    break;
                default:
                    Decoder();
                    break;
            }
            operationStopwatch.Stop();
            
            if (debug)
            {
                Debug(operationStopwatch, everyTick);
            }

            if (_tState == 32 || _instructionEnd) // 5 bits
            {
                _tState = 1;
                _instructionEnd = false;
            }
            else
            {
                _tState++;
            }
        }
        
        programStopwatch.Stop();
        Console.WriteLine($"Processing completed successfully (in {programStopwatch.ElapsedMilliseconds} ms.)");
    }

    private void Debug(Stopwatch stopwatch, bool everyTick)
    {
        _ticks++;
        if (_instructionEnd || everyTick)
        {
            Console.WriteLine($"t: {_tState.ToString("D2")} " +
                              $"a: {_alu.Accumulator.HexValue} " +
                              $"tmp: {_alu.TmpRegister.HexValue} " +
                              $"f: {_alu.FlagsRegister.BinValue} " +
                              $"bc: {_bRegister.HexValue}{_cRegister.HexValue} " +
                              $"de: {_dRegister.HexValue}{_eRegister.HexValue} " +
                              $"hl: {_hRegister.HexValue}{_lRegister.HexValue} " +
                              $"pc: {_programCounter.HexValue} " +
                              $"sp: {_stackPointer.HexValue} " +
                              $"mdr: {_memoryDataRegister.HexValue} " +
                              $"mar: {_memoryAddressRegister.HexValue} " +
                              $"ir: {_instructionRegister.HexValue} " +
                              $"tick: {_ticks}\t| " +
                              $"{stopwatch.ElapsedTicks * 1000000000 / Stopwatch.Frequency} ns");
        }
    }

    private void Decoder()
    {
        switch (_instructionRegister.HexValue)
            {
                // NOP
                case "00":
                    NoOperation();
                    break;
                // HLT
                case "76":
                    Halt();
                    break;
                // MOV B,reg
                case "40":
                    Move(ref _bRegister, ref _bRegister);
                    break;
                case "41":
                    Move(ref _bRegister, ref _cRegister);
                    break;
                case "42":
                    Move(ref _bRegister, ref _dRegister);
                    break;
                case "43":
                    Move(ref _bRegister, ref _eRegister);
                    break;
                case "44":
                    Move(ref _bRegister, ref _hRegister);
                    break;
                case "45":
                    Move(ref _bRegister, ref _lRegister);
                    break;
                case "46":
                    IndirectWriteOrRead(ref _bRegister, false);
                    break;
                case "47":
                    Move(ref _bRegister, ref _alu.Accumulator);
                    break;
                // MOV C,reg
                case "48":
                    Move(ref _cRegister, ref _bRegister);
                    break;
                case "49":
                    Move(ref _cRegister, ref _cRegister);
                    break;
                case "4A":
                    Move(ref _cRegister, ref _dRegister);
                    break;
                case "4B":
                    Move(ref _cRegister, ref _eRegister);
                    break;
                case "4C":
                    Move(ref _cRegister, ref _hRegister);
                    break;
                case "4D":
                    Move(ref _cRegister, ref _lRegister);
                    break;
                case "4E":
                    IndirectWriteOrRead(ref _cRegister, false);
                    break;
                case "4F":
                    Move(ref _cRegister, ref _alu.Accumulator);
                    break;
                // MOV D,reg
                case "50":
                    Move(ref _dRegister, ref _bRegister);
                    break;
                case "51":
                    Move(ref _dRegister, ref _cRegister);
                    break;
                case "52":
                    Move(ref _dRegister, ref _dRegister);
                    break;
                case "53":
                    Move(ref _dRegister, ref _eRegister);
                    break;
                case "54":
                    Move(ref _dRegister, ref _hRegister);
                    break;
                case "55":
                    Move(ref _dRegister, ref _lRegister);
                    break;
                case "56":
                    IndirectWriteOrRead(ref _dRegister, false);
                    break;
                case "57":
                    Move(ref _dRegister, ref _alu.Accumulator);
                    break;
                // MOV E,reg
                case "58":
                    Move(ref _eRegister, ref _bRegister);
                    break;
                case "59":
                    Move(ref _eRegister, ref _cRegister);
                    break;
                case "5A":
                    Move(ref _eRegister, ref _dRegister);
                    break;
                case "5B":
                    Move(ref _eRegister, ref _eRegister);
                    break;
                case "5C":
                    Move(ref _eRegister, ref _hRegister);
                    break;
                case "5D":
                    Move(ref _eRegister, ref _lRegister);
                    break;
                case "5E":
                    IndirectWriteOrRead(ref _eRegister, false);
                    break;
                case "5F":
                    Move(ref _eRegister, ref _alu.Accumulator);
                    break;
                // MOV H,reg
                case "60":
                    Move(ref _hRegister, ref _bRegister);
                    break;
                case "61":
                    Move(ref _hRegister, ref _cRegister);
                    break;
                case "62":
                    Move(ref _hRegister, ref _dRegister);
                    break;
                case "63":
                    Move(ref _hRegister, ref _eRegister);
                    break;
                case "64":
                    Move(ref _hRegister, ref _hRegister);
                    break;
                case "65":
                    Move(ref _hRegister, ref _lRegister);
                    break;
                case "66":
                    IndirectWriteOrRead(ref _hRegister, false);
                    break;
                case "67":
                    Move(ref _hRegister, ref _alu.Accumulator);
                    break;
                // MOV L,reg
                case "68":
                    Move(ref _lRegister, ref _bRegister);
                    break;
                case "69":
                    Move(ref _lRegister, ref _cRegister);
                    break;
                case "6A":
                    Move(ref _lRegister, ref _dRegister);
                    break;
                case "6B":
                    Move(ref _lRegister, ref _eRegister);
                    break;
                case "6C":
                    Move(ref _lRegister, ref _hRegister);
                    break;
                case "6D":
                    Move(ref _lRegister, ref _lRegister);
                    break;
                case "6E":
                    IndirectWriteOrRead(ref _lRegister, false);
                    break;
                case "6F":
                    Move(ref _lRegister, ref _alu.Accumulator);
                    break;
                // MOV M,reg
                case "70":
                    IndirectWriteOrRead(ref _bRegister, true);
                    break;
                case "71":
                    IndirectWriteOrRead(ref _cRegister, true);
                    break;
                case "72":
                    IndirectWriteOrRead(ref _dRegister, true);
                    break;
                case "73":
                    IndirectWriteOrRead(ref _eRegister, true);
                    break;
                case "74":
                    IndirectWriteOrRead(ref _hRegister, true);
                    break;
                case "75":
                    IndirectWriteOrRead(ref _lRegister, true);
                    break;
                case "77":
                    IndirectWriteOrRead(ref _alu.Accumulator, true);
                    break;
                // MOV A,reg
                case "78":
                    Move(ref _alu.Accumulator, ref _bRegister);
                    break;
                case "79":
                    Move(ref _alu.Accumulator, ref _cRegister);
                    break;
                case "7A":
                    Move(ref _alu.Accumulator, ref _dRegister);
                    break;
                case "7B":
                    Move(ref _alu.Accumulator, ref _eRegister);
                    break;
                case "7C":
                    Move(ref _alu.Accumulator, ref _hRegister);
                    break;
                case "7D":
                    Move(ref _alu.Accumulator, ref _lRegister);
                    break;
                case "7E":
                    IndirectWriteOrRead(ref _alu.Accumulator, false);
                    break;
                case "7F":
                    Move(ref _alu.Accumulator, ref _alu.Accumulator);
                    break;
                // MVI reg,byte
                case "06":
                    MoveImmediate(ref _bRegister);
                    break;
                case "16":
                    MoveImmediate(ref _dRegister);
                    break;
                case "26":
                    MoveImmediate(ref _hRegister);
                    break;
                case "36":
                    IndirectWriteImmediate();
                    break;
                case "0E":
                    MoveImmediate(ref _cRegister);
                    break;
                case "1E":
                    MoveImmediate(ref _eRegister);
                    break;
                case "2E":
                    MoveImmediate(ref _lRegister);
                    break;
                case "3E":
                    MoveImmediate(ref _alu.Accumulator);
                    break;
                // LDA addr
                case "3A":
                    LoadA();
                    break;
                // STA addr
                case "32":
                    StoreA();
                    break;
                // LXI reg,dble
                case "01":
                    LoadExtendedImmediate(ref _bRegister, ref _cRegister);
                    break;
                case "11":
                    LoadExtendedImmediate(ref _dRegister, ref _eRegister);
                    break;
                case "21":
                    LoadExtendedImmediate(ref _hRegister, ref _lRegister);
                    break;
                // LDAX xreg
                case "0A":
                    LoadAExtended(ref _bRegister, ref _cRegister);
                    break;
                case "1A":
                    LoadAExtended(ref _dRegister, ref _eRegister);
                    break;
                // STAX xreg
                case "02":
                    StoreAExtended(ref _bRegister, ref _cRegister);
                    break;
                case "12":
                    StoreAExtended(ref _dRegister, ref _eRegister);
                    break;
                // LHLD addr
                case "2A":
                    LoadHl();
                    break;
                // SHLD addr
                case "22":
                    StoreHl();
                    break;
                // XCHG
                case "EB":
                    ExchangeDeHl();
                    break;
                // STC
                case "37":
                    SetCarryFlag();
                    break;
                // CMC
                case "3F":
                    ComplementCarryFlag();
                    break;
                // ADD reg
                case "80":
                    ArithmeticOperation(ref _bRegister, ArithmeticLogicUnit.Operation.Add);
                    break;
                case "81":
                    ArithmeticOperation(ref _cRegister, ArithmeticLogicUnit.Operation.Add);
                    break;
                case "82":
                    ArithmeticOperation(ref _dRegister, ArithmeticLogicUnit.Operation.Add);
                    break;
                case "83":
                    ArithmeticOperation(ref _eRegister, ArithmeticLogicUnit.Operation.Add);
                    break;
                case "84":
                    ArithmeticOperation(ref _hRegister, ArithmeticLogicUnit.Operation.Add);
                    break;
                case "85":
                    ArithmeticOperation(ref _lRegister, ArithmeticLogicUnit.Operation.Add);
                    break;
                case "86":
                    IndirectArithmeticOperation(ArithmeticLogicUnit.Operation.Add);
                    break;
                case "87":
                    ArithmeticOperation(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.Add);
                    break;
                // ADC reg
                case "88":
                    ArithmeticOperation(ref _bRegister, ArithmeticLogicUnit.Operation.Adc);
                    break;
                case "89":
                    ArithmeticOperation(ref _cRegister, ArithmeticLogicUnit.Operation.Adc);
                    break;
                case "8A":
                    ArithmeticOperation(ref _dRegister, ArithmeticLogicUnit.Operation.Adc);
                    break;
                case "8B":
                    ArithmeticOperation(ref _eRegister, ArithmeticLogicUnit.Operation.Adc);
                    break;
                case "8C":
                    ArithmeticOperation(ref _hRegister, ArithmeticLogicUnit.Operation.Adc);
                    break;
                case "8D":
                    ArithmeticOperation(ref _lRegister, ArithmeticLogicUnit.Operation.Adc);
                    break;
                case "8E":
                    IndirectArithmeticOperation(ArithmeticLogicUnit.Operation.Adc);
                    break;
                case "8F":
                    ArithmeticOperation(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.Adc);
                    break;
                // SUB reg
                case "90":
                    ArithmeticOperation(ref _bRegister, ArithmeticLogicUnit.Operation.Sub);
                    break;
                case "91":
                    ArithmeticOperation(ref _cRegister, ArithmeticLogicUnit.Operation.Sub);
                    break;
                case "92":
                    ArithmeticOperation(ref _dRegister, ArithmeticLogicUnit.Operation.Sub);
                    break;
                case "93":
                    ArithmeticOperation(ref _eRegister, ArithmeticLogicUnit.Operation.Sub);
                    break;
                case "94":
                    ArithmeticOperation(ref _hRegister, ArithmeticLogicUnit.Operation.Sub);
                    break;
                case "95":
                    ArithmeticOperation(ref _lRegister, ArithmeticLogicUnit.Operation.Sub);
                    break;
                case "96":
                    IndirectArithmeticOperation(ArithmeticLogicUnit.Operation.Sub);
                    break;
                case "97":
                    ArithmeticOperation(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.Sub);
                    break;
                // SBB reg
                case "98":
                    ArithmeticOperation(ref _bRegister, ArithmeticLogicUnit.Operation.Sbb);
                    break;
                case "99":
                    ArithmeticOperation(ref _cRegister, ArithmeticLogicUnit.Operation.Sbb);
                    break;
                case "9A":
                    ArithmeticOperation(ref _dRegister, ArithmeticLogicUnit.Operation.Sbb);
                    break;
                case "9B":
                    ArithmeticOperation(ref _eRegister, ArithmeticLogicUnit.Operation.Sbb);
                    break;
                case "9C":
                    ArithmeticOperation(ref _hRegister, ArithmeticLogicUnit.Operation.Sbb);
                    break;
                case "9D":
                    ArithmeticOperation(ref _lRegister, ArithmeticLogicUnit.Operation.Sbb);
                    break;
                case "9E":
                    IndirectArithmeticOperation(ArithmeticLogicUnit.Operation.Sbb);
                    break;
                case "9F":
                    ArithmeticOperation(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.Sbb);
                    break;
                // ADI byte
                case "C6":
                    ArithmeticOperationImmediate(ArithmeticLogicUnit.Operation.Add);
                    break;
                // SUI byte
                case "D6":
                    ArithmeticOperationImmediate(ArithmeticLogicUnit.Operation.Sub);
                    break;
                // ACI byte
                case "CE":
                    ArithmeticOperationImmediate(ArithmeticLogicUnit.Operation.Adc);
                    break;
                // SBI byte
                case "DE":
                    ArithmeticOperationImmediate(ArithmeticLogicUnit.Operation.Sbb);
                    break;
                // DAD
                case "09":
                    DoubleAdd(ref _bRegister, ref _cRegister);
                    break;
                case "19":
                    DoubleAdd(ref _dRegister, ref _eRegister);
                    break;
                case "29":
                    DoubleAdd(ref _hRegister, ref _lRegister);
                    break;
                // ANA reg
                case "A0":
                    LogicOperation(ref _bRegister, ArithmeticLogicUnit.Operation.And);
                    break;
                case "A1":
                    LogicOperation(ref _cRegister, ArithmeticLogicUnit.Operation.And);
                    break;
                case "A2":
                    LogicOperation(ref _dRegister, ArithmeticLogicUnit.Operation.And);
                    break;
                case "A3":
                    LogicOperation(ref _eRegister, ArithmeticLogicUnit.Operation.And);
                    break;
                case "A4":
                    LogicOperation(ref _hRegister, ArithmeticLogicUnit.Operation.And);
                    break;
                case "A5":
                    LogicOperation(ref _lRegister, ArithmeticLogicUnit.Operation.And);
                    break;
                case "A6":
                    IndirectLogicOperation(ArithmeticLogicUnit.Operation.And);
                    break;
                case "A7":
                    LogicOperation(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.And);
                    break;
                // XRA reg
                case "A8":
                    LogicOperation(ref _bRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "A9":
                    LogicOperation(ref _cRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "AA":
                    LogicOperation(ref _dRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "AB":
                    LogicOperation(ref _eRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "AC":
                    LogicOperation(ref _hRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "AD":
                    LogicOperation(ref _lRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "AE":
                    IndirectLogicOperation(ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "AF":
                    LogicOperation(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.Xor);
                    break;
                // ORA reg
                case "B0":
                    LogicOperation(ref _bRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "B1":
                    LogicOperation(ref _cRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "B2":
                    LogicOperation(ref _dRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "B3":
                    LogicOperation(ref _eRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "B4":
                    LogicOperation(ref _hRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "B5":
                    LogicOperation(ref _lRegister, ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "B6":
                    IndirectLogicOperation(ArithmeticLogicUnit.Operation.Xor);
                    break;
                case "B7":
                    LogicOperation(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.Xor);
                    break;
                // ANI byte
                case "E6":
                    LogicOperationImmediate(ArithmeticLogicUnit.Operation.And);
                    break;
                // ORI byte
                case "F6":
                    LogicOperationImmediate(ArithmeticLogicUnit.Operation.Or);
                    break;
                // XRI byte
                case "EE":
                    LogicOperationImmediate(ArithmeticLogicUnit.Operation.And);
                    break;
                // CMA
                case "2F":
                    ComplementA();
                    break;
                // RAL
                case "17":
                    RotateOperation(ArithmeticLogicUnit.Operation.Ral);
                    break;
                // RAR
                case "1F":
                    RotateOperation(ArithmeticLogicUnit.Operation.Rar);
                    break;
                // RLC
                case "07":
                    RotateOperation(ArithmeticLogicUnit.Operation.Rlc);
                    break;
                // RRC
                case "0F":
                    RotateOperation(ArithmeticLogicUnit.Operation.Rrc);
                    break;
                // INR reg
                case "04":
                    IncrementDecrement(ref _bRegister, ArithmeticLogicUnit.Operation.Inr);
                    break;
                case "14":
                    IncrementDecrement(ref _dRegister, ArithmeticLogicUnit.Operation.Inr);
                    break;
                case "24":
                    IncrementDecrement(ref _hRegister, ArithmeticLogicUnit.Operation.Inr);
                    break;
                case "34":
                    IndirectIncrementDecrement(ArithmeticLogicUnit.Operation.Inr);
                    break;
                case "0C":
                    IncrementDecrement(ref _cRegister, ArithmeticLogicUnit.Operation.Inr);
                    break;
                case "1C":
                    IncrementDecrement(ref _eRegister, ArithmeticLogicUnit.Operation.Inr);
                    break;
                case "2C":
                    IncrementDecrement(ref _lRegister, ArithmeticLogicUnit.Operation.Inr);
                    break;
                case "3C":
                    IncrementDecrement(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.Inr);
                    break;
                // DCR reg
                case "05":
                    IncrementDecrement(ref _bRegister, ArithmeticLogicUnit.Operation.Dcr);
                    break;
                case "15":
                    IncrementDecrement(ref _dRegister, ArithmeticLogicUnit.Operation.Dcr);
                    break;
                case "25":
                    IncrementDecrement(ref _hRegister, ArithmeticLogicUnit.Operation.Dcr);
                    break;
                case "35":
                    IndirectIncrementDecrement(ArithmeticLogicUnit.Operation.Dcr);
                    break;
                case "0D":
                    IncrementDecrement(ref _cRegister, ArithmeticLogicUnit.Operation.Dcr);
                    break;
                case "1D":
                    IncrementDecrement(ref _eRegister, ArithmeticLogicUnit.Operation.Dcr);
                    break;
                case "2D":
                    IncrementDecrement(ref _lRegister, ArithmeticLogicUnit.Operation.Dcr);
                    break;
                case "3D":
                    IncrementDecrement(ref _alu.Accumulator, ArithmeticLogicUnit.Operation.Dcr);
                    break;
                // INX xreg
                case "03":
                    IncrementDecrementExtended(ArithmeticLogicUnit.Operation.Inx, ref _bRegister,
                        ref _cRegister);
                    break;
                case "13":
                    IncrementDecrementExtended(ArithmeticLogicUnit.Operation.Inx, ref _dRegister,
                        ref _eRegister);
                    break;
                case "23":
                    IncrementDecrementExtended(ArithmeticLogicUnit.Operation.Inx, ref _hRegister,
                        ref _lRegister);
                    break;
                // DCX xreg
                case "0B":
                    IncrementDecrementExtended(ArithmeticLogicUnit.Operation.Dcx, ref _bRegister,
                        ref _cRegister);
                    break;
                case "1B":
                    IncrementDecrementExtended(ArithmeticLogicUnit.Operation.Dcx, ref _dRegister,
                        ref _eRegister);
                    break;
                case "2B":
                    IncrementDecrementExtended(ArithmeticLogicUnit.Operation.Dcx, ref _hRegister,
                        ref _lRegister);
                    break;
                // JMP addr
                case "C3":
                    Jump();
                    break;
                // PCHL
                case "E9":
                    JumpToHl();
                    break;
                // CMP reg
                case "B8":
                    Compare(ref _bRegister);
                    break;
                case "B9":
                    Compare(ref _cRegister);
                    break;
                case "BA":
                    Compare(ref _dRegister);
                    break;
                case "BB":
                    Compare(ref _eRegister);
                    break;
                case "BC":
                    Compare(ref _hRegister);
                    break;
                case "BD":
                    Compare(ref _lRegister);
                    break;
                case "BE":
                    IndirectCompare();
                    break;
                case "BF":
                    Compare(ref _alu.Accumulator);
                    break;
                // CPI byte
                case "FE":
                    CompareImmediate();
                    break;
                // JNZ addr
                case "C2":
                    JumpIf(Condition.NotZero);
                    break;
                // JZ addr
                case "CA":
                    JumpIf(Condition.Zero);
                    break;
                // JNC addr
                case "D2":
                    JumpIf(Condition.NotCarry);
                    break;
                // JC addr
                case "DA":
                    JumpIf(Condition.Carry);
                    break;
                // JPO addr
                case "E2":
                    JumpIf(Condition.Odd);
                    break;
                // JPE addr
                case "EA":
                    JumpIf(Condition.Even);
                    break;
                // JP addr
                case "F2":
                    JumpIf(Condition.Plus);
                    break;
                // JM addr
                case "FA":
                    JumpIf(Condition.Minus);
                    break;
                // CALL addr
                case "CD":
                    Call();
                    break;
                // CNZ addr
                case "C4":
                    CallIf(Condition.NotZero);
                    break;
                // CZ addr
                case "CC":
                    CallIf(Condition.Zero);
                    break;
                // CNC addr
                case "D4":
                    CallIf(Condition.NotCarry);
                    break;
                // CC addr
                case "DC":
                    CallIf(Condition.Carry);
                    break;
                // CPO addr
                case "E4":
                    CallIf(Condition.Odd);
                    break;
                // CPE addr
                case "EC":
                    CallIf(Condition.Even);
                    break;
                // CP addr
                case "F4":
                    CallIf(Condition.Plus);
                    break;
                // CM addr
                case "FC":
                    CallIf(Condition.Minus);
                    break;
                // RET
                case "C9":
                    Return();
                    break;
                // RNZ addr
                case "C0":
                    ReturnIf(Condition.NotZero);
                    break;
                // RZ addr
                case "C8":
                    ReturnIf(Condition.Zero);
                    break;
                // RNC addr
                case "D0":
                    ReturnIf(Condition.NotCarry);
                    break;
                // RC addr
                case "D8":
                    ReturnIf(Condition.Carry);
                    break;
                // RPO addr
                case "E0":
                    ReturnIf(Condition.Odd);
                    break;
                // RPE addr
                case "E8":
                    ReturnIf(Condition.Even);
                    break;
                // RP addr
                case "F0":
                    ReturnIf(Condition.Plus);
                    break;
                // RM addr
                case "F8":
                    ReturnIf(Condition.Minus);
                    break;
                // LXI SP,addr
                case "31":
                    LoadImmediateStackPointer();
                    break;
                // DAD SP
                case "39":
                    DoubleAddStackPointer();
                    break;
                // INX SP
                case "33":
                    IncrementDecrementStackPointer(ArithmeticLogicUnit.Operation.Inx);
                    break;
                // DCX SP
                case "3B":
                    IncrementDecrementStackPointer(ArithmeticLogicUnit.Operation.Dcx);
                    break;
                // PUSH xreg
                case "C5":
                    Push(ref _bRegister, ref _cRegister);
                    break;
                case "D5":
                    Push(ref _dRegister, ref _eRegister);
                    break;
                case "E5":
                    Push(ref _hRegister, ref _lRegister);
                    break;
                case "F5":
                    Push(ref _alu.Accumulator, ref _alu.FlagsRegister);
                    break;
                // POP xreg
                case "C1":
                    Pop(ref _bRegister, ref _cRegister);
                    break;
                case "D1":
                    Pop(ref _dRegister, ref _eRegister);
                    break;
                case "E1":
                    Pop(ref _hRegister, ref _lRegister);
                    break;
                case "F1":
                    Pop(ref _alu.Accumulator, ref _alu.FlagsRegister);
                    break;
                default:
                    throw new Exception("Unknown instruction");
            }
    }
    
    private enum Condition
    {
        NotZero,
        Zero,
        NotCarry,
        Carry,
        Odd,
        Even,
        Plus,
        Minus
    }
}