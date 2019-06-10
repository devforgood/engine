using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CProp : core.Prop
{ 
    public static new core.NetGameObject StaticCreate(byte worldId) { return new CProp(); }

    public GameObject mTarget = null;

    protected CProp()
    {

    }

    public override void CompleteCreate()
    {
        Debug.Log("create prop " + GetNetworkId());

        GameObject go = Resources.Load("WoodBox") as GameObject;
        if (go == null)
        {
            Debug.Log("error load prefab");
            return;
        }

        GameObject bomb = GameObject.Instantiate(go, GetLocation(), go.transform.rotation);
        mTarget = bomb;
    }

    public override void HandleDying()
    {
        base.HandleDying();
        if (mTarget != null)
            GameObject.Destroy(mTarget, 0.3f);
    }
}
