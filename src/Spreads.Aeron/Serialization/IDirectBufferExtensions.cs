﻿using System;
using System.IO;

namespace Spreads.Serialization
{
    public static class IDirectBufferExtensions {
        /// <summary>
        /// 
        /// </summary>
        public static UnmanagedMemoryAccessor CreateAccessor(this IDirectBuffer fixedBuffer, long offset = 0, long length = 0, bool readOnly = false) {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (length + offset > fixedBuffer.Length) throw new ArgumentException("Length plus offset exceed capacity");
            if (length == 0) {
                length = fixedBuffer.Length - offset;
            }
            return new SafeBufferAccessor(fixedBuffer.CreateSafeBuffer(), offset, length, readOnly);
        }

        /// <summary>
        /// 
        /// </summary>
        public static UnmanagedMemoryStream CreateStream(this IDirectBuffer fixedBuffer, long offset = 0, long length = 0, bool readOnly = false) {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (length + offset > fixedBuffer.Length) throw new ArgumentException("Length plus offset exceed capacity");
            if (length == 0) {
                length = fixedBuffer.Length - offset;
            }
            return new SafeBufferStream(fixedBuffer.CreateSafeBuffer(), offset, length, readOnly);
        }

        /// <summary>
        /// 
        /// </summary>
        public static UnmanagedMemoryAccessor GetDirectAccessor(this ArraySegment<byte> arraySegment) {
            var fb = new FixedBuffer(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
            // NB offet/len are applied to fb 
            return fb.CreateAccessor();
        }

        /// <summary>
        /// 
        /// </summary>
        public static UnmanagedMemoryStream GetDirectStream(this ArraySegment<byte> arraySegment) {
            var fb = new FixedBuffer(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
            // NB offet/len are applied to fb
            return fb.CreateStream();
        }
    }
}