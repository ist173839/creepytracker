using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



[RequireComponent(typeof(Tracker))]
public class MyKnees : MonoBehaviour {

    public enum Knee
    {
        Right,
        Left
    }

    private List<GameObject> _humanList;

    private List<string> _idList;

    private Tracker _localTracker;

    private Color _colorTrack;
    private Color _colorInferred;

    private GameObject _humans;


    // Use this for initialization
    void Start () {
        _localTracker = gameObject.GetComponent<Tracker>();

        _humanList = new List<GameObject>();
        _idList    = new List<string>();

        _colorTrack = Color.blue;
        _colorTrack.a = 0.5f;

        _colorInferred = Color.green;
        _colorInferred.a = 0.5f;
        
        _humans = new GameObject { name = "Humans" };
        _humans.transform.parent = transform;
    }
	
	// Update is called once per frame
	void Update ()
    {
        var rightKneesInfo = _localTracker.RightKneesInfo;
        var leftKneesInfo  = _localTracker.LeftKneesInfo;
	    var countHuman     = _localTracker.CountHuman;

        var idList = _localTracker.IdList;
        // var idUpdateList = new List<string>();

        foreach (var id in idList)
        {
            //idUpdateList.Add(id);
            if (GameObject.Find(id)) continue;
            var human = new GameObject { name = id };
            human.transform.parent = _humans.transform;
            _humanList.Add(human);
            _idList.Add(id);


        }

        var listExcept = _idList.Except(idList).ToList();
        
        foreach (var except in listExcept)
        {
            var obj = GameObject.Find(except);
            _humanList.Remove(obj);
            _idList.Remove(except);
            Destroy(obj);
        }
        
        foreach (var rightKnee in rightKneesInfo)
        {
            
        }

        foreach (var leftKnee in leftKneesInfo)
	    {
	        
	    }
    }


    private GameObject CreateKnees(KneesInfo info, Knee knee)
    {
        var nameKnee = info.Id.ToString() + "_" + knee;

        var infoColor = info.Track ? _colorTrack : _colorInferred;

        var pos = info.Pos;

        return MyCreateSphere(nameKnee, pos, infoColor);

    }

    private string GetId(string nameId)
    {
        char[] del = { '_' };
        return nameId.Split(del)[0];
    }


    private static GameObject MyCreateSphere(string name, Vector3 position, Transform parent, float scale = 0.1f)
    {
        return MyCreateSphere(name, position, parent, Color.blue, scale);
    }

    private static GameObject MyCreateSphere(string name, Transform parent, Color cor, float scale = 0.1f)
    {
        return MyCreateSphere(name, Vector3.zero, parent, cor, scale);
    }

    private static GameObject MyCreateSphere(string name, float scale = 0.1f)
    {
        var gameObjectSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObjectSphere.GetComponent<SphereCollider>().enabled = false;
        gameObjectSphere.GetComponent<Renderer>().material = (Material)Instantiate(Resources.Load("Materials/Mt_Transparent"));
        gameObjectSphere.transform.localScale = new Vector3(scale, scale, scale);
        gameObjectSphere.name = name;

        return gameObjectSphere;
    }

    private static GameObject MyCreateSphere(string name, Vector3 position, Transform parent, Color cor, float scale = 0.1f)
    {
        var gameObjectSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObjectSphere.GetComponent<SphereCollider>().enabled = false;
        gameObjectSphere.GetComponent<Renderer>().material = (Material)Instantiate(Resources.Load("Materials/Mt_Transparent"));
        gameObjectSphere.GetComponent<Renderer>().material.color = cor;
        gameObjectSphere.transform.localScale = new Vector3(scale, scale, scale);
        gameObjectSphere.transform.position = position;
        gameObjectSphere.transform.parent = parent;
        gameObjectSphere.name = name;
        return gameObjectSphere;
    }

    private static GameObject MyCreateSphere(string name, Vector3 position, Color cor, float scale = 0.1f)
    {
        var gameObjectSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObjectSphere.GetComponent<SphereCollider>().enabled = false;
        gameObjectSphere.GetComponent<Renderer>().material = (Material)Instantiate(Resources.Load("Materials/Mt_Transparent"));
        gameObjectSphere.GetComponent<Renderer>().material.color = cor;
        gameObjectSphere.transform.localScale = new Vector3(scale, scale, scale);
        gameObjectSphere.transform.position = position;
        gameObjectSphere.name = name;
       
        return gameObjectSphere;
    }


}

/*
 * 
 * 
        foreach (var h in _humanList)
        {
            idUpdateList.Add(h.name);
            if (idList.Contains(h.name))
            {
            }
        }

        if (idList.Contains(h.name))
           {

           }
           else
           {

           }
        */
