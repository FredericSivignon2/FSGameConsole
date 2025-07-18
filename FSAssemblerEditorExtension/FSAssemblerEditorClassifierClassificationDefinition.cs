using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace FSAssemblerEditorExtension
{
    /// <summary>
    /// Classification type definitions for FS8 Assembler syntax highlighting
    /// </summary>
    internal static class FSAssemblerEditorClassifierClassificationDefinition
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        /// <summary>
        /// Defines the "FS8Instruction" classification type for assembly instructions.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("FS8Instruction")]
        private static ClassificationTypeDefinition instructionTypeDefinition;

        /// <summary>
        /// Defines the "FS8Register" classification type for CPU registers.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("FS8Register")]
        private static ClassificationTypeDefinition registerTypeDefinition;

        /// <summary>
        /// Defines the "FS8Label" classification type for labels.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("FS8Label")]
        private static ClassificationTypeDefinition labelTypeDefinition;

        /// <summary>
        /// Defines the "FS8Comment" classification type for comments.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("FS8Comment")]
        private static ClassificationTypeDefinition commentTypeDefinition;

        /// <summary>
        /// Defines the "FS8Number" classification type for numbers and addresses.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("FS8Number")]
        private static ClassificationTypeDefinition numberTypeDefinition;

        /// <summary>
        /// Defines the "FS8Directive" classification type for assembler directives like DB.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("FS8Directive")]
        private static ClassificationTypeDefinition directiveTypeDefinition;

        /// <summary>
        /// Defines the "FS8String" classification type for string literals.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("FS8String")]
        private static ClassificationTypeDefinition stringTypeDefinition;

        /// <summary>
        /// Defines the "FS8IndexedAddress" classification type for indexed addressing expressions.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("FS8IndexedAddress")]
        private static ClassificationTypeDefinition indexedAddressTypeDefinition;

#pragma warning restore 169
    }
}
