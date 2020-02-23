using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type definition or reference that can be referenced by a TypeDefOrRef coded index. 
    /// </summary>
    public interface ITypeDefOrRef : ITypeDescriptor, IMemberRefParent, IHasCustomAttribute
    {
        /// <summary>
        /// When this type is nested, gets the enclosing type.
        /// </summary>
        new ITypeDefOrRef DeclaringType
        {
            get;
        }

        /// <summary>
        /// Creates a signature of this type that can be used in various blob signatures.
        /// </summary>
        /// <returns>The type signature.</returns>
        TypeSignature ToTypeSignature();
    }
}