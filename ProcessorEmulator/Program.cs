using ProcessorEmulator.Compiler.Source;
using ProcessorEmulator.Source;

var processor = new Processor();
var compiler = new Compiler();

const int originalFrequency = 3000000;
const string fileName = "shift_multiply";

compiler.Compile($"files/src/{fileName}.txt", $"files/bin/{fileName}.bin"); 

processor.Run($"files/bin/{fileName}.bin", originalFrequency, true, false);