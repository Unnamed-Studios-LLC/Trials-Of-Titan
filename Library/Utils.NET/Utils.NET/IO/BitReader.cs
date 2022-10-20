using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Net;

namespace Utils.NET.IO
{
    public class BitReader
    {
        /// <summary>
        /// Scratch value used to read values
        /// </summary>
        private ulong scratch = 0;

        /// <summary>
        /// Length of bits left in the scratch value
        /// </summary>
        private int scratchBits = 0;

        /// <summary>
        /// Bit data buffer
        /// </summary>
        private uint[] buffer;

        /// <summary>
        /// The amount of bits read from the bit data buffer
        /// </summary>
        private long bitsRead = 0;

        public BitReader(byte[] data, int size)
        {
            InitBuffer(data, size);
        }

        public BitReader(Buffer buffer)
        {
            InitBuffer(buffer.data, buffer.size);
        }

        private void InitBuffer(byte[] data, int size)
        {
            buffer = new uint[(size + 3) / 4];
            int intByteCount = (size / 4) * 4;
            System.Buffer.BlockCopy(data, 0, buffer, 0, intByteCount);
            int remainder = size % 4;
            if (remainder > 0)
            {
                uint val = 0;
                for (int i = 0; i < remainder; i++)
                {
                    int shift = i * 8;
                    val |= (uint)data[intByteCount + (remainder - 1 - i)] << shift;
                }
                buffer[buffer.Length - 1] = val;
            }
        }

        /// <summary>
        /// Gets data from the buffer and adds 32 bits to the scratch
        /// </summary>
        private void FillScratch()
        {
            ulong bits = bitsRead < buffer.Length * 32 ? buffer[bitsRead / 32] : 0;
            bitsRead += 32;

            scratch = scratch | (bits << scratchBits);
            scratchBits += 32;
        }

        /// <summary>
        /// Reads bits from the buffer
        /// </summary>
        /// <param name="length">The amount of bits to read (max 32)</param>
        /// <returns>The bits read</returns>
        public uint Read(byte length)
        {
            if (scratchBits < 32)
            {
                FillScratch();
            }

            int shift = 32 + 32 - length;
            uint bits = (uint)((scratch << shift) >> shift);

            scratch = scratch >> length;
            scratchBits -= length;

            return bits;
        }

        public bool ReadBool() => Read(1) == 1;

        public byte ReadUInt8() => (byte)Read(8);
        public ushort ReadUInt16() => (ushort)Read(16);
        public uint ReadUInt32() => Read(32);
        public ulong ReadUInt64()
        {
            ulong a = Read(32);
            ulong b = ((ulong)Read(32) << 32);
            return a | b;
        }

        public sbyte ReadInt8() => (sbyte)Read(8);
        public short ReadInt16() => (short)Read(16);
        public int ReadInt32() => (int)Read(32);
        public long ReadInt64() => (long)Read(32) | ((long)Read(32) << 32);

        public float ReadFloat()
        {
            uint intVal = Read(32);
            unsafe
            {
                return *((float*)&intVal);
            }
        }

        public double ReadDouble()
        {
            ulong intVal = ReadUInt64();
            unsafe
            {
                return *((double*)&intVal);
            }
        }

        public DateTime ReadDateTime() => DateTime.SpecifyKind(new DateTime(ReadInt64()), DateTimeKind.Utc).ToLocalTime();

        public string ReadUTF(int maxSize)
        {
            var length = ReadUInt16();
            if (length > maxSize)
                throw new LengthCheckFailedException("Size of string received is larger than the expected length");
            byte[] bytes = ReadBytes(length);
            return Encoding.UTF8.GetString(bytes, 0, length);
        }

        public byte[] ReadBytes(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
                bytes[i] = ReadUInt8();
            return bytes;
        }

        public Vec2 ReadVec2()
        {
            return new Vec2(ReadFloat(), ReadFloat());
        }
    }
}
