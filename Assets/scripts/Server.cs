using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.IO;
public class Server : MonoBehaviour {

    public int port= 50000;
    private List<ServerClient> client =new List<ServerClient>();
    private List<ServerClient> disconnectList = new List<ServerClient>();
    private List<ServerClient> Update_Client = new List<ServerClient>();
    private decimal movmentSpeed = 1M;
    private decimal tickrate = .25M;
    private decimal timer = 0M;
    private decimal endtimer = 4M;
    private decimal addTimeperTick = 1M;

    public bool serverStarted;
    public float firstzone = 200;
    public float nextzone = 100;
    private float zonetimer = 0;
    private bool firstzone_ = true;
    private float FoodSpawnRange = 1000;
    private bool hightickrate = false;
    // private bool isUpdating = false;
    private int alive=0;
    private TcpListener _server;
    private List<food> food_list = new List<food>();
    struct food
    {
        public int x { get; set; }
        public int y { get; set; }
        public int id { get; set; }
        public bool active { get; set; }
    }
    // Use this for initialization
    public void tickrateChanger(decimal newtickrate)
    {
        movmentSpeed = 1M;
        tickrate = .25M;
        decimal tmp = (1M / System.Math.Round(((1M / newtickrate))));
        movmentSpeed = (movmentSpeed/100)*((tmp / tickrate) * 100);
        endtimer = ((4M / 100M) * ((100M - ((tmp / tickrate) * 100M)) + 100M));
        addTimeperTick = (1 / 100M) * (tmp / tickrate) * 100M;
        if (movmentSpeed == 1M)
        {
            hightickrate = false;
        }
        else
        {
            hightickrate = true;
        }
        tickrate = tmp;
        print(tmp + "|"+endtimer);
        BroadcastToAll("NewTickTime|" + (1 / tmp));
    }

    public void OnServerInitialized()
    {
        DontDestroyOnLoad(gameObject);
        _server = new TcpListener(IPAddress.Any, port);
        _server.Start();
        StartListiening();
        serverStarted = true;
    }
    private void StartListiening()
    {
        _server.BeginAcceptTcpClient(AcceptTcpClient,_server);
    }
    private void AcceptTcpClient(System.IAsyncResult ar)
    {
        print("NewUser|A User Connected");
        TcpListener listener = (TcpListener)ar.AsyncState;
        print("NewUser|CreateNewUser");
        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        if ((client.Count) == 0)
        {
            sc.host = true;
        }
        else
        {
            sc.host = false;
        }
        print("NewUser|SendInfo");
        BroadcastTo("HI|" + (client.Count).ToString() + "|" + sc.host + "|", sc);
        sc.id=client.Count;
        sc.clength = 0;
        sc.length = 1;
        sc.olddirection = 0;
        sc.dead = false;
        sc.spawned = false;


        client.Add(sc);
        StartListiening();
        string allUsers = "";
        foreach (ServerClient sc_2 in client)
        {
            allUsers += sc_2.clientName + '|';
            if (sc_2.id != (client.Count - 1))
            {
                BroadcastTo("AddClient|" + sc_2.id + "|" + sc_2.clientName + "|" + sc_2.host, client[(client.Count - 1)]);
            }
        }

    }
    void setPlayerDead(int id)
    {
        print(id + " isDead");
        BroadcastToAll("PlayerDestroy|" + id);
        alive--;
        if(int.Parse(PlayerCounter()) == 1)
        {
            if(alive == 0)
            {
                foreach (ServerClient sc in client)
                {
                    if (!sc.dead)
                    {
                        if (sc.id == id)
                        {
                            BroadcastTo("YouLose", sc);
                        }
                        else
                        {
                        }
                    }
                }
            }
        }else{
            if(alive == 1)
            {
                foreach(ServerClient sc in client)
                {
                    if (!sc.dead)
                    {
                        if (sc.id == id)
                        {
                            BroadcastTo("YouLose", sc);
                        }else
                        {
                        }
                    }
                }
            }
        }
        ServerClient s=client[id];
        s.dead = true;
        Update_Client.Add(s);
    }
    void startPlayerUpdate()
    {

        foreach (ServerClient us in Update_Client)
        {
            client[us.id] = us;
        }
        Update_Client.Clear();

    }
    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                return true;

            }
        }
        catch (System.Exception) { }

        return false;
    }
    void zoneController()
    {
        if (System.Math.Round(timer, 4) == System.Math.Round(endtimer, 4))
        {
            if (firstzone_)
            {
                if (zonetimer == 0)
                {
                    zonetimer = firstzone;
                }
                zonetimer--;
                if (zonetimer == 0)
                {
                    firstzone_ = false;
                }
                BroadcastToAll("ZoneTimer|" + zonetimer);
            }
            else
            {
                if (zonetimer == 0)
                {
                    print("NewFoodRange|" + FoodSpawnRange);
                    FoodSpawnRange = FoodSpawnRange / 2;
                    zonetimer = nextzone;
                }
                zonetimer--;
                BroadcastToAll("ZoneTimer|" + zonetimer);
            }
            timer = 0M;
        }
        timer += addTimeperTick;
    }
    private void BroadcastTo(string data,ServerClient c)
    {
        StreamWriter writer = new StreamWriter(c.tcp.GetStream());
        writer.WriteLine(data);
        writer.Flush();
    }
    public void BroadcastToAll(string data,List<ServerClient> c1)
    {
        foreach(ServerClient sc in c1)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (System.Exception)
            {

            }
        }
    }
    public void BroadcastToAll(string data)
    {
        foreach (ServerClient sc in client)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (System.Exception) { }
        }
    }
    private void OnIncomingData(ServerClient c,string data)
    {
        print("ServerIncNETMSG:" + data);
        string[] dataArray = data.Split('|');
        switch (dataArray[0])
        {
            case "NewTickrate":
                if (c.host)
                {
                    try
                    {
                        tickrateChanger(decimal.Parse(dataArray[1]));
                    }
                    catch (System.Exception)
                    {

                    }
                }
                break;
            case "PlayerDead":
                setPlayerDead(c.id);
                break;
            case "PlayerJumped":
                if (client[int.Parse(dataArray[1])].spawned == false)
                {
                    try
                    {
                        string msg2 = "SpawnPlayer|" + dataArray[1] + "|" + dataArray[2] + "|" + dataArray[3];
                        BroadcastToAll(msg2);
                    }
                    catch (System.Exception e)
                    {
                        print(e);
                    }
                    ServerClient serverC2 = client[int.Parse(dataArray[1])];
                    serverC2.x = int.Parse(dataArray[2]);
                    serverC2.y = int.Parse(dataArray[3]);
                    serverC2.nextpos = new Vector2(int.Parse(dataArray[2]), int.Parse(dataArray[3]) + 1);
                    serverC2.spawned = true;
                    serverC2.jumped = true;
                    client[int.Parse(dataArray[1])] = serverC2;

                    SpawnFood();
                    SpawnFood();
                    SpawnFood();
                }
                break;
            case "GetID":
                BroadcastTo("GetID|" + c.id, c);
                break;
            case "ZoneDMG":
                if (!client[int.Parse(dataArray[1])].dead) {
                    removetail(int.Parse(dataArray[1]));
                }
                break;
            case "SendName":
                ServerClient cl = client[int.Parse(dataArray[1])];
                cl.clientName = dataArray[2];
                client[int.Parse(dataArray[1])] = cl;
                BroadcastToAll("ClientJoint|"+c.id+"|"+ c.clientName+"|"+c.host, client);
                break;
            case "ChangeDirection":
                CheckFood();
                ServerClient serverC = client[int.Parse(dataArray[1])];
                serverC.olddirection = serverC.direction;
                serverC.direction = int.Parse(dataArray[2]);
                client[int.Parse(dataArray[1])] = serverC;
                if (Random.Range(0, 10) == Random.Range(0, 10))
                {
                    SpawnFood();
                }
                break;
            case "CloseServer":
                if (c.host)
                {
                    stop_server();
                }
                break;

            case "StartGame":

                string msg = "StartGame"+"|"+ PlayerCounter()+"|"+tickrate;
                alive = int.Parse(PlayerCounter());
                BroadcastToAll(msg);
                InvokeRepeating("TimerInvoke", 0, float.Parse(tickrate.ToString()));
                int x = 0;
                while (x != 20)
                {
                    SpawnFood();
                    x++;
                }
                break;

            case "EatFoodAt":

                    if (food_list[int.Parse(dataArray[4])].active)
                    {
                    try
                    {
                        food apple = food_list[int.Parse(dataArray[4])];
                        apple.active = false;
                        food_list[int.Parse(dataArray[4])] = apple;
                    }catch(System.Exception)
                    {
                        CheckFood();
                    }
                    string msg_ = "RemoveFood|" + dataArray[4];
                    BroadcastToAll(msg_);
                    if (Random.Range(0, 1) == Random.Range(0, 1))
                    {
                        SpawnFood();
                        if (Random.Range(0, 50) == Random.Range(0, 50))
                        {
                            SpawnFood();
                        }
                    }
                    ServerClient cla = client[int.Parse(dataArray[1])];
                    cla.length++;
                    client[int.Parse(dataArray[1])] = cla;

                }
                break;
            case "RequestUserInfo":
                BroadcastTo("RequestUserInfo|"+ dataArray[1], client[int.Parse(dataArray[2])]);
                break;
            case "SendUserInfo":
                BroadcastTo("RecieveResync|" + dataArray[2], client[int.Parse(dataArray[1])]);
                break;
        }
    }
    string PlayerCounter()
    {
        return client.Count.ToString();
    }
    void CheckFood()
    {
        int food_counter = 0;
        foreach (food toast in food_list)
        {
            if (toast.active) {
                food_counter++;
            }
        }
        if(food_counter == 0)
        {
            SpawnFood();
        }
    }
    void SpawnFood()
    {
        if (FoodSpawnRange > 300)
        {
            int xPos_Food = Random.Range(Mathf.RoundToInt(FoodSpawnRange) * -1, Mathf.RoundToInt(FoodSpawnRange));
            int yPos_Food = Random.Range(Mathf.RoundToInt(FoodSpawnRange) * -1, Mathf.RoundToInt(FoodSpawnRange));
            food melon = new food();
            melon.active = true;
            melon.x = xPos_Food;
            melon.y = yPos_Food;
            food_list.Add(melon);
            string msg = "SpawnFood|" + xPos_Food.ToString() + "|" + yPos_Food.ToString() + "|" + (food_list.Count - 1).ToString();
            BroadcastToAll(msg);
        }
    }
    void TimerInvoke()
    {
        zoneController();
        Movement();
    }
    void removetail(int pid)
    {

            ServerClient a = client[pid];
        if (a.pos.Count - 1 == 0)
        {
            a.dead = true;
            setPlayerDead(pid);
        }
            a.length=a.length - 1;
        a.clength = a.clength - 1;
        if (a.dead) { }
        else
        {
            a.pos.RemoveAt(a.pos.Count - 1);
        }

        client[pid] = a;
        BroadcastToAll("RemoveTail|" + pid);

    }
    public void stop_server()
    {
        serverStarted = false;
        _server.Stop();
        Destroy(this.gameObject);
    }
    void Movement()
    {
        foreach (ServerClient snake_player in client)
        {
            if (!snake_player.dead && snake_player.jumped)
            {
                Vector2 nextPos = new Vector2(0, 0);
                Vector2 tmp = snake_player.nextpos;

                switch (snake_player.direction)
                {
                    case 0:
                            nextPos = new Vector2(tmp.x, tmp.y + float.Parse(movmentSpeed.ToString()));
                        break;
                    case 1:
                            nextPos = new Vector2(tmp.x + float.Parse(movmentSpeed.ToString()), tmp.y);
                        break;
                    case 2:
                            nextPos = new Vector2(tmp.x, tmp.y - float.Parse(movmentSpeed.ToString()));
                        break;
                    case 3:
                            nextPos = new Vector2(tmp.x - float.Parse(movmentSpeed.ToString()), tmp.y);
                        break;
                }
                if (snake_player.clength == 0)
                {
                    snake_player.pos = new List<SnakePiece>();
                }
                if (snake_player.length != snake_player.clength)
                {
                    SnakePiece newpiece = new SnakePiece();
                    newpiece.pos = nextPos;
                    newpiece.nr = snake_player.clength;
                    snake_player.pos.Add(newpiece);
                    snake_player.clength++;
                }

                    List<SnakePiece> cache = new List<SnakePiece>();
                    SnakePiece cache2 = new SnakePiece();
                    cache2.pos = nextPos;
                    cache2.nr = 0;
                    int x=1;
                    foreach (SnakePiece v2 in snake_player.pos)
                    {
                        SnakePiece temp = v2;
                        cache.Add(cache2);
                        cache2 = temp;
                        cache2.nr=x;
                        x++;
                    }
                    snake_player.pos = cache;


                snake_player.nextpos = nextPos;
                Update_Client.Add(snake_player);
                BroadcastToAll("PlayerMovement|" + snake_player.id + "|" + snake_player.nextpos.x + "|" + snake_player.nextpos.y);
                if (food_list.Count < 1000)
                {
                    SpawnFood();
                }
            }
        }
        startPlayerUpdate();
        if (!hightickrate)
        {
            foreach (ServerClient snake_player in client)
            {
                if (!snake_player.dead && snake_player.spawned)
                {
                    foreach (ServerClient snake_player2 in client)
                    {
                        if (!snake_player2.dead && snake_player.spawned)
                        {
                            try
                            {
                                foreach (SnakePiece sp in snake_player.pos)
                                {
                                    foreach (SnakePiece sp2 in snake_player2.pos)
                                    {
                                        if (snake_player.id == snake_player2.id)
                                        {
                                            if (sp.nr != sp2.nr)
                                            {
                                                if (sp.pos == sp2.pos)
                                                {
                                                    setPlayerDead(snake_player.id);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (sp.pos == sp2.pos)
                                            {
                                                if (sp.nr < sp2.nr)
                                                {
                                                    setPlayerDead(snake_player.id);
                                                }
                                                else
                                                {
                                                    setPlayerDead(snake_player2.id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (System.Exception) { }
                        }
                    }
                }
            }
        }
        startPlayerUpdate();
    }
    void Start () {

	}

	// Update is called once per frame
	void Update () {
        if (!serverStarted)
        {
            return;
        }
        foreach(ServerClient c in client)
        {
            //IsClientDa

            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                BroadcastToAll("UserDisconnected|" + c.id,client);
                disconnectList.Add(c);
                continue;
            }
            else
            {
                NetworkStream s = c.tcp.GetStream();

                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();
                    if (!string.IsNullOrEmpty(data))
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }
        for (int i = 0; i < disconnectList.Count - 1; i++){
            client.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);

        }
	}

}

public class ServerClient
{
    public string clientName;
    public bool host;
    public bool jumped;
    public TcpClient tcp;
    public int id;
    public int x;
    public int y;
    public Vector2 nextpos;
    public int direction;
    public int olddirection;
    public bool spawned;
    public List<SnakePiece> pos;
    public int length;
    public int clength;
    public bool dead;
    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}
public struct SnakePiece
{
    public Vector2 pos;
    public int nr;
}
