using UnityEngine;
using System;
using System.Collections;

public class OptiObjects : MonoBehaviour {
	private Vector3 _position;
	private GameObject rigid;

	private DateTime _lastUpdated;
	private int _index;

	public DateTime lastUpdated { get { return _lastUpdated; } }
	public int index { get { return _index; } set { _index = value; }  }

	~OptiObjects() {
	}

	// Use this for initialization
	void Start () {
		rigid = CreateSphere ("rigid");
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
