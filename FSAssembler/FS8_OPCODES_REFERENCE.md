# FS8 Processor - Complete Opcode Reference

## Table of Contents

1. [Basic Instructions (0x00-0x01)](#basic-instructions-0x00-0x01)
2. [8-bit Load Instructions (0x10-0x15)](#8-bit-load-instructions-0x10-0x15)
3. [16-bit Load Instructions (0x16-0x1F)](#16-bit-load-instructions-0x16-0x1f)
4. [Arithmetic Instructions (0x20-0x2F)](#arithmetic-instructions-0x20-0x2f)
5. [Logical Instructions (0x30-0x3F)](#logical-instructions-0x30-0x3f)
6. [Jump Instructions (0x40-0x4F)](#jump-instructions-0x40-0x4f)
7. [Store Instructions (0x50-0x5F)](#store-instructions-0x50-0x5f)
8. [Subroutine Instructions (0x60-0x6F)](#subroutine-instructions-0x60-0x6f)
9. [Stack Instructions (0x70-0x7F)](#stack-instructions-0x70-0x7f)
10. [Memory Load Instructions (0x90-0x95)](#memory-load-instructions-0x90-0x95)
11. [Register Transfer Instructions (0xA0-0xAF)](#register-transfer-instructions-0xa0-0xaf)
12. [Relative Jump Instructions (0xC0-0xCF)](#relative-jump-instructions-0xc0-0xcf)
13. [Index Register Instructions (0xE0-0xFF)](#index-register-instructions-0xe0-0xff)

---

## Basic Instructions (0x00-0x01)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                                           |
| ------ | -------- | ---------- | ----- | ------ | ------------------------------------------------------------------------------------- |
| 0x00   | NOP      | -          | 1     | 1      | No operation. Does nothing, advances PC by 1. Used for timing delays or padding.      |
| 0x01   | HALT     | -          | 1     | 1      | Stop processor execution. Sets IsRunning = false, can only be restarted with Start(). |

**Usage Examples:**

```assembly
NOP         ; Wait one cycle
HALT        ; Stop program execution
```

---

## 8-bit Load Instructions (0x10-0x15)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                    |
| ------ | -------- | ---------- | ----- | ------ | -------------------------------------------------------------- |
| 0x10   | LDA      | #imm8      | 2     | 2      | Load 8-bit immediate value into register A. Updates Zero flag. |
| 0x11   | LDB      | #imm8      | 2     | 2      | Load 8-bit immediate value into register B. Updates Zero flag. |
| 0x12   | LDC      | #imm8      | 2     | 2      | Load 8-bit immediate value into register C. Updates Zero flag. |
| 0x13   | LDD      | #imm8      | 2     | 2      | Load 8-bit immediate value into register D. Updates Zero flag. |
| 0x14   | LDE      | #imm8      | 2     | 2      | Load 8-bit immediate value into register E. Updates Zero flag. |
| 0x15   | LDF      | #imm8      | 2     | 2      | Load 8-bit immediate value into register F. Updates Zero flag. |

**Supported Value Formats:**

- Decimal: `42`, `255`
- Hexadecimal: `0xFF`, `$FF`
- Character: `'A'`, `'Z'`

**Usage Examples:**

```assembly
LDA #42         ; Load decimal 42 into A
LDB #0xFF       ; Load hex FF into B
LDC #$80        ; Load hex 80 into C ($ prefix)
LDD #'A'        ; Load ASCII value of 'A' (65) into D
```

---

## 16-bit Load Instructions (0x16-0x1F)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                         |
| ------ | -------- | ---------- | ----- | ------ | ------------------------------------------------------------------- |
| 0x16   | LDDA     | #imm16     | 3     | 3      | Load 16-bit immediate value into register DA. Updates Zero flag.    |
| 0x17   | LDDB     | #imm16     | 3     | 3      | Load 16-bit immediate value into register DB. Updates Zero flag.    |
| 0x18   | LDDA     | addr       | 3     | 5      | Load DA from 16-bit value stored at memory address (little-endian). |
| 0x19   | LDDB     | addr       | 3     | 5      | Load DB from 16-bit value stored at memory address (little-endian). |
| 0x1A   | LDIDX    | #imm16     | 3     | 3      | Load 16-bit immediate value into IDX register. Updates Zero flag.   |
| 0x1C   | LDIDY    | #imm16     | 3     | 3      | Load 16-bit immediate value into IDY register. Updates Zero flag.   |

**Note:** All 16-bit values are stored in little-endian format (low byte first, high byte second).

**Usage Examples:**

```assembly
LDDA #0x1234    ; Load 0x1234 into DA
LDDB #$ABCD     ; Load 0xABCD into DB
LDDA 0x2000     ; Load 16-bit value from address 0x2000 into DA
LDIDX #0x8000   ; Load 0x8000 into IDX index register
LDIDY #BUFFER   ; Load address of BUFFER label into IDY
```

---

## Arithmetic Instructions (0x20-0x2F)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                        |
| ------ | -------- | ---------- | ----- | ------ | ------------------------------------------------------------------ |
| 0x20   | ADD      | -          | 1     | 1      | Add B to A, result in A. Updates all flags.                        |
| 0x21   | SUB      | -          | 1     | 1      | Subtract B from A, result in A. Updates all flags.                 |
| 0x22   | ADD16    | -          | 1     | 2      | Add DB to DA, result in DA. Updates all flags.                     |
| 0x23   | SUB16    | -          | 1     | 2      | Subtract DB from DA, result in DA. Updates all flags.              |
| 0x24   | INC16    | DA         | 1     | 2      | Increment 16-bit register DA by 1. Updates flags.                  |
| 0x25   | DEC16    | DA         | 1     | 2      | Decrement 16-bit register DA by 1. Updates flags.                  |
| 0x26   | INC16    | DB         | 1     | 2      | Increment 16-bit register DB by 1. Updates flags.                  |
| 0x27   | DEC16    | DB         | 1     | 2      | Decrement 16-bit register DB by 1. Updates flags.                  |
| 0x28   | INC      | A          | 1     | 1      | Increment register A by 1. Updates flags.                          |
| 0x29   | DEC      | A          | 1     | 1      | Decrement register A by 1. Updates flags.                          |
| 0x2A   | INC      | B          | 1     | 1      | Increment register B by 1. Updates flags.                          |
| 0x2B   | DEC      | B          | 1     | 1      | Decrement register B by 1. Updates flags.                          |
| 0x2C   | CMP      | A,B        | 1     | 1      | Compare A with B (A - B). Updates flags, doesn't modify registers. |
| 0x2D   | INC      | C          | 1     | 1      | Increment register C by 1. Updates flags.                          |
| 0x2E   | DEC      | C          | 1     | 1      | Decrement register C by 1. Updates flags.                          |
| 0x2F   | CMP      | A,C        | 1     | 1      | Compare A with C (A - C). Updates flags, doesn't modify registers. |

**Flags Updated:** Zero, Carry, Overflow, Negative
**Usage Examples:**

```assembly
LDA #10
LDB #5
ADD             ; A = A + B = 15
SUB             ; A = A - B = 5
INC A           ; A = A + 1 = 6
CMP A,B         ; Compare A(6) with B(5), sets flags
```

---

## Logical Instructions (0x30-0x3F)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                           |
| ------ | -------- | ---------- | ----- | ------ | --------------------------------------------------------------------- |
| 0x30   | AND      | A,B        | 1     | 1      | Bitwise AND of A and B, result in A. Updates Zero and Negative flags. |
| 0x31   | OR       | A,B        | 1     | 1      | Bitwise OR of A and B, result in A. Updates Zero and Negative flags.  |
| 0x32   | XOR      | A,B        | 1     | 1      | Bitwise XOR of A and B, result in A. Updates Zero and Negative flags. |
| 0x33   | NOT      | -          | 1     | 1      | Bitwise NOT of A, result in A (~A). Updates Zero and Negative flags.  |
| 0x34   | SHL      | A          | 1     | 1      | Shift Left A by 1 bit. MSB goes to Carry, LSB becomes 0.              |
| 0x35   | SHR      | -          | 1     | 1      | Shift Right A by 1 bit. LSB goes to Carry, MSB becomes 0.             |
| 0x36   | AND      | A,C        | 1     | 1      | Bitwise AND of A and C, result in A. Updates Zero and Negative flags. |
| 0x37   | OR       | A,C        | 1     | 1      | Bitwise OR of A and C, result in A. Updates Zero and Negative flags.  |
| 0x38   | XOR      | A,C        | 1     | 1      | Bitwise XOR of A and C, result in A. Updates Zero and Negative flags. |
| 0x39   | SHL      | B          | 1     | 1      | Shift Left B by 1 bit. MSB goes to Carry, LSB becomes 0.              |

**Usage Examples:**

```assembly
LDA #0xFF
LDB #0x0F
AND A,B         ; A = 0xFF & 0x0F = 0x0F
OR A,B          ; A = 0x0F | 0x0F = 0x0F
XOR A,B         ; A = 0x0F ^ 0x0F = 0x00
NOT             ; A = ~0x00 = 0xFF
SHL A           ; A = 0xFF << 1 = 0xFE, Carry = 1
```

---

## Jump Instructions (0x40-0x4F)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                              |
| ------ | -------- | ---------- | ----- | ------ | -------------------------------------------------------- |
| 0x40   | JMP      | addr       | 3     | 3      | Unconditional jump to 16-bit address. Sets PC = address. |
| 0x41   | JZ       | addr       | 3     | 3      | Jump to address if Zero flag is set.                     |
| 0x42   | JNZ      | addr       | 3     | 3      | Jump to address if Zero flag is clear.                   |
| 0x43   | JC       | addr       | 3     | 3      | Jump to address if Carry flag is set.                    |
| 0x44   | JNC      | addr       | 3     | 3      | Jump to address if Carry flag is clear.                  |
| 0x45   | JN       | addr       | 3     | 3      | Jump to address if Negative flag is set.                 |
| 0x46   | JNN      | addr       | 3     | 3      | Jump to address if Negative flag is clear.               |

**Address Formats:**

- Hexadecimal: `0x1000`, `$2000`
- Decimal: `4096`
- Labels: `LOOP`, `END`

**Usage Examples:**

```assembly
JMP 0x1000      ; Jump to address 0x1000
JMP START       ; Jump to label START
CMP A,B
JZ EQUAL        ; Jump if A == B
JC OVERFLOW     ; Jump if last operation caused carry
```

---

## Store Instructions (0x50-0x5F)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                 |
| ------ | -------- | ---------- | ----- | ------ | ----------------------------------------------------------- |
| 0x50   | STA      | addr       | 3     | 4      | Store register A at 16-bit memory address.                  |
| 0x51   | STDA     | addr       | 3     | 5      | Store 16-bit register DA at memory address (little-endian). |
| 0x52   | STDB     | addr       | 3     | 5      | Store 16-bit register DB at memory address (little-endian). |
| 0x53   | STB      | addr       | 3     | 4      | Store register B at 16-bit memory address.                  |
| 0x54   | STC      | addr       | 3     | 4      | Store register C at 16-bit memory address.                  |
| 0x55   | STD      | addr       | 3     | 4      | Store register D at 16-bit memory address.                  |

**Usage Examples:**

```assembly
LDA #42
STA 0x2000      ; Store A(42) at address 0x2000
LDDA #0x1234
STDA BUFFER     ; Store DA(0x1234) at BUFFER address (little-endian)
```

---

## Subroutine Instructions (0x60-0x6F)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                     |
| ------ | -------- | ---------- | ----- | ------ | --------------------------------------------------------------- |
| 0x60   | CALL     | addr       | 3     | 5      | Call subroutine. Push return address to stack, jump to address. |
| 0x61   | RET      | -          | 1     | 4      | Return from subroutine. Pop return address from stack to PC.    |

**Stack Operation (little-endian):**

- CALL: SP -= 2, store return address at [SP] and [SP+1]
- RET: read address from [SP] and [SP+1], SP += 2

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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                      |
| ------ | -------- | ---------- | ----- | ------ | ---------------------------------------------------------------- |
| 0x70   | PUSH     | A          | 1     | 2      | Push register A onto stack. SP--, [SP] = A.                      |
| 0x71   | POP      | A          | 1     | 2      | Pop from stack to register A. A = [SP], SP++. Updates Zero flag. |
| 0x72   | PUSH16   | DA         | 1     | 3      | Push 16-bit register DA onto stack (little-endian). SP -= 2.     |
| 0x73   | POP16    | DA         | 1     | 3      | Pop from stack to 16-bit register DA (little-endian). SP += 2.   |
| 0x74   | PUSH     | B          | 1     | 2      | Push register B onto stack. SP--, [SP] = B.                      |
| 0x75   | POP      | B          | 1     | 2      | Pop from stack to register B. B = [SP], SP++. Updates Zero flag. |
| 0x76   | PUSH16   | DB         | 1     | 3      | Push 16-bit register DB onto stack (little-endian). SP -= 2.     |
| 0x77   | POP16    | DB         | 1     | 3      | Pop from stack to 16-bit register DB (little-endian). SP += 2.   |
| 0x78   | PUSH     | C          | 1     | 2      | Push register C onto stack. SP--, [SP] = C.                      |
| 0x79   | POP      | C          | 1     | 2      | Pop from stack to register C. C = [SP], SP++. Updates Zero flag. |
| 0x7A   | PUSH     | D          | 1     | 2      | Push register D onto stack. SP--, [SP] = D.                      |
| 0x7B   | POP      | D          | 1     | 2      | Pop from stack to register D. D = [SP], SP++. Updates Zero flag. |
| 0x7C   | PUSH     | E          | 1     | 2      | Push register E onto stack. SP--, [SP] = E.                      |
| 0x7D   | POP      | E          | 1     | 2      | Pop from stack to register E. E = [SP], SP++. Updates Zero flag. |
| 0x7E   | PUSH     | F          | 1     | 2      | Push register F onto stack. SP--, [SP] = F.                      |
| 0x7F   | POP      | F          | 1     | 2      | Pop from stack to register F. F = [SP], SP++. Updates Zero flag. |

**Stack Notes:**

- Stack grows downward from 0xFFFF
- 16-bit values stored in little-endian format
- Always check stack pointer doesn't underflow/overflow

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

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                    |
| ------ | -------- | ---------- | ----- | ------ | -------------------------------------------------------------- |
| 0x90   | LDA      | addr       | 3     | 4      | Load register A from 16-bit memory address. Updates Zero flag. |
| 0x91   | LDB      | addr       | 3     | 4      | Load register B from 16-bit memory address. Updates Zero flag. |
| 0x92   | LDC      | addr       | 3     | 4      | Load register C from 16-bit memory address. Updates Zero flag. |
| 0x93   | LDD      | addr       | 3     | 4      | Load register D from 16-bit memory address. Updates Zero flag. |
| 0x94   | LDE      | addr       | 3     | 4      | Load register E from 16-bit memory address. Updates Zero flag. |
| 0x95   | LDF      | addr       | 3     | 4      | Load register F from 16-bit memory address. Updates Zero flag. |

**Note:** These differ from immediate loads (0x10-0x15) by loading from memory instead of immediate values.

**Usage Examples:**

```assembly
LDA 0x2000      ; Load A from memory address 0x2000
LDB VARIABLE    ; Load B from address of VARIABLE label
```

---

## Register Transfer Instructions (0xA0-0xAF)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                     |
| ------ | -------- | ---------- | ----- | ------ | --------------------------------------------------------------- |
| 0xA0   | MOV      | A,B        | 1     | 1      | Move B to A (A = B). Updates Zero flag based on A.              |
| 0xA1   | MOV      | A,C        | 1     | 1      | Move C to A (A = C). Updates Zero flag based on A.              |
| 0xA2   | MOV      | B,A        | 1     | 1      | Move A to B (B = A). Updates Zero flag based on B.              |
| 0xA3   | MOV      | B,C        | 1     | 1      | Move C to B (B = C). Updates Zero flag based on B.              |
| 0xA4   | MOV      | C,A        | 1     | 1      | Move A to C (C = A). Updates Zero flag based on C.              |
| 0xA5   | MOV      | C,B        | 1     | 1      | Move B to C (C = B). Updates Zero flag based on C.              |
| 0xA6   | SWP      | A,B        | 1     | 2      | Swap A and B registers. Updates Zero flag based on new A value. |
| 0xA7   | SWP      | A,C        | 1     | 2      | Swap A and C registers. Updates Zero flag based on new A value. |

**Usage Examples:**

```assembly
LDA #42
MOV B,A         ; B = A = 42
MOV C,B         ; C = B = 42
SWP A,B         ; A and B exchange values
```

---

## Relative Jump Instructions (0xC0-0xCF)

| Opcode | Mnemonic | Parameters | Bytes | Cycles | Description                                                       |
| ------ | -------- | ---------- | ----- | ------ | ----------------------------------------------------------------- |
| 0xC0   | JR       | offset     | 2     | 2      | Jump relative by signed 8-bit offset (-128 to +127).              |
| 0xC1   | JRZ      | offset     | 2     | 2      | Jump relative if Zero flag is set.                                |
| 0xC2   | JRNZ     | offset     | 2     | 2      | Jump relative if Zero flag is clear.                              |
| 0xC3   | JRC      | offset     | 2     | 2      | Jump relative if Carry flag is set.                               |
| 0xC4   | LDAIDX+  | -          | 1     | 2      | Load A from [IDX], then increment IDX. Auto-increment addressing. |
| 0xC5   | LDAIDY+  | -          | 1     | 2      | Load A from [IDY], then increment IDY. Auto-increment addressing. |
| 0xC6   | STAIDX+  | -          | 1     | 2      | Store A at [IDX], then increment IDX. Auto-increment addressing.  |
| 0xC7   | STAIDY+  | -          | 1     | 2      | Store A at [IDY], then increment IDY. Auto-increment addressing.  |
| 0xC8   | LDAIDX-  | -          | 1     | 2      | Load A from [IDX], then decrement IDX. Auto-decrement addressing. |
| 0xC9   | LDAIDY-  | -          | 1     | 2      | Load A from [IDY], then decrement IDY. Auto-decrement addressing. |
| 0xCA   | STAIDX-  | -          | 1     | 2      | Store A at [IDX], then decrement IDX. Auto-decrement addressing.  |
| 0xCB   | STAIDY-  | -          | 1     | 2      | Store A at [IDY], then decrement IDY. Auto-decrement addressing.  |

**Relative Jump Notes:**

- Offset range: -128 to +127 bytes from current PC
- Useful for loops and short branches
- More efficient than absolute jumps for nearby targets

**Auto-increment/decrement Notes:**

- Perfect for array operations and data processing
- Operation happens AFTER memory access
- Only available with IDX/IDY and register A

**Usage Examples:**

```assembly
LOOP:
    DEC B
    JRZ +10         ; Jump forward 10 bytes if B = 0
    JR LOOP         ; Jump back to LOOP (assembler calculates offset)

; Array copy using auto-increment
LDIDX #SOURCE       ; Point to source array
LDIDY #DEST         ; Point to destination array
LDB #10             ; 10 elements to copy
COPY_LOOP:
    LDAIDX+         ; Load from source, increment source pointer
    STAIDY+         ; Store to dest, increment dest pointer
    DEC B           ; Decrement counter
    JRNZ COPY_LOOP  ; Continue if more elements
```

---

## Index Register Instructions (0xE0-0xFF)

| Opcode | Mnemonic  | Parameters | Bytes | Cycles | Description                                                     |
| ------ | --------- | ---------- | ----- | ------ | --------------------------------------------------------------- |
| 0xE0   | INCIDX    | -          | 1     | 2      | Increment IDX register by 1. Updates Zero flag.                 |
| 0xE1   | DECIDX    | -          | 1     | 2      | Decrement IDX register by 1. Updates Zero flag.                 |
| 0xE2   | INCIDY    | -          | 1     | 2      | Increment IDY register by 1. Updates Zero flag.                 |
| 0xE3   | DECIDY    | -          | 1     | 2      | Decrement IDY register by 1. Updates Zero flag.                 |
| 0xE8   | ADDIDX    | #imm16     | 3     | 4      | Add 16-bit immediate to IDX. Updates Zero and Carry flags.      |
| 0xEA   | ADDIDY    | #imm16     | 3     | 4      | Add 16-bit immediate to IDY. Updates Zero and Carry flags.      |
| 0xF0   | SYS       | -          | 1     | 8      | System call. Invokes SystemCallManager with register values.    |
| 0xF5   | MVIDXIDY  | -          | 1     | 2      | Move IDX to IDY (IDY = IDX). Updates Zero flag based on IDY.    |
| 0xF6   | MVIDYIDX  | -          | 1     | 2      | Move IDY to IDX (IDX = IDY). Updates Zero flag based on IDX.    |
| 0xF9   | SWPIDXIDY | -          | 1     | 3      | Swap IDX and IDY registers. Updates Zero flag based on new IDX. |

**Index Register Usage:**

- IDX and IDY are 16-bit addressing registers
- Ideal for array indexing, data structure access
- Can address entire 64KB memory space
- Support arithmetic operations and transfers

**System Call (SYS):**

- Invokes external system functionality
- Register values passed as parameters
- Used for I/O, graphics, sound, etc.
- Implementation depends on SystemCallManager

**Usage Examples:**

```assembly
LDIDX #0x8000       ; Point to video memory
INCIDX              ; Move to next byte
ADDIDX #320         ; Move to next row (320 bytes)
MVIDXIDY            ; Copy IDX pointer to IDY
SWPIDXIDY           ; Exchange pointers

; System call example
LDA #1              ; System call number
LDB #65             ; ASCII 'A'
SYS                 ; Call system function (e.g., print character)
```

---

## Indexed Addressing Modes (Assembler Only)

The assembler supports special indexed addressing syntax that gets translated to appropriate opcodes:

**Direct Indexed Addressing:**

```assembly
LDA (IDX)       ; Translates to opcode 0x86 - Load A from [IDX]
LDB (IDX)       ; Translates to opcode 0x87 - Load B from [IDX]
LDA (IDY)       ; Translates to opcode 0x88 - Load A from [IDY]
LDB (IDY)       ; Translates to opcode 0x89 - Load B from [IDY]
STA (IDX)       ; Translates to opcode 0x8A - Store A at [IDX]
STB (IDX)       ; Translates to opcode 0x8B - Store B at [IDX]
STA (IDY)       ; Translates to opcode 0x8C - Store A at [IDY]
STB (IDY)       ; Translates to opcode 0x8D - Store B at [IDY]
```

**Indexed with Offset:**

```assembly
LDA (IDX+5)     ; Translates to opcode 0x90 + offset byte
LDB (IDX-10)    ; Translates to opcode 0x91 + offset byte
STA (IDY+127)   ; Translates to opcode 0x96 + offset byte
STB (IDY-128)   ; Translates to opcode 0x97 + offset byte
```

**Offset Range:** -128 to +127 (signed 8-bit)

---

## Memory Layout & Addressing

**Memory Map:**

- 0x0000-0x7FFF: RAM program space (32KB)
- 0x8000-0xBFFF: Video memory bitmap (16KB)
- 0xC000-0xF3FF: Extended RAM (13KB)
- 0xF400-0xF7FF: ROM BOOT (1KB) - Read-only
- 0xF800-0xFFFF: BIOS/System (2KB)

**Little-Endian Storage:**
All 16-bit values stored as [low byte][high byte]:

- 0x1234 stored as: 0x34, 0x12
- Applies to addresses, immediate values, stack operations

---

## Status Register Flags

**Zero Flag (Z):** Set when result is zero
**Carry Flag (C):** Set when arithmetic overflow/underflow
**Negative Flag (N):** Set when result has bit 7 set
**Overflow Flag (V):** Set when signed arithmetic overflow

**Flag Updates:**

- Load instructions: Update Z flag
- Arithmetic: Update all flags
- Logical: Update Z and N flags
- Shifts: Update Z, N, and C flags

---

## Performance Notes

**Instruction Timing:**

- Simple operations (NOP, register transfers): 1 cycle
- Immediate loads: 2 cycles
- Memory operations: 4-5 cycles
- System calls: 8 cycles

**Optimization Tips:**

1. Use registers instead of memory when possible
2. Use relative jumps for nearby branches
3. Use auto-increment/decrement for arrays
4. Use 16-bit operations for address arithmetic
5. Minimize memory accesses in tight loops

---

## Programming Examples

**Simple Loop:**

```assembly
LDA #10         ; Loop counter
LOOP:
    ; ... loop body ...
    DEC A       ; Decrement counter
    JNZ LOOP    ; Continue if not zero
```

**Array Processing:**

```assembly
LDIDX #ARRAY    ; Point to array
LDB #SIZE       ; Number of elements
PROCESS_LOOP:
    LDA (IDX)   ; Load current element
    ; ... process element in A ...
    STA (IDX)   ; Store back if modified
    INCIDX      ; Move to next element
    DEC B       ; Decrement counter
    JNZ PROCESS_LOOP
```

**Subroutine with Parameters:**

```assembly
; Main program
LDA #VALUE1     ; Parameter 1 in A
LDB #VALUE2     ; Parameter 2 in B
CALL MULTIPLY   ; Call subroutine
; Result returned in A

MULTIPLY:       ; Multiply A * B -> A
    PUSH C      ; Save C
    LDC #0      ; Clear result
MULT_LOOP:
    ADD C,A     ; Add A to result
    DEC B       ; Decrement multiplier
    JNZ MULT_LOOP
    MOV A,C     ; Move result to A
    POP C       ; Restore C
    RET
```

This reference covers all implemented opcodes in the FS8 processor. Use this as your complete guide for assembly programming and understanding the instruction set architecture.
