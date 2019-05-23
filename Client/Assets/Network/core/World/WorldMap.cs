using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    class WorldMap
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct WorldMapKey
        {
            [FieldOffset(0)]
            public UInt64 pos;

            [FieldOffset(0)]
            public short x;

            [FieldOffset(1)]
            public short y;

            [FieldOffset(2)]
            public short z;

            [FieldOffset(3)]
            public short group_id;
        }


        Dictionary<UInt64, Tile> map = new Dictionary<ulong, Tile>();

        void InitMap()
        {

        }

        //UInt64 MakeKey(short x, short y, short z)
        //{
        //    byte[] recbytes = new byte[8];
        //    recbytes[0] = BitConverter.GetBytes(x)[0];
        //    recbytes[1] = BitConverter.GetBytes(x)[1];
        //    recbytes[2] = BitConverter.GetBytes(y)[0];
        //    recbytes[3] = BitConverter.GetBytes(y)[1];
        //    recbytes[4] = BitConverter.GetBytes(z)[0];
        //    recbytes[5] = BitConverter.GetBytes(z)[1];
        //    return BitConverter.ToUInt64(recbytes, 0);
        //}

        //void GetPositionFromKey(UInt64 key, out short x, out short y, out short z)
        //{
        //    byte[] bytes = BitConverter.GetBytes(key);
        //    x = BitConverter.ToInt16(bytes, 0);
        //    y = BitConverter.ToInt16(bytes, 2);
        //    z = BitConverter.ToInt16(bytes, 4);
        //}

        Tile GetTile(short x, short y, short z)
        {
            WorldMapKey key;
            key.pos = 0;
            key.x = x;
            key.y = y;
            key.z = z;

            Tile t = null;
            map.TryGetValue(key.pos, out t);
            return t;
        }

        public void ChangeLocation(int network_id, Vector3 old_pos, Vector3 new_pos)
        {
            var x = (short)Math.Round(new_pos.mX);
            var y = (short)Math.Round(new_pos.mY);
            var z = (short)Math.Round(new_pos.mZ);

            var old_x = (short)Math.Round(old_pos.mX);
            var old_y = (short)Math.Round(old_pos.mY);
            var old_z = (short)Math.Round(old_pos.mZ);

            if ( old_x == x && old_y == y && old_z == z )
            {
                return;
            }

            var tile = GetTile(old_x, old_y, old_z);
            if(tile == null)
            {
                return;
            }
        }
    }
}
