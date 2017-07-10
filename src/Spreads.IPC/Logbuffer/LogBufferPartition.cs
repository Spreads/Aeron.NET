﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Spreads.Buffers;
using System;
using System.Runtime.CompilerServices;

namespace Spreads.IPC.Logbuffer
{
    /// <summary>
    /// Log buffer implementation containing common functionality for dealing with log partition terms.
    /// </summary>
    public struct LogBufferPartition
    {
        private readonly DirectBuffer _termBuffer;
        private readonly DirectBuffer _metaDataBuffer;

        public LogBufferPartition(DirectBuffer termBuffer, DirectBuffer metaDataBuffer)
        {
            this._termBuffer = termBuffer;
            this._metaDataBuffer = metaDataBuffer;
        }

        public DirectBuffer TermBuffer => _termBuffer;

        public DirectBuffer MetaDataBuffer => _metaDataBuffer;

        /**
         * Clean down the buffers for reuse by zeroing them out.
         */

        public void Clean()
        {
            checked
            {
                _termBuffer.Clear(0, (int)_termBuffer.Length);
            }
            _metaDataBuffer.WriteInt32(LogBufferDescriptor.TERM_STATUS_OFFSET, LogBufferDescriptor.CLEAN);
        }

        /**
         * What is the current status of the buffer.
         *
         * @return the status of buffer as described in {@link LogBufferDescriptor}
         */

        public int Status
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _metaDataBuffer.VolatileReadInt32(LogBufferDescriptor.TERM_STATUS_OFFSET); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _metaDataBuffer.VolatileWriteInt32(LogBufferDescriptor.TERM_STATUS_OFFSET, value); }
        }

        /**
         * Get the current tail value in a volatile memory ordering fashion. If raw tail is greater than
         * {@link #termBuffer()}.{@link org.agrona.DirectBuffer#capacity()} then capacity will be returned.
         *
         * @return the current tail value.
         */

        public int TailOffsetVolatile
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var tail = _metaDataBuffer.VolatileReadInt64(LogBufferDescriptor.TERM_TAIL_COUNTER_OFFSET) & 0xFFFFFFFFL;
                return (int)Math.Min(tail, _termBuffer.Length);
            }
        }

        /// <summary>
        /// Get the raw value for the tail containing both termId and offset.
        /// </summary>
        /// <returns> the raw value for the tail containing both termId and offset. </returns>
        public long RawTailVolatile
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _metaDataBuffer.VolatileReadInt64(LogBufferDescriptor.TERM_TAIL_COUNTER_OFFSET); }
        }

        public int TermId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var rawTail = _metaDataBuffer.ReadInt64(LogBufferDescriptor.TERM_TAIL_COUNTER_OFFSET);
                return (int)((long)((ulong)rawTail >> 32));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _metaDataBuffer.WriteInt64(LogBufferDescriptor.TERM_TAIL_COUNTER_OFFSET, ((long)value) << 32);
            }
        }
    }
}