using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FSAssemblerEditorExtension
{
    /// <summary>
    /// Classifier for FS8 Assembler syntax highlighting
    /// </summary>
    internal class FSAssemblerEditorClassifier : IClassifier
    {
        /// <summary>
        /// Classification types for different syntax elements
        /// </summary>
        private readonly IClassificationType instructionClassificationType;
        private readonly IClassificationType registerClassificationType;
        private readonly IClassificationType labelClassificationType;
        private readonly IClassificationType commentClassificationType;
        private readonly IClassificationType numberClassificationType;
        private readonly IClassificationType directiveClassificationType;
        private readonly IClassificationType stringClassificationType;
        private readonly IClassificationType indexedAddressClassificationType;

        /// <summary>
        /// FS8 instruction patterns based on the copilot instructions
        /// </summary>
        private static readonly HashSet<string> Instructions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Basic instructions
            "NOP", "HALT", "SYS",
            
            // Load instructions 8-bit
            "LDA", "LDB", "LDC", "LDD", "LDE", "LDF",
            
            // Load instructions 16-bit
            "LDDA", "LDDB", "LDIX1", "LDIX2", "LDIY1", "LDIY2",
            
            // Arithmetic instructions
            "ADD", "SUB", "ADD16", "SUB16", "INC", "DEC", "INC16", "DEC16", "CMP",
            
            // Logical instructions
            "AND", "OR", "XOR", "NOT", "SHL", "SHR",
            
            // Jump instructions
            "JMP", "JZ", "JNZ", "JC", "JNC", "JN", "JNN", "JR", "JRZ", "JRNZ", "JRC",
            
            // Store instructions
            "STA", "STB", "STC", "STD", "STDA", "STDB",
            
            // Transfer instructions
            "MOV", "SWP",
            
            // Stack instructions
            "PUSH", "POP", "PUSH16", "POP16",
            
            // Subroutine instructions
            "CALL", "RET",
            
            // Index instructions
            "INCIX1", "DECIX1", "INCIY1", "DECIY1", "INCIX2", "DECIX2", "INCIY2", "DECIY2",
            "ADDIX1", "ADDIX2", "ADDIY1", "ADDIY2",
            "MVIX1IX2", "MVIX2IX1", "MVIY1IY2", "MVIY2IY1", "MVIX1IY1", "MVIY1IX1",
            "SWPIX1IX2", "SWPIY1IY2", "SWPIX1IY1",
            
            // Auto-increment/decrement instructions
            "LDAIX1+", "LDAIY1+", "STAIX1+", "STAIY1+", "LDAIX1-", "LDAIY1-", "STAIX1-", "STAIY1-"
        };

        /// <summary>
        /// FS8 CPU registers
        /// </summary>
        private static readonly HashSet<string> Registers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "A", "B", "C", "D", "E", "F", "PC", "SP", "SR",
            "DA", "DB", "IDX1", "IDX2", "IDY1", "IDY2"
        };

        /// <summary>
        /// FS8 assembler directives
        /// </summary>
        private static readonly HashSet<string> Directives = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DB"
        };

        /// <summary>
        /// Regular expressions for pattern matching
        /// </summary>
        private static readonly Regex CommentRegex = new Regex(@";.*$", RegexOptions.Compiled);
        private static readonly Regex LabelRegex = new Regex(@"^[\s]*([a-zA-Z_][a-zA-Z0-9_]*)\s*:", RegexOptions.Compiled);
        private static readonly Regex NumberRegex = new Regex(@"\b(?:0x[0-9a-fA-F]+|\$[0-9a-fA-F]+|\b\d+)\b", RegexOptions.Compiled);
        private static readonly Regex StringRegex = new Regex(@"""[^""]*""", RegexOptions.Compiled);
        private static readonly Regex IndexedAddressRegex = new Regex(@"\([^)]+\)", RegexOptions.Compiled);
        private static readonly Regex WordRegex = new Regex(@"\b[a-zA-Z_][a-zA-Z0-9_+\-]*\b", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="FSAssemblerEditorClassifier"/> class.
        /// </summary>
        /// <param name="registry">Classification registry.</param>
        internal FSAssemblerEditorClassifier(IClassificationTypeRegistryService registry)
        {
            this.instructionClassificationType = registry.GetClassificationType("FS8Instruction");
            this.registerClassificationType = registry.GetClassificationType("FS8Register");
            this.labelClassificationType = registry.GetClassificationType("FS8Label");
            this.commentClassificationType = registry.GetClassificationType("FS8Comment");
            this.numberClassificationType = registry.GetClassificationType("FS8Number");
            this.directiveClassificationType = registry.GetClassificationType("FS8Directive");
            this.stringClassificationType = registry.GetClassificationType("FS8String");
            this.indexedAddressClassificationType = registry.GetClassificationType("FS8IndexedAddress");
        }

        #region IClassifier

#pragma warning disable 67

        /// <summary>
        /// An event that occurs when the classification of a span of text has changed.
        /// </summary>
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67

        /// <summary>
        /// Gets all the <see cref="ClassificationSpan"/> objects that intersect with the given range of text.
        /// </summary>
        /// <param name="span">The span currently being classified.</param>
        /// <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var result = new List<ClassificationSpan>();

            if (span.IsEmpty)
                return result;

            string text = span.GetText();
            
            // Split text by lines, handling both Windows and Unix line endings
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int currentPosition = span.Start;

            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var lineSpan = new SnapshotSpan(span.Snapshot, currentPosition, line.Length);
                    ProcessLine(line, lineSpan, result);
                }
                
                // Move to next line (account for line ending characters)
                currentPosition += line.Length;
                if (currentPosition < span.End)
                {
                    // Skip line ending characters
                    while (currentPosition < span.End && 
                           (span.Snapshot[currentPosition] == '\r' || span.Snapshot[currentPosition] == '\n'))
                    {
                        currentPosition++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Processes a single line and adds classification spans
        /// </summary>
        private void ProcessLine(string line, SnapshotSpan lineSpan, List<ClassificationSpan> result)
        {
            // First check for comments - they override everything else
            var commentMatch = CommentRegex.Match(line);
            if (commentMatch.Success)
            {
                var commentSpan = new SnapshotSpan(lineSpan.Snapshot, 
                    lineSpan.Start + commentMatch.Index, 
                    commentMatch.Length);
                result.Add(new ClassificationSpan(commentSpan, commentClassificationType));
                
                // Process the part before the comment
                if (commentMatch.Index > 0)
                {
                    string beforeComment = line.Substring(0, commentMatch.Index);
                    var beforeCommentSpan = new SnapshotSpan(lineSpan.Snapshot, lineSpan.Start, commentMatch.Index);
                    ProcessLineContent(beforeComment, beforeCommentSpan, result);
                }
                return;
            }

            // Process the entire line if no comment
            ProcessLineContent(line, lineSpan, result);
        }

        /// <summary>
        /// Processes line content (excluding comments)
        /// </summary>
        private void ProcessLineContent(string line, SnapshotSpan lineSpan, List<ClassificationSpan> result)
        {
            // Check for labels at the start of the line
            var labelMatch = LabelRegex.Match(line);
            if (labelMatch.Success)
            {
                var labelSpan = new SnapshotSpan(lineSpan.Snapshot, 
                    lineSpan.Start + labelMatch.Groups[0].Index, 
                    labelMatch.Groups[0].Length);
                result.Add(new ClassificationSpan(labelSpan, labelClassificationType));
            }

            // Check for string literals
            foreach (Match stringMatch in StringRegex.Matches(line))
            {
                var stringSpan = new SnapshotSpan(lineSpan.Snapshot, 
                    lineSpan.Start + stringMatch.Index, 
                    stringMatch.Length);
                result.Add(new ClassificationSpan(stringSpan, stringClassificationType));
            }

            // Check for indexed addressing expressions like (IDX1+5) or (IDY1)
            foreach (Match indexMatch in IndexedAddressRegex.Matches(line))
            {
                var indexSpan = new SnapshotSpan(lineSpan.Snapshot, 
                    lineSpan.Start + indexMatch.Index, 
                    indexMatch.Length);
                result.Add(new ClassificationSpan(indexSpan, indexedAddressClassificationType));
            }

            // Check for numbers (hex, decimal)
            foreach (Match numberMatch in NumberRegex.Matches(line))
            {
                // Skip if this number is part of a string
                if (IsInsideString(line, numberMatch.Index))
                    continue;

                var numberSpan = new SnapshotSpan(lineSpan.Snapshot, 
                    lineSpan.Start + numberMatch.Index, 
                    numberMatch.Length);
                result.Add(new ClassificationSpan(numberSpan, numberClassificationType));
            }

            // Check for words (instructions, registers, directives)
            foreach (Match wordMatch in WordRegex.Matches(line))
            {
                string word = wordMatch.Value;

                // Skip if this word is part of a string, comment, or label
                if (IsInsideString(line, wordMatch.Index) || 
                    IsInsideComment(line, wordMatch.Index) ||
                    IsPartOfLabel(line, wordMatch.Index))
                    continue;

                IClassificationType classificationType = null;

                if (Instructions.Contains(word))
                {
                    classificationType = instructionClassificationType;
                }
                else if (Registers.Contains(word))
                {
                    classificationType = registerClassificationType;
                }
                else if (Directives.Contains(word))
                {
                    classificationType = directiveClassificationType;
                }

                if (classificationType != null)
                {
                    var wordSpan = new SnapshotSpan(lineSpan.Snapshot, 
                        lineSpan.Start + wordMatch.Index, 
                        wordMatch.Length);
                    result.Add(new ClassificationSpan(wordSpan, classificationType));
                }
            }
        }

        /// <summary>
        /// Checks if a position is inside a string literal
        /// </summary>
        private bool IsInsideString(string line, int position)
        {
            foreach (Match stringMatch in StringRegex.Matches(line))
            {
                if (position >= stringMatch.Index && position < stringMatch.Index + stringMatch.Length)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a position is inside a comment
        /// </summary>
        private bool IsInsideComment(string line, int position)
        {
            var commentMatch = CommentRegex.Match(line);
            if (commentMatch.Success)
            {
                return position >= commentMatch.Index;
            }
            return false;
        }

        /// <summary>
        /// Checks if a position is part of a label definition
        /// </summary>
        private bool IsPartOfLabel(string line, int position)
        {
            var labelMatch = LabelRegex.Match(line);
            if (labelMatch.Success)
            {
                return position >= labelMatch.Groups[0].Index && position < labelMatch.Groups[0].Index + labelMatch.Groups[0].Length;
            }
            return false;
        }

        #endregion
    }
}
