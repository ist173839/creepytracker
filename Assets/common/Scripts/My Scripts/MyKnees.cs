using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Tracker))]
public class MyKnees : MonoBehaviour {

    public enum Knee
    {
        Right,
        Left
    }


    public enum OtherKnee
    {
        Mean,
        Close
    }

    private List<GameObject> _humanList;


    private List<string> _rightKneeList;
    private List<string> _leftKneeList;
    private List<string> _meanKneeList;

    private List<string> _idList;

    private Tracker _localTracker;

    private Color _colorTrack;
    private Color _colorInferred;

    private Color _colorMean;
    private Color _colorCloser;

    private GameObject _humans;


    public bool Track;


    private Vector3? _lastRigthPosition;
    private Vector3? _lastLeftPosition;

    // Use this for initialization
    void Start () {
        _localTracker = gameObject.GetComponent<Tracker>();

        _humanList     = new List<GameObject>();

        _rightKneeList = new List<string>();
        _leftKneeList  = new List<string>();
        _meanKneeList  = new List<string>();


        _idList    = new List<string>();

        _colorTrack = Color.blue;
        _colorTrack.a = 0.5f;

        _colorInferred = Color.green;
        _colorInferred.a = 0.5f;

        _colorMean = Color.yellow;
        _colorMean.a = 0.5f;

        _colorCloser = Color.black;
        _colorCloser.a = 0.5f;

        _humans = new GameObject { name = "Humans" };
        _humans.transform.parent = transform;


        _lastRigthPosition = null;
        _lastLeftPosition  = null;
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

            var newMean = new GameObject { name = "Mean" };
            newMean.transform.parent = human.transform;

            CreateKnees(human.name, OtherKnee.Mean, Knee.Right, newMean.transform);
            CreateKnees(human.name, OtherKnee.Mean, Knee.Left,  newMean.transform);

            var newClose = new GameObject { name = "Close" };
            newClose.transform.parent = human.transform;

            CreateKnees(human.name, OtherKnee.Close, Knee.Right, newClose.transform);
            CreateKnees(human.name, OtherKnee.Close, Knee.Left,  newClose.transform);
            
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


        UpdateKnees(rightKneesInfo, _idList, Knee.Right, OtherKnee.Mean);
        UpdateKnees(leftKneesInfo,  _idList, Knee.Left,  OtherKnee.Mean);




	    //if (_lastRigthPosition == null)
	    //{

     //       _lastRigthPosition =  


     //   }
     ////  ;
     //   _lastLeftPosition = null;

        UpdateKnees(rightKneesInfo, _idList, Knee.Right, OtherKnee.Close);
        UpdateKnees(leftKneesInfo,  _idList, Knee.Left,  OtherKnee.Close);



    }

    private void UpdateKnees(Dictionary<string, KneesInfo> theKnees, List<string> idList, Knee thisKnee, OtherKnee otherKnee)
    {
        foreach (var idHuman in idList)
        {
            var kneeName = idHuman + "_" + otherKnee + "_" + thisKnee;

            var knee = GameObject.Find(kneeName);
            if (knee == null)
            {
                Debug.Log("Knee not found");
                continue;
            }
            
            switch (otherKnee)
            {
                case OtherKnee.Mean:
                    knee.transform.position = GetMeanList(theKnees, Track);
                    break;
                case OtherKnee.Close:
                    switch (thisKnee)
                    {
                        case Knee.Right:
                            knee.transform.position = CloseKnee(theKnees, _localTracker, Knee.Right, Track, idHuman, _lastRigthPosition);
                            break;
                        case Knee.Left:
                            knee.transform.position = CloseKnee(theKnees, _localTracker, Knee.Left, Track, idHuman, _lastLeftPosition);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("thisKnee", thisKnee, null);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("otherKnee", otherKnee, null);
            }


            //   knee.transform.position = kneesInfo.Value.Pos.Value;

            knee.GetComponent<Renderer>().material.color = otherKnee == OtherKnee.Mean ? _colorMean : _colorCloser;
        }
    }

    private void UpdateKnees(Dictionary<string, KneesInfo> theKnees, Knee thisKnee)
    {
        var tempKneeList = new List<string>();

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

            knee.transform.position = kneesInfo.Value.Pos.Value;

            knee.GetComponent<Renderer>().material.color = kneesInfo.Value.Track ? _colorTrack : _colorInferred;
        }

        var listExcept = thisKnee == Knee.Right ? _rightKneeList.Except(tempKneeList).ToList() : _leftKneeList.Except(tempKneeList).ToList();

        foreach (var except in listExcept)
        {
            var obj = GameObject.Find(except);
            KillAllChildren(obj);
            switch (thisKnee)
            {
                case Knee.Right:
                    _rightKneeList.Remove(obj.name);
                    break;
                case Knee.Left:
                    _leftKneeList.Remove(obj.name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("thisKnee", thisKnee, null);
            }
            // _humanList.Remove(obj);
            // _idList.Remove(except);
            Destroy(obj);
        }
    }

    /////////////////////////////////////////////

    private static Vector3 GetMeanList(Dictionary<string, KneesInfo> meanList, bool track)
    {
        return track ? GetMeanTrackList(meanList) : GetMeanList(meanList);
    }

    private static Vector3 GetMeanList(Dictionary<string, KneesInfo> meanList)
    {
        var meanResult = meanList.Aggregate(Vector3.zero, (current, pair) => current + pair.Value.Pos.Value);
        meanResult /= meanList.Count;
        return meanResult;
    }

    private static Vector3 GetMeanTrackList(Dictionary<string, KneesInfo> meanList)
    {
        var meanResult = Vector3.zero;
        var trackCount = 0;
        foreach (var info in meanList)
        {
            if (!info.Value.Track) continue;
            trackCount++;
            meanResult = meanResult + info.Value.Pos.Value;
        }
        meanResult /= trackCount;
        return meanResult;
    }

    private static void KillAllChildren(GameObject obj)
    {
        for (var i = 0; i < obj.transform.childCount; i++)
        {
            Destroy(obj.transform.GetChild(i).gameObject);
        }
    }


    private static Vector3 CloseKnee(Dictionary<string, KneesInfo> kneesList, Tracker localTracker, Knee thisKnee, bool track, string idHuman, Vector3? lastPosition)
    {
        var human = localTracker.GetHuman(Convert.ToInt32(idHuman));

        Vector3 position;
        if (lastPosition != null) position = lastPosition.Value;
        else
        {
            switch (thisKnee)
            {
                case Knee.Right:
                    position = human.Skeleton.GetRightKnee();
                    break;
                case Knee.Left:
                    position = human.Skeleton.GetLeftKnee();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("thisKnee", thisKnee, null);
            }
        }

        var res = new Vector3();
        var diff = float.MaxValue;
        //var t = 0;
        foreach (var info in kneesList)
        {
            if (track)
            {
                if (!info.Value.Track) continue;
                //t++;
                var d = (info.Value.Pos.Value - position).magnitude;
                if (!(d < diff)) continue;
                diff = d;
                res = info.Value.Pos.Value;
            }
            else
            {
                var d = (info.Value.Pos.Value - position).magnitude;
                if (!(d < diff)) continue;
                diff = d;
                res = info.Value.Pos.Value;
            }
        }
        // if ((track && t == 0) || kneesList.Count == 0 || Math.Abs(diff - float.MaxValue) < 0) return null;

        return res;
    }


    /////////////////////////////////////////////
    private static GameObject CreateKnees(string idHuman, OtherKnee otherKneen, Knee knee, Transform parent)
    {
        return MyCreateSphere(idHuman + "_" + otherKneen + "_" + knee, parent, Color.white);
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

        return MyCreateSphere(nameKnee, pos.Value, parent, infoColor);
    }

    private string GetIdHuman(string nameKnee)
    {
        char[] del = {'_'};
        return nameKnee.Split(del)[0];
    }

    private string GetIdBody(string nameKnee)
    {
        char[] del = {'_'};
        return nameKnee.Split(del)[1];
    }

    private string GetKnee(string nameKnee)
    {
        char[] del = {'_'};
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
        gameObjectSphere.GetComponent<Renderer>().material = (Material) Instantiate(Resources.Load("Materials/Mt_Transparent"));
        gameObjectSphere.transform.localScale = new Vector3(scale, scale, scale);
        gameObjectSphere.name = name;

        return gameObjectSphere;
    }

    private static GameObject MyCreateSphere(string name, Vector3 position, Transform parent, Color cor, float scale = 0.1f)
    {
        var gameObjectSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        gameObjectSphere.GetComponent<SphereCollider>().enabled = false;

        gameObjectSphere.GetComponent<Renderer>().material = (Material) Instantiate(Resources.Load("Materials/Mt_Transparent"));
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
        gameObjectSphere.GetComponent<Renderer>().material = (Material) Instantiate(Resources.Load("Materials/Mt_Transparent"));
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
