using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OgreAgent : MonoBehaviour {
    public WorldState world;
    public AgentWorldState localState;
    public Planner planner = new Planner();
    public CompoundTask rootTask;
    public NavMeshAgent navAgent;
    private List<PrimitiveTask> currentPlan = null;

    private float FOVRange = 100f;
    private float FOVAngle = 45f;
    private float meleeRange = 5f;
    private float throwForce = 30f;
    public Vector3 spawnPoint;

    void Start() {
        world = GameManager.Instance.World;
        localState = new AgentWorldState();
        navAgent = GetComponent<NavMeshAgent>();
        rootTask = HTN.BuildOgreRoot(() => world, spawnPoint);
        BuildAndStartPlan();
    }

    void Update() {
        UpdateState();
        // If plan is empty or invalid, replan
        if (ShouldReplan()) {
            BuildAndStartPlan();
        }
    }

    void UpdateState()
    {
        localState.updateOgrePosition(transform.position);
        localState.ogreHungerLevel += 2;
        localState.timeSinceLastIdle += Time.deltaTime;


        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 toPlayer = playerPos - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        // FIELD OF VIEW CHECK
        float angle = Vector3.Angle(transform.forward, toPlayer);
        bool inFOV = angle < FOVAngle * 0.5f;
        bool visible = !world.playerInvisible; // global invisibility
        bool inRange = distanceToPlayer <= FOVRange;

        bool hasLineOfSight = false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f,
                            toPlayer.normalized,
                            out hit,
                            distanceToPlayer))
        {
            if (hit.collider.CompareTag("Player"))
                hasLineOfSight = true;
        }

        localState.playerInFOV = inFOV && inRange && visible && hasLineOfSight;

        // ATTACK RANGE CHECK
        localState.playerInAttackRange = distanceToPlayer <= meleeRange;

    }



    bool ShouldReplan()
    {
        // simple rule: replan if no plan or player enters FOV or treasure stolen
        if (currentPlan == null) return true;
        if (localState.playerInFOV) return true;
        if (world.TreasureStolen) return true;
        return false;
    }

    void BuildAndStartPlan()
    {
        var plan = planner.BuildPlan(rootTask, world, localState);
        if (plan == null)
        {
            currentPlan = null;
            return;
        }
        currentPlan = plan;
        StopAllCoroutines();
        StartCoroutine(ExecutePlanCoroutine());
    }

    IEnumerator ExecutePlanCoroutine()
    {
        foreach (var step in currentPlan)
        {
            // for MoveToTasks where the target depends on world, set targets here:
            if (step is MoveToTask mt)
            {
                // example: if MoveToTask.name == "MoveToPlayer" then set mt.Target = world.player.position
                if (mt.name == "MoveToPlayer" || mt.name == "MoveToLastKnownPlayer")
                    mt.Target = world.getPlayerPosition();
                else if (mt.name == "MoveToBoulder" && localState.boulderNearby)
                    mt.Target = localState.nearestBoulderPos;
                else if (mt.name == "GoToMushroom")
                    mt.Target = world.randomMushroomPos;
                // you can also use delegates to compute target on Execute()
            }

            // run the primitive
            yield return StartCoroutine(step.Execute(this));

            // apply post-effects if any
            step.PostEffects(world, localState);

            // quick re-evaluation: if treasure stolen or player now visible, break to replan
            if (world.TreasureStolen || localState.playerInFOV)
                break;
        }
        // plan finished — will replan on Update()
    }

    // small helper wrappers used in tasks
    public void MoveTo(Vector3 pos) => navAgent.SetDestination(pos);
    public bool Reached(Vector3 pos, float dist = 1.2f) => Vector3.Distance(transform.position, pos) <= dist;
    public void PickUpNearestBoulder() { /* attach boulder */ }
    public void ThrowHeldBoulder(Vector3 target, float force) { /* spawn impulse */ }
    public void PerformMeleeAttack() { /* damage player if in range & visible */ }
    public void ConsumeNearestMushroom() { /* remove mushroom GameObject */ }
}