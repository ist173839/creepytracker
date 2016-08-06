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


    private List<string> _rightKneeList;
    private List<string> _leftKneeList;

    private List<string> _idList;

    private Tracker _localTracker;

    private Color _colorTrack;
    private Color _colorInferred;

    private GameObject _humans;


    // Use this for initialization
    void Start () {
        _localTracker = gameObject.GetComponent<Tracker>();

        _humanList     = new List<GameObject>();
        _rightKneeList = new List<string>();
        _leftKneeList  = new List<string>();

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
	    //var countHuman     = _localTracker.CountHuman;

        var idList = _localTracker.IdList;
        // var idUpdateList = new List<string>();

        foreach (var id in idList)
        {
            //idUpdateList.Add(idHuman);
            if (GameObject.Find(id)) continue;

            var human = new GameObject { name = id };
            human.transform.parent = _humans.transform;

            _humanList.Add(human);
            _idList.Add(id);

           
            var body =  _localTracker.GetHuman(int.Parse(id)).bodies;
            foreach (var b in body)
            {
                var newBody = new GameObject { name = "Body_" + b.sensorID };
                newBody.transform.parent = human.transform;

                var newRightKnee = CreateKnees(id, b.sensorID, Knee.Right, newBody.transform);
                var newLeftKnee = CreateKnees(id, b.sensorID, Knee.Left,  newBody.transform);

                _rightKneeList.Add(newRightKnee.name);
                _leftKneeList.Add(newLeftKnee.name);

            }
        }

        var listExcept = _idList.Except(idList).ToList();
        
        foreach (var except in listExcept)
        {

            var obj = GameObject.Find(except);

            KillAllChildren(obj);

            _humanList.Remove(obj);
            _idList.Remove(except);
            Destroy(obj);
        }

        UpdateKnees(rightKneesInfo, Knee.Right);
        UpdateKnees(leftKneesInfo,  Knee.Left);
        
    }

    private void UpdateKnees(Dictionary<string, KneesInfo> theKnees, Knee thisKnee)
    {

        var tempKneeList =  new List<string>();
  

        foreach (var kneesInfo in theKnees)
        {
            var kneeName = kneesInfo.Value.IdHuman + "_" + kneesInfo.Value.IdBody + "_" + thisKnee;

            tempKneeList.Add(kneeName);

            var knee = GameObject.Find(kneeName);
            if (knee == null)
            {
                Debug.Log("Knee not found");
                continue;
            }

            knee.transform.position = kneesInfo.Value.Pos;
            
            knee.GetComponent<Renderer>().material.color = kneesInfo.Value.Track ? _colorTrack : _colorInferred;
        }

        var listExcept = thisKnee == Knee.Right ? _rightKneeList.Except(tempKneeList).ToList() : _leftKneeList.Except(tempKneeList).ToList();
        
        foreach (var except in listExcept)
        {

            var obj = GameObject.Find(except);

            KillAllChildren(obj);

            if (thisKnee == Knee.Right)
            {
                _rightKneeList.Remove(obj.name);
            }
            else
            {
                _leftKneeList.Remove(obj.name);
            }

            //_humanList.Remove(obj);
            //_idList.Remove(except);
            
            Destroy(obj);
        }



    }

    private static void KillAllChildren(GameObject obj)
    {
        for (var i = 0; i < obj.transform.childCount; i++)
        {
            Destroy(obj.transform.GetChild(i).gameObject);
        }
    }


    private static GameObject CreateKnees(string idHuman, string idBody, Knee knee, Transform parent)
    {
        return MyCreateSphere(idHuman + "_" + idBody + "_" + knee, parent, Color.white);
    }

    private GameObject CreateKnees(KneesInfo info, Knee knee, Transform parent)
    {
        var nameKnee = info.IdHuman.ToString() + "_" + knee;

        var infoColor = info.Track ? _colorTrack : _colorInferred;

        var pos = info.Pos;

        return MyCreateSphere(nameKnee, pos, parent, infoColor);
    }

    private string GetIdHuman(string nameKnee)
    {
        char[] del = { '_' };
        return nameKnee.Split(del)[0];
    }
    private string GetIdBody(string nameKnee)
    {
        char[] del = { '_' };
        return nameKnee.Split(del)[1];
    }
    private string GetKnee(string nameKnee)
    {
        char[] del = { '_' };
        return nameKnee.Split(del)[2];
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

 
     
     *
     * */
