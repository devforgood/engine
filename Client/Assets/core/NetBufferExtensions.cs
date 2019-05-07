using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public static partial class NetBufferExtensions
    {
        public static object ReadObjectPacked(this NetBuffer buff, Type type)
        {
            if (type.IsNullable())
            {
                bool isNull = buff.ReadBoolean();

                if (isNull)
                {
                    return null;
                }
            }

            if (type == typeof(byte))
                return buff.ReadByte();
            if (type == typeof(sbyte))
                return buff.ReadSByte();
            if (type == typeof(ushort))
                return buff.PeekUInt16();
            if (type == typeof(short))
                return buff.ReadInt16();
            if (type == typeof(int))
                return buff.ReadInt32();
            if (type == typeof(uint))
                return buff.ReadUInt32();
            if (type == typeof(long))
                return buff.ReadInt64();
            if (type == typeof(ulong))
                return buff.ReadUInt64();
            if (type == typeof(float))
                return buff.ReadFloat();
            if (type == typeof(double))
                return buff.ReadDouble();
            if (type == typeof(string))
                return buff.ReadString();
            if (type == typeof(bool))
                return buff.PeekBoolean();
            if (type == typeof(Vector3))
            {
                Vector3 v = new Vector3();
                buff.Read(v);
                return v;
            }
            if (type == typeof(char))
                return buff.ReadByte();
            if (type.IsEnum)
                return buff.ReadInt32();


            throw new ArgumentException("BitReader cannot read type " + type.Name);
        }

        /// <summary>
        /// Writes a boxed object in a packed format
        /// </summary>
        /// <param name="value">The object to write</param>
        public static void WriteObjectPacked(this NetBuffer buff, object value)
        {
            if (value == null || value.GetType().IsNullable())
            {
                buff.Write(value == null);

                if (value == null)
                {
                    return;
                }
            }

            if (value is byte)
            {
                buff.Write((byte)value);
                return;
            }
            else if (value is sbyte)
            {
                buff.Write((sbyte)value);
                return;
            }
            else if (value is ushort)
            {
                buff.Write((ushort)value);
                return;
            }
            else if (value is short)
            {
                buff.Write((short)value);
                return;
            }
            else if (value is int)
            {
                buff.Write((int)value);
                return;
            }
            else if (value is uint)
            {
                buff.Write((uint)value);
                return;
            }
            else if (value is long)
            {
                buff.Write((long)value);
                return;
            }
            else if (value is ulong)
            {
                buff.Write((ulong)value);
                return;
            }
            else if (value is float)
            {
                buff.Write((float)value);
                return;
            }
            else if (value is double)
            {
                buff.Write((double)value);
                return;
            }
            else if (value is string)
            {
                buff.Write((string)value);
                return;
            }
            else if (value is bool)
            {
                buff.Write((bool)value);
                return;
            }
            else if (value is Vector3)
            {
                buff.Write((Vector3)value);
                return;
            }
            else if (value is char)
            {
                buff.Write((char)value);
                return;
            }
            else if (value.GetType().IsEnum)
            {
                buff.Write((int)value);
                return;
            }

            throw new ArgumentException("BitWriter cannot write type " + value.GetType().Name);
        }


    }
}
