using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testMesh : MonoBehaviour
{
    Mesh mesh;
    Vector3[] verts;
    Vector3 vertPos;
    GameObject[] handles;
    public MeshCollider mc;
    // Start is called before the first frame update
    void Start()
    {
        //var mesh = GetComponent<MeshCollider>().sharedMesh;
        //var verts = mesh.vertices;
        //foreach (Vector3 vert in verts)
        //{
        //    var vertPos = transform.TransformPoint(vert);
        //    GameObject handle = new GameObject("handle");
        //    handle.transform.position = vertPos;
        //    handle.transform.parent = transform;
        //    handle.tag = "handle";
        //    //handle.AddComponent<Gizmo_Sphere>();
        //    print(vertPos);
        //}
    }

    // Update is called once per frame

    float GetRound(float a)
    {
        return (float)(Mathf.Round(a * 1000) * 0.001f);
    }
    void Update()
    {

        var mesh = GetComponent<MeshCollider>().sharedMesh;
        var verts = mesh.vertices;
        string str2 = "";
        foreach (var v in verts)
        {
            str2 += GetRound(v.x) + "f, " + GetRound(v.y) + "f, " + GetRound(v.z) + "f\n";

            Debug.DrawLine(v, v);
        }

        //for(int i=0;i<verts.Length;++i)
        //{
        //    for(int j=1;j<verts.Length;++j)
        //    {
        //        if (i != j)
        //        {
        //            Debug.DrawLine(verts[i], verts[j]);
        //        }
        //    }
        //}


        for (int k = 0; k < mesh.subMeshCount; ++k)
        {
            var Indices = mesh.GetIndices(k);
            string str = "";
            foreach (var i in Indices)
            {
                str += i + ", ";
            }

            int i1, i2;
            for(i1=0, i2=1;i2< Indices.Length;++i1, ++i2)
            {
                Debug.DrawLine(verts[Indices[i1]], verts[Indices[i2]]);
            }
        }
        //foreach (Vector3 vert in verts)
        //{
        //    vertPos = transform.TransformPoint(vert);
        //    Debug.Log("vertPos ====== " + vertPos);
        //}


    }
}
