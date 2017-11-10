using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LobbyUI : MonoBehaviour {
    public static LobbyUI Instance { set; get; }
    public GameObject playerlist;
    public GameObject playerprefab;
    public GameObject startButton;
    public GameObject tickrate;
    public GameObject displaytickrater;
    private Client c;
    private List<GameObject> PlayerList = new List<GameObject>();

    // Use this for initialization
    void Start () {
        Instance = this;
        try
        {
            c = FindObjectOfType<Client>();
            c.LUI = this;
            setCurrentUserHost();
            c.GetPlayerList();

        }
        catch (System.Exception)
        {
            back();
        }
        try
        {
            Destroy(MainMenu.Instance.gameObject);
        }
        catch (System.Exception) { }

    }

	// Update is called once per frame
	void Update () {

	}

    public void addPlayer(int id,string name,bool isHost=false)
    {
        print("AddPlayer");
        GameObject newG =Instantiate(playerprefab);
        PlayerList.Add(newG);
        GameObject Host= newG.transform.Find("Host").gameObject;
        GameObject NewName =newG.transform.Find("Name").gameObject;
        NewName.GetComponent<UnityEngine.UI.Text>().text = name;
        Host.SetActive(isHost);
        if(id == c.localnr)
        {
            setCurrentUserHost(isHost);
        }
        newG.transform.SetParent(playerlist.transform);
    }
    public void removePlayer(int id)
    {
        try
        {
            GameObject temp = PlayerList[id];
            PlayerList.RemoveAt(id);
            Destroy(temp);
        }
        catch (System.Exception) { }

    }
    public void StartGame()
    {
        c.Send("StartGame|");
    }
    public void ChangeTickRate()
    {
        c.Send("NewTickrate|"+tickrate.GetComponent<UnityEngine.UI.Slider>().value);
    }
    public void ChangeTickTimer(float i)
    {
        print("ChangeTick");
        displaytickrater.GetComponent<UnityEngine.UI.Text>().text=i.ToString();
    }
    public void setCurrentUserHost()
    {
        print(c.IsHost);
        startButton.SetActive(c.IsHost);
    }
    public void setCurrentUserHost(bool ishost)
    {
        startButton.SetActive(ishost);
    }
    public void GoInGame()
    {

    }
    public void LoadPlayers()
    {
        print("GetClient");
        c.GetPlayerList();
        setCurrentUserHost();
    }
    public void back()
    {
        try
        {
            if (c.IsHost)
            {
                c.Send("CloseServer");
            }

            c.CloseSocket();
            Destroy(c.gameObject);
        }
        catch (System.Exception) { }
        SceneManager.LoadScene(0);

    }

}
