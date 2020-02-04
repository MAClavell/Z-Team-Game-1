﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Starting, Playing, Paused, Ended }

public class GameManager : Singleton<GameManager>
{
	public const float CONSTANT_Y_POS = -0.92f;
    public const ushort ROBOT_ATTACK_DAMAGE = 1;

    //Colors
    private const float TRANSPARENT_ALPHA = 110f / 255f;
    public static readonly Color WHITE = Color.white;
    public static readonly Color GREEN = new Color(125f / 255f, 1f, 100f / 255f, 1);
    public static readonly Color RED = new Color(1, 100f / 255f, 115f / 255f, 1);
    public static readonly Color GREEN_TRANSPARENT = new Color(GREEN.r, GREEN.g, GREEN.b, TRANSPARENT_ALPHA);
    public static readonly Color RED_TRANSPARENT = new Color(RED.r, RED.g, RED.b, TRANSPARENT_ALPHA);
    public static readonly Color GREY_TRANSPARENT = new Color(180f / 255f, 180f / 255f, 180f / 255f, TRANSPARENT_ALPHA);

    [SerializeField] RobotSpawnZone[] robotSpawnZones;
    [SerializeField] GameObject robotPrefab;
    [SerializeField] GameObject towerPrefab;
    [SerializeField] GameObject zbuckPrefab;
    [SerializeField] Sprite upgradedTowerSprite;
    [SerializeField] float boundsX;
    [SerializeField] float boundsY;

    public Player player { get; private set; }
    public Sprite UpgradedTowerSprite { get => upgradedTowerSprite; }

    public float BoundsX { get { return boundsX; } }
    public float BoundsY { get { return boundsY; } }


    /// <summary>
    /// The current state of the game
    /// </summary>
    public GameState CurrentState { get; private set; }

    private List<Tower> towers;
    private RobotManager robotManager;

    //Initialize vars
    private void Awake()
    {
        towers = new List<Tower>();
        robotManager = new RobotManager(robotPrefab, robotSpawnZones);
        player = GameObject.FindObjectOfType<Player>();
        player.TowerSize = towerPrefab.GetComponent<SphereCollider>().radius;
    }

    // Start is called before the first frame update
    void Start()
    {
        NewGame();
    }

    /// <summary>
    /// Reset data in the manager for a new game
    /// </summary>
    private void NewGame()
    {
        CurrentState = GameState.Starting;
        robotManager.Start();
        player.Init();

        //Remove any towers
        foreach (var t in towers)
           Destroy(t.gameObject);
        towers.Clear();
    }

    /// <summary>
    /// Begin the game
    /// </summary>
    private void BeginGame()
    {
        CurrentState = GameState.Playing;
    }

    // Update is called once per frame
    void Update()
    {
        switch (CurrentState)
        {
            case GameState.Starting:
                BeginGame();
                break;

            case GameState.Playing:
                robotManager.Update();

#if UNITY_EDITOR
                //Press 'R' to add zombies (debug only)
                if (Input.GetKeyDown(KeyCode.R))
                    robotManager.Spawn();

                if (Input.GetKeyDown(KeyCode.T))
                    SpawnZBucks(2, Vector3.zero, 1);
#endif
                break;

            case GameState.Paused:
                break;

            case GameState.Ended:
                SceneManager.LoadScene("Menu");
                break;

            default:
                Debug.LogError("Unknown game state reached. What did you do??");
                break;
        }
    }

    /// <summary>
    /// Set the game state to a new state.
    /// </summary>
    /// <param name="newState"></param>
    public void SetGamestate(GameState newState)
    {
        CurrentState = newState;
    }

    /// <summary>
    /// Spawns a tower into the world
    /// </summary>
    /// <param name="position">Position to spawn at</param>
    /// <param name="rotation">Rotation to spawn at</param>
    public void SpawnTower(Vector3 position, Quaternion rotation)
    {
        var tower = Instantiate(towerPrefab, position, Quaternion.Euler(90,0,0)).GetComponent<Tower>();
        tower.InitRotation(rotation);
        towers.Add(tower);
    }

    /// <summary>
    /// Remove the tower from the list
    /// </summary>
    public void RemoveTower(Tower tower)
    {
        towers.Remove(tower);
    }

    /// <summary>
    /// Spawn an amount of zbucks at a position
    /// </summary>
    /// <param name="amount">The amount to spawn</param>
    /// <param name="position">The center position</param>
    /// <param name="valuePerBuck">The value of each spawned zbuck</param>
    public void SpawnZBucks(ushort amount, Vector3 position, ushort valuePerBuck)
    {
        for(ushort i = 0; i < amount; i++)
        {
            //TODO: stop coins from going offscreen
            float angle = Random.Range(0, 360);
            Quaternion rotation = Quaternion.Euler(90, 0, angle);
            Vector3 target = position + (rotation * new Vector3(1, CONSTANT_Y_POS, 0));
            
            //Initialize the zbuck
            Instantiate(zbuckPrefab, position, rotation).GetComponent<ZBuck>().Init(target, valuePerBuck);
        }
    }

    /// <summary>
    /// Sets the build mode setting (display the radius of all turrets)
    /// </summary>
    /// <param name="buildOn">Whether build mode is on or not</param>
    public void SetBuildMode(bool buildOn)
    {
        foreach(Tower t in towers)
        {
            t.SetBuildMode(buildOn);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Gizmos
    /// </summary>
    [ExecuteInEditMode]
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(0,0,0), new Vector3(boundsX*2, 1, boundsY*2));

        Gizmos.color = Color.white;
        foreach (var rsz in robotSpawnZones)
            Gizmos.DrawWireCube(new Vector3(rsz.position.x, CONSTANT_Y_POS, rsz.position.y), new Vector3(rsz.size.x * 2, 0, rsz.size.y * 2));
    }
#endif
}
