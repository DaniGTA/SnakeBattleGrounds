using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameController : MonoBehaviour {

    public static GameController Instance { set; get; }
    //Spawn {0} players at start
    public int player_count=1;
    //End The Game At {0} Players
    public int End_game_at = 0;
    public int localplayernr=0;
    public Vector2 nextPos;
    public Vector2 lastPos;
    public int xBound;
    public Text scoreText;
    public int yBound;
    public List<player_snake> completeSnake_allPlayers = new List<player_snake>();
    public GameObject foodPrefab;
    public List<GameObject> currentFood_list = new List<GameObject>();
    public GameObject currentFood;
    public GameObject snakePrefab;
    public GameObject arrow;
    public GameObject Zone;
    public GameObject Zonemsg;
    public GameObject airplane;
    public GameObject Lose_canavas;
    public GameObject Win_canavas;
    public GameObject AliveCounter;
    public Camera MainCamera;
    public decimal movmentSpeed= 1.0M;
    public bool smoothmovment = true;
    public bool online = false;
    public decimal tickrate = .25M;
    public float firstzone = 200;
    public float nextzone = 100;
    public bool localplayerjumped = false;
    private Vector3 lastzonescale = new Vector3(4000, 4000,1);
    private Vector3 nextzonescale = new Vector3(4000, 4000,1);
    private Vector3 airplane_end = new Vector3(-999, 0, 2);
    private Vector3 airplane_start = new Vector3(1000, 0, 2);
    private bool localplayeroutofzone=false;
    private float zonestarttime;
    private bool airplainealive = true;
    private float airplainstarttime;
    public decimal timer = 0M;
    public decimal endtimer = 4M;
    public decimal addTimeperTick = 1M;
    public float zonetimer=0;
    private bool firstzone_=true;
    private Client c;
    private bool isUpdating=false;
    private bool win=false;
    private bool hightickrate = false;
    public LineRenderer lineRenderer;
    private List<player_snake> update_snake_list = new List<player_snake>();
    private List<SmoothMovment> UpdateList=new List<SmoothMovment>();
    public struct SmoothMovment
    {
        public GameObject GameObject { get; set; }
        public Vector2 StartPos { get; set; }
        public Vector2 EndPos { get; set; }
        public float startTime { get; set; }
        public float endTime { get; set; }
        public bool _3D {get;set;}
    }
    public struct Add_Tail
    {
        public Vector2 pos { get; set; }
        public int tailpos { get; set; }
        public int ctailpos { get; set; }
    }
    public struct player_snake
    {
        public bool removetail { get; set; }
        public string name { get; set; }
        public int playernr {get; set;}
        public List<GameObject> snake {get; set;}
        public int currentsize { get; set; }
        public int score { get; set; }
        public int NESW_Direction { get; set; }
        public bool localplayer { get; set; }
        public List<Add_Tail> newtail { get; set; }
        public bool neednewtail { get; set; }
        public bool dead { get; set; }
        public Vector2 nextpos { get; set; }
    }
    public void tickrateChanger(decimal newtickrate)
    {
        movmentSpeed = 1M;
        tickrate = .25M;
        decimal tmp = (1M / System.Math.Round(((1M / newtickrate))));
        movmentSpeed = (movmentSpeed / 100) * ((tmp / tickrate) * 100);
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
        tickrate = newtickrate;
    }

    public void delPlayer(int id)
    {
        if(id == localplayernr)
        {
            localplayerdeathmanager();
        }
        try
        {
            player_snake ps = completeSnake_allPlayers[id];
            ps.dead = true;
            foreach (GameObject snakepart in ps.snake)
            {
                Destroy(snakepart);
            }
            update_snake_list.Add(ps);

            startPlayerUpdate();
        }
        catch(System.Exception) { }
    }
    public void setNextPlayerPos(int id, Vector2 pos)
    {
        try
        {
            player_snake ps = completeSnake_allPlayers[id];
            ps.nextpos = pos;
            update_snake_list.Add(ps);
            startPlayerUpdate();
        }
        catch (System.Exception)
        {
            create_new_player(id, pos.x, pos.y);
        }
    }
    void Start () {

        Instance = this;
        airplainstarttime = 0;
        c = FindObjectOfType<Client>();
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.numPositions=2;
        try
        {
            online = c.server;
            player_count = c.playerscount;
            tickrateChanger(c.tickrate);
            End_game_at = 1;
            localplayernr = 0;

        }
        catch (System.Exception e)
        {
            print(e.Message);
            online = false;
            player_count = 1;
            End_game_at = 1;
            localplayernr = 0;
        }
        int x = 0;
        while(x!= player_count)
        {
            player_snake tmp = new player_snake();
            tmp.playernr = -1;
            completeSnake_allPlayers.Add(tmp);
            x++;
        }
        if (online)
        {
            get_local_id();
        }
        print("OnlineMode:" + online + " | " + "Players:" + player_count+" | ID:"+ localplayernr);

        InvokeRepeating("TimerInvoke", 0, float.Parse(tickrate.ToString()));
        if (!online)
        {
            FoodFunction();
        }


    }
    private void get_local_id()
    {
        c.Send("GetID");
    }
    public void set_local_id(int id)
    {
        print("set id to " + id);
        localplayernr = id;

        //print("Request Resync");
    }
    public void create_new_player(int id)
    {
        create_new_player(id, 0f, 0f);
    }
    public void create_new_player(int id,int x = 0, int y = 0)
    {
        create_new_player(id, float.Parse(x.ToString()), float.Parse(y.ToString()));
    }
    public void create_new_player(int id,float x=0,float y=0,string name="_")
    {
        print("AddPlayer "+id);
        bool alreadyexist = false;
        foreach (player_snake p in completeSnake_allPlayers)
        {
            if(p.playernr == id)
            {
                alreadyexist = true;
            }
        }
        if (!alreadyexist)
        {
            List<GameObject> p_snake = new List<GameObject>();
            GameObject newHead;
            if (x == 0 && y == 0)
            {
                newHead = (GameObject)Instantiate(snakePrefab, new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), transform.rotation);
            }
            else
            {
                newHead = (GameObject)Instantiate(snakePrefab, new Vector2(x, y), transform.rotation);
            }
            player_snake N_ps = new player_snake();
            N_ps.neednewtail = false;
            N_ps.newtail = new List<Add_Tail>();
            N_ps.playernr = id;
            N_ps.localplayer = false;
            N_ps.score = 0;
            N_ps.NESW_Direction = 0;
            N_ps.dead = false;
            N_ps.name = name;
            p_snake.Add(newHead);
            N_ps.snake = p_snake;
            N_ps.currentsize = 1;
            try
            {
                completeSnake_allPlayers[id] = N_ps;
            }
            catch (System.Exception) { }
        }
    }
	// Update is called once per frame
	void Update (){
        if (online)
        {
            if (win) {
                Win();
            }
            else
            {
                int alive = 0;
                foreach (player_snake ps in completeSnake_allPlayers)
                {
                    if (!ps.dead)
                    {
                        alive++;
                    }

                }
                AliveCounter.GetComponent<UnityEngine.UI.Text>().text = alive.ToString();
                if (player_count > 1)
                {
                    if (alive == 1)
                    {
                    try{
                        if (completeSnake_allPlayers[localplayernr].dead)
                        {
                            localplayerdeathmanager();
                        }
                        else
                        {
                            Win();
                        }
                    }
                        catch (System.Exception)
                    {
                        localplayerdeathmanager();
                    }
                }
                }
                else
                {
                    if (alive == 0)
                    {
                        try
                        {
                            if (completeSnake_allPlayers[localplayernr].dead)
                            {
                                localplayerdeathmanager();
                            }
                            else
                            {
                                Win();
                            }
                        }
                        catch (System.Exception)
                        {
                            localplayerdeathmanager();
                        }
                    }
                }
            }
        }
        if (localplayerjumped)
        {
            try
            {
                if (completeSnake_allPlayers[localplayernr].dead)
                {
                    localplayerdeathmanager();
                }
            }
            catch (System.Exception)
            {
                localplayerdeathmanager();
            }
        }
        if (!localplayerjumped)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                create_new_player(localplayernr, Mathf.RoundToInt(airplane.transform.position.x), Mathf.RoundToInt(airplane.transform.position.y));
                if (online)
                {
                    c.Send("PlayerJumped|" + localplayernr + "|" + Mathf.RoundToInt(airplane.transform.position.x) + "|" + Mathf.RoundToInt(airplane.transform.position.y));
                }
                localplayerjumped = true;
            }

        }
        if (airplainealive)
        {
            if (airplane.transform.position.x !=-999)
            {
                float timeProgressed = ((Time.timeSinceLevelLoad / 100) - airplainstarttime) / .25f;
                airplane.transform.position = Vector3.Lerp(airplane_start, airplane_end, timeProgressed);
                if (!localplayerjumped)
                {
                    MainCamera.gameObject.transform.position = new Vector3(airplane.transform.position.x, airplane.transform.position.y, 1);
                }
            }
            else
            {
                if (!localplayerjumped)
                {
                    create_new_player(localplayernr, Mathf.RoundToInt(airplane.transform.position.x), Mathf.RoundToInt(airplane.transform.position.y));
                    if (online)
                    {
                        c.Send("PlayerJumped|" + localplayernr + "|" + Mathf.RoundToInt(airplane.transform.position.x) + "|" + Mathf.RoundToInt(airplane.transform.position.y));
                    }
                    localplayerjumped = true;
                }
                    Destroy(airplane.gameObject);
                    airplainealive = false;

            }
        }
        ComChangeDirection();
        UpdateList.Reverse();
        zonemovment();
        if (online)
        {
                float diff_ = 0;
                Vector3 lowest_distance = new Vector3();
            foreach (GameObject apple in currentFood_list)
            {
                if (apple != null)
                {

                    float distance = Vector3.Distance(apple.transform.position, arrow.transform.position);
                    if (diff_ == 0)
                    {
                        diff_ = distance;
                        lowest_distance = apple.transform.position;
                    }
                    else
                    {
                        if (diff_ > distance)
                        {
                            diff_ = distance;
                            lowest_distance = apple.transform.position;
                        }
                    }
                }
            }

                Vector3 diff = lowest_distance - arrow.transform.position;
                diff.Normalize();

                float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        }
        else
        {
            if (currentFood != null)
            {
                Vector3 diff = currentFood.transform.position - arrow.transform.position;
                diff.Normalize();

                float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
            }
        }
        foreach (SmoothMovment sm in UpdateList)
            {
            if (sm.GameObject != null)
            {
                if (sm._3D)
                {
                    float timeProgressed = (Time.time - sm.startTime) / .25f;


                        Vector3 startp = new Vector3(sm.StartPos.x, sm.StartPos.y,-10);
                        Vector3 endp = new Vector3(sm.EndPos.x, sm.EndPos.y, -10);
                        sm.GameObject.transform.position = Vector3.Lerp(startp, endp, timeProgressed);
                        startp = new Vector3(sm.StartPos.x, sm.StartPos.y, -1);
                        endp = new Vector3(sm.EndPos.x, sm.EndPos.y, -1);
                    arrow.transform.position = Vector3.Lerp(startp, endp, timeProgressed);



                }
                else
                {
                    float timeProgressed = (Time.time - sm.startTime) / .25f;
                    sm.GameObject.transform.position = Vector2.Lerp(sm.StartPos, sm.EndPos, timeProgressed);
                }
            }
            }
        UpdateList.Reverse();



    }
    private void localplayerdeathmanager()
    {
        if (!win)
        {
            CancelInvoke("TimerInvoke");
            Win_canavas.SetActive(false);
            Lose_canavas.SetActive(true);
            completeSnake_allPlayers.Clear();
        }
    }
    public void Win()
    {
        win = true;
        CancelInvoke("TimerInvoke");
        Lose_canavas.SetActive(false);
        Win_canavas.SetActive(true);
        completeSnake_allPlayers.Clear();
    }
    private void OnApplicationPause()
    {
        if (online)
        {
            ReSync();
        }
    }
    private void ReSync()
    {
        int x = 0;
        while (player_count != x)
        {
            c.Send("RequestUserInfo|"+localplayernr+"|" + x.ToString());
            x++;
        }
    }
    public void ReSyncUser(string s)
    {
        print("Starting Resync");
        player_snake resyn = ConvertUserInfoBackFromString(s);
        try
        {
            foreach (GameObject del in completeSnake_allPlayers[resyn.playernr].snake)
            {
                Destroy(del);
            }
            List<GameObject> newSnake = new List<GameObject>();
            foreach (GameObject resyncObjects in resyn.snake)
            {
                GameObject snake = Instantiate(resyncObjects);
                newSnake.Add(snake);
            }
            resyn.snake = newSnake;
            update_snake_list.Add(resyn);
        }catch(System.Exception)
        {
            c.Send("getPlayer|" + resyn.playernr.ToString());
        }
    }
    private void OnEnable()
    {

        Snake.hit += hit;
    }
    private void OnDisable()
    {
        completeSnake_allPlayers.Clear();
        Snake.hit -= hit;
    }
    public void RemoveTail(int pid)
    {

        player_snake a=completeSnake_allPlayers[pid];
        if (a.dead != true)
        {
            if (a.snake.Count - 1 == 0)
            {
                a.dead = true;
            }
            a.removetail = true;
            try
            {
                Destroy(a.snake[a.snake.Count - 1].gameObject);
                a.snake.Reverse();
                a.snake.RemoveAt(a.snake.Count - 1);
                a.snake.Reverse();
                a.currentsize--;
                a.score--;
            }
            catch (System.Exception)
            {

            }
        }
        completeSnake_allPlayers[pid] = a;
    }
    void TimerInvoke()
    {
        zoneController();
        Movement();
    }
    void zoneController()
    {
        if (System.Math.Round(timer, 4) == System.Math.Round(endtimer, 4))
        {
            if (localplayeroutofzone)
            {
                RemoveTail(localplayernr);
                if (online)
                {
                    c.Send("ZoneDMG|" + localplayernr);
                }
            }
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
                zonemanager(zonetimer, firstzone);
            }
            else
            {
                if (zonetimer == 0)
                {
                    zonetimer = nextzone;
                }
                zonetimer--;
                zonemanager(zonetimer, nextzone);

            }
            timer = 0M;
        }
        timer+= addTimeperTick;
    }
    public void SetZoneTimer(int time)
    {
        zonetimer = time;
    }
    void zonemanager(float ctime, float zone_)
    {
        if (Mathf.RoundToInt(zone_) / 4 == Mathf.RoundToInt(ctime))
        {
            print("ZoneMSG");
            Zonemsg.GetComponent<UnityEngine.UI.Text>().text = "Next Zone in " + ((zone_ / 4).ToString()) + " seconds";
        }
        else
        {

            Zonemsg.GetComponent<UnityEngine.UI.Text>().text = "";
            if (ctime == 0)
            {
                zonestarttime = Time.time/25;
                lastzonescale = Zone.transform.localScale;
                nextzonescale = new Vector3(Zone.transform.localScale.x / 2, Zone.transform.localScale.y / 2,1);
                Zonemsg.GetComponent<UnityEngine.UI.Text>().text = "Zone is coming";
            }
        }
    }
    void zonemovment()
    {
        if (new Vector3(Zone.transform.localScale.x, Zone.transform.localScale.y,1) != nextzonescale)
        {
            float timeProgressed = ((Time.time / 25) - zonestarttime) / .25f;
            Zone.transform.localScale = Vector3.Lerp(lastzonescale, nextzonescale, timeProgressed);

        }
        try
        {
            float dist = Vector3.Distance(completeSnake_allPlayers[localplayernr].snake[0].transform.position, Zone.transform.position);
            if (dist < (Zone.transform.localScale.x / 4))
            {
                lineRenderer.SetPosition(0, new Vector3(0, 0));
                lineRenderer.SetPosition(1, new Vector3(0, 0));
            }
            else
            {
                lineRenderer.SetPosition(0, completeSnake_allPlayers[localplayernr].snake[completeSnake_allPlayers[localplayernr].snake.Count-1].transform.position);
                lineRenderer.SetPosition(1, Zone.transform.position);
                Zonemsg.GetComponent<UnityEngine.UI.Text>().text = "YOU WILL BE NOT IN THE ZONE! FOLLOW THE LINE";
            }
            if (dist < (Zone.transform.localScale.x / 2))
            {
                localplayeroutofzone = false;
            }
            else
            {
                localplayeroutofzone = true;
            }
        }
        catch (System.Exception) { }
    }
    void Movement()
    {
        UpdateList.Clear();
        int playernr = 0;
        foreach (player_snake snake_player in completeSnake_allPlayers)
        {
            if (!snake_player.dead)
            {
                bool firstTail = true;
                Vector2 newpos = new Vector2(0, 0);
                Vector2 lastpos = new Vector2(0, 0);
                if (snake_player.snake != null)
                {
                    snake_player.snake.Reverse();
                    bool tailremoved = false;
                    int x = 0;
                    foreach (GameObject temp in snake_player.snake)
                    {

                        if (temp != null)
                        {
                            Vector2 temppos = temp.transform.position;
                            GameObject tmp = temp.gameObject;
                            if (snake_player.snake.Count == x)
                            {
                                if (snake_player.removetail)
                                {
                                    player_snake a = completeSnake_allPlayers[snake_player.playernr];
                                    Destroy(a.snake[x].gameObject);
                                    a.snake.RemoveAt(x);
                                    tailremoved = true;
                                    update_snake_list.Add(a);
                                }
                            }
                            if (!tailremoved)
                            {
                                if (!online)
                                {
                                    switch (snake_player.NESW_Direction)
                                    {
                                        case 0: //w
                                            nextPos = new Vector2(temppos.x, temppos.y + float.Parse(movmentSpeed.ToString()));
                                            tmp.transform.rotation = new Quaternion(0, 0, 0, 0);
                                            break;
                                        case 1://d
                                            nextPos = new Vector2(temppos.x + float.Parse(movmentSpeed.ToString()), temppos.y);
                                            tmp.transform.rotation = new Quaternion(0, 0, 90, 0);
                                            break;
                                        case 2://s
                                            nextPos = new Vector2(temppos.x, temppos.y - float.Parse(movmentSpeed.ToString()));
                                            tmp.transform.rotation = new Quaternion(0, 0, 180, 0);
                                            break;
                                        case 3://a
                                            nextPos = new Vector2(temppos.x - float.Parse(movmentSpeed.ToString()), temppos.y);
                                            tmp.transform.rotation = new Quaternion(0, 0, 270, 0);
                                            break;
                                    }
                                }
                                else
                                {
                                    nextPos = snake_player.nextpos;
                                }

                                if (lastpos.x == 0 && lastpos.y == 0)
                                {
                                    if (localplayernr == snake_player.playernr)
                                    {
                                        SmoothMovment camera_sm = new SmoothMovment();
                                        camera_sm.GameObject = MainCamera.gameObject;
                                        camera_sm.startTime = Time.time;
                                        camera_sm.StartPos = MainCamera.transform.position;
                                        camera_sm.EndPos = nextPos;
                                        camera_sm._3D = true;
                                        UpdateList.Add(camera_sm);
                                    }

                                    lastpos = temp.transform.position;
                                    newpos = temp.transform.position;

                                    temp.tag = "SnakeHead";
                                    temp.name = snake_player.playernr.ToString();
                                    if (smoothmovment)
                                    {
                                        SmoothMovment sm = new SmoothMovment();
                                        sm.GameObject = temp;
                                        sm.StartPos = temp.transform.position;
                                        sm.EndPos = nextPos;
                                        sm.startTime = Time.time;
                                        sm.endTime = sm.startTime + float.Parse(movmentSpeed.ToString());
                                        sm._3D = false;
                                        UpdateList.Add(sm);
                                    }
                                    else
                                    {
                                        temp.transform.position = Vector2.Lerp(temp.transform.position, nextPos, float.Parse(movmentSpeed.ToString()));
                                    }
                                }
                                else
                                {
                                    if (firstTail == true)
                                    {
                                        temp.tag = "Tail2";
                                        temp.name = snake_player.playernr.ToString();
                                        firstTail = false;
                                    }
                                    else
                                    {
                                        temp.tag = "Snake";
                                        temp.name = snake_player.playernr.ToString();
                                    }
                                    Vector2 _temp = temp.transform.position;
                                    if (smoothmovment)
                                    {
                                        SmoothMovment sm = new SmoothMovment();
                                        sm.GameObject = temp;
                                        sm.StartPos = temp.transform.position;
                                        sm.EndPos = lastpos;
                                        sm.startTime = Time.time;
                                        sm.endTime = sm.startTime + float.Parse(movmentSpeed.ToString());
                                        sm._3D = false;
                                        UpdateList.Add(sm);
                                    }
                                    else
                                    {
                                        temp.transform.position = Vector2.Lerp(temp.transform.position, lastpos, float.Parse(movmentSpeed.ToString()));
                                    }
                                    lastpos = _temp;
                                }
                            }
                        }
                        x++;
                    }
                }
                player_snake nps = new player_snake();
                nps = snake_player;
                if (snake_player.name != null)
                {
                    if (snake_player.newtail.Count != 0)
                    {
                        nps.neednewtail = true;
                        List<Add_Tail> new_temp = new List<Add_Tail>();

                        foreach (Add_Tail newTail in snake_player.newtail)
                        {
                            Add_Tail nt = newTail;
                            if (nt.ctailpos == 0)
                            {
                                nt.pos = newpos;

                            }
                            if (nt.tailpos == nt.ctailpos)
                            {
                                GameObject n = (GameObject)Instantiate(snakePrefab, nt.pos, transform.rotation);
                                nps.snake.Add(n);

                            }
                            else
                            {
                                nt.ctailpos++;
                                new_temp.Add(nt);
                            }

                        }
                        nps.newtail = new_temp;
                    }
                    else
                    {
                        nps.neednewtail = false;
                    }

                    update_snake_list.Add(nps);

                    snake_player.snake.Reverse();

                    playernr++;
                }
            }
        }
        startPlayerUpdate();
        return;
    }
    void ComChangeDirection()
    {
        foreach (player_snake ps in completeSnake_allPlayers)
        {
            if (!ps.dead)
            {
                if (localplayernr == ps.playernr)
                {
                    player_snake nps = ps;
                    int int_cache = nps.NESW_Direction;
                    if (nps.NESW_Direction != 2 && Input.GetKeyDown(KeyCode.W))
                        nps.NESW_Direction = 0;
                    if (nps.NESW_Direction != 3 && Input.GetKeyDown(KeyCode.D))
                        nps.NESW_Direction = 1;
                    if (nps.NESW_Direction != 0 && Input.GetKeyDown(KeyCode.S))
                        nps.NESW_Direction = 2;
                    if (nps.NESW_Direction != 1 && Input.GetKeyDown(KeyCode.A))
                        nps.NESW_Direction = 3;
                    if (online)
                    {
                        if (int_cache != nps.NESW_Direction)
                        {
                            c.Send("ChangeDirection|" + nps.playernr + "|" + nps.NESW_Direction.ToString());
                        }
                    }
                        update_snake_list.Add(nps);

                }
            }
        }
        startPlayerUpdate();
    }
    public void ChangeDirectionForPlayer(int playernr,int direction)
    {
        if (playernr != localplayernr)
        {
            foreach (player_snake ps in completeSnake_allPlayers)
            {
                if (!ps.dead)
                {
                    if (playernr == ps.playernr)
                    {
                        player_snake nps = ps;
                        nps.NESW_Direction = direction;
                        update_snake_list.Add(nps);
                    }
                }
            }
            startPlayerUpdate();
        }
    }
    void AddPlayerSnakeTooUpdateList(player_snake ps)
    {
        bool added = false;
        while (added == false)
        {
            if (isUpdating == false)
            {
                update_snake_list.Add(ps);
                added = true;
            }
        }
    }
    void startPlayerUpdate()
    {
        isUpdating = true;
        foreach (player_snake us in update_snake_list)
        {
            completeSnake_allPlayers[us.playernr]= us;
        }
        update_snake_list.Clear();
        isUpdating = false;
    }
    public void add_food(int x,int y,int id)
    {
        GameObject new_food=(GameObject)Instantiate(foodPrefab, new Vector2(x, y), transform.rotation);
        new_food.name = id.ToString();
        currentFood_list.Add(new_food);
    }
    public void remove_food(int id)
    {
        Destroy(currentFood_list[id]);
    }
    void FoodFunction()
    {
        if (!online)
        {
            int xPos_Food = Random.Range(-xBound, xBound);
            int yPos_Food = Random.Range(-yBound, yBound);

            currentFood = (GameObject)Instantiate(foodPrefab, new Vector2(xPos_Food, yPos_Food), transform.rotation);
            StartCoroutine(CheckRender(currentFood));
        }

    }
    IEnumerator CheckRender(GameObject IN)
    {
        if (IN != null)
        {
            yield return new WaitForEndOfFrame();
            if (IN != null)
            {
                if (IN.GetComponent<Renderer>().isVisible == false)
                {
                    if (IN.tag == "Food")
                    {
                        if (IN != null)
                        {
                            Destroy(IN);
                        }
                        FoodFunction();

                    }
                }
            }else
            {
                FoodFunction();
            }
        }
        else
        {
            FoodFunction();
        }
    }
    public player_snake ConvertUserInfoBackFromString(string s)
    {
        player_snake ps = new player_snake();
        string[] PlayerData = s.Split('#');
        if (PlayerData[0] == "UserInfo")
        {
            ps.name = PlayerData[1];
            ps.neednewtail = bool.Parse(PlayerData[2]);
            ps.NESW_Direction = int.Parse(PlayerData[3]);
            ps.playernr = int.Parse(PlayerData[4]);
            ps.score = int.Parse(PlayerData[5]);
            ps.currentsize = int.Parse(PlayerData[6]);
            ps.dead = bool.Parse(PlayerData[7]);
            ps.newtail = new List<Add_Tail>();
            ps.snake = new List<GameObject>();
            int count_ = 0;
            bool snakeMode = false;
            Add_Tail Tailcache = new Add_Tail();
            int cache_x = 0;
            int cache_y = 0;
            GameObject GOcache = new GameObject();
            foreach (string data in PlayerData[8].Split('-'))
            {
                if (data == "TAILLIST")
                {
                    count_ = 0;
                    snakeMode = false;
                    continue;
                }
                if (data == "SNAKELIST")
                {
                    count_ = 0;
                    snakeMode = true;
                    continue;
                }
                if (snakeMode)
                {
                    if (count_ == 0)
                        GOcache.tag = data;

                    if (count_ == 1)
                        GOcache.name = data;

                    if (count_ == 2)
                        cache_x = int.Parse(data);

                    if (count_ == 3)
                    {
                        cache_y = int.Parse(data);
                        GOcache.transform.position = new Vector2(cache_x, cache_y);
                        ps.snake.Add(GOcache);
                    }
                    else
                    {
                        if (count_ == 0)
                            Tailcache.ctailpos = int.Parse(data);

                        if (count_ == 1)
                            Tailcache.tailpos = int.Parse(data);

                        if (count_ == 2)
                            cache_x = int.Parse(data);

                        if (count_ == 3)
                        {
                            cache_y = int.Parse(data);
                            Tailcache.pos = new Vector2(cache_x, cache_y);
                            ps.newtail.Add(Tailcache);
                        }
                        count_++;
                    }
                }
            }
        }
        return ps;
    }
    public string getLocalUserInfo()
    {
        foreach (player_snake ps in completeSnake_allPlayers)
        {
            if (!ps.dead)
            {
                if (localplayernr == ps.playernr)
                {
                    string buildmsg="";
                    buildmsg += "UserInfo#";//0
                    buildmsg += ps.name+"#";//1
                    buildmsg += (ps.neednewtail?"TRUE":"FALSE") + "#";
                    buildmsg += ps.NESW_Direction.ToString() + "#";
                    buildmsg += ps.playernr.ToString() + "#";
                    buildmsg += ps.score.ToString() + "#";
                    buildmsg += ps.currentsize.ToString() + "#";
                    buildmsg += (ps.dead ? "TRUE" : "FALSE") + "#";
                    foreach (Add_Tail tail_i in ps.newtail)
                    {
                        buildmsg += "TAILLIST-";
                        buildmsg += tail_i.ctailpos.ToString()+"-";
                        buildmsg += tail_i.tailpos.ToString() + "-";
                        buildmsg += tail_i.pos.x.ToString() + "-";
                        buildmsg += tail_i.pos.y.ToString() + "-";
                    }
                    buildmsg += "#";
                    foreach (GameObject snake in ps.snake)
                    {
                        buildmsg += "SNAKELIST-";
                        buildmsg += snake.tag + "-";
                        buildmsg += snake.name + "-";
                        buildmsg += snake.transform.position.x.ToString()+"-";
                        buildmsg += snake.transform.position.y.ToString()+"-";
                    }
                    buildmsg += "|";
                    return buildmsg;
                }
            }
        }
        return "";
    }
    void hit(string s,string name,string foodid,Vector3 ColliderPositionV3)
    {
        int int_nr = 0;
        try
        {
             int_nr = int.Parse(name);
        }
        catch (System.Exception) {
            print(name);
        }
        player_snake ps = new player_snake();
        try

        {
             ps = completeSnake_allPlayers[int_nr];
        }
        catch (System.Exception)
        {
             ps = new player_snake();
        }
        if (s == "Food")
        {
            Add_Tail tail_temp = new Add_Tail();
            tail_temp.pos = new Vector2(0, 0);
            tail_temp.tailpos = ps.currentsize;
            tail_temp.ctailpos = 0;
            ps.newtail.Add(tail_temp);
            if (!online)
            {
                FoodFunction();
            }else
            {
                if (int_nr == localplayernr) {
                    string tcp_msg = "EatFoodAt|" + name + "|" + ColliderPositionV3.x + "|" + ColliderPositionV3.y + "|" + foodid;
                    c.Send(tcp_msg);
                }
            }
            ps.currentsize++;
            ps.score++;
            scoreText.text = ps.score.ToString();
            int temp = PlayerPrefs.GetInt("HighScore");
            if(ps.score > temp)
            {
                if (!online)
                {
                    PlayerPrefs.SetInt("HighScore", ps.score);
                }
                else
                {
                    if (ps.playernr == localplayernr)
                    {
                        PlayerPrefs.SetInt("HighScore", ps.score);
                    }
                }
            }
        }
        if (s == "Snake") {
            if (!online)
            {
                foreach (GameObject snakepart in ps.snake)
                {
                    Destroy(snakepart);
                }
                ps.dead = true;
                if (completeSnake_allPlayers.Count <= End_game_at)
                {
                    CancelInvoke("TimerInvoke");
                    GameOver();
                }
            }
            else
            {
                if (hightickrate)
                {
                    if (!ps.neednewtail)
                    {
                        c.Send("PlayerDead");
                    }
                }
            }
        }
        if(s == "Tail2")
        {

        }
        if(s == "SnakeHead")
        {

        }
        if(s == "Wall")
        {
            if (online)
            {
                c.Send("PlayerDead");
            }
            foreach (GameObject snakepart in ps.snake)
            {
                Destroy(snakepart);
            }
            ps.dead = true;
        }
        completeSnake_allPlayers[int_nr] = ps;
    }
    public void GameOver()
    {
        try
        {
            if (online)
            {
                c.Send("CloseServer");
                Destroy(c.gameObject);

            }
        }
        catch (System.Exception) { }
        SceneManager.LoadScene(0);
    }
}
