using UnityEngine;
using System.Collections.Generic;
using System;

public class Human
{
    public int ID;
    public List<SensorBody> bodies;
    public GameObject gameObject;
    public DateTime TimeOfDeath;
    public string SeenBySensor;

    private HumanSkeleton skeleton;
    public HumanSkeleton Skeleton
    {
        get { return skeleton; }
    }

    private Vector3 position;
    public Vector3 Position
    {
        get { return position; }
        set { position = value; gameObject.transform.position = position; }
    }

    public Human(GameObject gameObject, Tracker tracker)
    {
        ID = CommonUtils.GetNewId();
        bodies = new List<SensorBody>();
        this.gameObject = gameObject;
        this.gameObject.name = "Human " + ID;

        skeleton = this.gameObject.GetComponent<HumanSkeleton>();
        skeleton.tracker = tracker;
        skeleton.ID = ID;
        skeleton.UpdateSkeleton();
    }

    internal void UpdateSkeleton()
    {
        Skeleton.UpdateSkeleton();
    }

    internal string GetPdu()
    {
        return "Sensor" + MessageSeparators.SET + SeenBySensor + MessageSeparators.L2 + Skeleton.GetPdu();
    }
}

///////////////////////////////
/*
 =======
        skeleton.updateSkeleton();
>>>>>>> refs/remotes/mauriciosousa/master
     
     */
