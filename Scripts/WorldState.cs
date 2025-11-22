using Unity.VisualScripting;
using UnityEngine;


public class WorldState
{
    private Vector3 playerPosition = new Vector3(0,0,0); // player pos 
    public Vector3 randomMushroomPos = new Vector3(0,0,0);


    public bool TreasureStolen = false;                 // treasure stolen 
    public bool playerInvisible = false;                // true while invisibility active
    public bool treasureStolenNotified = false;         // ogre is aware that treasure was stolen (global event)

    public Vector3 getPlayerPosition()
    {
        return playerPosition;
    }

    public void updatePlayerPosition(Vector3 pos)
    {
        playerPosition = new Vector3(pos.x,pos.y,pos.z);
    }
}

public class AgentWorldState
{
    private Vector3 ogrePosition = new Vector3(0,0,0);  // current ogre pos
    public Vector3 nearestBoulderPos = new Vector3(0,0,0);
    public int ogreHungerLevel = 0;                   // 0 to 100
    public float timeSinceLastIdle = 0;                 // optional for creating idle behavior switching
    public float distanceToPlayer = 0;                  // ogre to player distance cached to speed up checks
    public bool playerInFOV = false;                    // computed from positions + facing
    public bool playerInAttackRange = false;            // melee or throw range check
    public bool holdingBoulder = false;                 // ogre currently holding boulder
    public bool boulderNearby = false;


    public void updateOgrePosition(Vector3 pos)
    {
        ogrePosition = new Vector3(pos.x,pos.y,pos.z);
    }

    public Vector3 getOgrePosition()
    {
        return ogrePosition;
    }
}