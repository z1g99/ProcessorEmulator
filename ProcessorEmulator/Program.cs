using ProcessorEmulator.Compiler.Source;
using ProcessorEmulator.Source;

var processor = new Processor();
var compiler = new Compiler();

compiler.Compile("files/src/asmfile.txt", "files/bin/asmfile.bin");
processor.Run("files/bin/asmfile.bin", 1000000000, true);