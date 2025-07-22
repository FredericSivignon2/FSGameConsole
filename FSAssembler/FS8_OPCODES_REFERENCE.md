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
14. [Relative Jump &amp; Index Operations (0xC0-0xCB)](#relative-jump--index-operations-0xc0-0xcb)
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

| Address Range | Size | Description                         | Notes              |
| ------------- | ---- | ----------------------------------- | ------------------ |
| 0x0000-0x7FFF | 32KB | **RAM Program Space**               | User programs      |
| 0x8000-0xBFFF | 16KB | **Video Bitmap Memory (CPC-style)** | 320x200, 16 colors |
| 0xC000-0xF3FF | 13KB | **Extended RAM**                    | Additional storage |
| 0xF400-0xF7FF | 1KB  | **ROM BOOT** _(Read-only)_          | System boot code   |
| 0xF800-0xFFFF | 2KB  | **BIOS/System**                     | System functions   |

### Little-Endian Format

All 16-bit values are stored with the **low byte first**:

- **Example**: 0x1234 → Memory: [0x34] [0x12]

---

## Basic Instructions (0x00-0x01)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                     |
| ------ | -------- | ---------- | ----- | ------ | ------------------------------- |
| 0x00   | NOP      | -          | 1     | 1      | No operation. Advances PC by 1. |
| 0x01   | HALT     | -          | 1     | 1      | Stop processor execution.       |

**Usage Examples:**

```assembly
NOP         ; Wait one cycle
HALT        ; Stop program execution
```

---

## 8-bit Load Instructions (0x10-0x1E)

### Immediate Load Instructions

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                 |
| ------ | -------- | ---------- | ----- | ------ | --------------------------- |
| 0x10   | LDA      | #imm8      | 2     | 2      | Load immediate value into A |
| 0x11   | LDB      | #imm8      | 2     | 2      | Load immediate value into B |
| 0x12   | LDC      | #imm8      | 2     | 2      | Load immediate value into C |
| 0x13   | LDD      | #imm8      | 2     | 2      | Load immediate value into D |
| 0x14   | LDE      | #imm8      | 2     | 2      | Load immediate value into E |
| 0x15   | LDF      | #imm8      | 2     | 2      | Load immediate value into F |

### 16-bit Load Instructions

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                    |
| ------ | -------- | ---------- | ----- | ------ | ------------------------------ |
| 0x16   | LDDA     | #imm16     | 3     | 3      | Load 16-bit immediate into DA  |
| 0x17   | LDDB     | #imm16     | 3     | 3      | Load 16-bit immediate into DB  |
| 0x18   | LDDA     | addr       | 3     | 5      | Load DA from memory address    |
| 0x19   | LDDB     | addr       | 3     | 5      | Load DB from memory address    |
| 0x1A   | LDIDX    | #imm16     | 3     | 3      | Load 16-bit immediate into IDX |
| 0x1B   | LDIDY    | #imm16     | 3     | 3      | Load 16-bit immediate into IDY |

### Legacy Load Instructions (0x1C-0x1E)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description           | Notes                   |
| ------ | -------- | ---------- | ----- | ------ | --------------------- | ----------------------- |
| 0x1C   | LDDC     | #imm8      | 2     | 2      | Load immediate into C | _Legacy implementation_ |
| 0x1D   | LDDD     | #imm8      | 2     | 2      | Load immediate into C | _Bug: should load D_    |
| 0x1E   | LDDE     | #imm8      | 2     | 2      | Load immediate into E | _Legacy implementation_ |

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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                       |
| ------ | -------- | ---------- | ----- | ------ | --------------------------------- |
| 0x20   | ADD      | A,B        | 1     | 1      | Add B to A, result in A           |
| 0x21   | SUB      | A,B        | 1     | 1      | Subtract B from A, result in A    |
| 0x22   | ADD16    | DA,DB      | 1     | 2      | Add DB to DA, result in DA        |
| 0x23   | SUB16    | DA,DB      | 1     | 2      | Subtract DB from DA, result in DA |
| 0x24   | INC16    | DA         | 1     | 2      | Increment DA by 1                 |
| 0x25   | DEC16    | DA         | 1     | 2      | Decrement DA by 1                 |
| 0x26   | INC16    | DB         | 1     | 2      | Increment DB by 1                 |
| 0x27   | DEC16    | DB         | 1     | 2      | Decrement DB by 1                 |
| 0x28   | INC      | A          | 1     | 1      | Increment A by 1                  |
| 0x29   | DEC      | A          | 1     | 1      | Decrement A by 1                  |
| 0x2A   | INC      | B          | 1     | 1      | Increment B by 1                  |
| 0x2B   | DEC      | B          | 1     | 1      | Decrement B by 1                  |
| 0x2C   | CMP      | A,B        | 1     | 1      | Compare A with B (A - B)          |
| 0x2D   | INC      | C          | 1     | 1      | Increment C by 1                  |
| 0x2E   | DEC      | C          | 1     | 1      | Decrement C by 1                  |
| 0x2F   | CMP      | A,C        | 1     | 1      | Compare A with C (A - C)          |

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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                |
| ------ | -------- | ---------- | ----- | ------ | -------------------------- |
| 0x30   | AND      | A,B        | 1     | 1      | Bitwise AND of A and B → A |
| 0x31   | OR       | A,B        | 1     | 1      | Bitwise OR of A and B → A  |
| 0x32   | XOR      | A,B        | 1     | 1      | Bitwise XOR of A and B → A |
| 0x33   | NOT      | A          | 1     | 1      | Bitwise NOT of A → A (~A)  |
| 0x34   | SHL      | A          | 1     | 1      | Shift A left 1 bit         |
| 0x35   | SHR      | A          | 1     | 1      | Shift A right 1 bit        |
| 0x36   | AND      | A,C        | 1     | 1      | Bitwise AND of A and C → A |
| 0x37   | OR       | A,C        | 1     | 1      | Bitwise OR of A and C → A  |
| 0x38   | XOR      | A,C        | 1     | 1      | Bitwise XOR of A and C → A |
| 0x39   | SHL      | B          | 1     | 1      | Shift B left 1 bit         |

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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                   |
| ------ | -------- | ---------- | ----- | ------ | ----------------------------- |
| 0x40   | JMP      | addr       | 3     | 3      | Unconditional jump to address |
| 0x41   | JZ       | addr       | 3     | 3      | Jump if Zero flag set         |
| 0x42   | JNZ      | addr       | 3     | 3      | Jump if Zero flag clear       |
| 0x43   | JC       | addr       | 3     | 3      | Jump if Carry flag set        |
| 0x44   | JNC      | addr       | 3     | 3      | Jump if Carry flag clear      |
| 0x45   | JN       | addr       | 3     | 3      | Jump if Negative flag set     |
| 0x46   | JNN      | addr       | 3     | 3      | Jump if Negative flag clear   |

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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                  |
| ------ | -------- | ---------- | ----- | ------ | ---------------------------- |
| 0x50   | STA      | addr       | 3     | 4      | Store A at memory address    |
| 0x51   | STDA     | addr       | 3     | 5      | Store DA at address (16-bit) |
| 0x52   | STDB     | addr       | 3     | 5      | Store DB at address (16-bit) |
| 0x53   | STB      | addr       | 3     | 4      | Store B at memory address    |
| 0x54   | STC      | addr       | 3     | 4      | Store C at memory address    |
| 0x55   | STD      | addr       | 3     | 4      | Store D at memory address    |
| 0x56   | STE      | addr       | 3     | 4      | Store E at memory address    |
| 0x57   | STF      | addr       | 3     | 4      | Store F at memory address    |

**Usage Examples:**

```assembly
LDA #42
STA 0x2000      ; Store A(42) at address 0x2000
LDDA #0x1234
STDA BUFFER     ; Store DA(0x1234) at BUFFER (little-endian)
```

---

## Subroutine Instructions (0x60-0x61)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                |
| ------ | -------- | ---------- | ----- | ------ | -------------------------- |
| 0x60   | CALL     | addr       | 3     | 5      | Call subroutine at address |
| 0x61   | RET      | -          | 1     | 4      | Return from subroutine     |

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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description         |
| ------ | -------- | ---------- | ----- | ------ | ------------------- |
| 0x70   | PUSH     | A          | 1     | 2      | Push A onto stack   |
| 0x71   | POP      | A          | 1     | 2      | Pop from stack to A |
| 0x74   | PUSH     | B          | 1     | 2      | Push B onto stack   |
| 0x75   | POP      | B          | 1     | 2      | Pop from stack to B |
| 0x78   | PUSH     | C          | 1     | 2      | Push C onto stack   |
| 0x79   | POP      | C          | 1     | 2      | Pop from stack to C |
| 0x7A   | PUSH     | D          | 1     | 2      | Push D onto stack   |
| 0x7B   | POP      | D          | 1     | 2      | Pop from stack to D |
| 0x7C   | PUSH     | E          | 1     | 2      | Push E onto stack   |
| 0x7D   | POP      | E          | 1     | 2      | Pop from stack to E |
| 0x7E   | PUSH     | F          | 1     | 2      | Push F onto stack   |
| 0x7F   | POP      | F          | 1     | 2      | Pop from stack to F |

### 16-bit Stack Operations

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                   |
| ------ | -------- | ---------- | ----- | ------ | ----------------------------- |
| 0x72   | PUSH16   | DA         | 1     | 3      | Push DA onto stack (16-bit)   |
| 0x73   | POP16    | DA         | 1     | 3      | Pop from stack to DA (16-bit) |
| 0x76   | PUSH16   | DB         | 1     | 3      | Push DB onto stack (16-bit)   |
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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                |
| ------ | -------- | ---------- | ----- | ------ | -------------------------- |
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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description         |
| ------ | -------- | ---------- | ----- | ------ | ------------------- |
| 0xA0   | MOV      | A,B        | 1     | 1      | Move B to A (A = B) |
| 0xA1   | MOV      | A,C        | 1     | 1      | Move C to A (A = C) |
| 0xA2   | MOV      | B,A        | 1     | 1      | Move A to B (B = A) |
| 0xA3   | MOV      | B,C        | 1     | 1      | Move C to B (B = C) |
| 0xA4   | MOV      | C,A        | 1     | 1      | Move A to C (C = A) |
| 0xA5   | MOV      | C,B        | 1     | 1      | Move B to C (C = B) |

### Register Swaps

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description             |
| ------ | -------- | ---------- | ----- | ------ | ----------------------- |
| 0xA6   | SWP      | A,B        | 1     | 2      | Swap A and B registers  |
| 0xA7   | SWP      | A,C        | 1     | 2      | Swap A and C registers  |
| 0xA8   | SWP      | A,D        | 1     | 2      | Swap A and D registers  |
| 0xA9   | SWP      | A,E        | 1     | 2      | Swap A and E registers  |
| 0xAA   | SWP      | A,F        | 1     | 2      | Swap A and F registers  |
| 0xAB   | SWP      | DA,DB      | 1     | 2      | Swap DA and DB (16-bit) |

### 16-bit Register Operations

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description               |
| ------ | -------- | ---------- | ----- | ------ | ------------------------- |
| 0xAC   | MOV      | DA,DB      | 1     | 1      | Move DB to DA (DA = DB)   |
| 0xAD   | MOV      | DB,DA      | 1     | 1      | Move DA to DB (DB = DA)   |
| 0xAE   | SWP      | DA,IDX     | 1     | 2      | Swap DA and IDX           |
| 0xAF   | SWP      | DA,IDY     | 1     | 2      | Swap DA and IDY           |
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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                          |
| ------ | -------- | ---------- | ----- | ------ | ------------------------------------ |
| 0xC0   | JR       | offset     | 2     | 2      | Jump relative by signed 8-bit offset |
| 0xC1   | JRZ      | offset     | 2     | 2      | Jump relative if Zero flag set       |
| 0xC2   | JRNZ     | offset     | 2     | 2      | Jump relative if Zero flag clear     |
| 0xC3   | JRC      | offset     | 2     | 2      | Jump relative if Carry flag set      |

### Auto-increment/decrement Operations

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                   |
| ------ | -------- | ---------- | ----- | ------ | ----------------------------- |
| 0xC4   | LDAIDX+  | -          | 1     | 2      | Load A from [IDX], then IDX++ |
| 0xC5   | LDAIDY+  | -          | 1     | 2      | Load A from [IDY], then IDY++ |
| 0xC6   | STAIDX+  | -          | 1     | 2      | Store A at [IDX], then IDX++  |
| 0xC7   | STAIDY+  | -          | 1     | 2      | Store A at [IDY], then IDY++  |
| 0xC8   | LDAIDX-  | -          | 1     | 2      | Load A from [IDX], then IDX-- |
| 0xC9   | LDAIDY-  | -          | 1     | 2      | Load A from [IDY], then IDY-- |
| 0xCA   | STAIDX-  | -          | 1     | 2      | Store A at [IDX], then IDX--  |
| 0xCB   | STAIDY-  | -          | 1     | 2      | Store A at [IDY], then IDY--  |

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

; Array traversal
LDIDX #ARRAY        ; Point to start of array
INCIDX              ; Move to next element
INCIDX              ; Move to element 2
DECIDX              ; Back to element 1

; Block operations
LDIDY #BUFFER       ; Point to buffer start
ADDIDY #256         ; Move 256 bytes forward

; Efficient pointer arithmetic
LDIDX #DATA_TABLE
ADDIDX #RECORD_SIZE ; Move to next record
LDAIDX+             ; Read first byte of record
```

---

## Compare Immediate Instructions (0xD0-0xD7) - NEW

**Enhanced Feature**: Direct comparison with immediate values without temporary register usage.

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                            |
| ------ | -------- | ---------- | ----- | ------ | -------------------------------------- |
| 0xD0   | CMP      | A,#imm8    | 2     | 2      | Compare A with immediate 8-bit value   |
| 0xD1   | CMP      | B,#imm8    | 2     | 2      | Compare B with immediate 8-bit value   |
| 0xD2   | CMP      | C,#imm8    | 2     | 2      | Compare C with immediate 8-bit value   |
| 0xD3   | CMP      | D,#imm8    | 2     | 2      | Compare D with immediate 8-bit value   |
| 0xD4   | CMP      | E,#imm8    | 2     | 2      | Compare E with immediate 8-bit value   |
| 0xD5   | CMP      | F,#imm8    | 2     | 2      | Compare F with immediate 8-bit value   |
| 0xD6   | CMP      | DA,#imm16  | 3     | 3      | Compare DA with immediate 16-bit value |
| 0xD7   | CMP      | DB,#imm16  | 3     | 3      | Compare DB with immediate 16-bit value |

**Operation**: Performs subtraction (register - immediate) and updates flags without storing result.

**Flags Updated**: Zero, Carry, Overflow, Negative

**Usage Examples:**

```assembly
; 8-bit immediate comparisons
LDA #100
CMP A,#50       ; Compare A(100) with 50, sets flags
JC GREATER      ; Jump if A > 50 (no carry = A >= immediate)

LDB #42
CMP B,#42       ; Compare B(42) with 42
JZ EQUAL        ; Jump if B == 42 (zero flag set)

; 16-bit immediate comparisons
LDDA #0x1000
CMP DA,#0x0800  ; Compare DA(0x1000) with 0x0800
JNC HIGHER      ; Jump if DA >= 0x0800

; Efficient range checking
LDC #75
CMP C,#50       ; Check if C >= 50
JC TOO_LOW      ; Jump if C < 50
CMP C,#100      ; Check if C <= 100
JNC TOO_HIGH    ; Jump if C > 100
; C is in range [50, 100]
```

---

## Index Register Instructions (0xE0-0xEA)

### Index Register Arithmetic

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description        |
| ------ | -------- | ---------- | ----- | ------ | ------------------ |
| 0xE0   | INCIDX   | -          | 1     | 1      | Increment IDX by 1 |
| 0xE1   | DECIDX   | -          | 1     | 1      | Decrement IDX by 1 |
| 0xE2   | INCIDY   | -          | 1     | 1      | Increment IDY by 1 |
| 0xE3   | DECIDY   | -          | 1     | 1      | Decrement IDY by 1 |

### Index Register Add Immediate

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                 |
| ------ | -------- | ---------- | ----- | ------ | --------------------------- |
| 0xE8   | ADDIDX   | #imm16     | 3     | 3      | Add 16-bit immediate to IDX |
| 0xEA   | ADDIDY   | #imm16     | 3     | 3      | Add 16-bit immediate to IDY |

**Note**: Opcodes 0xE4-0xE7, 0xE9, 0xEB are reserved for future extended index operations.

**Flags Updated**: Zero flag (for INCIDX/DECIDX), Carry flag (for ADDIDX/ADDIDY if overflow)

**Usage Examples:**

```assembly
; Array traversal
LDIDX #ARRAY        ; Point to start of array
INCIDX              ; Move to next element
INCIDX              ; Move to element 2
DECIDX              ; Back to element 1

; Block operations
LDIDY #BUFFER       ; Point to buffer start
ADDIDY #256         ; Move 256 bytes forward

; Efficient pointer arithmetic
LDIDX #DATA_TABLE
ADDIDX #RECORD_SIZE ; Move to next record
LDAIDX+             ; Read first byte of record
```

---

## Advanced Index Transfer (0xF5-0xF9)

**High-performance register operations** for advanced index manipulation.

| Opcode | Mnemonic  | Parameters | Bytes | Cycles | Description                 |
| ------ | --------- | ---------- | ----- | ------ | --------------------------- |
| 0xF5   | MVIDXIDY  | -          | 1     | 1      | Move IDX to IDY (IDY = IDX) |
| 0xF6   | MVIDYIDX  | -          | 1     | 1      | Move IDY to IDX (IDX = IDY) |
| 0xF9   | SWPIDXIDY | -          | 1     | 2      | Swap IDX and IDY registers  |

**Note**: Opcodes 0xF1-0xF4, 0xF7-0xF8 are reserved for extended index transfer operations.

**Flags Updated**: Zero flag (based on destination register value)

**Usage Examples:**

```assembly
; Efficient pointer swapping
LDIDX #BUFFER1      ; IDX points to first buffer
LDIDY #BUFFER2      ; IDY points to second buffer
SWPIDXIDY           ; Swap pointers (IDX↔IDY)

; Save/restore pointer
LDIDX #CURRENT_POS  ; Set working pointer
MVIDXIDY           ; Save position in IDY
; ... use IDX for other operations
MVIDYIDX           ; Restore position to IDX
```

---

## I/O Port Instructions (0x80-0x83)

**Hardware interface** instructions for peripheral communication.

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                      |
| ------ | -------- | ---------- | ----- | ------ | -------------------------------- |
| 0x80   | OUT      | (port),A   | 2     | 3      | Output A to 8-bit I/O port       |
| 0x81   | IN       | A,(port)   | 2     | 3      | Input from 8-bit I/O port to A   |
| 0x82   | OUT16    | (port),DA  | 2     | 4      | Output DA to 16-bit I/O port     |
| 0x83   | IN16     | DA,(port)  | 2     | 4      | Input from 16-bit I/O port to DA |

**Port Numbers**: 0x00-0xFF (256 possible ports)

**Usage Examples:**

```assembly
; Write to hardware port
LDA #0xFF       ; Data to output
OUT (0x20),A    ; Send to port 0x20

; Read from hardware port
IN A,(0x21)     ; Read from port 0x21 into A

; 16-bit I/O operations
LDDA #0x1234    ; 16-bit data
OUT (0x30),DA   ; Send to 16-bit port 0x30
```

---

## Indexed Addressing Instructions (0x86-0x99)

**Advanced memory access** with index register addressing modes.

### Direct Indexed Addressing

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                |
| ------ | -------- | ---------- | ----- | ------ | -------------------------- |
| 0x86   | LDA      | (IDX)      | 1     | 2      | Load A from address in IDX |
| 0x87   | LDB      | (IDX)      | 1     | 2      | Load B from address in IDX |
| 0x88   | LDA      | (IDY)      | 1     | 2      | Load A from address in IDY |
| 0x89   | LDB      | (IDY)      | 1     | 2      | Load B from address in IDY |
| 0x8A   | STA      | (IDX)      | 1     | 2      | Store A at address in IDX  |
| 0x8B   | STB      | (IDX)      | 1     | 2      | Store B at address in IDX  |
| 0x8C   | STA      | (IDY)      | 1     | 2      | Store A at address in IDY  |
| 0x8D   | STB      | (IDY)      | 1     | 2      | Store B at address in IDY  |

### Indexed with Offset Addressing

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                     |
| ------ | -------- | ---------- | ----- | ------ | ------------------------------- |
| 0x90   | LDA      | (IDX+off)  | 2     | 3      | Load A from IDX + signed offset |
| 0x91   | LDB      | (IDX+off)  | 2     | 3      | Load B from IDX + signed offset |
| 0x92   | LDA      | (IDY+off)  | 2     | 3      | Load A from IDY + signed offset |
| 0x93   | LDB      | (IDY+off)  | 2     | 3      | Load B from IDY + signed offset |
| 0x94   | STA      | (IDX+off)  | 2     | 3      | Store A at IDX + signed offset  |
| 0x95   | STB      | (IDX+off)  | 2     | 3      | Store B at IDX + signed offset  |
| 0x96   | STA      | (IDY+off)  | 2     | 3      | Store A at IDY + signed offset  |
| 0x97   | STB      | (IDY+off)  | 2     | 3      | Store B at IDY + signed offset  |

**Note**: In current CPU8Bit.cs implementation, opcodes 0x90-0x95 are used for direct memory load instructions (LDA addr, LDB addr, etc.). The indexed addressing with offset described here represents the intended design but is not currently implemented. Only direct indexed addressing (0x86-0x8D) is currently supported.

**Offset Range**: -128 to +127 (signed 8-bit)

**Usage Examples:**

```assembly
; Direct indexed access
LDIDX #DATA_ARRAY   ; Point to array
LDA (IDX)           ; Load first element
INCIDX              ; Point to next element
LDB (IDX)           ; Load second element

; Note: Indexed with offset examples below show intended functionality
; but are not currently implemented in CPU8Bit.cs

; Indexed with offset (not currently implemented)
LDIDY #STRUCT_BASE  ; Point to structure
LDA (IDY+0)         ; Load first field
LDB (IDY+1)         ; Load second field
LDC (IDY+5)         ; Load field at offset 5

; Record processing (not currently implemented)
LDIDX #RECORDS      ; Point to record array
LDA (IDX+0)         ; Load record ID
LDB (IDX+1)         ; Load record type
STA (IDX+10)        ; Store processed flag
```

---

## Extended Instructions (0xFE-0xFF)

**Future expansion** opcodes for extended instruction sets.

| Opcode | Mnemonic | Parameters | Bytes  | Cycles | Description                |
| ------ | -------- | ---------- | ------ | ------ | -------------------------- |
| 0xFE   | EXT1     | varies     | varies | varies | Extended instruction set 1 |
| 0xFF   | EXT2     | varies     | varies | varies | Extended instruction set 2 |

**Current Status**: Reserved for future CPU enhancements

**Potential Future Uses**:

- Floating point operations
- Advanced SIMD instructions
- Hardware acceleration commands
- Extended addressing modes
- Interrupt control instructions

---

## Programming Examples

### Example 1: Array Sum Calculation

Calculate the sum of 10 bytes in an array using index registers.

```assembly
START:                      ; pos  size
    LDIDX #ARRAY           ; 0    3   Load array address
    LDA #0                 ; 3    2   Initialize sum to 0
    LDB #10                ; 5    2   Load counter (10 elements)

SUM_LOOP:                   ;  -   0   Loop label (position 7)
    LDC (IDX)              ; 7    1   Load array element into C
    ADD A,C                ; 8    1   Add element to sum (A = A + C)
    INCIDX                 ; 9    1   Move to next array element
    DEC B                  ; 10   1   Decrement counter
    JNZ SUM_LOOP           ; 11   3   Continue if not zero (jump to 7)

    STA RESULT             ; 14   3   Store final sum
    HALT                   ; 17   1   End program

ARRAY:      DB 5,10,15,20,25,30,35,40,45,50  ; Test data
RESULT:     DB 0           ; Storage for result
```

**Expected result**: Sum = 275 (0x113)

---

### Example 2: String Copy with Auto-Increment

Copy a null-terminated string from source to destination.

```assembly
STRING_COPY:                ; pos  size
    LDIDX #SOURCE          ; 0    3   Point IDX to source string
    LDIDY #DEST            ; 3    3   Point IDY to destination

COPY_LOOP:                  ;  -   0   Loop label (position 6)
    LDAIDX+                ; 6    1   Load byte from source, increment IDX
    STAIDY+                ; 7    1   Store byte to dest, increment IDY
    CMP A,#0               ; 8    2   Check for null terminator
    JNZ COPY_LOOP          ; 10   3   Continue if not null (jump to 6)

    HALT                   ; 13   1   End program

SOURCE:     DB "Hello World!", 0    ; Source string with null terminator
DEST:       DB 0,0,0,0,0,0,0,0,0,0,0,0,0,0  ; Destination buffer
```

---

### Example 3: 16-bit Addition with Compare Immediate

Demonstrate 16-bit operations and immediate comparisons.

```assembly
MATH_DEMO:                  ; pos  size
    LDDA #1000             ; 0    3   Load first 16-bit number
    LDDB #2000             ; 3    3   Load second 16-bit number
    ADD16 DA,DB            ; 6    1   DA = DA + DB = 3000

    ; Check if result is greater than 2500
    CMP DA,#2500           ; 7    3   Compare DA with 2500
    JNC LARGE_RESULT       ; 10   3   Jump if DA >= 2500

    ; Small result path
    LDA #'S'               ; 13   2   Load 'S' for Small
    JMP PRINT_RESULT       ; 15   3   Jump to print

LARGE_RESULT:               ;  -   0   Label (position 18)
    LDA #'L'               ; 18   2   Load 'L' for Large

PRINT_RESULT:               ;  -   0   Label (position 20)
    LDB #1                 ; 20   2   System call: print character
    SYS                    ; 22   1   Execute system call
    HALT                   ; 23   1   End program
```

---

### Example 4: System Call Demonstration

Show various system calls for display output.

```assembly
SYSCALL_DEMO:               ; pos  size
    ; Clear screen with blue background
    LDA #3                 ; 0    2   SYSCALL_CLEAR_SCREEN
    LDB #1                 ; 2    2   Blue color
    SYS                    ; 4    1   Execute system call

    ; Set text color to yellow on blue
    LDA #5                 ; 5    2   SYSCALL_SET_COLOR
    LDB #14                ; 7    2   Yellow foreground
    LDC #1                 ; 9    2   Blue background
    SYS                    ; 11   1   Execute system call

    ; Set cursor position to (5, 3)
    LDA #4                 ; 12   2   SYSCALL_SET_CURSOR
    LDB #5                 ; 14   2   X = 5
    LDC #3                 ; 16   2   Y = 3
    SYS                    ; 18   1   Execute system call

    ; Print "FS8" character by character
    LDA #1                 ; 19   2   SYSCALL_PRINT_CHAR
    LDB #'F'               ; 21   2   Character 'F'
    SYS                    ; 23   1   Execute system call

    LDA #1                 ; 24   2   SYSCALL_PRINT_CHAR
    LDB #'S'               ; 26   2   Character 'S'
    SYS                    ; 28   1   Execute system call

    LDA #1                 ; 29   2   SYSCALL_PRINT_CHAR
    LDB #'8'               ; 31   2   Character '8'
    SYS                    ; 33   1   Execute system call

    HALT                   ; 34   1   End program
```

---

### Example 5: I/O Port Communication

Demonstrate hardware port communication.

```assembly
IO_DEMO:                    ; pos  size
    ; Read status from port 0x20
    IN A,(0x20)            ; 0    2   Read from port 0x20
    CMP A,#0               ; 2    2   Check if zero
    JZ NO_DATA             ; 4    3   Jump if no data available

    ; Data available, read it from port 0x21
    IN B,(0x21)            ; 7    2   Read data from port 0x21

    ; Process data (add 1)
    INC B                  ; 9    1   Process data

    ; Send processed data to port 0x22
    OUT (0x22),B           ; 10   2   Output to port 0x22
    JMP END_PROGRAM        ; 12   3   Jump to end

NO_DATA:                    ;  -   0   Label (position 15)
    ; Send "no data" signal (0xFF) to port 0x22
    LDA #0xFF              ; 15   2   Load "no data" value
    OUT (0x22),A           ; 17   2   Output to port 0x22

END_PROGRAM:                ;  -   0   Label (position 19)
    HALT                   ; 19   1   End program
```

---

## Instruction Timing Reference

### Summary by Category

| Category               | Instructions                | Typical Cycles |
| ---------------------- | --------------------------- | -------------- |
| **Basic**              | NOP, HALT                   | 1              |
| **Load Immediate**     | LDA #imm, LDB #imm, etc.    | 2              |
| **Load Memory**        | LDA addr, LDB addr, etc.    | 4              |
| **Load 16-bit**        | LDDA #imm16, LDDB #imm16    | 3              |
| **Arithmetic 8-bit**   | ADD, SUB, INC, DEC          | 1              |
| **Arithmetic 16-bit**  | ADD16, SUB16, INC16, DEC16  | 2              |
| **Logic**              | AND, OR, XOR, NOT, SHL, SHR | 1              |
| **Compare**            | CMP reg,reg                 | 1              |
| **Compare Immediate**  | CMP reg,#imm                | 2-3            |
| **Jump Absolute**      | JMP, JZ, JNZ, etc.          | 3              |
| **Jump Relative**      | JR, JRZ, JRNZ, JRC          | 2              |
| **Store**              | STA addr, STB addr, etc.    | 4              |
| **Store 16-bit**       | STDA addr, STDB addr        | 5              |
| **Stack 8-bit**        | PUSH/POP                    | 2              |
| **Stack 16-bit**       | PUSH16/POP16                | 3              |
| **Register Transfer**  | MOV, SWP                    | 1-2            |
| **Subroutines**        | CALL, RET                   | 5-4            |
| **Index Auto-Inc/Dec** | LDAIDX+, STAIDY-, etc.      | 2              |
| **Index Arithmetic**   | INCIDX, ADDIDX, etc.        | 1-3            |
| **Index Transfer**     | MVIDXIDY, SWPIDXIDY         | 1-2            |
| **I/O Ports**          | IN, OUT                     | 3-4            |
| **System Calls**       | SYS                         | 8              |

---

## Architecture Notes

### Little-Endian Memory Organization

All 16-bit values are stored with the low byte first:

- Address 0x1000: Low byte (0x34)
- Address 0x1001: High byte (0x12)
- Result: 16-bit value 0x1234

### Flag Behavior

**Zero Flag**: Set when result equals zero
**Carry Flag**: Set on arithmetic overflow or borrow
**Negative Flag**: Set when bit 7 (8-bit) or bit 15 (16-bit) is set
**Overflow Flag**: Set on signed arithmetic overflow

### Index Register Usage

Index registers (IDX, IDY) provide powerful addressing capabilities:

- **Auto-increment/decrement**: Efficient for array processing
- **Pointer arithmetic**: Direct manipulation with ADDIDX/ADDIDY
- **Register transfer**: Move pointers between registers
- **Future expansion**: Reserved opcodes for advanced indexing modes

### System Integration

The FS8 processor integrates seamlessly with:

- **Memory-mapped I/O**: Ports 0x00-0xFF
- **Video system**: CPC-style bitmap memory at 0x8000
- **System calls**: Hardware abstraction layer
- **ROM services**: Boot code and system routines

---

This comprehensive reference documents all 120+ instructions of the FS8 Enhanced processor, providing complete coverage of the instruction set architecture for programmers and system developers.
