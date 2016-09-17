﻿/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace OptitrackManagement
{
    public class Marker
    {
        public int ID = -1;
        public Vector3 pos;
    }

    public class OtherMarker {
        public int ID = -1;
        public Vector3 pos;
        public GameObject markerGameObject = null;
    }

    public class RigidBody
    {
        public string Name = "";
        public int Id = -1;
        public int ParentId = -1;
        public float Length = 1.0f;
        public Vector3 Pos;
        public Quaternion Ori;
        public Quaternion InitOri;
        public Transform Trans;
        public GameObject RigidBodyGameObject = null;
    }

    public class StreemData
    {

        public RigidBody[] RigidBody = new RigidBody[200];
        public OtherMarker[] OtherMarkers = new OtherMarker[200];
        public int NRigidBodies = 0;
        public int NOtherMarkers = 0;
        //public List<RigidBody> _listRigidBody = new List<RigidBody>();

        public StreemData()
        {

            //Debug.Log("StreemData: Construct");
            InitializeMarkers();
        }

        public bool InitializeMarkers()
        {
            NRigidBodies = 0;
            for (int i = 0; i < 200; i++)
            {
                RigidBody[i] = new RigidBody();
            }

            NOtherMarkers = 0;
            for (int i = 0; i < 200; i++)
            {
                OtherMarkers[i] = new OtherMarker();
            }
            return true;
        }

        public bool InitializeSkeleton()
        {
            return true;
        }

        public bool InitializeMarkerSet()
        {
            return true;
        }
    }

    public class DirectStateObject
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 65507;
        public byte[] Buffer = new byte[BufferSize];
        public StringBuilder Sb = new StringBuilder();
    }

    public static class DirectMulticastSocketClient
    {
        private static Socket _client;

        private static StreemData _streemData = null;

        private static string _strFrameLog = string.Empty;
        private static string _multicastIPAddress = "239.255.42.99";

        public static bool DEBUG = false;

        private static bool _isInitRecieveStatus = false;
        private static bool _isIsActiveThread    = false;

        //bool returnValue = false;
        private static int _dataPort = 1511;
        private static int _commandPort = 1510;


        private static void StartClient()
        {
            // Connect to a remote device.
            try
            {

                Debug.Log("[UDP] Starting client");
                _streemData = new StreemData();
                _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //client.ExclusiveAddressUse = false;

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, _dataPort);
                _client.Bind(ipep);

                IPAddress ip = IPAddress.Parse(_multicastIPAddress);
                _client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));

                _isInitRecieveStatus = Receive(_client);
                _isIsActiveThread = _isInitRecieveStatus;

            }
            catch (Exception e)
            {
                Debug.LogError("[UDP] DirectMulticastSocketClient: " + e.ToString());
            }
        }

        private static bool Receive(Socket client)
        {
            try
            {
                // Create the state object.
                DirectStateObject state = new DirectStateObject();
                state.WorkSocket = client;

                Debug.Log("[UDP multicast] Receive");

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.Buffer, 0, DirectStateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return false;
            }

            return true;
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //Debug.Log("[UDP multicast] Start ReceiveCallback");
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                DirectStateObject state = (DirectStateObject)ar.AsyncState;
                Socket client = state.WorkSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0 && _isIsActiveThread)
                {
                    ReadPacket(state.Buffer);

                    //client.Shutdown(SocketShutdown.Both);
                    //client.Close();   

                    client.BeginReceive(state.Buffer, 0, DirectStateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    Debug.LogWarning("[UDP] - End ReceiveCallback");

                    if (_isIsActiveThread == false)
                    {
                        Debug.LogWarning("[UDP] - Closing port");
                        _isInitRecieveStatus = false;
                        //client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }

                    // Signal that all bytes have been received.
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        private static void ReadPacket(Byte[] b)
        {
            int offset = 0;
            int nBytes = 0;
            int[] iData = new int[100];
            float[] fData = new float[500];
            char[] cData = new char[500];

            Buffer.BlockCopy(b, offset, iData, 0, 2); offset += 2;
            int messageID = iData[0];

            //Debug.Log("Message Recieved ID: " + messageID);

            Buffer.BlockCopy(b, offset, iData, 0, 2); offset += 2;
            nBytes = iData[0];

            //Debug.Log("[UDPClient] Processing Received Packet (Message ID : " + messageID + ")");
            if (messageID == 5)      // Data descriptions
            {
                Debug.Log("DirectParseClient: Data descriptions");

            }
            else if (messageID == 7)   // Frame of Mocap Data
            {
                _strFrameLog = String.Format("DirectParseClient: [UDPClient] Read FrameOfMocapData: {0}\n", nBytes);
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                _strFrameLog += String.Format("Frame # : {0}\n", iData[0]);

                //number of data sets (markersets, rigidbodies, etc)
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int nMarkerSets = iData[0];
                _strFrameLog += String.Format("MarkerSets # : {0}\n", iData[0]);

                for (int i = 0; i < nMarkerSets; i++)
                {
                    String strName = "";
                    int nChars = 0;
                    while (b[offset + nChars] != '\0')
                    {
                        nChars++;
                    }
                    strName = System.Text.Encoding.ASCII.GetString(b, offset, nChars);
                    offset += nChars + 1;

                    Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                    _strFrameLog += String.Format("Model Name:" + strName +"\n" + "marker count : {1}\n", i, iData[0]);

                    nBytes = iData[0] * 3 * 4;
                    Buffer.BlockCopy(b, offset, fData, 0, nBytes); offset += nBytes;
                    //do not need   
                }

                // Other Markers - All 3D points that were triangulated but not labeled for the given frame.
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int nOtherMarkers = iData[0];
                _streemData.NOtherMarkers = iData[0];
                _strFrameLog += String.Format("Unidentified Marker Count : {0}\n", iData[0]);
                for (int j = 0; j < nOtherMarkers; j++)
                {
                    _streemData.OtherMarkers[j].ID = j;
                    ReadOtherMarket(b, ref offset, _streemData.OtherMarkers[j]);
                }

                //nBytes = iData[0] * 3 * 4;
                //Buffer.BlockCopy(b, offset, fData, 0, nBytes); offset += nBytes;

                // Rigid Bodies
                //RigidBody rb = new RigidBody();
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                _streemData.NRigidBodies = iData[0];
                _strFrameLog += String.Format("Rigid Bodies : {0}\n", iData[0]);
                
                if(DEBUG)
                    Debug.Log(_strFrameLog);   
                
                for (int i = 0; i < _streemData.NRigidBodies; i++)
                {
                    ReadRigidBody(b, ref offset, _streemData.RigidBody[i]);
                    //_streemData._rigidBody[0];
                }

                if(DEBUG)
                Debug.Log(_strFrameLog);   

            }
            else if (messageID == 100)
            {
                Debug.Log("[Client] received 'unrecognized request'\n");
            }

        }

        private static void ReadOtherMarket(Byte[] b, ref int offset, OtherMarker marker) 
        {
            float[] pos = new float[3];
            Buffer.BlockCopy(b, offset, pos, 0, 4 * 3); offset += 4 * 3;
            marker.pos.x = pos[0];
            marker.pos.y = pos[1];
            marker.pos.z = pos[2];
            //Debug.Log("marker ID: " + marker.ID + " is in: " + pos[0] + " " + pos[1] + " " + pos[2]);
        }

        // Unpack RigidBody data
        private static void ReadRigidBody(Byte[] b, ref int offset, RigidBody rb)
        {
            try
            {
                String debugString = String.Empty;
                int[] iData = new int[100];
                float[] fData = new float[100];

                // RB ID
                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int iSkelID = iData[0] >> 16;           // hi 16 bits = ID of bone's parent skeleton
                int iBoneID = iData[0] & 0xffff;       // lo 16 bits = ID of bone
                rb.Id = iData[0]; // already have it from data descriptions
                debugString += String.Format("Rigid Body ID : {0}\n", iData[0]);

                // RB pos
                float[] pos = new float[3];
                Buffer.BlockCopy(b, offset, pos, 0, 4 * 3); offset += 4 * 3;
                rb.Pos.x = pos[0]; rb.Pos.y = pos[1]; rb.Pos.z = pos[2];
                debugString += String.Format("pos : {0} {1} {2}\n", pos[0], pos[1],pos[2]);
                

                // RB ori
                float[] ori = new float[4];
                Buffer.BlockCopy(b, offset, ori, 0, 4 * 4); offset += 4 * 4;
                rb.Ori.x = -ori[0]; rb.Ori.y = ori[1]; rb.Ori.z = ori[2]; rb.Ori.w = ori[3];
                debugString += String.Format("ori : {0} {1} {2} {3}\n", ori[0], ori[1], ori[2], ori[3]);

                Buffer.BlockCopy(b, offset, iData, 0, 4); offset += 4;
                int nMarkers = iData[0];
                debugString += String.Format("Markers Count : {0}\n", iData[0]);
                //Debug.Log(debugString);
                Buffer.BlockCopy(b, offset, fData, 0, 4 * 3 * nMarkers); offset += 4 * 3 * nMarkers;

                Buffer.BlockCopy(b, offset, iData, 0, 4 * nMarkers); offset += 4 * nMarkers;

                Buffer.BlockCopy(b, offset, fData, 0, 4 * nMarkers); offset += 4 * nMarkers;

                Buffer.BlockCopy(b, offset, fData, 0, 4); offset += 4;

                if(DEBUG)
                    Debug.Log(debugString);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }


        // Use this for initialization
        public static void Start()
        {
            StartClient();
        }
        // Update is called once per frame
        public static void Update()
        {
        }
        public static void Close()
        {
            _isIsActiveThread = false;
        }
        public static bool IsInit()
        {
            return _isInitRecieveStatus;
        }
        public static StreemData GetStreemData()
        {
            return _streemData;
        }
    }
}