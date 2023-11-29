using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    [SyncVar]
    public int playersReady = 0;

    [SyncVar]
    public int playersEntity = 0;

    [SyncVar]
    public int playersHumans = 0;

    [SyncVar]
    public int maxPlayersEntity = 1;

    [SyncVar]
    public int maxPlayersHumans = 4;

    [SerializeField]
    private Transform _humanSpawnTransform;

    [SerializeField]
    private Transform _entitySpawnTransform;

    [SerializeField]
    private List<GameObject> characters = new List<GameObject>();

    [SerializeField]
    private GameObject ClientPrefab;
    private GameObject clientObject;

    public event Action<int> OnEntityValueChanged;
    public event Action<int> OnHumansValueChanged;

    private int spawnCount = 0;
    private CustomNetworkManager manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //SpawnClient();
    }
    private void OnEnable()
    {
        if(Manager != null)
        {
            if (NetworkClient.ready)
            {
                SpawnClients();
                Debug.Log("Должен был заспавнить челов");
            }
            else
           {
                StartCoroutine(ReadyWaiter());
           }
        }
    }
    IEnumerator ReadyWaiter()
    {
        yield return new WaitUntil(() => NetworkClient.ready);
       // Manager.OnAddPlayer += SpawnClients;
        SpawnClients();
        Debug.Log("Должен был заспавнить челов Корутина");
    }

    [Command(requiresAuthority =false)]
    public void SpawnClients()
    {
        Debug.Log("Должны спавниться");
        Debug.Log("Players Count: " + Manager.Gameplayers.Count);
        for (int i = 0; i < Manager.Gameplayers.Count; i++)
        {
        var client = Instantiate(ClientPrefab);
            Debug.Log("При спавне конн: " + Manager.Gameplayers[i].playerConnection);
            NetworkServer.Spawn(client, Manager.Gameplayers[i].playerConnection);
            client.GetComponent<ClientDataPlayer>().playerConnection = Manager.Gameplayers[i].playerConnection;
           // spawnCount++;
        }
    }

    public void SpawnHuman(GameObject characterSelectorPanel, ClientDataPlayer conn)
    {
        Debug.Log("Try spawn human");
        characterSelectorPanel.SetActive(false);
        CmdSpawn(0, conn, false);
    }

    public void SpawnEntity(GameObject characterSelectorPanel, ClientDataPlayer conn)
    {
        Debug.Log("Try spawn entity");

        characterSelectorPanel.SetActive(false);
        CmdSpawn(1, conn, true);
    }

    [Command(requiresAuthority = false)]
    void CmdSpawn(int spawnIndex, ClientDataPlayer conn, bool isEntity)
    {
        Debug.Log("Spawn Method");
        Vector3 spawnPosition;
        if (isEntity)
            spawnPosition = _entitySpawnTransform.position;
        else
            spawnPosition = _humanSpawnTransform.position;
        GameObject player = Instantiate(characters[spawnIndex], spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(player, conn.playerConnection);
        NetworkServer.Destroy(clientObject);
    }

    [Command(requiresAuthority = false)]
    public void UpdateSelectedEntity(
        LobbyManager lobbyManager,
        int amountToChange,
        ToggleButtonSelector senderButton
    )
    {
        UpdateSelectedServ();
        lobbyManager.playersEntity += amountToChange;
        //  GameManager.Instance.playersEntity = lobbyManager.playersEntity;
        OnEntityValueChanged?.Invoke(playersEntity);
        //  lobbyManager.playersEntityCountText.text = lobbyManager.playersEntity.ToString();
        // if (lobbyManager.playersEntity >= lobbyManager.maxPlayersEntity)
        //     senderButton.DisableButton();
        // else
        //     senderButton.EnableButton();
    }

    [Command(requiresAuthority = false)]
    public void UpdateSelectedHumans(
        LobbyManager lobbyManager,
        int amountToChange,
        ToggleButtonSelector senderButton
    )
    {
        lobbyManager.playersHumans += amountToChange;
        //   GameManager.Instance.playersHumans = lobbyManager.playersHumans;

        // OnHumansValueChanged?.Invoke(playersHumans);
        UpdateSelectedServ();

        if (lobbyManager.playersHumans >= lobbyManager.maxPlayersHumans)
            senderButton.DisableButton();
        else
            senderButton.EnableButton();
    }

    [Command(requiresAuthority = false)]
    public void UpdateSelectedServ()
    {
        OnHumansValueChanged?.Invoke(playersHumans);
        OnEntityValueChanged?.Invoke(playersEntity);
    }

    [Command(requiresAuthority = false)]
    public void UpdateReadyPlayers(
        LobbyManager lobbyManager,
        int amountToChange,
        ToggleButtonSelector senderButton
    )
    {
        lobbyManager.playersReady += amountToChange;
    }
}
