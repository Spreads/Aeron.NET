﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Spreads.Buffers;

namespace Spreads.IPC.Logbuffer
{
    /// <summary>
    /// Rebuild a term buffer based on incoming frames that can be out-of-order.
    /// </summary>
    public static class TermRebuilder
    {
        /// <summary>
        /// Insert a packet of frames into the log at the appropriate offset as indicated by the term offset header.
        /// </summary>
        /// <param name="termBuffer"> into which the packet should be inserted. </param>
        /// <param name="offset">     offset in the term at which the packet should be inserted. </param>
        /// <param name="packet">     containing a sequence of frames. </param>
        /// <param name="length">     of the sequence of frames in bytes. </param>

        public static void Insert(DirectBuffer termBuffer, int offset, DirectBuffer packet, int length)
        {
            int firstFrameLength = packet.ReadInt32(0);
            packet.VolatileWriteInt32(0, 0);

            termBuffer.WriteBytes(offset, packet, 0, length);
            FrameDescriptor.FrameLengthOrdered(termBuffer, offset, firstFrameLength);
        }
    }
}