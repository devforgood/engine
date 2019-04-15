using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class MouseClient : core.Mouse
{
    public static new core.GameObject StaticCreate() { return new MouseClient(); }

    protected MouseClient()
    {

    }
}
