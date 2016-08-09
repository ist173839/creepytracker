using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;


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


[RequireComponent(typeof(Tracker))]
public class MyKnees : MonoBehaviour {

    private Dictionary<string, GameObject> _humanList;
    
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

        _humanList = new Dictionary<string, GameObject>();

        _rightKneeList = new List<string>();
        _leftKneeList  = new List<string>();
        _meanKneeList  = new List<string>();


        _idList    = new List<string>();

        _colorTrack = Color.yellow;
        _colorTrack.a = 0.5f;

        _colorInferred = Color.grey;
        _colorInferred.a = 0.5f;

        _colorMean = Color.blue;
        _colorMean.a = 0.5f;

        _colorCloser = Color.green;
        _colorCloser.a = 0.5f;

        _humans = new GameObject { name = "Humans" };
        _humans.transform.parent = transform;

        _lastRigthPosition = null;
        _lastLeftPosition  = null;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
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

            _humanList.Add(id,human);
            _idList.Add(id);
            
            //var body =  _localTracker.GetHuman(int.Parse(id)).bodies;
            //foreach (var b in body)
            //{
            //    var newBody = new GameObject { name = "Body_" + b.sensorID };
            //    newBody.transform.parent = human.transform;
            //    var newRightKnee = CreateKnees(id, b.sensorID, Knee.Right, newBody.transform);
            //    var newLeftKnee  = CreateKnees(id, b.sensorID, Knee.Left,  newBody.transform);
            //    _rightKneeList.Add(newRightKnee.name);
            //    _leftKneeList.Add(newLeftKnee.name);
            //}

            //////////////////////////////////////////////////
            var newMean = new GameObject { name = "Mean" };
            newMean.transform.parent = human.transform;

            var rightMeanKnee = CreateKnees(human.name, OtherKnee.Mean, Knee.Right, newMean.transform);
            rightMeanKnee.GetComponent<Renderer>().material.color =  _colorMean;

            var leftMeanKnee = CreateKnees(human.name, OtherKnee.Mean, Knee.Left,  newMean.transform);
            leftMeanKnee.GetComponent<Renderer>().material.color = _colorMean;

            //////////////////////////////////////////////////
            var newClose = new GameObject { name = "Close" };
            newClose.transform.parent = human.transform;

            var rightCloseKnee =  CreateKnees(human.name, OtherKnee.Close, Knee.Right, newClose.transform);
            rightCloseKnee.GetComponent<Renderer>().material.color = _colorCloser;

            var leftCloseKnee = CreateKnees(human.name, OtherKnee.Close, Knee.Left,  newClose.transform);
            leftCloseKnee.GetComponent<Renderer>().material.color = _colorCloser;

        }

        var listExcept = _idList.Except(idList).ToList();

        foreach (var except in listExcept)
        {
            var obj = GameObject.Find(except);
            KillAllChildren(obj);
            _humanList.Remove(obj.name);
            _idList.Remove(except);
            Destroy(obj);
        }

        //UpdateKnees(rightKneesInfo, Knee.Right);
        //UpdateKnees(leftKneesInfo,  Knee.Left);


        UpdateKnees(rightKneesInfo, _idList, Knee.Right, OtherKnee.Mean);
        UpdateKnees(leftKneesInfo,  _idList, Knee.Left,  OtherKnee.Mean);
        
	 
        UpdateKnees(rightKneesInfo, _idList, Knee.Right, OtherKnee.Close);
        UpdateKnees(leftKneesInfo,  _idList, Knee.Left,  OtherKnee.Close);
        
    }

    private void UpdateKnees(Dictionary<string, KneesInfo> theKnees, List<string> idList, Knee thisKnee, OtherKnee otherKnee)
    {
        foreach (var idHuman in idList)
        {
            var kneeName = idHuman + "_" + otherKnee + "_" + thisKnee;

            var knee = GetKneeObject(otherKnee, kneeName, idHuman);
            if (knee == null) continue;
        
            if (theKnees.Count == 0)
            {
                knee.transform.position = Vector3.zero;
            }
            else
            {
                switch (otherKnee)
                {
                    case OtherKnee.Mean:

                        knee.transform.position = GetMeanList(theKnees, Track);
                        break;
                    case OtherKnee.Close:
                        switch (thisKnee)
                        {
                            case Knee.Right:
                                _lastRigthPosition = knee.transform.position = CloseKnee(theKnees, _localTracker, Knee.Right, Track, idHuman, _lastRigthPosition);
                                break;
                            case Knee.Left:
                                _lastLeftPosition  = knee.transform.position = CloseKnee(theKnees, _localTracker, Knee.Left, Track, idHuman, _lastLeftPosition);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("thisKnee", thisKnee, null);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("otherKnee", otherKnee, null);
                }
            }
            
           // knee.GetComponent<Renderer>().material.color = otherKnee == OtherKnee.Mean ? _colorMean : _colorCloser;
        }
    }

    private GameObject GetKneeObject(OtherKnee otherKnee, string kneeName, string idHuman)
    {
        var knee = GameObject.Find(kneeName);
        if (knee != null) return knee;
        Debug.Log("Knee not found");
        CreateBodiesAndKnees(idHuman, otherKnee);
        knee = GameObject.Find(kneeName);
        if (knee != null) return knee;
        Debug.Log("Knee Still not found");
        return null;
    }


    private void CreateBodiesAndKnees(string idHuman, OtherKnee otherKnee)
    {
        var human = _humanList[idHuman];
        switch (otherKnee)
        {
            case OtherKnee.Mean:
                var newMean = new GameObject { name = "Mean" };
                newMean.transform.parent = human.transform;

                var rightMeanKnee = CreateKnees(human.name, OtherKnee.Mean, Knee.Right, newMean.transform);
                rightMeanKnee.GetComponent<Renderer>().material.color = _colorMean;

                var leftMeanKnee = CreateKnees(human.name, OtherKnee.Mean, Knee.Left, newMean.transform);
                leftMeanKnee.GetComponent<Renderer>().material.color = _colorMean;

                break;
            case OtherKnee.Close:
                var newClose = new GameObject { name = "Close" };
                newClose.transform.parent = human.transform;


                var rightCloseKnee = CreateKnees(human.name, OtherKnee.Close, Knee.Right, newClose.transform);
                rightCloseKnee.GetComponent<Renderer>().material.color = _colorCloser;

                var leftCloseKnee = CreateKnees(human.name, OtherKnee.Close, Knee.Left, newClose.transform);
                leftCloseKnee.GetComponent<Renderer>().material.color = _colorCloser;
                break;
            default:
                throw new ArgumentOutOfRangeException("otherKnee", otherKnee, null);
        }
    }

    private void CreateBodiesAndKnees(string idHuman)
    {
        var human = _localTracker.GetHuman(Convert.ToInt32(idHuman));

        foreach (var b in human.bodies)
        {
            var newBody = GameObject.Find("Body_" + b.sensorID);
            if (newBody == null)
            {
                newBody = new GameObject {name = "Body_" + b.sensorID};
                newBody.transform.parent = _humanList[idHuman].transform;


                var newRightKnee = CreateKnees(idHuman, b.sensorID, Knee.Right, newBody.transform);
                var newLeftKnee = CreateKnees(idHuman, b.sensorID, Knee.Left, newBody.transform);

                _rightKneeList.Add(newRightKnee.name);
                _leftKneeList.Add(newLeftKnee.name);
            }
        }
    }

    /////////////////////////////////////////////

    private static Vector3 GetMeanList(Dictionary<string, KneesInfo> meanList, bool track)
    {
        return track ? GetMeanTrackList(meanList) : GetMeanList(meanList);
    }

    private static Vector3 GetMeanList(Dictionary<string, KneesInfo> meanList)
    {
        if (meanList.Count == 0)
        {
            Debug.Log("meanList.Count == 0");
            return Vector3.zero;
        }

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
        if (trackCount == 0) return GetMeanList(meanList);

        return meanResult;
    }

    private static Vector3 CloseKnee(Dictionary<string, KneesInfo> kneesList, Tracker localTracker, Knee thisKnee, bool track, string idHuman, Vector3? lastPosition)
    {
        var position = GetLastPosition(localTracker, thisKnee, idHuman, lastPosition);

        return GetCloserKnee(kneesList, track, position);
    }

    private static Vector3 GetLastPosition(Tracker localTracker, Knee thisKnee, string idHuman, Vector3? lastPosition)
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
        return position;
    }

    private static Vector3 GetCloserKnee(Dictionary<string, KneesInfo> kneesList, bool track, Vector3 position)
    {
        return track ? GetCloseKneeTrack(kneesList, position) : GetCloseKneeAll(kneesList, position);
    }

    private static Vector3 GetCloseKneeAll(Dictionary<string, KneesInfo> kneesList, Vector3 position)
    {
        if (kneesList.Count == 0) return Vector3.zero;

        var res = new Vector3();
        var diff = float.MaxValue;

        foreach (var info in kneesList)
        {
            var d = (info.Value.Pos.Value - position).magnitude;
            if (!(d < diff)) continue;
            diff = d;
            res = info.Value.Pos.Value;
        }

        if (Math.Abs(diff - float.MaxValue) < 0) return Vector3.zero;
        return res;
    }

    private static Vector3 GetCloseKneeTrack(Dictionary<string, KneesInfo> kneesList, Vector3 position)
    {
        if (kneesList.Count == 0) return Vector3.zero;

        var res = new Vector3();
        var diff = float.MaxValue;
        var hasTrack = false;

        foreach (var info in kneesList)
        {
            if (!info.Value.Track) continue;
            hasTrack = true;
            var d = (info.Value.Pos.Value - position).magnitude;
            if (!(d < diff)) continue;
            diff = d;
            res = info.Value.Pos.Value;
        }

        if (!hasTrack) return GetCloseKneeAll(kneesList, position);

        if (Math.Abs(diff - float.MaxValue) < 0) return Vector3.zero;


        return res;
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

                CreateBodiesAndKnees(kneesInfo.Value.IdHuman.ToString());
                knee = GameObject.Find(kneeName);
                if (knee == null)
                {
                    Debug.Log("Knee Still not found");
                    continue;
                }

                continue;
            }

            knee.transform.position = kneesInfo.Value.Pos.Value;

            knee.GetComponent<Renderer>().material.color = kneesInfo.Value.Track ? _colorTrack : _colorInferred;
        }

        //var listExcept = thisKnee == Knee.Right ? _rightKneeList.Except(tempKneeList).ToList() : _leftKneeList.Except(tempKneeList).ToList();

        //foreach (var except in listExcept)
        //{
        //    var obj = GameObject.Find(except);
        //    KillAllChildren(obj);
        //    switch (thisKnee)
        //    {
        //        case Knee.Right:
        //            _rightKneeList.Remove(obj.name);
        //            break;
        //        case Knee.Left:
        //            _leftKneeList.Remove(obj.name);
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException("thisKnee", thisKnee, null);
        //    }
        //    // _humanList.Remove(obj);
        //    // _idList.Remove(except);
        //    Destroy(obj);
        //}
    }

    /////////////////////////////////////////////
    /// 
    private static void KillAllChildren(GameObject obj)
    {
        for (var i = 0; i < obj.transform.childCount; i++)
        {
            Destroy(obj.transform.GetChild(i).gameObject);
        }
    }

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
 * 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        foreach (var h in _humanList)
        {
            idUpdateList.Add(h.name);
            if (idList.Contains(h.name)) { }
        }

        if (idList.Contains(h.name)) { }
        else { }


    // if (_lastRigthPosition == null)
    // {
    // _lastRigthPosition =  
    //   }
    //  
    //   _lastLeftPosition = null;
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
 
 private void UpdateKnees(Dictionary</ string, KneesInfo> theKnees, List<string> idList, Knee thisKnee, OtherKnee otherKnee)
{
   foreach (var idHuman in idList)
   {
       var kneeName = idHuman + "_" + otherKnee + "_" + thisKnee;

       var knee = GetKneeObject(otherKnee, kneeName, idHuman);
       if (knee == null) continue;
   
       if (theKnees.Count == 0)
       {
           knee.transform.position = Vector3.zero;
       }
       else
       {
           switch (otherKnee)
           {
               case OtherKnee.Mean:

                   knee.transform.position = GetMeanList(theKnees, Track);
                   break;
               case OtherKnee.Close:
                   switch (thisKnee)
                   {
                       case Knee.Right:
                           _lastRigthPosition = knee.transform.position = CloseKnee(theKnees, _localTracker, Knee.Right, Track, idHuman, _lastRigthPosition);
                           break;
                       case Knee.Left:
                           _lastLeftPosition  = knee.transform.position = CloseKnee(theKnees, _localTracker, Knee.Left, Track, idHuman, _lastLeftPosition);
                           break;
                       default:
                           throw new ArgumentOutOfRangeException("thisKnee", thisKnee, null);
                   }
                   break;
               default:
                   throw new ArgumentOutOfRangeException("otherKnee", otherKnee, null);
           }
       }
       
      // knee.GetComponent<Renderer>().material.color = otherKnee == OtherKnee.Mean ? _colorMean : _colorCloser;
   }
}
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private static Vector3 GetCloseKnee(Dictionary<string, KneesInfo> kneesList, bool track, Vector3 position)
    {
        if (kneesList.Count == 0) return Vector3.zero;

        var res = new Vector3();
        var diff = float.MaxValue;
        var hasTrack = false;

        foreach (var info in kneesList)
        {
            if (track)
            {
                if (!info.Value.Track) continue;
                hasTrack = true;
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

        if (track && !hasTrack || Math.Abs(diff - float.MaxValue) < 0) return Vector3.zero;


        return res;
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private static int GetTheCloseKnee(Dictionary<string, KneesInfo> kneesList, bool track, int t, Vector3 position, ref float diff, ref Vector3 res)
    {
        foreach (var info in kneesList)
        {
            if (track)
            {
                if (!info.Value.Track) continue;
                t++;
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
        return t;
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private static Vector3 GetTheCloseKneeTrack(Dictionary<string, KneesInfo> kneesList, bool track, int t, Vector3 position, ref float diff, ref Vector3 res)
    {
        var isTrack = false;
        foreach (var info in kneesList)
        {
            if (!info.Value.Track) continue;
            isTrack = true;
            var d = (info.Value.Pos.Value - position).magnitude;
            if (!(d < diff)) continue;
            diff = d;
            res = info.Value.Pos.Value;
        }
        if (!isTrack)
        {
        }

        return res;
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



 * 
 */
