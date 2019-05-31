using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class CProp : core.Prop
{ 
    public static new core.NetGameObject StaticCreate(byte worldId) { return new CProp(); }

    protected CProp()
    {

    }
}
