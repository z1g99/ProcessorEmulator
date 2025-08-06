? -> MAR; RDM; MDR -> ? ✅  
? -> MAR; ? -> MDR; WDM ❌

# Fetch cycle

T<sub>1</sub> - PC -> MAR  
T<sub>2</sub> - PC++; RDM; MDR -> IR  

# CPU control instructions

## NOP
desc: No operation

## HLT
desc: Halt

T<sub>3</sub> - HLT  

# Data transfer instructions

## MOV reg1,reg2
desc: Move data to reg1 from reg2

T<sub>3</sub> - reg2 -> reg1  

If indirect write/read: (MOV reg,M | MOV M,reg)

T<sub>3</sub> - HL -> MAR;
T<sub>4</sub> - RDM/WDM; MDR <-> reg

## MVI reg,byte
desc: Move immediate byte to reg

T<sub>3</sub> - PC -> MAR  
T<sub>4</sub> - PC++; RDM; MDR -> reg   

If indirect write: (MVI M,byte)

T<sub>3</sub> - PC -> MAR; RDM  
T<sub>4</sub> - PC++; HL -> MAR;
T<sub>5</sub> - WDM

## LDA/STA addr
desc: Load A with data from mem loc addr / Store A in mem loc addr

T<sub>3</sub> - PC -> MAR  
T<sub>4</sub> - PC++; RDM; MDR -> Z  
T<sub>5</sub> - PC -> MAR  
T<sub>6</sub> - PC++; RDM; MDR -> W  
T<sub>7</sub> - WZ -> MAR  
T<sub>8</sub> - RDM/WDM; MDR <-> A

## LXI xreg,dble 
desc: Load extended immediate dble into xreg

T<sub>3</sub> - PC -> MAR  
T<sub>4</sub> - PC++; RDM; MDR -> xreg<sub>L</sub>  
T<sub>5</sub> - PC -> MAR  
T<sub>6</sub> - PC++; RDM; MDR -> xreg<sub>H</sub>

## LDAX/STAX xreg
desc: Load/Store A with data from mem loc xreg

T<sub>3</sub> - xreg -> MAR  
T<sub>4</sub> - RDM/WDM; MDR <-> A

## LHLD/SHLD addr
desc: Load/Store HL direct with data starting at addr


T<sub>3</sub> - PC -> MAR  
T<sub>4</sub> - PC++; RDM; MDR -> Z  
T<sub>5</sub> - PC -> MAR  
T<sub>6</sub> - PC++; RDM; MDR -> W  
T<sub>7</sub> - WZ -> MAR  
T<sub>8</sub> - RDM/WDM; MDR <-> L  
T<sub>9</sub> - MAR++ 
T<sub>10</sub> - RDM/WDM; MDR <-> H

## XCHG
desc: Exchange DE with HL

T<sub>3</sub> - HL -> WZ
T<sub>4</sub> - DE -> HL
T<sub>5</sub> - WZ -> DE

# Flag instructions

## STC/CMC
desc: Set/Complement carry flag (CY = 1 / ~CY)  
flags: C

T<sub>3</sub> - CY = 1 / ~CY

# Arithmetic instructions

## ADD/SUB reg
desc: Add reg to A/Subtract reg from A
flags: SZPC

T<sub>3</sub> - reg -> TMP  
T<sub>4</sub> - ALU -> A

If indirect: (ADD/SUB M)

T<sub>3</sub> - HL -> MAR; RDM; MDR -> TMP  
T<sub>4</sub> - ALU -> A

## ADC/SBB reg
desc: Add with carry reg to A / Subtract with borrow reg from A  
flags: SZPC

T<sub>3</sub> - reg -> TMP  
T<sub>4</sub> - ALU -> A

If indirect: (ADC/SBB M)

T<sub>3</sub> - HL -> MAR; RDM; MDR -> TMP  
T<sub>4</sub> - ALU -> A

## ADI/SUI byte
desc: Add immediate byte to A / Subtract immediate byte from A   
flags: SZPC

T<sub>3</sub> - PC -> MAR; RDM; MDR -> TMP  
T<sub>4</sub> - PC++; ALU -> A

## ACI/SBI byte
desc: Add with carry immediate byte to A / Subtract with borrow immediate byte from A    
flags: SZPC

T<sub>3</sub> - PC -> MAR; RDM; MDR -> TMP  
T<sub>4</sub> - PC++; ALU -> A

## DAD xreg
desc: Double add xreg to HL

flags: SZPC

T<sub>3</sub> - xreg<sub>L</sub> -> A  
T<sub>4</sub> - L -> TMP  
T<sub>5</sub> - ALU(add) -> L  
T<sub>6</sub> - xreg<sub>H</sub> -> A  
T<sub>7</sub> - H -> TMP  
T<sub>8</sub> - ALU(adc) -> H

# Logical instructions

## ANA/ORA/XRA reg
desc: And/Or/Xor reg with A  
flags: SZP

T<sub>3</sub> - reg -> TMP  
T<sub>4</sub> - ALU -> A

If indirect: (ANA/ORA/XRA M)

T<sub>3</sub> - HL -> MAR; RDM; MDR -> TMP  
T<sub>4</sub> - ALU -> A

## ANI/ORI/XRI byte
desc: And/Or/Xor immediate byte with A  
flags: SZP

T<sub>3</sub> - PC -> MAR; RDM; MDR -> TMP  
T<sub>4</sub> - PC++; ALU -> A

## CMA
desc: Complement A

T<sub>3</sub> - ALU -> A

# Rotate and shift instructions

## RAR/RAL
desc: Rotate all right/left  
flags: C

T<sub>3</sub> - ALU -> A

## RRC/RLC
desc: Rotate right/left with carry  
flags: C

T<sub>3</sub> - ALU -> A

# Increment and decrement instructions

## INR/DCR reg
desc: Increment/decrement reg  
flags: SZP

T<sub>3</sub> - reg -> TMP  
T<sub>4</sub> - TMP++/TMP--; TMP -> reg

If indirect:

T<sub>3</sub> - HL -> MAR; RDM; MDR -> TMP  
T<sub>4</sub> - TMP++/TMP--; TMP -> MDR  
T<sub>5</sub> - WDM  

## INX/DCX xreg
desc: Increment/decrement extended xreg  
flags: SZP

T<sub>3</sub> - xreg<sub>L</sub> -> TMP   
T<sub>4</sub> - TMP++/TMP--; TMP -> xreg<sub>L</sub>  
T<sub>5</sub> - xreg<sub>H</sub> -> TMP   
T<sub>6</sub> - TMP++/TMP--/TMP; TMP -> xreg<sub>H</sub>  

# Compare instructions

## CMP reg
desc: Compare reg to A  
flags: SZPC

T<sub>3</sub> - reg -> TMP; ALU(sub)

If indirect: (CMP M)

T<sub>3</sub> - HL -> MAR; RDM; MDR -> TMP; ALU(sub)

## CPI byte
desc: Compare immediate byte to A  
flags: SZPC

T<sub>3</sub> - PC -> MAR; RDM; MDR -> TMP; ALU(sub)  
T<sub>4</sub> - PC++;

# Unconditional jump instructions

## JMP addr
desc: Jump to mem loc addr

T<sub>3</sub> - PC -> MAR  
T<sub>4</sub> - PC++; RDM; MDR -> Z  
T<sub>5</sub> - PC -> MAR; RDM; MDR -> W   
T<sub>6</sub> - WZ -> PC;

## PCHL
desc: Transfer to the program counter HL

T<sub>3</sub> - HL -> PC

# Conditional jump instructions

## JNZ/JZ JNC/JC JPO/JPE JP/JM addr
desc: Jump if not zero/zero/not carry/carry/odd/even/plus/minus

T<sub>3</sub> - PC -> MAR;  
T<sub>4</sub> - PC++; RDM; MDR -> Z  
T<sub>5</sub> - PC -> MAR; RDM; MDR -> W   
T<sub>6</sub> - PC++; (RESET IF FALSE)  
T<sub>7</sub> - WZ -> PC;

# Subroutine instructions

## CALL addr
desc: Call subroutine

T<sub>3</sub> - PC -> MAR  
T<sub>4</sub> - PC++; RDM; MDR -> Z   
T<sub>5</sub> - PC -> MAR; RDM; MDR -> W; SP--  
T<sub>6</sub> - PC++; SP -> MAR  
T<sub>7</sub> - SP--; PC<sub>H</sub> -> MDR; WDM  
T<sub>8</sub> - SP -> MAR   
T<sub>9</sub> - PC<sub>L</sub> -> MDR; WDM  
T<sub>10</sub> - WZ -> PC  

## CNZ/CZ CNC/CC CPO/CPE CP/CM addr
desc: Call subroutine if not zero/zero/not carry/carry/odd/even/plus/minus

T<sub>3</sub> - PC -> MAR  
T<sub>4</sub> - PC++; RDM; MDR -> Z   
T<sub>5</sub> - PC -> MAR; RDM; MDR -> W  
T<sub>6</sub> - PC++; (RESET IF FALSE)  
T<sub>7</sub> - SP--; SP -> MAR  
T<sub>8</sub> - SP--; PC<sub>H</sub> -> MDR; WDM  
T<sub>9</sub> - SP -> MAR  
T<sub>10</sub> - PC<sub>L</sub> -> MDR; WDM  
T<sub>11</sub> - WZ -> PC

## RET
desc: Return

T<sub>3</sub> - SP -> MAR  
T<sub>4</sub> - SP++; RDM; MDR -> Z  
T<sub>5</sub> - SP -> MAR; RDM; MDR -> W   
T<sub>6</sub> - WZ -> PC; SP++

## RNZ/RZ RNC/RC RPO/RPE RP/RM
desc: Return if not zero/zero/not carry/carry/odd/even/plus/minus

T<sub>3</sub> - SP -> MAR; (RESET IF FALSE)  
T<sub>4</sub> - SP++; RDM; MDR -> Z  
T<sub>5</sub> - SP -> MAR; RDM; MDR -> W   
T<sub>6</sub> - WZ -> PC; SP++

# Stack instructions

## LXI SP,dble
desc: desc: Load extended immediate dble into SP

## DAD SP
desc: Double add SP to HL

## INX SP
desc: Increment extended SP

## DCX SP
desc: Decrement extended SP

## PUSH xreg
desc: Push xreg to mem loc SP (+PSW)

T<sub>3</sub> - SP--; SP -> MAR   
T<sub>4</sub> - xreg<sub>H</sub> -> MDR; WDM  
T<sub>5</sub> - SP--; SP -> MAR  
T<sub>6</sub> - xreg<sub>L</sub> -> MDR; WDM  

## POP xreg
desc: Pop xreg from mem loc SP (+PSW)

T<sub>3</sub> - SP -> MAR; RDM; MDR -> xreg<sub>L</sub>  
T<sub>4</sub> - SP++; SP -> MAR; RDM; MDR -> xreg<sub>H</sub>  
T<sub>5</sub> - SP++

## XTHL
desc: Exchange top of stack with HL

T<sub>3</sub> - HL -> WZ  
T<sub>4</sub> - SP -> MAR; RDM; MDR -> L  
T<sub>5</sub> - Z -> MDR; WDM  
T<sub>6</sub> - MAR++; RDM; MDR -> H  
T<sub>7</sub> - W -> MDR; WDM

## SPHL
desc: Move from HL to SP

T<sub>3</sub> - HL -> SP

# Unimplemented i8085 instructions

## Arithmetic instructions

## DAA
desc: Decimal adjust A

## Subroutine instructions

## RST 0-8
desc: Call subroutine to mem locs: 0x0, 0x8, 0x10, 0x18, 0x20, 0x28, 0x30, 0x38


## Interrupt instructions

## DI
desc: Disable interrupts

## EI
desc: Enable interrupts

## RIM/SIM
desc: Read/Set interrupt mask

## Input-output instructions

## OUT byte
desc: Output to port *byte* contents of A

## IN byte
desc: Input into A contents from input port *byte*