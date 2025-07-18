using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace FSAssemblerEditorExtension
{
    /// <summary>
    /// Defines editor formats for FS8 Assembler syntax highlighting
    /// </summary>
    internal static class FSAssemblerEditorClassifierFormats
    {
        /// <summary>
        /// Format for FS8 assembly instructions - bold blue
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "FS8Instruction")]
        [Name("FS8Instruction")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)]
        internal sealed class FS8InstructionFormat : ClassificationFormatDefinition
        {
            public FS8InstructionFormat()
            {
                this.DisplayName = "FS8 Assembly Instruction";
                this.ForegroundColor = Colors.Blue;
                this.IsBold = true;
            }
        }

        /// <summary>
        /// Format for FS8 CPU registers - dark green
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "FS8Register")]
        [Name("FS8Register")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)]
        internal sealed class FS8RegisterFormat : ClassificationFormatDefinition
        {
            public FS8RegisterFormat()
            {
                this.DisplayName = "FS8 CPU Register";
                this.ForegroundColor = Colors.DarkGreen;
                this.IsBold = true;
            }
        }

        /// <summary>
        /// Format for FS8 labels - dark red
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "FS8Label")]
        [Name("FS8Label")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)]
        internal sealed class FS8LabelFormat : ClassificationFormatDefinition
        {
            public FS8LabelFormat()
            {
                this.DisplayName = "FS8 Label";
                this.ForegroundColor = Colors.DarkRed;
                this.IsBold = true;
            }
        }

        /// <summary>
        /// Format for FS8 comments - green italic
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "FS8Comment")]
        [Name("FS8Comment")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)]
        internal sealed class FS8CommentFormat : ClassificationFormatDefinition
        {
            public FS8CommentFormat()
            {
                this.DisplayName = "FS8 Comment";
                this.ForegroundColor = Colors.Green;
                this.IsItalic = true;
            }
        }

        /// <summary>
        /// Format for FS8 numbers and addresses - purple
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "FS8Number")]
        [Name("FS8Number")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)]
        internal sealed class FS8NumberFormat : ClassificationFormatDefinition
        {
            public FS8NumberFormat()
            {
                this.DisplayName = "FS8 Number";
                this.ForegroundColor = Colors.Purple;
            }
        }

        /// <summary>
        /// Format for FS8 assembler directives - dark orange
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "FS8Directive")]
        [Name("FS8Directive")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)]
        internal sealed class FS8DirectiveFormat : ClassificationFormatDefinition
        {
            public FS8DirectiveFormat()
            {
                this.DisplayName = "FS8 Directive";
                this.ForegroundColor = Colors.DarkOrange;
                this.IsBold = true;
            }
        }

        /// <summary>
        /// Format for FS8 string literals - brown
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "FS8String")]
        [Name("FS8String")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)]
        internal sealed class FS8StringFormat : ClassificationFormatDefinition
        {
            public FS8StringFormat()
            {
                this.DisplayName = "FS8 String";
                this.ForegroundColor = Colors.Brown;
            }
        }

        /// <summary>
        /// Format for FS8 indexed addressing expressions - dark cyan
        /// </summary>
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "FS8IndexedAddress")]
        [Name("FS8IndexedAddress")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)]
        internal sealed class FS8IndexedAddressFormat : ClassificationFormatDefinition
        {
            public FS8IndexedAddressFormat()
            {
                this.DisplayName = "FS8 Indexed Address";
                this.ForegroundColor = Colors.DarkCyan;
                this.IsBold = true;
            }
        }
    }
}
