using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core
{
    public partial class World
    {
        int WORLD_X = 1000;
        int WORLD_Y = 1000;
        int WORLD_Z = 1000;

        Tile[,,] map = null;

        void InitMap()
        {
            map = new Tile[WORLD_X, WORLD_Y, WORLD_Z];
        }

    }
}

