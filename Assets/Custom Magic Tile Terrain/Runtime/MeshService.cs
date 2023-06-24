using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomMagicSandbox.TileTerrain
{
  public static class MeshService
  {
    //public static Mesh ToUnityMesh(this TMeshDTO tmesh)
    //{
    //  var mesh = new Mesh();

    //  //TODO: Calculate this in the factory
    //  var tempVerts = new List<Vector3>();
    //  var tempTriangles = new List<int>();
    //  var tempColors = new List<Color>();

    //  for (int i = 0; i < tmesh.Quads.Count; i++)
    //  {
    //    int offset = i * 4;
    //    foreach (var triangle in tmesh.Quads[i].Triangles)
    //    {
    //      tempVerts.Add(triangle.Vertices[offset + 0].Position);
    //      tempColors.Add(triangle.Vertices[offset + 0].Color);

    //      tempVerts.Add(triangle.Vertices[offset + 1].Position);
    //      tempColors.Add(triangle.Vertices[offset + 1].Color);

    //      tempVerts.Add(triangle.Vertices[offset + 2].Position);
    //      tempColors.Add(triangle.Vertices[offset + 2].Color);
    //      tempTriangles.AddRange(new int[] { offset + 0, offset + 1, offset + 2 });
          

    //      tempVerts.Add(triangle.Vertices[offset + 2].Position);
    //      tempColors.Add(triangle.Vertices[offset + 2].Color);

    //      tempVerts.Add(triangle.Vertices[offset + 1].Position);
    //      tempColors.Add(triangle.Vertices[offset + 1].Color);

    //      tempVerts.Add(triangle.Vertices[offset + 3].Position);
    //      tempColors.Add(triangle.Vertices[offset + 3].Color);
    //      tempTriangles.AddRange(new int[] { offset + 2, offset + 1, offset + 3 });
    //    }
    //  }

    //  mesh.vertices = tmesh.vertices;
    //  mesh.triangles = tmesh.triangles;
    //  mesh.normals = tmesh.normals;

    //  mesh.SetColors(tempColors.ToArray());
    //}
  }
}