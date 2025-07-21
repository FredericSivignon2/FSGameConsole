# FS8 Processor - Complete Opcode Reference (Enhanced Edition)

**Version 2.0** - Updated with 120+ instructions including CMP immediate operations and extended register set

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Register Set](#register-set)
3. [Memory Organization](#memory-organization)
4. [Basic Instructions (0x00-0x01)](#basic-instructions-0x00-0x01)
5. [8-bit Load Instructions (0x10-0x1E)](#8-bit-load-instructions-0x10-0x1e)
6. [Arithmetic Instructions (0x20-0x2F)](#arithmetic-instructions-0x20-0x2f)
7. [Logical Instructions (0x30-0x39)](#logical-instructions-0x30-0x39)
8. [Jump Instructions (0x40-0x46)](#jump-instructions-0x40-0x46)
9. [Store Instructions (0x50-0x57)](#store-instructions-0x50-0x57)
10. [Subroutine Instructions (0x60-0x61)](#subroutine-instructions-0x60-0x61)
11. [Stack Instructions (0x70-0x7F)](#stack-instructions-0x70-0x7f)
12. [Memory Load Instructions (0x90-0x95)](#memory-load-instructions-0x90-0x95)
13. [Register Transfer Instructions (0xA0-0xB3)](#register-transfer-instructions-0xa0-0xb3)
14. [Relative Jump & Index Operations (0xC0-0xCB)](#relative-jump--index-operations-0xc0-0xcb)
15. [Compare Immediate Instructions (0xD0-0xD7) - NEW](#compare-immediate-instructions-0xd0-0xd7---new)
16. [Index Register Instructions (0xE0-0xEA)](#index-register-instructions-0xe0-0xea)
17. [Advanced Index Transfer (0xF5-0xF9)](#advanced-index-transfer-0xf5-0xf9)
18. [Extended Instructions (0xFE-0xFF)](#extended-instructions-0xfe-0xff)
19. [Programming Examples](#programming-examples)

---

## Architecture Overview

The **FS8 Enhanced** is an 8-bit processor with advanced 16-bit capabilities, inspired by the Amstrad CPC architecture. It features:

- **Little-endian** memory organization
- **64KB addressable memory** space
- **120+ instructions** with extensive addressing modes
- **Realistic timing** with cycle-accurate execution
- **Index registers** for advanced data manipulation
- **System calls** for hardware interfacing

### Key Features

- 6 general-purpose 8-bit registers (A, B, C, D, E, F)
- 2 extended 16-bit registers (DA, DB)
- 2 index registers (IDX, IDY)
- 16-bit Program Counter and Stack Pointer
- Status Register with Zero, Carry, Overflow, Negative flags
- Integrated ALU with 16-bit operations
- **NEW**: Complete immediate comparison instructions

---

## Register Set

### 8-bit General Purpose Registers
- **A, B, C, D, E, F**: Primary data registers for arithmetic and logical operations

### 16-bit Extended Registers
- **DA, DB**: Enhanced 16-bit registers for accelerated calculations and extended storage

### 16-bit Index Registers (Simplified Architecture)
- **IDX, IDY**: Address pointers for advanced indexing (simplified from v1.0's 4-register system)

### 16-bit System Registers
- **PC**: Program Counter (points to next instruction)
- **SP**: Stack Pointer (stack grows downward from 0xFFFF)
- **SR**: Status Register containing flags

### Status Register Flags
- **Zero (Z)**: Set when result equals zero
- **Carry (C)**: Set on arithmetic overflow/underflow or borrow
- **Overflow (V)**: Set on signed arithmetic overflow
- **Negative (N)**: Set when result has bit 7 (or bit 15 for 16-bit) set

---

## Memory Organization

**Total Address Space**: 64KB (0x0000-0xFFFF)

| Address Range | Size  | Description                                    | Notes                    |
|---------------|-------|------------------------------------------------|--------------------------|
| 0x0000-0x7FFF | 32KB  | **RAM Program Space**                         | User programs            |
| 0x8000-0xBFFF | 16KB  | **Video Bitmap Memory (CPC-style)**          | 320x200, 16 colors      |
| 0xC000-0xF3FF | 13KB  | **Extended RAM**                              | Additional storage       |
| 0xF400-0xF7FF | 1KB   | **ROM BOOT** *(Read-only)*                   | System boot code         |
| 0xF800-0xFFFF | 2KB   | **BIOS/System**                              | System functions         |

### Little-Endian Format
All 16-bit values are stored with the **low byte first**:
- **Example**: 0x1234 → Memory: [0x34] [0x12]

---

## Basic Instructions (0x00-0x01)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x00   | NOP      | -          | 1     | 1      | No operation. Advances PC by 1. |
| 0x01   | HALT     | -          | 1     | 1      | Stop processor execution. |

**Usage Examples:**
```assembly
NOP         ; Wait one cycle
HALT        ; Stop program execution
```

---

## 8-bit Load Instructions (0x10-0x1E)

### Immediate Load Instructions

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x10   | LDA      | #imm8      | 2     | 2      | Load immediate value into A |
| 0x11   | LDB      | #imm8      | 2     | 2      | Load immediate value into B |
| 0x12   | LDC      | #imm8      | 2     | 2      | Load immediate value into C |
| 0x13   | LDD      | #imm8      | 2     | 2      | Load immediate value into D |
| 0x14   | LDE      | #imm8      | 2     | 2      | Load immediate value into E |
| 0x15   | LDF      | #imm8      | 2     | 2      | Load immediate value into F |

### 16-bit Load Instructions

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x16   | LDDA     | #imm16     | 3     | 3      | Load 16-bit immediate into DA |
| 0x17   | LDDB     | #imm16     | 3     | 3      | Load 16-bit immediate into DB |
| 0x18   | LDDA     | addr       | 3     | 5      | Load DA from memory address |
| 0x19   | LDDB     | addr       | 3     | 5      | Load DB from memory address |
| 0x1A   | LDIDX    | #imm16     | 3     | 3      | Load 16-bit immediate into IDX |
| 0x1B   | LDIDY    | #imm16     | 3     | 3      | Load 16-bit immediate into IDY |

### Legacy Load Instructions (0x1C-0x1E)
| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description | Notes |
|--------|----------|------------|-------|--------|-------------|--------|
| 0x1C   | LDDC     | #imm8      | 2     | 2      | Load immediate into C | *Legacy implementation* |
| 0x1D   | LDDD     | #imm8      | 2     | 2      | Load immediate into C | *Bug: should load D* |
| 0x1E   | LDDE     | #imm8      | 2     | 2      | Load immediate into E | *Legacy implementation* |

**Supported Value Formats:**
- **Decimal**: `42`, `255`
- **Hexadecimal**: `0xFF`, `$FF`
- **Character**: `'A'`, `'Z'`

**Usage Examples:**
```assembly
LDA #42         ; Load decimal 42 into A
LDB #0xFF       ; Load hex FF into B
LDC #$80        ; Load hex 80 into C ($ prefix)
LDD #'A'        ; Load ASCII value of 'A' (65) into D
LDDA #0x1234    ; Load 0x1234 into DA register
LDIDX #BUFFER   ; Load address of BUFFER label into IDX
```

---

## Arithmetic Instructions (0x20-0x2F)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x20   | ADD      | A,B        | 1     | 1      | Add B to A, result in A |
| 0x21   | SUB      | A,B        | 1     | 1      | Subtract B from A, result in A |
| 0x22   | ADD16    | DA,DB      | 1     | 2      | Add DB to DA, result in DA |
| 0x23   | SUB16    | DA,DB      | 1     | 2      | Subtract DB from DA, result in DA |
| 0x24   | INC16    | DA         | 1     | 2      | Increment DA by 1 |
| 0x25   | DEC16    | DA         | 1     | 2      | Decrement DA by 1 |
| 0x26   | INC16    | DB         | 1     | 2      | Increment DB by 1 |
| 0x27   | DEC16    | DB         | 1     | 2      | Decrement DB by 1 |
| 0x28   | INC      | A          | 1     | 1      | Increment A by 1 |
| 0x29   | DEC      | A          | 1     | 1      | Decrement A by 1 |
| 0x2A   | INC      | B          | 1     | 1      | Increment B by 1 |
| 0x2B   | DEC      | B          | 1     | 1      | Decrement B by 1 |
| 0x2C   | CMP      | A,B        | 1     | 1      | Compare A with B (A - B) |
| 0x2D   | INC      | C          | 1     | 1      | Increment C by 1 |
| 0x2E   | DEC      | C          | 1     | 1      | Decrement C by 1 |
| 0x2F   | CMP      | A,C        | 1     | 1      | Compare A with C (A - C) |

**Flags Updated**: Zero, Carry, Overflow, Negative

**Usage Examples:**
```assembly
LDA #10
LDB #5
ADD A,B         ; A = A + B = 15
SUB A,B         ; A = A - B = 10
INC A           ; A = A + 1 = 11
CMP A,B         ; Compare A(11) with B(5), sets flags
```

---

## Logical Instructions (0x30-0x39)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x30   | AND      | A,B        | 1     | 1      | Bitwise AND of A and B → A |
| 0x31   | OR       | A,B        | 1     | 1      | Bitwise OR of A and B → A |
| 0x32   | XOR      | A,B        | 1     | 1      | Bitwise XOR of A and B → A |
| 0x33   | NOT      | A          | 1     | 1      | Bitwise NOT of A → A (~A) |
| 0x34   | SHL      | A          | 1     | 1      | Shift A left 1 bit |
| 0x35   | SHR      | A          | 1     | 1      | Shift A right 1 bit |
| 0x36   | AND      | A,C        | 1     | 1      | Bitwise AND of A and C → A |
| 0x37   | OR       | A,C        | 1     | 1      | Bitwise OR of A and C → A |
| 0x38   | XOR      | A,C        | 1     | 1      | Bitwise XOR of A and C → A |
| 0x39   | SHL      | B          | 1     | 1      | Shift B left 1 bit |

**Usage Examples:**
```assembly
LDA #0xFF
LDB #0x0F
AND A,B         ; A = 0xFF & 0x0F = 0x0F
OR A,B          ; A = 0x0F | 0x0F = 0x0F
XOR A,B         ; A = 0x0F ^ 0x0F = 0x00
NOT A           ; A = ~0x00 = 0xFF
SHL A           ; A = 0xFF << 1, Carry = 1
```

---

## Jump Instructions (0x40-0x46)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x40   | JMP      | addr       | 3     | 3      | Unconditional jump to address |
| 0x41   | JZ       | addr       | 3     | 3      | Jump if Zero flag set |
| 0x42   | JNZ      | addr       | 3     | 3      | Jump if Zero flag clear |
| 0x43   | JC       | addr       | 3     | 3      | Jump if Carry flag set |
| 0x44   | JNC      | addr       | 3     | 3      | Jump if Carry flag clear |
| 0x45   | JN       | addr       | 3     | 3      | Jump if Negative flag set |
| 0x46   | JNN      | addr       | 3     | 3      | Jump if Negative flag clear |

**Address Formats:**
- **Hexadecimal**: `0x1000`, `$2000`
- **Decimal**: `4096`
- **Labels**: `LOOP`, `END`

**Usage Examples:**
```assembly
JMP 0x1000      ; Jump to address 0x1000
JMP START       ; Jump to label START
CMP A,B
JZ EQUAL        ; Jump if A == B
JC OVERFLOW     ; Jump if last operation caused carry
```

---

## Store Instructions (0x50-0x57)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x50   | STA      | addr       | 3     | 4      | Store A at memory address |
| 0x51   | STDA     | addr       | 3     | 5      | Store DA at address (16-bit) |
| 0x52   | STDB     | addr       | 3     | 5      | Store DB at address (16-bit) |
| 0x53   | STB      | addr       | 3     | 4      | Store B at memory address |
| 0x54   | STC      | addr       | 3     | 4      | Store C at memory address |
| 0x55   | STD      | addr       | 3     | 4      | Store D at memory address |
| 0x56   | STE      | addr       | 3     | 4      | Store E at memory address |
| 0x57   | STF      | addr       | 3     | 4      | Store F at memory address |

**Usage Examples:**
```assembly
LDA #42
STA 0x2000      ; Store A(42) at address 0x2000
LDDA #0x1234
STDA BUFFER     ; Store DA(0x1234) at BUFFER (little-endian)
```

---

## Subroutine Instructions (0x60-0x61)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x60   | CALL     | addr       | 3     | 5      | Call subroutine at address |
| 0x61   | RET      | -          | 1     | 4      | Return from subroutine |

**Stack Operation** (little-endian):
- **CALL**: SP -= 2, store return address at [SP] and [SP+1]
- **RET**: read address from [SP] and [SP+1], SP += 2

**Usage Examples:**
```assembly
CALL MULTIPLY   ; Call subroutine at MULTIPLY
; ... main code continues here after RET

MULTIPLY:       ; Subroutine starts here
    ; ... subroutine code
    RET         ; Return to caller
```

---

## Stack Instructions (0x70-0x7F)

### 8-bit Stack Operations

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x70   | PUSH     | A          | 1     | 2      | Push A onto stack |
| 0x71   | POP      | A          | 1     | 2      | Pop from stack to A |
| 0x74   | PUSH     | B          | 1     | 2      | Push B onto stack |
| 0x75   | POP      | B          | 1     | 2      | Pop from stack to B |
| 0x78   | PUSH     | C          | 1     | 2      | Push C onto stack |
| 0x79   | POP      | C          | 1     | 2      | Pop from stack to C |
| 0x7A   | PUSH     | D          | 1     | 2      | Push D onto stack |
| 0x7B   | POP      | D          | 1     | 2      | Pop from stack to D |
| 0x7C   | PUSH     | E          | 1     | 2      | Push E onto stack |
| 0x7D   | POP      | E          | 1     | 2      | Pop from stack to E |
| 0x7E   | PUSH     | F          | 1     | 2      | Push F onto stack |
| 0x7F   | POP      | F          | 1     | 2      | Pop from stack to F |

### 16-bit Stack Operations

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x72   | PUSH16   | DA         | 1     | 3      | Push DA onto stack (16-bit) |
| 0x73   | POP16    | DA         | 1     | 3      | Pop from stack to DA (16-bit) |
| 0x76   | PUSH16   | DB         | 1     | 3      | Push DB onto stack (16-bit) |
| 0x77   | POP16    | DB         | 1     | 3      | Pop from stack to DB (16-bit) |

**Stack Notes:**
- Stack grows downward from 0xFFFF
- 16-bit values stored in little-endian format
- POP operations update Zero flag

**Usage Examples:**
```assembly
LDA #42
PUSH A          ; Save A on stack
LDA #100        ; Modify A
POP A           ; Restore A (now 42 again)

LDDA #0x1234
PUSH16 DA       ; Save DA on stack
POP16 DA        ; Restore DA
```

---

## Memory Load Instructions (0x90-0x95)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0x90   | LDA      | addr       | 3     | 4      | Load A from memory address |
| 0x91   | LDB      | addr       | 3     | 4      | Load B from memory address |
| 0x92   | LDC      | addr       | 3     | 4      | Load C from memory address |
| 0x93   | LDD      | addr       | 3     | 4      | Load D from memory address |
| 0x94   | LDE      | addr       | 3     | 4      | Load E from memory address |
| 0x95   | LDF      | addr       | 3     | 4      | Load F from memory address |

**Note**: These load from memory addresses, unlike immediate loads (0x10-0x15).

**Usage Examples:**
```assembly
LDA 0x2000      ; Load A from memory address 0x2000
LDB VARIABLE    ; Load B from address of VARIABLE label
```

---

## Register Transfer Instructions (0xA0-0xB3)

### Basic Register Moves

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0xA0   | MOV      | A,B        | 1     | 1      | Move B to A (A = B) |
| 0xA1   | MOV      | A,C        | 1     | 1      | Move C to A (A = C) |
| 0xA2   | MOV      | B,A        | 1     | 1      | Move A to B (B = A) |
| 0xA3   | MOV      | B,C        | 1     | 1      | Move C to B (B = C) |
| 0xA4   | MOV      | C,A        | 1     | 1      | Move A to C (C = A) |
| 0xA5   | MOV      | C,B        | 1     | 1      | Move B to C (C = B) |

### Register Swaps

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0xA6   | SWP      | A,B        | 1     | 2      | Swap A and B registers |
| 0xA7   | SWP      | A,C        | 1     | 2      | Swap A and C registers |
| 0xA8   | SWP      | A,D        | 1     | 2      | Swap A and D registers |
| 0xA9   | SWP      | A,E        | 1     | 2      | Swap A and E registers |
| 0xAA   | SWP      | A,F        | 1     | 2      | Swap A and F registers |
| 0xAB   | SWP      | DA,DB      | 1     | 2      | Swap DA and DB (16-bit) |

### 16-bit Register Operations

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0xAC   | MOV      | DA,DB      | 1     | 1      | Move DB to DA (DA = DB) |
| 0xAD   | MOV      | DB,DA      | 1     | 1      | Move DA to DB (DB = DA) |
| 0xAE   | SWP      | DA,IDX     | 1     | 2      | Swap DA and IDX |
| 0xAF   | SWP      | DA,IDY     | 1     | 2      | Swap DA and IDY |
| 0xB0   | MOV      | DA,IDX     | 1     | 1      | Move IDX to DA (DA = IDX) |
| 0xB1   | MOV      | DA,IDY     | 1     | 1      | Move IDY to DA (DA = IDY) |
| 0xB2   | MOV      | IDX,DA     | 1     | 1      | Move DA to IDX (IDX = DA) |
| 0xB3   | MOV      | IDY,DA     | 1     | 1      | Move DA to IDY (IDY = DA) |

**Usage Examples:**
```assembly
LDA #42
MOV B,A         ; B = A = 42
MOV C,B         ; C = B = 42
SWP A,B         ; A and B exchange values
SWP DA,DB       ; Exchange 16-bit registers
```

---

## Relative Jump & Index Operations (0xC0-0xCB)

### Relative Jumps

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0xC0   | JR       | offset     | 2     | 2      | Jump relative by signed 8-bit offset |
| 0xC1   | JRZ      | offset     | 2     | 2      | Jump relative if Zero flag set |
| 0xC2   | JRNZ     | offset     | 2     | 2      | Jump relative if Zero flag clear |
| 0xC3   | JRC      | offset     | 2     | 2      | Jump relative if Carry flag set |

### Auto-increment/decrement Operations

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description |
|--------|----------|------------|-------|--------|-------------|
| 0xC4   | LDAIDX+  | -          | 1     | 2      | Load A from [IDX], then IDX++ |
| 0xC5   | LDAIDY+  | -          | 1     | 2      | Load A from [IDY], then IDY++ |
| 0xC6   | STAIDX+  | -          | 1     | 2      | Store A at [IDX], then IDX++ |
| 0xC7   | STAIDY+  | -          | 1     | 2      | Store A at [IDY], then IDY++ |
| 0xC8   | LDAIDX-  | -          | 1     | 2      | Load A from [IDX], then IDX-- |
| 0xC9   | LDAIDY-  | -          | 1     | 2      | Load A from [IDY], then IDY-- |
| 0xCA   | STAIDX-  | -          | 1     | 2      | Store A at [IDX], then IDX-- |
| 0xCB   | STAIDY-  | -          | 1     | 2      | Store A at [IDY], then IDY-- |

**Usage Examples:**
```assembly
; Array copy with auto-increment
LDIDX #SOURCE       ; Point to source array
LDIDY #DEST         ; Point to destination array
LDB #10             ; 10 elements to copy
COPY_LOOP:
    LDAIDX+         ; Load from source, increment source pointer
    STAIDY+         ; Store to dest, increment dest pointer
    DEC B           ; Decrement counter
    JRNZ COPY_LOOP  ; Continue if more elements

; Relative jump example
LOOP:
    DEC B
    JRZ +10         ; Jump forward 10 bytes if B = 0
    JR LOOP         ; Jump back to LOOP
