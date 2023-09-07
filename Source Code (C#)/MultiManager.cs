using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class MultiManager : MonoBehaviour, INetworkRunnerCallbacks
{

    public void OnConnectedToServer(NetworkRunner runner)
    {
        //throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        //throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        //throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        //throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        //throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        //throw new NotImplementedException();
    }

    Camera cam;
    NetworkInputData inputData = new();
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        input.Set(inputData);
        inputData = default;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        //throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            NetworkObject playerNetOb = runner.Spawn(playerPrefab,
                spawnGrid[UnityEngine.Random.Range(0, spawnGrid.Count)].position, 
                Quaternion.identity, player);
            spawnedPlayers.Add(player, playerNetOb);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (spawnedPlayers.TryGetValue(player, out NetworkObject netOb))
            {
                runner.Despawn(netOb);
                spawnedPlayers.Remove(player);
            }
        }
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        //throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        //throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        //throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        //throw new NotImplementedException();
    }
    [SerializeField] List<Transform> spawnGrid;
    [SerializeField] NetworkPrefabRef playerPrefab;
    [SerializeField] GameObject menu;
    [SerializeField] GameObject inGameUI;
    [SerializeField] Button hostButton, joinButton;
    [SerializeField] TMP_Text score;
    bool isGameStarted = false;
    Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new();
    Transform sp;
    NetworkRunner runner;
    public EnemySpawner enemySpawner;
    public AudioClip menuClip;
    public AudioClip[] ambientClips;
    AudioSource aSource;
    // Start is called before the first frame update
    void Start()
    {
        menu.SetActive(true);
        cam = Camera.main;
        aSource = cam.gameObject.GetComponent<AudioSource>();
        aSource.clip = menuClip;
        aSource.loop = true;
        aSource.volume = 0.6f;
        aSource.Play();

        if (spawnGrid == null)
            Debug.LogWarning("SpawnGrid is not assigned - Multimanager");

        if (playerPrefab == null)
            Debug.LogWarning("PlayerPrefab is not assigned - Multimanager");

        if (menu == null)
            Debug.LogWarning("Menu is not assigned - Multimanager");
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameStarted)
        {
            if (!aSource.isPlaying){
                PlayAmbient();
            }

            if (Input.GetKey(KeyCode.A))
                inputData.dir += Vector3.left;
            if (Input.GetKey(KeyCode.D))
                inputData.dir += Vector3.right;
            if (Input.GetKey(KeyCode.W))
                inputData.dir += Vector3.forward;
            if (Input.GetKey(KeyCode.S))
                inputData.dir += Vector3.back;
            inputData.dir.Normalize();

            if (Input.GetKey(KeyCode.Mouse0))
                inputData.primary = true;

            if (Input.GetKey(KeyCode.Q))
                inputData.ability1 = true;
            if (Input.GetKey(KeyCode.E))
                inputData.ability2 = true;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                inputData.upgrade1 = true;
            if (Input.GetKeyUp(KeyCode.Alpha1))
                inputData.upgrade1 = false;

            if (Input.GetKeyDown(KeyCode.Alpha2))
                inputData.upgrade2 = true;
            if (Input.GetKeyUp(KeyCode.Alpha2))
                inputData.upgrade2 = false;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
                inputData.aim = hit.point;
        }

        if (inGameUI.activeSelf && isGameStarted)
        {
            score.text = enemySpawner.score.ToString("F2");
        }
    }

    public void PlayAmbient(){
        aSource.clip = ambientClips[UnityEngine.Random.Range(0, ambientClips.Length)];
        aSource.Play();
    }

    public void StartHost()
    {
        StartGame(GameMode.Host);
    }
    public void StartClient()
    {
        StartGame(GameMode.Client);
    }

    public void Restart(){
        //SceneManager.LoadScene(0);
        Application.Quit();
    }
    async void StartGame(GameMode mode)
    {
        menu.SetActive(false);
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        cam = Camera.main;
        isGameStarted = true;
        enemySpawner.isSpawning = true;
        inGameUI.SetActive(true);
        aSource.loop = false;
        aSource.Stop();
        
    }
}
