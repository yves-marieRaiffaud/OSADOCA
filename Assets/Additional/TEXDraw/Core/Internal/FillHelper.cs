using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq.Expressions;

namespace TexDrawLib
{
    public class FillHelper
    {
        // The specs that used in TEXDraw are ...
        public List<Vector3> m_Positions = new List<Vector3>(); // Character (verts) position (XYZ)
        public List<Color32> m_Colors = new List<Color32>();    // Character colors (RGBA)
        public List<Vector2> m_Uv0S = new List<Vector2>();      // Contain primary map to each font Texture (UV1)
        public List<Vector2> m_Uv1S = new List<Vector2>();      // Additional map (like Filling) (UV2)
        public List<Vector2> m_Uv2S = new List<Vector2>();      // Additional map (like Filling) (UV2)
        public List<int> m_Indicies = new List<int>();          // Usual triangle list data (Index)

        public int vertexcount { get { return m_Positions.Count; } }

        public int indicecount { get { return m_Indicies.Count; } }

        public void Clear()
        {
            m_Positions.Clear();
            m_Colors.Clear();
            m_Uv0S.Clear();
            m_Uv1S.Clear();
            m_Uv2S.Clear();
            m_Indicies.Clear();
        }

        public void FillMesh(Mesh mesh)
        {
            mesh.Clear();

            if (vertexcount >= 0xFFFF)
                throw new System.ArgumentException("Mesh can not have more than 65000 vertices");

            mesh.SetVertices(m_Positions);
            mesh.SetColors(m_Colors);
            mesh.SetUVs(0, m_Uv0S);
            mesh.SetUVs(1, m_Uv1S);
            mesh.SetUVs(2, m_Uv2S);
            mesh.SetTriangles(m_Indicies, 0, true);
        }

        public void AddVert(Vector3 position, Color32 color, Vector2 uv0, Vector2 uv1)
        {
            m_Positions.Add(position);
            m_Colors.Add(color);
            m_Uv0S.Add(uv0);
            m_Uv1S.Add(uv1);
            m_Uv2S.Add(new Vector2());
        }

        public void SetUV2(Vector2 uv, int idx)
        {
            m_Uv2S[idx] = uv;
        }

        public void AddTriangle(int idx0, int idx1, int idx2)
        {
            m_Indicies.Add(idx0);
            m_Indicies.Add(idx1);
            m_Indicies.Add(idx2);
        }

        public void AddQuad ()
        {
            var n = vertexcount;
            AddTriangle(n - 2, n - 3, n - 4);
            AddTriangle(n - 1, n - 2, n - 4);
        }
    }
}