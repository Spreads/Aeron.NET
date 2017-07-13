﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Runtime.InteropServices;
using Spreads.Buffers;

namespace Spreads.IPC.Logbuffer.Protocol
{

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    internal struct VersionFlagsType
    {
        public byte Version;
        public byte Flags;
        public short Type;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
    internal struct Header
    {
        [FieldOffset(0)]
        public int FrameLength;

        [FieldOffset(4)]
        public VersionFlagsType VersionFlagsType;

        [FieldOffset(4)]
        public byte Version;
        [FieldOffset(5)]
        public byte Flags;
        [FieldOffset(6)]
        public short Type;
    }

    /// <summary>
    ///  * Flyweight for general Aeron network protocol header
    ///*
    ///* 0                   1                   2                   3
    ///* 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ///* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///* |                        Frame Length                           |
    ///* +---------------------------------------------------------------+
    ///* |  Version    |     Flags     |               Type              |
    ///* +-------------+---------------+---------------------------------+
    ///* |                       Depends on Type...
    ///*
    /// </summary>
    internal struct HeaderFlyweight
    {
        private readonly DirectBuffer _buffer;
        public static byte[] EMPTY_BUFFER = new byte[0];

        /** header type PAD */
        public const int HDR_TYPE_PAD = 0x00;
        /** header type DATA */
        public const int HDR_TYPE_DATA = 0x01;
        /** header type NAK */
        public const int HDR_TYPE_NAK = 0x02;
        /** header type SM */
        public const int HDR_TYPE_SM = 0x03;
        /** header type ERR */
        public const int HDR_TYPE_ERR = 0x04;
        /** header type SETUP */
        public const int HDR_TYPE_SETUP = 0x05;
        /** header type EXT */
        public const int HDR_TYPE_EXT = 0xFFFF;

        /** default version */
        public const byte CURRENT_VERSION = 0x0;

        public const int FRAME_LENGTH_FIELD_OFFSET = 0;
        public const int VERSION_FIELD_OFFSET = 4;
        public const int FLAGS_FIELD_OFFSET = 5;
        public const int TYPE_FIELD_OFFSET = 6;
        // NB comment out, easy to confuse with DataHeaderFlyweights member
        //public const int HEADER_LENGTH = TYPE_FIELD_OFFSET + sizeof(short);

        public HeaderFlyweight(DirectBuffer buffer)
        {
            _buffer = buffer;
        }

        public short Version
        {
            get { return (short)(_buffer.ReadByte(VERSION_FIELD_OFFSET) & 0xFF); }
            set { _buffer.WriteByte(VERSION_FIELD_OFFSET, (byte)value); }
        }

        public short Flags
        {
            get { return (short)(_buffer.ReadByte(FLAGS_FIELD_OFFSET) & 0xFF); }
            set { _buffer.WriteByte(FLAGS_FIELD_OFFSET, (byte)value); }
        }

        public int HeaderType
        {
            get { return (_buffer.ReadInt16(TYPE_FIELD_OFFSET) & 0xFFFF); }
            set { _buffer.WriteInt16(TYPE_FIELD_OFFSET, (short)value); }
        }

        public int FrameLength
        {
            get { return _buffer.ReadInt32(FRAME_LENGTH_FIELD_OFFSET); }
            set { _buffer.WriteInt32(FRAME_LENGTH_FIELD_OFFSET, (short)value); }
        }

        public unsafe Header Header
        {
            get { return *(Header*)_buffer.Data; }
            set { *(Header*)_buffer.Data = value; }
        }
    }
}