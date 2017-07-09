﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Spreads.Buffers;
using Spreads.Utils;

namespace Spreads.IPC.Logbuffer
{
    public static class LogBufferUnblocker
    {
        /// <summary>
        /// Attempt to unblock a log buffer at given position
        /// </summary>
        /// <param name="logPartitions">     for current blockedOffset </param>
        /// <param name="logMetaDataBuffer"> for log buffer </param>
        /// <param name="blockedPosition">   to attempt to unblock </param>
        /// <returns> whether unblocked or not </returns>
        public static bool Unblock(LogBufferPartition[] logPartitions, DirectBuffer logMetaDataBuffer, long blockedPosition)
        {
            checked
            {
                int termLength = (int)logPartitions[0].TermBuffer.Length;
                int positionBitsToShift = IntUtil.NumberOfTrailingZeros(termLength);
                int activeIndex = LogBufferDescriptor.IndexByPosition(blockedPosition, positionBitsToShift);
                LogBufferPartition activePartition = logPartitions[activeIndex];
                DirectBuffer termBuffer = activePartition.TermBuffer;
                long rawTail = activePartition.RawTailVolatile;
                int termId = LogBufferDescriptor.TermId(rawTail);
                int tailOffset = LogBufferDescriptor.TermOffset(rawTail, termLength);
                int blockedOffset = LogBufferDescriptor.ComputeTermOffsetFromPosition(blockedPosition, positionBitsToShift);

                bool result = false;

                switch (TermUnblocker.Unblock(logMetaDataBuffer, termBuffer, blockedOffset, tailOffset, termId))
                {
                    case TermUnblockerStatus.UNBLOCKED_TO_END:
                        LogBufferDescriptor.RotateLog(logPartitions, logMetaDataBuffer, activeIndex, termId + 1);
                        // fall through
                        goto case TermUnblockerStatus.UNBLOCKED;
                    case TermUnblockerStatus.UNBLOCKED:
                        result = true;
                        break;
                }

                return result;
            }
        }
    }
}