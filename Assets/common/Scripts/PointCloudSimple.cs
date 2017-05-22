﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once CheckNamespace
public class PointCloudSimple : MonoBehaviour
{
    List<Vector3> _pointsH;
    List<int>     _indH;
    List<Color>   _colorsH;

    List<Vector3> _pointsL;
    List<int>     _indL;
    List<Color>   _colorsL;

    Mesh[] highres_cloud;
    Mesh[] lowres_cloud;

    uint id;

    int highres_nclouds = 0;
    int lowres_nclouds  = 0;
    int pointCount;
    int l;
    int h;

    Vector3[] posBucket;
    Color[] colBucket;
    
    byte[] buffer;

    void ReadFileWithColor(string f)
    {
        FileStream fs = new FileStream(f, FileMode.Open);
        StreamReader sr = new StreamReader(fs);

        List<byte> pts = new List<byte>();

        string line = "";
        int i = 0;

        while (!sr.EndOfStream)
        {
            line = sr.ReadLine();
            line = line.Replace(",", ".");
            char[] sep = { ' ' };
            string[] lin = line.Split(sep);

            float x = float.Parse(lin[0]);
            float y = float.Parse(lin[1]);
            float z = float.Parse(lin[2]);

            byte[] xb = System.BitConverter.GetBytes(x);
            byte[] yb = System.BitConverter.GetBytes(y);
            byte[] zb = System.BitConverter.GetBytes(z);

            byte r = byte.Parse(lin[5]);
            byte g = byte.Parse(lin[6]);
            byte b = byte.Parse(lin[7]);
            i++;
            pts.AddRange(xb);
            pts.AddRange(yb);
            pts.AddRange(zb);
            pts.Add(r);
            pts.Add(g);
            pts.Add(b);
            pts.Add(1);
        }

        SetPoints(pts.ToArray(), 0, ++id, i);
        setToView();
    }

    [StructLayout(LayoutKind.Explicit)]
    struct UnionArray
    {
        [FieldOffset(0)]
        public byte[] Bytes;

        [FieldOffset(0)]
        public float[] Floats;

    }

    int _countPack = 0;
    public void SetPoints(byte[] receivedBytes, int step, uint newid, int size)
    {
        _pointsH = new List<Vector3>();
        _colorsH = new List<Color>();
        _indH    = new List<int>();
        _pointsL = new List<Vector3>();
        _indL    = new List<int>();
        _colorsL = new List<Color>();

        UnionArray rec = new UnionArray { Bytes = receivedBytes };

        if (newid > id)
        {
            id = newid;

            for (int a = 0; a < 4; a++)
            {
                lowres_cloud[a] = new Mesh();
                highres_cloud[a] = new Mesh();
            }

            highres_nclouds = 0;
            lowres_nclouds = 0;
            l = 0;
            h = 0;
            pointCount = 0;
            _countPack = 0;
        }
        else if (newid == id)
        {
            _countPack++;
            _pointsL.AddRange(lowres_cloud[lowres_nclouds].vertices);
            _colorsL.AddRange(lowres_cloud[lowres_nclouds].colors);
            _indL.AddRange(lowres_cloud[lowres_nclouds].GetIndices(0));

            _pointsH.AddRange(highres_cloud[highres_nclouds].vertices);
            _colorsH.AddRange((highres_cloud[highres_nclouds].colors));
            _indH.AddRange(highres_cloud[highres_nclouds].GetIndices(0));

            l = _pointsL.Count;
            h = _pointsH.Count;

        }
        else
        {
            Debug.Log("Old packet" + newid);
            return;
        }

        float x, y, z;
        byte r, g, b;

        for (int i = step; i < size; i += 16) // Each point is represented by 16 bytes.
        {
            try
            {
                if (i + 15 > receivedBytes.Length) break; // Insurance.
                int floatindex = (int)(i / 4.0);
                x = rec.Floats[floatindex];
                y = rec.Floats[floatindex + 1];
                z = rec.Floats[floatindex + 2];

                r = receivedBytes[i + 12]; // r
                g = receivedBytes[i + 13]; // g
                b = receivedBytes[i + 14]; // b

                Vector3 pos = posBucket[pointCount];
                pos.Set(x, y, z);
                Color c = colBucket[pointCount++];
                c.r = (float) r / 255;
                c.g = (float) g / 255;
                c.b = (float) b / 255;

                if (receivedBytes[i + 15] == 1)// If it's a HR point, save it to the high resolution points.
                {
                    _pointsH.Add(pos);
                    _colorsH.Add(c);
                    _indH.Add(h++);
                }
                else
                {
                    _pointsL.Add(pos);
                    _colorsL.Add(c);
                    _indL.Add(l++);
                }

                if (h == 65000)
                {
                    highres_cloud[highres_nclouds].vertices = _pointsH.ToArray();
                    highres_cloud[highres_nclouds].colors = _colorsH.ToArray();
                    highres_cloud[highres_nclouds].SetIndices(_indH.ToArray(), MeshTopology.Points, 0);

                    h = 0;
                    _pointsH = new List<Vector3>();
                    _colorsH = new List<Color>();
                    _indH = new List<int>();
                    highres_nclouds++;
                }

                if (l == 65000)
                {
                    lowres_cloud[lowres_nclouds].vertices = _pointsL.ToArray();
                    lowres_cloud[lowres_nclouds].colors = _colorsL.ToArray();
                    lowres_cloud[lowres_nclouds].SetIndices(_indL.ToArray(), MeshTopology.Points, 0);

                    l = 0;
                    _pointsL = new List<Vector3>();
                    _indL = new List<int>();
                    _colorsL = new List<Color>();
                    lowres_nclouds++;
                }
            }
            catch (System.Exception exc)
            {
                Debug.Log("Reached out of the array: " + exc.StackTrace);
            }
        }

        highres_cloud[highres_nclouds].vertices = _pointsH.ToArray();
        highres_cloud[highres_nclouds].colors = _colorsH.ToArray();
        highres_cloud[highres_nclouds].SetIndices(_indH.ToArray(), MeshTopology.Points, 0);

        lowres_cloud[lowres_nclouds].vertices = _pointsL.ToArray();
        lowres_cloud[lowres_nclouds].colors = _colorsL.ToArray();
        lowres_cloud[lowres_nclouds].SetIndices(_indL.ToArray(), MeshTopology.Points, 0);
    }

    public void setToView()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        // Note that there are 8 MeshFilter -> [HR HR HR HR LR LR LR LR]
        int lr = lowres_nclouds + 4;  // Therefore, the low resolution clouds start at index 4
        for (int i = 0; i < filters.Length; i++)
        {
            MeshFilter mf = filters[i];
            if (i <= highres_nclouds)
            {
                mf.mesh = highres_cloud[i];
            }
            else if (i <= lr && i >= 4)
            {
                mf.mesh = lowres_cloud[i - 4];
            }
            else
            {
                mf.mesh.Clear();
            }
        }
    }

    public void hideFromView()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in filters)
        {
            mf.mesh.Clear();
        }
    }

    // Use this for initialization
    void Start()
    {
        // Material for the high resolution points
        Material mat = Resources.Load("Materials/cloudmat") as Material;
        // Material for the low resolution points
        Material other = Instantiate(mat) as Material;

        // Update size for each material.
        mat.SetFloat("_Size", 3);  // HR
        other.SetFloat("_Size", 5); // LR

        for (int i = 0; i < 4; i++)
        {
            GameObject a = new GameObject("highres_cloud" + i);
            MeshRenderer mr = a.AddComponent<MeshRenderer>();
            mr.material = mat;
            a.transform.parent = this.gameObject.transform;
            a.transform.localPosition = Vector3.zero;
            a.transform.localRotation = Quaternion.identity;
            a.transform.localScale = new Vector3(1, 1, 1);
        }
        for (int i = 0; i < 4; i++)
        {
            GameObject a = new GameObject("lowres_cloud" + i);
            MeshRenderer mr = a.AddComponent<MeshRenderer>();
            mr.material = other;
            a.transform.parent = this.gameObject.transform;
            a.transform.localPosition = Vector3.zero;
            a.transform.localRotation = Quaternion.identity;
            a.transform.localScale = new Vector3(1, 1, 1);
        }
        _pointsH = new List<Vector3>();
        _colorsH = new List<Color>();
        _indH = new List<int>();
        _pointsL = new List<Vector3>();
        _indL = new List<int>();
        _colorsL = new List<Color>();
        highres_cloud = new Mesh[4];
        lowres_cloud = new Mesh[4];
        for (int a = 0; a < 4; a++)
        {
            lowres_cloud[a] = new Mesh();
            highres_cloud[a] = new Mesh();
        }
        colBucket = new Color[217088];
        posBucket = new Vector3[217088];
        for (int i = 0; i < 217088; i++)
        {
            posBucket[i] = new Vector3();
            colBucket[i] = new Color();
        }

        pointCount = 0;
        l = 0;
        h = 0;
    }
}
////////////////////////////////////////////////////////////////////
/*


//void readFileWithColor(string f)


 */
