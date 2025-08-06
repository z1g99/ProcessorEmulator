using System.Text.RegularExpressions;

namespace ProcessorEmulator.Compiler.Source;

public class Compiler
{
    private StreamReader _sr;
    private BinaryWriter _bw;
    private string? _line;
    private int _lineNum;
    private ushort _addrPtr;
    private Dictionary<string, ushort> _labelsDict = new();
    private Dictionary<string, List<ushort>> _labelsUsageDict = new();
    
    public void Compile(string inputFilePath, string outputFilePath)
    {
        Console.WriteLine("Compiling...");
        
        try
        {
            _sr = new StreamReader(inputFilePath);
            _bw = new BinaryWriter(File.Open(outputFilePath, FileMode.Create));
            _line = _sr.ReadLine();

            for (_lineNum = 1; _line != null; _lineNum++)
            {
                ClearString();

                if (_line == "") // empty line
                {
                    _line = _sr.ReadLine();
                }
                else if (_line[^1] == ':') // label
                {
                    var label = _line[..^1]; // without last char

                    CheckLabel(label);

                    try
                    {
                        _labelsDict.Add(label, _addrPtr);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new Exception($"({_lineNum} line) The label can't be repeated. Label: {label}");
                    }

                    _line = _sr.ReadLine();
                }
                else if (_line[0] == '.') // directive
                {
                    var directive = _line.Split(' ')[0];

                    switch (directive) 
                    {
                        case ".org":
                            var split = _line.Split(' ');
                            if (split.Length != 2)
                            {
                                throw new Exception($"({_lineNum} line) {directive} directive must have one argument");
                            }
                            _addrPtr = ConvertToWord(split[1]);
                            break;
                        default: 
                            throw new Exception($"({_lineNum} line) Unknown directive. Directive: {directive}");
                    }
                    
                    _line = _sr.ReadLine();
                }
                else // instruction
                {
                    var instruction = _line.Split(' ')[0];
                    
                    Decoder(instruction);

                    _line = _sr.ReadLine();
                    _addrPtr++;
                }
            }
            
            _sr.Close();
            
            WriteLabelAddresses();
            
            _bw.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
        finally
        {
            _addrPtr = 0;
            _labelsDict = new Dictionary<string, ushort>();
            _labelsUsageDict = new Dictionary<string, List<ushort>>();
        }
        
        Console.WriteLine("Сompilation completed successfully.");
    }

    private void ClearString()
    {
        var split = _line.Split(';');
        _line = split[0].Trim();
    }

    private void CheckLabel(string label)
    {
        if (char.IsDigit(label[0]))
        {
            throw new Exception($"({_lineNum} line) The label name must not start with a number");
        }

        if (!Regex.IsMatch(label, @"^[a-zA-Z0-9_]+$"))
        {
            throw new Exception($"({_lineNum} line) Invalid label name (only letters, numbers and underscore)");
        }
    }

    private void WriteLabelAddresses()
    {
        foreach (var label in _labelsDict)
        {
            if (!_labelsUsageDict.ContainsKey(label.Key))
            {
                throw new Exception($"Label '{label.Key}' is not used");
            }

            foreach (var labelUsageAddr in _labelsUsageDict[label.Key])
            {
                _bw.Seek(labelUsageAddr, SeekOrigin.Begin);
                _bw.Write(label.Value);
            }
            
            _labelsUsageDict.Remove(label.Key); // remove label after write
        }

        if (_labelsUsageDict.Count != 0)
        {
            throw new Exception($"Label '{_labelsUsageDict.First().Key}' is not defined");
        }
    }

    private void Decoder(string instruction)
    {
        byte opcode, data8;
        ushort addr, data16; 
        string arg, arg1, arg2;
        switch (instruction)
        {
            // 1 byte instructions
            case "nop":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x00);
                break;
            case "hlt":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x76);
                break;
            case "mov":
                GetTwoArguments(instruction, out arg1, out arg2);
                opcode = (arg1, arg2) switch
                {
                    ("b", "b") => 0x40,
                    ("b", "c") => 0x41,
                    ("b", "d") => 0x42,
                    ("b", "e") => 0x43,
                    ("b", "h") => 0x44,
                    ("b", "l") => 0x45,
                    ("b", "m") => 0x46,
                    ("b", "a") => 0x47,
                    ("c", "b") => 0x48,
                    ("c", "c") => 0x49,
                    ("c", "d") => 0x4a,
                    ("c", "e") => 0x4b,
                    ("c", "h") => 0x4c,
                    ("c", "l") => 0x4d,
                    ("c", "m") => 0x4e,
                    ("c", "a") => 0x4f,
                    ("d", "b") => 0x50,
                    ("d", "c") => 0x51,
                    ("d", "d") => 0x52,
                    ("d", "e") => 0x53,
                    ("d", "h") => 0x54,
                    ("d", "l") => 0x55,
                    ("d", "m") => 0x56,
                    ("d", "a") => 0x57,
                    ("e", "b") => 0x58,
                    ("e", "c") => 0x59,
                    ("e", "d") => 0x5a,
                    ("e", "e") => 0x5b,
                    ("e", "h") => 0x5c,
                    ("e", "l") => 0x5d,
                    ("e", "m") => 0x5e,
                    ("e", "a") => 0x5f,
                    ("h", "b") => 0x60,
                    ("h", "c") => 0x61,
                    ("h", "d") => 0x62,
                    ("h", "e") => 0x63,
                    ("h", "h") => 0x64,
                    ("h", "l") => 0x65,
                    ("h", "m") => 0x66,
                    ("h", "a") => 0x67,
                    ("l", "b") => 0x68,
                    ("l", "c") => 0x69,
                    ("l", "d") => 0x6a,
                    ("l", "e") => 0x6b,
                    ("l", "h") => 0x6c,
                    ("l", "l") => 0x6d,
                    ("l", "m") => 0x6e,
                    ("l", "a") => 0x6f,
                    ("m", "b") => 0x70,
                    ("m", "c") => 0x71,
                    ("m", "d") => 0x72,
                    ("m", "e") => 0x73,
                    ("m", "h") => 0x74,
                    ("m", "l") => 0x75,
                    ("m", "a") => 0x77,
                    ("a", "b") => 0x78,
                    ("a", "c") => 0x79,
                    ("a", "d") => 0x7a,
                    ("a", "e") => 0x7b,
                    ("a", "h") => 0x7c,
                    ("a", "l") => 0x7d,
                    ("a", "m") => 0x7e,
                    ("a", "a") => 0x7f,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "ldax":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x0a,
                    "d" => 0x1a
                };
                LoadOneByte(opcode);
                break;
            case "stax":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x02,
                    "d" => 0x12
                };
                LoadOneByte(opcode);
                break;
            case "stc":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x37);
                break;
            case "cmc":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x3f);
                break;
            case "add":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x80,
                    "c" => 0x81,
                    "d" => 0x82,
                    "e" => 0x83,
                    "h" => 0x84,
                    "l" => 0x85,
                    "m" => 0x86,
                    "a" => 0x87,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "adc":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x88,
                    "c" => 0x89,
                    "d" => 0x8a,
                    "e" => 0x8b,
                    "h" => 0x8c,
                    "l" => 0x8d,
                    "m" => 0x8e,
                    "a" => 0x8f,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "sub":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x90,
                    "c" => 0x91,
                    "d" => 0x92,
                    "e" => 0x93,
                    "h" => 0x94,
                    "l" => 0x95,
                    "m" => 0x96,
                    "a" => 0x97,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "sbb":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x98,
                    "c" => 0x99,
                    "d" => 0x9a,
                    "e" => 0x9b,
                    "h" => 0x9c,
                    "l" => 0x9d,
                    "m" => 0x9e,
                    "a" => 0x9f,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "ana":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0xa0,
                    "c" => 0xa1,
                    "d" => 0xa2,
                    "e" => 0xa3,
                    "h" => 0xa4,
                    "l" => 0xa5,
                    "m" => 0xa6,
                    "a" => 0xa7,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "xra":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0xa8,
                    "c" => 0xa9,
                    "d" => 0xaa,
                    "e" => 0xab,
                    "h" => 0xac,
                    "l" => 0xad,
                    "m" => 0xae,
                    "a" => 0xaf,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "ora":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0xb0,
                    "c" => 0xb1,
                    "d" => 0xb2,
                    "e" => 0xb3,
                    "h" => 0xb4,
                    "l" => 0xb5,
                    "m" => 0xb6,
                    "a" => 0xb7,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "cmp":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0xb8,
                    "c" => 0xb9,
                    "d" => 0xba,
                    "e" => 0xbb,
                    "h" => 0xbc,
                    "l" => 0xbd,
                    "m" => 0xbe,
                    "a" => 0xbf,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "cma":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x2f);
                break;
            case "rar":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x1f);
                break;
            case "rrc":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x0f);
                break;
            case "ral":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x17);
                break;
            case "rlc":
                NoArgumentsCheck(instruction);
                LoadOneByte(0x07);
                break;
            case "inr":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x04,
                    "c" => 0x0c,
                    "d" => 0x14,
                    "e" => 0x1c,
                    "h" => 0x24,
                    "l" => 0x2c,
                    "m" => 0x34,
                    "a" => 0x3c,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "dcr":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x05,
                    "c" => 0x0d,
                    "d" => 0x15,
                    "e" => 0x1d,
                    "h" => 0x25,
                    "l" => 0x2d,
                    "m" => 0x35,
                    "a" => 0x3d,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadOneByte(opcode);
                break;
            case "inx":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x03,
                    "d" => 0x13,
                    "h" => 0x23,
                    "sp" => 0x33,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, d, h, sp)")
                };
                LoadOneByte(opcode);
                break;
            case "dcx":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x0b,
                    "d" => 0x1b,
                    "h" => 0x2b,
                    "sp" => 0x3b,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, d, h, sp)")
                };
                LoadOneByte(opcode);
                break;
            case "dad":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0x09,
                    "d" => 0x19,
                    "h" => 0x29,
                    "sp" => 0x39,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, d, h, sp)")
                };
                LoadOneByte(opcode);
                break;
            case "pchl":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xe9);
                break;
            case "ret":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xc9);
                break;
            case "rnz":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xc0);
                break;
            case "rz":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xc8);
                break;
            case "rnc":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xd0);
                break;
            case "rc":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xd8);
                break;
            case "rpo":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xe0);
                break;
            case "rpe":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xe8);
                break;
            case "rp":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xf0);
                break;
            case "rm":
                NoArgumentsCheck(instruction);
                LoadOneByte(0xf8);
                break;
            case "push":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0xc5,
                    "d" => 0xd5,
                    "h" => 0xe5,
                    "psw" => 0xf5,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, d, h, psw)")
                };
                LoadOneByte(opcode);
                break;
            case "pop":
                GetOneArgument(instruction, out arg);
                opcode = arg switch
                {
                    "b" => 0xc1,
                    "d" => 0xd1,
                    "h" => 0xe1,
                    "psw" => 0xf1,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, d, h, psw)")
                };
                LoadOneByte(opcode);
                break;
            // 2 bytes instructions
            case "mvi":
                GetTwoArguments(instruction, out arg1, out arg2);
                data8 = ConvertToByte(arg2);
                opcode = arg1 switch
                {
                    "b" => 0x06,
                    "c" => 0x0e,
                    "d" => 0x16,
                    "e" => 0x1e,
                    "h" => 0x26,
                    "l" => 0x2e,
                    "m" => 0x36,
                    "a" => 0x3e,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, c, d, e, h, l, m, a)")
                };
                LoadTwoBytes(opcode, data8);
                break;
            case "adi":
                GetOneArgument(instruction, out arg);
                data8 = ConvertToByte(arg);
                LoadTwoBytes(0xc6, data8);
                break;
            case "sui":
                GetOneArgument(instruction, out arg);
                data8 = ConvertToByte(arg);
                LoadTwoBytes(0xd6, data8);
                break;
            case "ani":
                GetOneArgument(instruction, out arg);
                data8 = ConvertToByte(arg);
                LoadTwoBytes(0xe6, data8);
                break;
            case "ori":
                GetOneArgument(instruction, out arg);
                data8 = ConvertToByte(arg);
                LoadTwoBytes(0xf6, data8);
                break;
            case "aci":
                GetOneArgument(instruction, out arg);
                data8 = ConvertToByte(arg);
                LoadTwoBytes(0xce, data8);
                break;
            case "sbi":
                GetOneArgument(instruction, out arg);
                data8 = ConvertToByte(arg);
                LoadTwoBytes(0xde, data8);
                break;
            case "xri":
                GetOneArgument(instruction, out arg);
                data8 = ConvertToByte(arg);
                LoadTwoBytes(0xee, data8);
                break;
            case "cpi":
                GetOneArgument(instruction, out arg);
                data8 = ConvertToByte(arg);
                LoadTwoBytes(0xfe, data8);
                break;
            // 3 bytes instructions
            case "lda":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0x3a, addr);
                break;
            case "sta":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0x32, addr);
                break;
            case "lxi":
                GetTwoArguments(instruction, out arg1, out arg2);
                data16 = ConvertToWord(arg2);
                opcode = arg1 switch
                {
                    "b" => 0x01,
                    "d" => 0x11,
                    "h" => 0x21,
                    "sp" => 0x31,
                    _ => throw new Exception($"({_lineNum} line) Incorrect register name (use: b, d, h, sp)")
                };
                LoadThreeBytes(opcode, data16);
                break;
            case "jmp":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xc3, addr);
                break;
            case "jnz":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xc2, addr);
                break;
            case "jz":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xca, addr);
                break;
            case "jnc":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xd2, addr);
                break;
            case "jc":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xda, addr);
                break;
            case "jpo":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xe2, addr);
                break;
            case "jpe":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xea, addr);
                break;
            case "jp":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xf2, addr);
                break;
            case "jm":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xfa, addr);
                break;
            case "call":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xcd, addr);
                break;
            case "cnz":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xc4, addr);
                break;
            case "cz":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xcc, addr);
                break;
            case "cnc":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xd4, addr);
                break;
            case "cc":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xdc, addr);
                break;
            case "cpo":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xe4, addr);
                break;
            case "cpe":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xec, addr);
                break;
            case "cp":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xf4, addr);
                break;
            case "cm":
                GetOneArgument(instruction, out arg);
                addr = GetAddress(arg);
                LoadThreeBytes(0xfc, addr);
                break;
            default:
                throw new Exception($"({_lineNum} line) Unknown instruction. Instruction: {instruction}");
        }
    }

    private void LoadOneByte(byte opcode)
    {
        _bw.Seek(_addrPtr, SeekOrigin.Begin);
        _bw.Write(opcode);
    }

    private void LoadTwoBytes(byte opcode, byte data)
    {
        _bw.Seek(_addrPtr, SeekOrigin.Begin);
        _bw.Write(opcode);
        _addrPtr++;
        _bw.Seek(_addrPtr, SeekOrigin.Begin);
        _bw.Write(data);
    }
    
    private void LoadThreeBytes(byte opcode, ushort data)
    {
        _bw.Seek(_addrPtr, SeekOrigin.Begin);
        _bw.Write(opcode);
        _addrPtr++;
        _bw.Seek(_addrPtr, SeekOrigin.Begin);
        _bw.Write(data);
        _addrPtr++;
    }

    private ushort GetAddress(string arg)
    {
        if (char.IsDigit(arg[0]))
        {
            return ConvertToWord(arg);
        }
        else
        {
            CheckLabel(arg);

            if (_labelsUsageDict.ContainsKey(arg))
            {
                _labelsUsageDict[arg].Add((ushort)(_addrPtr + 1));
            }
            else
            {
                _labelsUsageDict.Add(arg, [(ushort)(_addrPtr + 1)]);
            }
            
            return 0;
        }
    }

    private void NoArgumentsCheck(string instruction)
    {
        if (_line != instruction)
        {
            throw new Exception($"({_lineNum} line) {instruction} instruction can't have arguments");
        }
    }

    private void GetOneArgument(string instruction, out string arg)
    {
        var split = _line.Split(' ');
        
        if (split.Length != 2)
        {
            throw new Exception($"({_lineNum} line) {instruction} instruction must have one argument");
        }

        arg = split[1];
    }

    private void GetTwoArguments(string instruction, out string arg1, out string arg2)
    {
        var split = _line.Split(' ');
        
        if (split.Length != 3)
        {
            throw new Exception($"({_lineNum} line) {instruction} instruction must have two arguments");
        }

        if (split[1][^1] != ',') // first argument, last char
        {
            throw new Exception($"({_lineNum} line) Arguments must be separated by commas");
        }

        arg1 = split[1][..^1];
        arg2 = split[2];
    }

    private byte ConvertToByte(string data)
    {
        try
        {
            if (data.Length < 2)
            {
                return (byte)Convert.ToSByte(data);
            }
            else
            {
                return data[..2] switch
                {
                    "0x" => Convert.ToByte(data[2..], 16), // hexadecimal prefix
                    "0b" => Convert.ToByte(data[2..], 2), // binary prefix
                    _ => (byte)Convert.ToSByte(data)
                };
            }
            
        }
        catch (FormatException ex)
        {
            throw new Exception($"({_lineNum} line) Value is not a number");
        }
        catch (OverflowException ex)
        {
            throw new Exception($"({_lineNum} line) Value was either too large or too small for a byte");
        }
    }
    
    private ushort ConvertToWord(string data)
    {
        try
        {
            if (data.Length < 2)
            {
                return (ushort)Convert.ToInt16(data);
            }
            else
            {
                return data[..2] switch
                {
                    "0x" => Convert.ToUInt16(data[2..], 16), // hexadecimal prefix
                    "0b" => Convert.ToUInt16(data[2..], 2), // binary prefix
                    _ => (ushort)Convert.ToInt16(data)
                };
            }
            
        }
        catch (FormatException ex)
        {
            throw new Exception($"({_lineNum} line) Value is not a number");
        }
        catch (OverflowException ex)
        {
            throw new Exception($"({_lineNum} line) Value was either too large or too small for a 2 bytes");
        }
    }
}