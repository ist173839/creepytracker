using UnityEngine;
using System.Collections;

public class RPCServer : MonoBehaviour {

    public GameObject TrackerGameObject;

    public string Port;
    public string BroadcastPort;

    public Texture onlineTex;
    public Texture offlineTex;

    public bool ShowNetworkOptions;

    void Start()
    {
        ShowNetworkOptions = false;

        Port = "" + TrackerProperties.Instance.ListenPort;
        BroadcastPort = "" + TrackerProperties.Instance.BroadcastPort;

        Network.InitializeServer(32, int.Parse(Port), false);
    }

    void Update() { }

    void OnGUI()
    {
        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            GUI.DrawTexture(new Rect(Screen.width - 48, 1, 48, 48), offlineTex);
        }
        else
        {
            GUI.DrawTexture(new Rect(Screen.width - 48, 1, 48, 48), onlineTex);
            if (Network.peerType == NetworkPeerType.Server)
            {
                GUI.Label(new Rect(Screen.width - 20, 30, 48, 48), "" + Network.connections.Length);
            }
        }

        if (Input.mousePosition.x < Screen.width && Input.mousePosition.x > Screen.width - 100
            && Input.mousePosition.y < Screen.height && Input.mousePosition.y > Screen.height - 100)
        {
            ShowNetworkOptions = true;
        }
        if (Input.mousePosition.x < Screen.width / 2 || Input.mousePosition.y < ((3 / 4) * Screen.height))
        {
            ShowNetworkOptions = false;
        }

        if (ShowNetworkOptions)
        {

            int left = Screen.width - 270;
            int top = 50;

            GUI.Box(new Rect(left, top, 260, 80), "");


            if (Network.peerType == NetworkPeerType.Disconnected)
            {
                left += 20;
                top += 10;
                GUI.Label(new Rect(left, top, 100, 25), "Sensors port:");

                left += 100 + 10;
                Port = GUI.TextField(new Rect(left, top, 50, 25), Port);

                left += 50 + 10;
                if (GUI.Button(new Rect(left, top, 50, 25), "Start"))
                {
                    Network.InitializeServer(32, int.Parse(Port), false);
                    ShowNetworkOptions = false;
                }
            }
            else
            {
                if (Network.peerType == NetworkPeerType.Server)
                {
                    
                    top += 10;
                    GUI.Label(new Rect(left + 20, top, 150, 25), "I'm a server on: " + Port);
                    if (GUI.Button(new Rect(left + 190, top, 50, 25), "Stop"))
                    {
                        Network.Disconnect(250);
                    }
                }
            }

            top += 35;
            left = Screen.width - 270;

            left += 20;
            GUI.Label(new Rect(left, top, 100, 25), "Broadcast port:");

            left += 100 + 10;
            BroadcastPort = GUI.TextField(new Rect(left, top, 50, 25), BroadcastPort);

            left += 50 + 10;
            if (GUI.Button(new Rect(left, top, 50, 25), "Reset"))
            {
                TrackerProperties.Instance.BroadcastPort = int.Parse(BroadcastPort);
                TrackerGameObject.GetComponent<Tracker>().ResetBroadcast();
                ShowNetworkOptions = false;
            }
        }
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        DoNotify n = gameObject.GetComponent<DoNotify>();
        n.notifySend(NotificationLevel.INFO, "Network", "New Connection", 5000);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);

        DoNotify n = gameObject.GetComponent<DoNotify>();
        n.notifySend(NotificationLevel.IMPORTANT, "Network", "Lost Connection", 5000);
    }

    [RPC]
    public void newFrameFromSensor(string bodies)
    {
        try
        {
            BodiesMessage b = new BodiesMessage(bodies);
            TrackerGameObject.GetComponent<Tracker>().SetNewFrame(b);
        }
        catch (BodiesMessageException e)
        {
            Debug.Log(e.Message);
        }
    }
}
