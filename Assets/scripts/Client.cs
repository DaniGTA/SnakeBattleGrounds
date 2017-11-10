using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
public class Client : MonoBehaviour {

    private bool socketReady=false;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    public List<GameClient> Players=new List<GameClient>();
    public int localnr;
    public string localname;
    public int playerscount=1;
    public bool server = true;
    public bool IsHost = false;
    public decimal tickrate = .25M;
    public LobbyUI LUI;
    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if(!string.IsNullOrEmpty(data))
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    public bool ConnectToServer(string host,int port)
    {
        if (socketReady)
            return false;
        try
        {
            DontDestroyOnLoad(gameObject);
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socketReady = true;
        }catch(System.Exception)
        {
            socketReady = false;
        }

        return socketReady;

    }
    //SendNetMSG
    public void Send(string data)
    {
        if (!socketReady)
            return;
        print("Stream:" + stream.CanWrite);
        writer.WriteLine(data);
        writer.Flush();



    }
    //ReadNetMSG
    private void OnIncomingData(string data)
    {
        print("ClientIncNETMSG:" + data);
        string[] dataArray = data.Split('|');
        switch (dataArray[0])
        {
            case "NewTickTime":
                LobbyUI.Instance.ChangeTickTimer(float.Parse(dataArray[1]));
                break;
            case "ZoneTimer":
                GameController.Instance.SetZoneTimer(int.Parse(dataArray[1]));
                break;
            case "YouWin":
                GameController.Instance.Win();
                break;
            case "RemoveTail":
                if(int.Parse(dataArray[1]) != localnr)
                {
                GameController.Instance.RemoveTail(int.Parse(dataArray[1]));
                }
                break;
            case "EndGame":
                SceneManager.LoadScene(0);
                Send("CloseServer");
                Destroy(this.gameObject);
                break;
            case "PlayerDestroy":
                GameController.Instance.delPlayer(int.Parse(dataArray[1]));
                break;
            case "PlayerMovement":
                GameController.Instance.setNextPlayerPos(int.Parse(dataArray[1]), new Vector2(float.Parse(dataArray[2]), float.Parse(dataArray[3])));
                break;
            case "SpawnPlayer":
                GameController.Instance.create_new_player(int.Parse(dataArray[1]), int.Parse(dataArray[2]), int.Parse(dataArray[3]));
                break;
            case "GetID":
                GameController.Instance.set_local_id(int.Parse(dataArray[1]));
                break;
            case "RequestName":
                Send("SendName|" + localname);
                break;
            case "ClientJoint":
                UserConnected(int.Parse(dataArray[1]), dataArray[2], bool.Parse(dataArray[3]));
                break;
            case "AddClient":
                UserConnected(int.Parse(dataArray[1]), dataArray[2], bool.Parse(dataArray[3]));
                break;
            case "ChangeDirection":
                GameController.Instance.ChangeDirectionForPlayer(int.Parse(dataArray[1]), int.Parse(dataArray[2]));
                break;
            case "StartGame":
                tickrate = decimal.Parse(dataArray[2]);
                try
                {
                    playerscount = int.Parse(dataArray[1]);
                }
                catch (System.Exception e)
                {
                    print(dataArray[1]);
                    print(dataArray.Length);
                    print(e.Message);
                }
                SceneManager.LoadScene(1);
                break;
            case "HI":
                localnr = int.Parse(dataArray[1]);
                IsHost = bool.Parse(dataArray[2]);
                Send("SendName|" + localnr + "|" + localname);
                break;
            case "SpawnFood":
                GameController.Instance.add_food(int.Parse(dataArray[1]), int.Parse(dataArray[2]), int.Parse(dataArray[3]));
                    break;
            case "RemoveFood":
                GameController.Instance.remove_food(int.Parse(dataArray[1]));
                break;
            case "RequestUserInfo":
                Send("SendUserInfo|" + dataArray[1] + "|" + GameController.Instance.getLocalUserInfo());
                break;
            case "RecieveResync":
                GameController.Instance.ReSyncUser(dataArray[1]);
                break;
            case "UserDisconnected":
                try
                {
                    GameController.Instance.delPlayer(int.Parse(dataArray[1]));
                }catch(System.Exception)
                {

                }
                LUI.removePlayer(int.Parse(dataArray[1]));
                break;
        }
    }
    private void UserConnected(int id,string name, bool host)
    {
        GameClient c = new GameClient();
        c.id = id;
        c.name = name;
        c.isHost = host;
        Players.Add(c);
        LUI.addPlayer(id, name, host);
    }
    public void GetPlayerList()
    {
        LUI.setCurrentUserHost();
        foreach (GameClient gc in Players)
        {
            LUI.addPlayer(gc.id, gc.name, gc.isHost);
        }
    }
    public void LobbyLinked()
    {

    }
    public bool IsHostRequest()
    {
        return IsHost;
    }
    public void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
        Destroy(this.gameObject);
    }
    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    private void OnDisable()
    {
        CloseSocket();
    }
    public Client getInstance()
    {
        return this;
    }
}
public class GameClient
{
    public int id;
    public string name;
    public bool isHost;
    public Snake snake;
}
