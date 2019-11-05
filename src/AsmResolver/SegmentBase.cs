namespace AsmResolver
{
    /// <summary>
    /// Provides a base for a segment in a file that can be relocated.
    /// </summary>
    public abstract class SegmentBase : ISegment
    {
        /// <inheritdoc />
        public uint FileOffset
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public uint Rva
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public bool CanUpdateOffsets => true;

        /// <inheritdoc />
        public virtual void UpdateOffsets(uint newFileOffset, uint newRva)
        {
            FileOffset = newFileOffset;
            Rva = newRva;
        }

        /// <inheritdoc />
        public abstract uint GetPhysicalSize();

        /// <inheritdoc />
        public abstract uint GetVirtualSize();

        /// <inheritdoc />
        public abstract void Write(IBinaryStreamWriter writer);
    }
}