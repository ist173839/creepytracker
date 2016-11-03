/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public static class GameObjectHelper
{
    public static GameObject ResizeMesh(GameObject objectToResize, float scale, bool recalculateNormals)
    {
        var mesh = objectToResize.GetComponent<MeshFilter>().mesh;
        var baseVertices = mesh.vertices;
        var vertices = new Vector3[baseVertices.Length];

        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = baseVertices[i];
            vertex.x = vertex.x * scale;
            vertex.y = vertex.y * scale;
            vertex.z = vertex.z * scale;

            vertices[i] = vertex;
        }

        mesh.vertices = vertices;

        if (recalculateNormals) mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return objectToResize;
    }

    public static GameObject ResizeMesh(GameObject objectToResize, Vector3 scale, bool recalculateNormals)
    {
        var mesh = objectToResize.GetComponent<MeshFilter>().mesh;
        var baseVertices = mesh.vertices;
        var vertices = new Vector3[baseVertices.Length];

        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = baseVertices[i];
            vertex.x = vertex.x * scale.x;
            vertex.y = vertex.y * scale.y;
            vertex.z = vertex.z * scale.z;

            vertices[i] = vertex;
        }

        mesh.vertices = vertices;

        if (recalculateNormals) mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return objectToResize;
    }


    public static GameObject MyCreateSphere(string name, float scale = 0.1f)
    {
        return MyCreateSphere(name, false, scale);
    }

    public static GameObject MyCreateSphere(string name, bool render, float scale = 0.1f)
    {
        return MyCreateSphere(name, Vector3.zero, render, scale);
    }

    public static GameObject MyCreateSphere(string name, Vector3 position, float scale = 0.1f)
    {
        return MyCreateSphere(name, position, false, scale);
    }

    public static GameObject MyCreateSphere(string name, Vector3 position, bool render, float scale = 0.1f)
    {
        return MyCreateSphere(name, position, false, null, render, scale);
    }

    public static GameObject MyCreateSphere(string name, Vector3 position, Transform parent, bool render, float scale = 0.1f)
    {
        return MyCreateSphere(name, position, false, parent, render, scale);
    }

    public static GameObject MyCreateSphere(string name, Vector3 position, bool isCollider, Transform parent, bool render, float scale = 0.1f)
    {
        var gameObjectSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObjectSphere.GetComponent<Collider>().enabled = isCollider;
        gameObjectSphere.GetComponent<MeshRenderer>().enabled = render;

        gameObjectSphere.transform.position = position;
        gameObjectSphere.transform.localScale = new Vector3(scale, scale, scale);
        gameObjectSphere.name = name;

        if (parent != null) gameObjectSphere.transform.parent = parent;

        return gameObjectSphere;
    }

    public static GameObject MyCreatePrimitiveObject(PrimitiveType primitiveType, string name, Vector3 position, Transform parent, bool isRender = false, bool isCollider = false, float scale = 0.1f)
    {
        return MyCreatePrimitiveObject(primitiveType, name, position, parent, new Color(), isRender, isCollider, scale);
    }

    public static GameObject MyCreatePrimitiveObject(PrimitiveType primitiveType, string name, Vector3 position, Transform parent, Color color = default(Color), bool isRender = false, bool isCollider = false, float scale = 0.1f)
    {
        var primitiveGameObject = GameObject.CreatePrimitive(primitiveType);

        primitiveGameObject.GetComponent<MeshRenderer>().enabled = isRender;
        primitiveGameObject.GetComponent<Collider>().enabled     = isCollider;
        primitiveGameObject.GetComponent<Renderer>().material.color = color;

        primitiveGameObject.transform.position = position;
        primitiveGameObject.transform.localScale = new Vector3(scale, scale, scale);

        if (parent != null) primitiveGameObject.transform.parent = parent;
        
        primitiveGameObject.name = name;
        
        return primitiveGameObject;
    }

    public static GameObject MyCreatePrimitiveObject(PrimitiveType primitiveType, string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Color color = default(Color), bool isRender = false, bool isCollider = false)
    {
        var primitiveGameObject = GameObject.CreatePrimitive(primitiveType);

        primitiveGameObject.GetComponent<MeshRenderer>().enabled = isRender;
        primitiveGameObject.GetComponent<Collider>().enabled = isCollider;
        primitiveGameObject.GetComponent<Renderer>().material.color = color;


        primitiveGameObject.transform.position   = position;
        primitiveGameObject.transform.rotation   = rotation;
        primitiveGameObject.transform.localScale = scale;

        if (parent != null) primitiveGameObject.transform.parent = parent;

        primitiveGameObject.name = name;

        return primitiveGameObject;
    }

    public static GameObject MyCreatePrimitiveObject(PrimitiveType primitiveType, string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material, Color color = default(Color), bool isRender = false, bool isCollider = false)
    {
        var primitiveGameObject = GameObject.CreatePrimitive(primitiveType);

        primitiveGameObject.GetComponent<Collider>().enabled = isCollider;
        primitiveGameObject.GetComponent<MeshRenderer>().enabled = isRender;
        primitiveGameObject.GetComponent<Renderer>().material = material;
        primitiveGameObject.GetComponent<Renderer>().material.color = color;

        primitiveGameObject.transform.position = position;
        primitiveGameObject.transform.rotation = rotation;
        primitiveGameObject.transform.localScale = scale;


        if (parent != null) primitiveGameObject.transform.parent = parent;
        primitiveGameObject.name = name;

        return primitiveGameObject;
    }

    public static GameObject MyCreatePrimitiveObjectMesh(PrimitiveType primitiveType, string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material, Color color = default(Color), bool isRender = false, bool isCollider = false)
    {
        var primitiveGameObject = GameObject.CreatePrimitive(primitiveType);
        primitiveGameObject = ResizeMesh(primitiveGameObject, 0.5f, true);

        primitiveGameObject.GetComponent<Collider>().enabled = isCollider;
        primitiveGameObject.GetComponent<MeshRenderer>().enabled = isRender;
        primitiveGameObject.GetComponent<Renderer>().material = material;
        primitiveGameObject.GetComponent<Renderer>().material.color = color;

        primitiveGameObject.transform.position = position;
        primitiveGameObject.transform.rotation = rotation;
        //primitiveGameObject.transform.localScale = scale;


        if (parent != null) primitiveGameObject.transform.parent = parent;
        primitiveGameObject.name = name;

        return primitiveGameObject;
    }


    // public static GameObject MyCreatePrimitiveObjectMesh(PrimitiveType primitiveType, string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material, Color color = default(Color), bool isRender = false, bool isCollider = false)
    //{
    //    var primitiveGameObject = GameObject.CreatePrimitive(primitiveType);
    //    primitiveGameObject = ResizeMesh(primitiveGameObject, 0.5f, true);

    //    primitiveGameObject.GetComponent<Collider>().enabled = isCollider;
    //    primitiveGameObject.GetComponent<MeshRenderer>().enabled = isRender;
    //    primitiveGameObject.GetComponent<Renderer>().material = material;
    //    primitiveGameObject.GetComponent<Renderer>().material.color = color;

    //    primitiveGameObject.transform.position = position;
    //    primitiveGameObject.transform.rotation = rotation;
    //    //primitiveGameObject.transform.localScale = scale;


    //    if (parent != null) primitiveGameObject.transform.parent = parent;
    //    primitiveGameObject.name = name;

    //    return primitiveGameObject;
    //}



    public static GameObject MyCreateObject(GameObject theObject, string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material, Color color = default(Color), bool isRender = false, bool isCollider = false)
    {
        var customGameObject = theObject;
        
        if (customGameObject.GetComponent<Collider>() != null) customGameObject.GetComponent<Collider>().enabled= isCollider;

        if (customGameObject.GetComponent<MeshRenderer>() != null)  customGameObject.GetComponent<MeshRenderer>().enabled    = isRender;

        if (customGameObject.GetComponent<Renderer>() != null)
        {
            customGameObject.GetComponent<Renderer>().material = material;
            customGameObject.GetComponent<Renderer>().material.color = color;
        }


        customGameObject.transform.position   = position;
        customGameObject.transform.rotation   = rotation;

        if (parent != null) customGameObject.transform.parent = parent;

        customGameObject.transform.localScale = scale;


        customGameObject.name = name;

        return customGameObject;
    }

    public static GameObject MyCreatePrimitiveObject(PrimitiveType primitiveType, string name, Vector3 position, Transform parent, Material material, bool isRender = false, bool isCollider = false, float scale = 0.1f)
    {
        var primitiveGameObject = GameObject.CreatePrimitive(primitiveType);

        primitiveGameObject.GetComponent<MeshRenderer>().enabled = isRender;
        primitiveGameObject.GetComponent<Collider>().enabled     = isCollider;
        primitiveGameObject.GetComponent<Renderer>().material    = material;

        primitiveGameObject.transform.position = position;
        primitiveGameObject.transform.localScale = new Vector3(scale, scale, scale);

        if (parent != null) primitiveGameObject.transform.parent = parent;

        primitiveGameObject.name = name;

        return primitiveGameObject;
    }

    public static GameObject MyCreatePrimitiveObject(PrimitiveType primitiveType, string name, Vector3 position, Transform parent, Material material, Color color = default(Color), bool isRender = false, bool isCollider = false, float scale = 0.1f)
    {
        var primitiveGameObject = GameObject.CreatePrimitive(primitiveType);

        primitiveGameObject.GetComponent<MeshRenderer>().enabled    = isRender;
        primitiveGameObject.GetComponent<Collider>().enabled        = isCollider;
        primitiveGameObject.GetComponent<Renderer>().material       = material;
        primitiveGameObject.GetComponent<Renderer>().material.color = color;

        primitiveGameObject.transform.position = position;
        primitiveGameObject.transform.localScale = new Vector3(scale, scale, scale);

        if (parent != null) primitiveGameObject.transform.parent = parent;

        primitiveGameObject.name = name;

        return primitiveGameObject;
    }


    public static GameObject MyCreateObject(GameObject theObject, string name, Vector3 position, Transform parent, Material material, Color color = default(Color), bool isRender = false, bool isCollider = false, float scale = 0.1f)
    {
        var primitiveGameObject = theObject;

        primitiveGameObject.GetComponent<MeshRenderer>().enabled    = isRender;
        primitiveGameObject.GetComponent<Collider>().enabled        = isCollider;
        primitiveGameObject.GetComponent<Renderer>().material       = material;
        primitiveGameObject.GetComponent<Renderer>().material.color = color;

        primitiveGameObject.transform.position = position;
        primitiveGameObject.transform.localScale = new Vector3(scale, scale, scale);

        if (parent != null) primitiveGameObject.transform.parent = parent;

        primitiveGameObject.name = name;

        return primitiveGameObject;
    }


    public static Mesh CreateMesh(float width, float height)
    {
        var m = new Mesh
        {
            name = "ScriptedMesh",
            vertices = new Vector3[]
            {
                new Vector3(-width, -height, 0.01f), new Vector3(width, -height, 0.01f), new Vector3(width, height, 0.01f), new Vector3(-width, height, 0.01f)
            },
            uv = new Vector2[]
            {
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)
            },
            triangles = new int[] { 0, 1, 2, 0, 2, 3 }
        };
        m.RecalculateNormals();

        return m;
    }


    public static GameObject CreatePlane(float width, float height, Material material)
    {
        return CreatePlane(width, height, material, Color.green, true);
    }

    public static GameObject CreatePlane(float width, float height, Material material, Color color, bool meshRendererEnabled)
    {
        GameObject plane = new GameObject("Plane");

        plane.transform.position = new Vector3(plane.transform.position.x, height, plane.transform.position.z);
        Plane planeComponet = new Plane(new Vector3(-width, -height, 0.01f), new Vector3(width, -height, 0.01f), new Vector3(width, height, 0.01f));

        // plane.AddComponent(typeof(Plane)); 
        // plane.AddComponent(typeof(Plane));
        // var planeCom = plane.GetComponent<Plane>();// = planeComponet;
        // planeCom = planeComponet;

        var meshFilter = (MeshFilter) plane.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = GameObjectHelper.CreateMesh(width, height);
        var meshRenderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        if (meshRenderer == null) return plane;

        // meshRenderer.material.shader = Shader.Find("Standard");
        // Texture2D tex = new Texture2D(1, 1);
        // tex.SetPixel(0, 0, Color.green);
        // tex.Apply();
        // meshRenderer.material.mainTexture = tex;

        // meshRenderer.material = (Material) Instantiate(Resources.Load("Materials/Mt_Transparent"));
        // meshRenderer.material.color = Color.green;
        // meshRenderer.enabled = true;

        meshRenderer.material = material;
        meshRenderer.material.color = color;
        meshRenderer.enabled = meshRendererEnabled;


        return plane;
    }


    public static Vector3 GetObjectPosition(Indicator ind)
    {

        return ind.ObjectIndicator.transform.position;
    }
    public static float GetObjectPosition(Indicator ind, Coordenadas coor)
    {
        switch (coor)
        {
            case Coordenadas.X:
                return ind.ObjectIndicator.transform.position.x;
            case Coordenadas.Y:
                return ind.ObjectIndicator.transform.position.y;
            case Coordenadas.Z:
                return ind.ObjectIndicator.transform.position.z;
            default:
                throw new ArgumentOutOfRangeException("coor", coor, null);
        }
        
    }
}
