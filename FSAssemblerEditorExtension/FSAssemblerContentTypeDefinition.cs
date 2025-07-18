using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace FSAssemblerEditorExtension
{
    /// <summary>
    /// Content type definition for FS8 assembler files
    /// </summary>
    internal static class FSAssemblerContentTypeDefinition
    {
        /// <summary>
        /// Defines the "fs8" content type that is a subtype of the "text" content type.
        /// </summary>
        [Export]
        [Name("fs8")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition FS8ContentTypeDefinition = null;

        /// <summary>
        /// Associates the ".fs8" file extension with the "fs8" content type.
        /// </summary>
        [Export]
        [FileExtension(".fs8")]
        [ContentType("fs8")]
        internal static FileExtensionToContentTypeDefinition FS8FileTypeDefinition = null;
    }
}