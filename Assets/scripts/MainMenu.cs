using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour {
    public Text HighScoreValueText;
    // Use this for initialization
    public static MainMenu Instance;
    public GameObject mainMenu;
    public GameObject lobby;
    public GameObject connect_menu;
    public GameObject serverPrefab;
    public GameObject clientPrefab;
    public GameObject ClientName;
    public int port=50000;
    private Client c;
    private Server s;
    void Start () {
        connect_menu.SetActive(false);
        lobby.SetActive(false);
        HighScoreValue();
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


	// Update is called once per frame
	void Update () {

	}
    public void PlayButton()
    {
        SceneManager.LoadScene(1);
    }
    public void ConnectButton()
    {
        connect_menu.SetActive(true);
        mainMenu.SetActive(false);
        Debug.Log("Connect");
    }
    public void StartConnectButton()
    {
        string hostA = GameObject.Find("InputField").GetComponent<InputField>().text;
        string CName = ClientName.GetComponent<UnityEngine.UI.Text>().text;
        if (string.IsNullOrEmpty(CName))
        {
            CName = "Player" + Random.Range(0, 1000000);
        }
        if (string.IsNullOrEmpty(hostA))
        {
            hostA = "127.0.0.1";
        }
        c = Instantiate(clientPrefab).GetComponent<Client>();
        c.localname = CName;
        if (c.ConnectToServer(hostA, port))
        {
            connect_menu.SetActive(false);

            SceneManager.LoadScene(2);
        }else
        {
            Destroy(c.gameObject);
        }
    }
    public void HostButton()
    {
        string CName = ClientName.GetComponent<UnityEngine.UI.Text>().text;
        mainMenu.SetActive(false);
        s = Instantiate(serverPrefab).GetComponent<Server>();
        s.OnServerInitialized();

        if (string.IsNullOrEmpty(CName))
        {
            CName = "Player" + Random.Range(0, 1000000);
        }
        c = Instantiate(clientPrefab).GetComponent<Client>();
        c.localname = CName;
        if (c.ConnectToServer("127.0.0.1", port))
        {
            lobby.SetActive(true);
            SceneManager.LoadScene(2);
        }else
        {
            Destroy(s.gameObject);
            Destroy(c.gameObject);
        }
    }
    public void StartGameHostButton()
    {
        c.Send("StartGame|");
    }
    void HighScoreValue()
    {
        HighScoreValueText.text = PlayerPrefs.GetInt("HighScore").ToString();
    }
    public void BackToMainMenu()
    {
        try
        {
            if (s.serverStarted)
            {
                s.stop_server();
            }
        }
        catch (System.Exception) { }
        try
        {
            c.CloseSocket();
        }
        catch (System.Exception) { }
        mainMenu.SetActive(true);
        lobby.SetActive(false);
        connect_menu.SetActive(false);
    }
}
