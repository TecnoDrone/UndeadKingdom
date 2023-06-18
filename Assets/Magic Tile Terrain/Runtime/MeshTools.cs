using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSandbox.TileTerrain
{
    public static class MeshTools
    {
        public static Vector2[] GetQuadUV(Vector2Int textureGridPosition, int textureGridSize)
        {
            float uvSnap = 1 / textureGridSize;//UV wird zwischen 0-1.

            //Return UV points clockwise from bottom left
            // t1 (0,1,2) t2 (2,1,3)
            Vector2[] _uvQuad = new Vector2[]{
              new Vector2((float)textureGridPosition.x * uvSnap, (float)textureGridPosition.y * uvSnap),
              new Vector2((float)textureGridPosition.x * uvSnap, (float)textureGridPosition.y * uvSnap + uvSnap),
              new Vector2((float)textureGridPosition.x * uvSnap + uvSnap, (float)textureGridPosition.y * uvSnap + uvSnap),
              new Vector2((float)textureGridPosition.x * uvSnap + uvSnap, (float)textureGridPosition.y * uvSnap),
          };

            return _uvQuad;
        }

        /*    //Returns a list of Quads of a given mesh.
           public static List<Quad> CalculateQuads(Mesh mesh)
           {
               List<Quad> _quads = new List<Quad>();

               List<int> _triIndices = new List<int>(mesh.triangles);

               int _i = 0;
               foreach (int _triIndex in _triIndices)
               {
                   if (_i + 5 >= _triIndices.Count)
                       break;

                   //Grab the next two tris and check if they form a quad.
                   int _start = _i;
                   Debug.Log(_start);
                   int[] _tri1 = new int[] { _triIndices[_start], _triIndices[_start + 1], _triIndices[_start + 2] };
                   int[] _tri2 = new int[] { _triIndices[_start + 3], _triIndices[_start + 4], _triIndices[_start + 5] };
                   Triangle _t1 = new Triangle(_tri1);
                   Triangle _t2 = new Triangle(_tri2);
                   if (_t1.IsQuad(_t2))
                   {
                       _quads.Add(new Quad(_t1, _t2));
                       _i += 6; //Jump to the next Quad
                       continue;
                   }

                   //Jump to the next Tri
                   _i += 3;
               }

               return _quads;
           } */

        public static Vector3 GetTriangleInpoint(Vector3[] triangle)
        {
            //Get the 3 sides of the triangle
            float _sideA = Vector3.Distance(triangle[1], triangle[2]);
            float _sideB = Vector3.Distance(triangle[0], triangle[2]);
            float _sideC = Vector3.Distance(triangle[0], triangle[1]);
            float _sum = _sideA + _sideB + _sideC;

            float _x = _sideA * triangle[0].x + _sideB * triangle[1].x + _sideC * triangle[2].x;
            float _y = _sideA * triangle[0].y + _sideB * triangle[1].y + _sideC * triangle[2].y;
            float _z = _sideA * triangle[0].z + _sideB * triangle[1].z + _sideC * triangle[2].z;

            return new Vector3(_x / _sum, _y / _sum, _z / _sum);
        }

        public static Vector3 GetQuadCenter(Vector3[] quad)
        {
            //We exploit the fact that the first 3 verts of a quad are a triangle and just return the center of the Hypotenuse
            return (quad[1] + quad[2]) * 0.5f;
        }
    }
}