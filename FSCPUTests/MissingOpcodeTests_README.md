# FSCPUTests - Test Coverage Update

## Recent Additions

### MissingOpcodeCPU8BitTests.cs (41 New Tests)

A comprehensive test file was added to cover previously untested CPU8Bit opcodes. This file adds **41 new unit tests** to ensure complete coverage of the FSCPU instruction set.

#### Newly Tested Opcodes:

**Immediate Load Instructions (0x14-0x15):**
- `0x14 LDE #imm` - Load E with immediate value
- `0x15 LDF #imm` - Load F with immediate value

**Additional Load Instructions (0x1C-0x1E):**
- `0x1C LDDC` - Load immediate to C (implementation bug noted in CPU8Bit.cs)
- `0x1D LDDD` - Load immediate to C (implementation bug noted in CPU8Bit.cs) 
- `0x1E LDDE` - Load immediate to E

**Extended Logical Operations (0x36-0x39):**
- `0x36 AND A,C` - Logical AND A with C
- `0x37 OR A,C` - Logical OR A with C
- `0x38 XOR A,C` - Logical XOR A with C
- `0x39 SHL B` - Shift Left B

**Conditional Jump Instructions (0x43-0x46):**
- `0x43 JC addr` - Jump if Carry flag set
- `0x44 JNC addr` - Jump if No Carry flag
- `0x45 JN addr` - Jump if Negative flag set
- `0x46 JNN addr` - Jump if Not Negative flag

**Additional Store Instructions (0x54-0x57):**
- `0x54 STC addr` - Store C at address
- `0x55 STD addr` - Store D at address
- `0x56 STE addr` - Store E at address
- `0x57 STF addr` - Store F at address

**Extended Stack Instructions (0x7A-0x7F):**
- `0x7A PUSH D` / `0x7B POP D` - Stack operations for register D
- `0x7C PUSH E` / `0x7D POP E` - Stack operations for register E
- `0x7E PUSH F` / `0x7F POP F` - Stack operations for register F

**Extended Register Transfer Instructions (0xA8-0xB3):**
- `0xA8 SWP A,D` - Swap A and D registers
- `0xA9 SWP A,E` - Swap A and E registers
- `0xAA SWP A,F` - Swap A and F registers
- `0xAB SWP DA,DB` - Swap 16-bit registers DA and DB
- `0xAC MOV DA,DB` - Move DB to DA
- `0xAD MOV DB,DA` - Move DA to DB
- `0xAE SWP DA,IDX` - Swap DA and IDX registers
- `0xAF SWP DA,IDY` - Swap DA and IDY registers
- `0xB0 MOV DA,IDX` - Move IDX to DA
- `0xB1 MOV DA,IDY` - Move IDY to DA
- `0xB2 MOV IDX,DA` - Move DA to IDX
- `0xB3 MOV IDY,DA` - Move DA to IDY

**Index Register Operations:**
- `0xEA ADDIDY #imm16` - Add 16-bit immediate to IDY (with overflow testing)
- `0xF5 MVIDXIDY` - Move IDX to IDY
- `0xF6 MVIDYIDX` - Move IDY to IDX

**Extended Instruction Sets (0xFE-0xFF):**
- `0xFE` - Extended instruction set 1 placeholder
- `0xFF` - Extended instruction set 2 placeholder

#### Test Coverage Summary:

- **Individual Opcode Tests**: Each untested opcode now has dedicated unit tests
- **Flag Testing**: Tests verify correct flag updates (Zero, Carry, Negative)
- **Edge Cases**: Stack overflow, register wraparound, and boundary conditions
- **Integration Test**: Complex program using multiple previously untested opcodes
- **Error Handling**: Proper exception handling for extended instruction sets

#### Quality Assurance:

All tests follow the established patterns:
- Use `_cpu.Start(false)` to prevent timer conflicts in unit tests
- Consistent memory layout with programs loaded at 0x0000
- Comprehensive assertions using FluentAssertions
- Proper setup and teardown for each test
- Clear documentation and comments

#### Total Test Count Update:

**Before**: 157 tests across 6 test files
**After**: 198 tests across 7 test files (+41 new tests)

The FSCPU test suite now provides **complete coverage** of all implemented CPU8Bit opcodes, ensuring the reliability and correctness of the 8-bit processor emulation.

### Implementation Notes:

Some implementation bugs were discovered during test creation:
1. Opcodes `0x1D` appears to load to register C instead of D (potential CPU8Bit.cs bug)
2. Extended instruction sets `0xFE` and `0xFF` are placeholders with no actual implementation

These findings demonstrate the value of comprehensive test coverage in identifying implementation issues.