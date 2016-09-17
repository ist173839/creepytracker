using UnityEngine;
using System;
using System.Collections;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class OptiObjects : MonoBehaviour {

    private GameObject _rigid;

    private Vector3 _position;

    private DateTime _lastUpdated;

    public DateTime LastUpdated
    {
        get
        {
            return _lastUpdated;
        }
    }

    public int Index { get; set; }

    ~OptiObjects() {
    }

    // Use this for initialization
    void Start () {
        _rigid = CreateSphere ("rigid");
        _lastUpdated = DateTime.Now;
    }
	
    // Update is called once per frame
    void Update () {
    }

    public void Update(Vector3 pos) {
        _position = pos;
        _lastUpdated = DateTime.Now;
    }

    private GameObject CreateSphere (string stringName, float scale = 0.1f) 
    {
        GameObject primitive = GameObject.CreatePrimitive (PrimitiveType.Sphere);
        primitive.GetComponent<SphereCollider> ().enabled = false;
        primitive.transform.parent = transform;
        primitive.transform.localScale = new Vector3 (scale, scale, scale);
        primitive.name = stringName;
        return primitive;
    }
}
