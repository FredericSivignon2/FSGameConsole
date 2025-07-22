# FS8 Assembler Editor Extension

This Visual Studio extension provides syntax highlighting for FS8 assembler files (.fs8) used by the FSGameConsole emulator project.

## Features

### Syntax Highlighting
The extension provides colorized syntax highlighting for all FS8 assembler elements:

- **Instructions** (Blue, Bold): All 80+ FS8 CPU instructions including:
  - Basic: `NOP`, `HALT`
  - Load: `LDA`, `LDB`, `LDDA`, `LDIX1`, etc.
  - Arithmetic: `ADD`, `SUB`, `ADD16`, `INC`, `DEC`, etc.
  - Logical: `AND`, `OR`, `XOR`, `NOT`, `SHL`, `SHR`
  - Jump: `JMP`, `JZ`, `JNZ`, `JR`, etc.
  - Store: `STA`, `STB`, `STDA`, etc.
  - Stack: `PUSH`, `POP`, `PUSH16`, `POP16`
  - Index: `INCIX1`, `LDAIX1+`, `STAIY1-`, etc.

- **Registers** (Dark Green, Bold): All FS8 CPU registers:
  - 8-bit: `A`, `B`, `C`, `D`, `E`, `F`
  - 16-bit: `DA`, `DB`, `PC`, `SP`, `SR`
  - Index: `IDX1`, `IDX2`, `IDY1`, `IDY2`

- **Labels** (Dark Red, Bold): Label definitions ending with `:`
  - Example: `MainLoop:`, `PrintString:`, `END:`

- **Comments** (Green, Italic): Comments starting with `;`
  - Example: `; This is a comment`

- **Numbers** (Purple): All number formats supported by FS8:
  - Decimal: `123`, `0`
  - Hexadecimal: `0xFF`, `$A0`

- **Directives** (Dark Orange, Bold): Assembler directives:
  - `DB` for data bytes

- **Strings** (Brown): String literals in double quotes:
  - Example: `"Hello World"`

- **Indexed Addressing** (Dark Cyan, Bold): Index register expressions:
  - Example: `(IDX1)`, `(IDY1+5)`, `(IDX2-10)`

### File Association
The extension automatically activates for files with the `.fs8` extension.

## Installation

1. Build the FSAssemblerEditorExtension project
2. Install the generated `.vsix` file in Visual Studio
3. Open any `.fs8` file to see syntax highlighting

## Example FS8 Code with Highlighting

```assembly
; Main program entry point
Startup:
    LDDA WelcomeMessage    ; Load message address
    CALL PrintString       ; Call print function
    HALT                   ; Stop execution

; Print string function
PrintString:
    PUSH DA                ; Save DA register
    
PrintString_Loop:
    LDA (DA)               ; Load character from address
    CMP A, 0               ; Check for null terminator
    JZ PrintString_End     ; Jump if zero
    
    ; Print character via system call
    LDB A                  ; Move character to B
    LDA 1                  ; System call code
    SYS                    ; Execute system call
    
    INC16 DA               ; Increment address
    JMP PrintString_Loop   ; Continue loop
    
PrintString_End:
    POP DA                 ; Restore DA register
    RET                    ; Return to caller

; Data section
WelcomeMessage:
    DB "FS System v1.0", 0x00
```

## Supported FS8 Instructions

The extension recognizes all instructions from the FS8 Enhanced instruction set:

### Basic Instructions (0x00-0x01)
- `NOP` - No Operation
- `HALT` - Stop processor

### Load Instructions (0x10-0x1D)
- 8-bit: `LDA`, `LDB`, `LDC`, `LDD`, `LDE`, `LDF`
- 16-bit: `LDDA`, `LDDB`
- Index: `LDIX1`, `LDIX2`, `LDIY1`, `LDIY2`

### Arithmetic Instructions (0x20-0x2F)
- 8-bit: `ADD`, `SUB`, `INC`, `DEC`, `CMP`
- 16-bit: `ADD16`, `SUB16`, `INC16`, `DEC16`

### Logical Instructions (0x30-0x39)
- `AND`, `OR`, `XOR`, `NOT`, `SHL`, `SHR`

### Jump Instructions (0x40-0x46, 0xC0-0xC3)
- Absolute: `JMP`, `JZ`, `JNZ`, `JC`, `JNC`, `JN`, `JNN`
- Relative: `JR`, `JRZ`, `JRNZ`, `JRC`

### Store Instructions (0x50-0x55)
- `STA`, `STB`, `STC`, `STD`, `STDA`, `STDB`

### Stack Instructions (0x70-0x79)
- 8-bit: `PUSH`, `POP` for registers A, B, C
- 16-bit: `PUSH16`, `POP16` for DA, DB

### Index Instructions (0x80-0x99, 0xC4-0xCB, 0xE0-0xEB)
- Direct: `LDA (IDX1)`, `STA (IDY1)`
- Offset: `LDA (IDX1+5)`, `STA (IDY1-10)`
- Auto-increment: `LDAIX1+`, `STAIY1+`
- Auto-decrement: `LDAIX1-`, `STAIY1-`
- Arithmetic: `INCIX1`, `DECIY1`, `ADDIX1`
- Transfer: `MVIX1IX2`, `SWPIX1IY1`

### Transfer Instructions (0xA0-0xA7)
- `MOV`, `SWP`

### Subroutine Instructions (0x60-0x61)
- `CALL`, `RET`

## Technical Details

The extension is built using Visual Studio SDK and MEF (Managed Extensibility Framework). It consists of:

- **Content Type Definition**: Associates `.fs8` files with the FS8 content type
- **Classification Types**: Defines different syntax element types
- **Classifier**: Analyzes text and applies classifications using regex patterns
- **Format Definitions**: Defines colors and styles for each classification type

## Development

To modify or extend the highlighting:

1. Edit `FSAssemblerEditorClassifier.cs` to add new patterns or instructions
2. Modify `FSAssemblerEditorClassifierFormats.cs` to change colors and styles
3. Update `FSAssemblerEditorClassifierClassificationDefinition.cs` for new classification types

## Compatibility

- Visual Studio 2022 (17.0+)
- .NET Framework 4.7.2+
- Supports Community, Professional, and Enterprise editions

## License

This extension is part of the FSGameConsole emulator project.