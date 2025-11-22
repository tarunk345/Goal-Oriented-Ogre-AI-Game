using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public WorldState World;
    
    [Header("Game States")]
    public bool treasureStolen = false;
    public bool playerInvisible = false;
    public int playerLives = 2;
    public float invisTimeRemaining = 10;
    public Transform player;

    [Header("Game Objects")]
    public List<GameObject> mushrooms;
    public List<GameObject> boulders;
    public List<Cave> caves;  
    public GameObject spawnPoint;

    [Header("UI variables")]
    public TMPro.TextMeshProUGUI timerText;
    public Image invislayer;

    void Awake()
    {
        Instance = this;
        World = new WorldState();
    }


    IEnumerator Start() {
        yield return null;
        boulders = GameObject.FindGameObjectsWithTag("Boulder").ToList<GameObject>();
        mushrooms = GameObject.FindGameObjectsWithTag("Mushroom").ToList<GameObject>();
        invislayer.enabled = false;
    } 

    void Update()
    {
       if (World.randomMushroomPos == Vector3.zero)
        {
            World.randomMushroomPos = GameObject.FindGameObjectWithTag("Mushroom").transform.position;
        }

        World.updatePlayerPosition(player.transform.position);
        World.playerInvisible = playerInvisible;
        
        if (!treasureStolen) {
            foreach (Cave c in caves)
            {
                if (c.treasureStolen)
                {
                    treasureStolen = true;
                    World.TreasureStolen = true;
                    World.treasureStolenNotified = true;
                }
            }
        }

        if (playerLives != 0 && treasureStolen && spawnPoint.GetComponent<Collider>().bounds.Intersects(player.GetComponent<Collider>().bounds))
        {
            EndGame();
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!playerInvisible && invisTimeRemaining > 0) {
                playerInvisible = true;
            }
            else
                playerInvisible = false;
                invislayer.enabled = false;
        }
        if (playerInvisible && invisTimeRemaining > 0)
        {
            invislayer.enabled = true;
            invisTimeRemaining -= Time.deltaTime;
            timerText.text = $"Invisibility Remaining: {invisTimeRemaining}s";
        }
        if (invisTimeRemaining <= 0)
        {
            playerInvisible = false;
            invislayer.enabled = false;
            invisTimeRemaining = 0;
            timerText.text = $"Invisibility Remaining: 0.000000s";
        }
    }


    private void EndGame()
    {
        
    }
}
