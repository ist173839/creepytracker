using UnityEngine;
using System.Collections;

public class ApplyTransformation : MonoBehaviour {
    public Matrix4x4 transformMatrix = Matrix4x4.identity;
    public Vector3 scaledPosition = Vector3.zero;
    public Vector3 transformPosition = Vector3.zero;
	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void CalcTransformPosition()
    {
        Vector3 pos3aux = scaledPosition;
        Vector4 pos = transformMatrix * pos3aux;
        transformPosition = new Vector3(pos.x, pos.y, pos.z);
    }
}
