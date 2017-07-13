﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Spreads.Buffers;
using Spreads.Utils;
using System;
using System.Runtime.CompilerServices;
using Spreads.IPC.Logbuffer.Protocol;

namespace Spreads.IPC.Logbuffer
{
    /// <summary>
    /// A term buffer reader.
    /// <para>
    /// <b>Note:</b> Reading from the term is thread safe, but each thread needs its own instance of this class.
    /// </para>
    /// </summary>
    internal static class TermReader
    {
        /// <summary>
        /// Reads data from a term in a log buffer.
        ///
        /// If a fragmentsLimit of 0 or less is passed then at least one read will be attempted.
        /// </summary>
        /// <param name="termBuffer">     to be read for fragments. </param>
        /// <param name="offset">         offset within the buffer that the read should begin. </param>
        /// <param name="handler">        the handler for data that has been read </param>
        /// <param name="fragmentsLimit"> limit the number of fragments read. </param>
        /// <param name="header">         to be used for mapping over the header for a given fragment. </param>
        /// <param name="errorHandler">   to be notified if an error occurs during the callback. </param>
        /// <returns> the number of fragments read </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Read(
            DirectBuffer termBuffer,
            int offset,
            FragmentHandler handler,
            int fragmentsLimit,
            Header header,
            ErrorHandler errorHandler)
        {
            int fragmentsRead = 0;
            int capacity = (int)termBuffer.Length;

            try
            {
                do
                {
                    int frameLength = FrameDescriptor.FrameLengthVolatile(termBuffer, offset);
                    if (frameLength <= 0)
                    {
                        break;
                    }

                    int termOffset = offset;
                    offset += BitUtil.Align(frameLength, FrameDescriptor.FRAME_ALIGNMENT);

                    if (!FrameDescriptor.IsPaddingFrame(termBuffer, termOffset))
                    {
                        header.Buffer = termBuffer;
                        header.Offset = termOffset;

                        handler?.Invoke(termBuffer, termOffset + DataHeaderFlyweight.HEADER_LENGTH, frameLength - DataHeaderFlyweight.HEADER_LENGTH, header);

                        ++fragmentsRead;
                    }
                }
                while (fragmentsRead < fragmentsLimit && offset < capacity);
            }
            catch (Exception t)
            {
                errorHandler?.Invoke(t);
            }

            return Pack(offset, fragmentsRead);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Read(
            DirectBuffer termBuffer,
            int offset,
            OnAppendHandler handler,
            int fragmentsLimit)
        {
            int fragmentsRead = 0;
            int capacity = (int)termBuffer.Length;

            do
            {
                int frameLength = FrameDescriptor.FrameLengthVolatile(termBuffer, offset);
                if (frameLength <= 0)
                {
                    break;
                }

                int termOffset = offset;
                offset += BitUtil.Align(frameLength, FrameDescriptor.FRAME_ALIGNMENT);
                // TODO check for padding
                if (!FrameDescriptor.IsPaddingFrame(termBuffer, termOffset))
                {
                    var messageBuffer = DirectBuffer.CreateWithoutChecks(frameLength, termBuffer.Data + termOffset);
                    handler?.Invoke(messageBuffer);
                    ++fragmentsRead;
                }
            }
            while (fragmentsRead < fragmentsLimit && offset < capacity);

            return Pack(offset, fragmentsRead);
        }

        /// <summary>
        /// Pack the values for fragmentsRead and offset into a long for returning on the stack.
        /// </summary>
        /// <param name="offset">        value to be packed. </param>
        /// <param name="fragmentsRead"> value to be packed. </param>
        /// <returns> a long with both ints packed into it. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Pack(int offset, int fragmentsRead)
        {
            return ((long)offset << 32) | (long)fragmentsRead;
        }

        /// <summary>
        /// The number of fragments that have been read.
        /// </summary>
        /// <param name="readOutcome"> into which the fragments read value has been packed. </param>
        /// <returns> the number of fragments that have been read. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FragmentsRead(long readOutcome)
        {
            return (int)readOutcome;
        }

        /// <summary>
        /// The offset up to which the term has progressed.
        /// </summary>
        /// <param name="readOutcome"> into which the offset value has been packed. </param>
        /// <returns> the offset up to which the term has progressed. </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Offset(long readOutcome)
        {
            return (int)((long)((ulong)readOutcome >> 32));
        }
    }
}