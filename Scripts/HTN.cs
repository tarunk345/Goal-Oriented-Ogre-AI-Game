using UnityEngine;

public static class HTN
{   
    public static CompoundTask BuildOgreRoot(System.Func<WorldState> getWorld, Vector3 spawnPoint)
    {
        // helper to access world in method preconditions
        CompoundTask root = new SelectorTask("OgreRoot");

        // ----- IdleBehaviour compound -----
        var idle = new SequenceTask("IdleBehaviour");
        // Idle method A: pace in front of cave
        idle.AddMethod(new Method(
            (w,aw) => true, // always allowed
            new MoveToTask("GoOutsideCave", spawnPoint, 1.2f),
            new MoveToTask("PaceLeft", Vector3.zero, 1.2f), // replace Vector3.zero later with cave-left
            new IdleTask(1f, "LookLeft"),
            new MoveToTask("PaceRight", Vector3.zero, 1.2f), // replace with cave-right
            new IdleTask(1f, "LookRight")
        ));
        // Idle method B: stand / rotate in place
        idle.AddMethod(new Method((w,aw) => true,
            new IdleTask(2f, "Stand"),
            new IdleTask(1f, "LookAround")
        ));
        // Idle method C: forage (only if hungry and mushroom nearby)
        idle.AddMethod(new Method(
            (w,aw) => aw.ogreHungerLevel >= 50,
            new MoveToTask("GoToMushroom", Vector3.zero),
            new EatMushroomTask()
        ));

        // ----- ChooseAttack compound -----
        var chooseAttack = new SequenceTask("ChooseAttack");
        // Method boulder: if boulder nearby and not holding one
        chooseAttack.AddMethod(new Method(
            (w,aw) => aw.boulderNearby && !aw.holdingBoulder,
            new MoveToTask("MoveToBoulder", Vector3.zero),
            new PickUpBoulderTask(),
            new MoveToTask("GetThrowPos", Vector3.zero),
            new ThrowBoulderTask()
        ));
        // Method melee: if in melee range OR no boulder available
        chooseAttack.AddMethod(new Method(
            (w,aw) => aw.playerInAttackRange,
            new MoveToTask("CloseIn", Vector3.zero),
            new MeleeAttackTask()
        ));

        // ----- EngagePlayer compound -----
        var engage = new SequenceTask("EngagePlayer");
        engage.AddMethod(new Method(
            (w,aw) => aw.playerInFOV && !w.playerInvisible,
            new MoveToTask("MoveToPlayer", Vector3.zero),
            chooseAttack
        ));

        // ----- SearchForTreasure (simple chase) -----
        var chase = new SequenceTask("ChaseThief");
        chase.AddMethod(new Method(
            (w,aw) => w.TreasureStolen,
            new MoveToTask("MoveToLastKnownPlayer", Vector3.zero),
            chooseAttack
        ));

        // ----- Root methods -----
        // Method 0: Guard treasure (default)
        root.AddMethod(new Method(
            (w,aw) => !w.TreasureStolen,
            idle // note: compound as a subtask
        ));

        // Method 1: Player detected
        root.AddMethod(new Method(
            (w,aw) => aw.playerInFOV && !w.playerInvisible,
            engage
        ));

        // Method 2: Treasure stolen (higher priority if stolen)
        root.AddMethod(new Method(
            (w,aw) => w.TreasureStolen,
            chase
        ));

        return root;
    }
}