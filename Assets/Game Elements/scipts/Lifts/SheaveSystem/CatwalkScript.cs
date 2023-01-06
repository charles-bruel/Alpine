using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatwalkScript : MonoBehaviour
{
    [Header("Mesh Data")]
    public Material Material;
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;
    public Mesh Mesh;

    [Header("Config Data")]
    public Vector3 CatwalkOffset;
    public float RadiusOffset;
    public float TubeSize;
    public float CatwalkSegmentSpacing;
    public CatwalkSegment CatwalkSegment;
    public Vector2 TextureCoords;

    [Header("Runtime Data")]
    public float Radius;
    public float RequiredAngle;
    public float RotationOffset;
    public List<CatwalkSegment> CatwalkSegments;

    public readonly int[] SegmentIndices = new int[] {
         2,  1, 10,
         9, 10,  1,
         4,  3, 12,
        11, 12,  2,
         6,  5, 14,
        13, 14,  5,
         0,  7,  8,
        15,  8,  7
    };

    public readonly int[] SegmentCapIndicies = new int[]
    {
       0, 1, 2,
       2, 3, 0,
       5, 4, 6,
       7, 6, 4
    };

    public void Generate()
    {
        GenereateMesh();
        GenerateCatwalkSegments();
    }

    private void GenereateMesh()
    {
        if (MeshRenderer == null)
        {
            MeshRenderer = gameObject.AddComponent<MeshRenderer>();
            MeshRenderer.sharedMaterial = Material;

            MeshFilter = gameObject.AddComponent<MeshFilter>();

            Mesh = new Mesh();
        }
        else
        {
            Mesh = MeshFilter.mesh;
            MeshFilter.mesh = null;
        }

        int segments = (int)(Mathf.Abs(RequiredAngle) + 3);

        Vector3[] vertices = new Vector3[(segments + 2) * 8];
        Vector2[] uvs = new Vector2[(segments + 2) * 8];
        int[] tris = new int[segments * 24 + 12];

        float sizeHalf = TubeSize / 2f;
        float pihalf = Mathf.PI / 2f;

        float radiusInner = Radius + RadiusOffset - sizeHalf;
        float radiusOuter = Radius + RadiusOffset + sizeHalf;
        float anglePerSegment = (RequiredAngle / segments) * Mathf.Deg2Rad;

        int capsVerticesBase = (segments + 1) * 8;

        for (int i = 0; i <= segments; i++)
        {
            Vector2 pos2di = new Vector2(Mathf.Cos(-anglePerSegment * i + pihalf) * radiusInner, -Mathf.Sin(-anglePerSegment * i + pihalf) * radiusInner);
            Vector2 pos2do = new Vector2(Mathf.Cos(-anglePerSegment * i + pihalf) * radiusOuter, -Mathf.Sin(-anglePerSegment * i + pihalf) * radiusOuter);

            vertices[i * 8 + 0] = new Vector3(sizeHalf, pos2di.y, pos2di.x) + CatwalkOffset;
            vertices[i * 8 + 1] = new Vector3(sizeHalf, pos2di.y, pos2di.x) + CatwalkOffset;
            vertices[i * 8 + 2] = new Vector3(sizeHalf, pos2do.y, pos2do.x) + CatwalkOffset;
            vertices[i * 8 + 3] = new Vector3(sizeHalf, pos2do.y, pos2do.x) + CatwalkOffset;
            vertices[i * 8 + 4] = new Vector3(-sizeHalf, pos2do.y, pos2do.x) + CatwalkOffset;
            vertices[i * 8 + 5] = new Vector3(-sizeHalf, pos2do.y, pos2do.x) + CatwalkOffset;
            vertices[i * 8 + 6] = new Vector3(-sizeHalf, pos2di.y, pos2di.x) + CatwalkOffset;
            vertices[i * 8 + 7] = new Vector3(-sizeHalf, pos2di.y, pos2di.x) + CatwalkOffset;

            if (i == 0)
            {
                vertices[capsVerticesBase + 0] = new Vector3(sizeHalf, pos2di.y, pos2di.x) + CatwalkOffset;
                vertices[capsVerticesBase + 1] = new Vector3(sizeHalf, pos2do.y, pos2do.x) + CatwalkOffset;
                vertices[capsVerticesBase + 2] = new Vector3(-sizeHalf, pos2do.y, pos2do.x) + CatwalkOffset;
                vertices[capsVerticesBase + 3] = new Vector3(-sizeHalf, pos2di.y, pos2di.x) + CatwalkOffset;
            }
            if (i == segments)
            {
                vertices[capsVerticesBase + 4] = new Vector3(sizeHalf, pos2di.y, pos2di.x) + CatwalkOffset;
                vertices[capsVerticesBase + 5] = new Vector3(sizeHalf, pos2do.y, pos2do.x) + CatwalkOffset;
                vertices[capsVerticesBase + 6] = new Vector3(-sizeHalf, pos2do.y, pos2do.x) + CatwalkOffset;
                vertices[capsVerticesBase + 7] = new Vector3(-sizeHalf, pos2di.y, pos2di.x) + CatwalkOffset;
            }
        }

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = TextureCoords;
        }

        for (int i = 0; i < segments; i++)
        {
            int vertexBase = i * 8;
            for (int j = 0; j < 24; j++)
            {
                tris[i * 24 + j] = vertexBase + SegmentIndices[j];
            }
        }

        int capsTrisBase = segments * 24;
        for (int i = 0; i < 12; i++)
        {
            tris[capsTrisBase + i] = capsVerticesBase + SegmentCapIndicies[i];
        }

        Mesh.Clear();
        Mesh.vertices = vertices;
        Mesh.triangles = tris;
        Mesh.uv = uvs;

        Mesh.RecalculateNormals();
        MeshFilter.mesh = Mesh;
    }

    private void GenerateCatwalkSegments()
    {
        float radius = Radius + RadiusOffset;
        float length = Mathf.Abs(RequiredAngle * Mathf.Deg2Rad) * Mathf.Abs(radius);
        int segments = (int)((length / CatwalkSegmentSpacing) + 1);

        InitializeCatwalkSegments(segments);

        float pihalf = Mathf.PI / 2f;
        float anglePerSegment = (RequiredAngle / segments) * Mathf.Deg2Rad;
        float apshalf = anglePerSegment / 2f;

        for (int i = 0; i < segments; i++)
        {
            CatwalkSegments[i].transform.localPosition = new Vector3(0, -Mathf.Sin(-anglePerSegment * i + pihalf - apshalf) * radius, Mathf.Cos(-anglePerSegment * i + pihalf - apshalf) * radius);
            CatwalkSegments[i].transform.localEulerAngles = new Vector3(-RotationOffset, 0, 0);
            CatwalkSegments[i].IsStart = i == 0;
            CatwalkSegments[i].IsEnd = i == segments - 1;
        }
    }

    private void InitializeCatwalkSegments(int numberSegments)
    {
        for (int i = 0; i < CatwalkSegments.Count; i++)
        {
            CatwalkSegments[i].gameObject.SetActive(i < numberSegments);
        }
        if (CatwalkSegments == null)
        {
            CatwalkSegments = new List<CatwalkSegment>(numberSegments);
        }
        if (CatwalkSegments.Count == numberSegments)
        {
            return;
        }
        if (numberSegments > CatwalkSegments.Count)
        {
            for (int i = CatwalkSegments.Count; i < numberSegments; i++)
            {
                CatwalkSegments.Add(Instantiate(CatwalkSegment, transform));
            }
        }
    }
}