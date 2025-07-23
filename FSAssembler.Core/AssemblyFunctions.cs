using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FSAssembler.Core
{
    /// <summary>
    /// Contains all assembly functions for different instruction types.
    /// These functions are referenced in InstructionInfo to eliminate code duplication.
    /// </summary>
    public static class AssemblyFunctions
    {
        /// <summary>
        /// Assemble simple instructions with standard operand handling
        /// Handles instructions like NOP (no params), JMP (address param), etc.
        /// </summary>
        public static List<byte> AssembleStandardInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            // Get the instruction info to understand what parameters are expected
            var instructionsProperty = assembler.GetType().GetField("_instructions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (instructionsProperty == null)
            {
                // Create exception from assembler's namespace using reflection
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "Instructions dictionary not found in assembler");
            }
                
            var instructions = (Dictionary<string, InstructionInfo>)instructionsProperty.GetValue(assembler);
            if (!instructions.TryGetValue(mnemonic.ToUpper(), out InstructionInfo instructionInfo))
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, $"Unknown instruction: {mnemonic}");
            }

            var bytes = new List<byte>();
            bytes.Add(instructionInfo.BaseOpcode);

            // Validate parameter count with better error messages
            if (parts.Length != instructionInfo.RequiredParameters)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                if (instructionInfo.RequiredParameters == 1)
                    throw (Exception)Activator.CreateInstance(assemblerExceptionType, $"Instruction {mnemonic} takes no parameters");
                else if (instructionInfo.HasAddressParameter)
                    throw (Exception)Activator.CreateInstance(assemblerExceptionType, $"Instruction {mnemonic} requires an address");
                else
                    throw (Exception)Activator.CreateInstance(assemblerExceptionType, $"Instruction {mnemonic} requires {instructionInfo.RequiredParameters - 1} parameter(s)");
            }

            // Handle address parameter if present
            if (instructionInfo.HasAddressParameter && parts.Length == 2)
            {
                // Use reflection to call ParseAddress from assembler
                var parseAddressMethod = assembler.GetType().GetMethod("ParseAddress", BindingFlags.NonPublic | BindingFlags.Instance);
                if (parseAddressMethod == null)
                {
                    var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                    throw (Exception)Activator.CreateInstance(assemblerExceptionType, "ParseAddress method not found in assembler");
                }
                    
                ushort address = (ushort)parseAddressMethod.Invoke(assembler, new object[] { parts[1], currentAddress });
                bytes.Add((byte)(address & 0xFF));       // Low byte
                bytes.Add((byte)((address >> 8) & 0xFF)); // High byte
            }

            return bytes;
        }

        /// <summary>
        /// Assemble DB directive (data bytes)
        /// </summary>
        public static List<byte> AssembleDbDirective(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var bytes = new List<byte>();
            
            if (parts.Length == 1)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "DB directive requires at least one value");
            }

            // Join all parts after "DB" to handle comma-separated values properly
            string datapart = string.Join(" ", parts.Skip(1));
            
            // Use reflection to call ParseDataValues method from assembler
            var parseMethod = assembler.GetType().GetMethod("ParseDataValues", BindingFlags.NonPublic | BindingFlags.Instance);
            if (parseMethod == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "ParseDataValues method not found in assembler");
            }
                
            var values = (List<byte>)parseMethod.Invoke(assembler, new object[] { datapart });
            bytes.AddRange(values);
            
            return bytes;
        }

        /// <summary>
        /// Assemble 8-bit load instructions (LDA, LDB, LDC, LDD, LDE, LDF)
        /// </summary>
        public static List<byte> AssembleLoadInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            // Use reflection to call the assembler's AssembleLoadInstruction method
            var method = assembler.GetType().GetMethod("AssembleLoadInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleLoadInstruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts, currentAddress });
        }

        /// <summary>
        /// Assemble 16-bit load instructions (LDDA, LDDB)
        /// </summary>
        public static List<byte> AssembleLoadInstruction16(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleLoadInstruction16", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleLoadInstruction16 method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts, currentAddress });
        }

        /// <summary>
        /// Assemble index load instructions (LDIDX, LDIDY)
        /// </summary>
        public static List<byte> AssembleIndexLoadInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleIndexLoadInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleIndexLoadInstruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts, currentAddress });
        }

        /// <summary>
        /// Assemble store instructions (STA, STB, STC, STD, STE, STF, STDA, STDB)
        /// </summary>
        public static List<byte> AssembleStoreInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleStoreInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleStoreInstruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts, currentAddress });
        }

        /// <summary>
        /// Assemble index add instructions (ADDIDX, ADDIDY)
        /// </summary>
        public static List<byte> AssembleIndexAddInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleIndexAddInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleIndexAddInstruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts, currentAddress });
        }

        /// <summary>
        /// Assemble increment/decrement instructions (INC, DEC)
        /// </summary>
        public static List<byte> AssembleIncDecInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleIncDecInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleIncDecInstruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts });
        }

        /// <summary>
        /// Assemble 16-bit increment/decrement instructions (INC16, DEC16)
        /// </summary>
        public static List<byte> AssembleInc16Dec16Instruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleInc16Dec16Instruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleInc16Dec16Instruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts });
        }

        /// <summary>
        /// Assemble compare instructions (CMP)
        /// </summary>
        public static List<byte> AssembleCmpInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleCmpInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleCmpInstruction method not found in assembler");
            }
                
            // Note: AssembleCmpInstruction needs the original line for some logic
            string originalLine = string.Join(" ", parts);
            return (List<byte>)method.Invoke(assembler, new object[] { parts, originalLine });
        }

        /// <summary>
        /// Assemble logical instructions (AND, OR, XOR)
        /// </summary>
        public static List<byte> AssembleLogicalInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleLogicalInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleLogicalInstruction method not found in assembler");
            }
                
            string originalLine = string.Join(" ", parts);
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts, originalLine });
        }

        /// <summary>
        /// Assemble shift left instructions (SHL)
        /// </summary>
        public static List<byte> AssembleShlInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleShlInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleShlInstruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { parts });
        }

        /// <summary>
        /// Assemble relative jump instructions (JR, JRZ, JRNZ, JRC)
        /// </summary>
        public static List<byte> AssembleRelativeJumpInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleRelativeJumpInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleRelativeJumpInstruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts, currentAddress });
        }

        /// <summary>
        /// Assemble stack instructions (PUSH, POP, PUSH16, POP16)
        /// </summary>
        public static List<byte> AssembleStackInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleStackInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleStackInstruction method not found in assembler");
            }
                
            return (List<byte>)method.Invoke(assembler, new object[] { mnemonic, parts });
        }

        /// <summary>
        /// Assemble move instructions (MOV)
        /// </summary>
        public static List<byte> AssembleMovInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleMovInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleMovInstruction method not found in assembler");
            }
                
            string originalLine = string.Join(" ", parts);
            return (List<byte>)method.Invoke(assembler, new object[] { parts, originalLine });
        }

        /// <summary>
        /// Assemble swap instructions (SWP)
        /// </summary>
        public static List<byte> AssembleSwpInstruction(object assembler, string mnemonic, string[] parts, ushort currentAddress)
        {
            var method = assembler.GetType().GetMethod("AssembleSwpInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                var assemblerExceptionType = assembler.GetType().Assembly.GetType("FSAssembler.AssemblerException");
                throw (Exception)Activator.CreateInstance(assemblerExceptionType, "AssembleSwpInstruction method not found in assembler");
            }
                
            string originalLine = string.Join(" ", parts);
            return (List<byte>)method.Invoke(assembler, new object[] { parts, originalLine });
        }
    }
}