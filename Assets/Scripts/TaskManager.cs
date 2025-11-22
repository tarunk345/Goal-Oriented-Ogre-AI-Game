using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task
{
    public string name;
    public Task(string name) {this.name = name;}
    public abstract bool IsPrimitive{ get; }
}

public abstract class PrimitiveTask : Task {
    public PrimitiveTask(string name) : base(name) {}
    public override bool IsPrimitive => true;

    // Execute returns IEnumerator so it can be yielded (NavMesh movement, waits)
    public abstract IEnumerator Execute(OgreAgent agent);
    // Optional precondition for safekeeping (planner already handles method-level preconds)
    public virtual bool Preconditions(WorldState w, AgentWorldState aw) => true;
    // Optional post effects (helper for your docs; not auto-applied)
    public virtual void PostEffects(WorldState w, AgentWorldState aw) {}
}

public abstract class CompoundTask : Task {
    public List<Method> Methods = new List<Method>();
    public CompoundTask(string name) : base(name) {}
    public override bool IsPrimitive => false;
    public void AddMethod(Method m) => Methods.Add(m);
}

public class SequenceTask : CompoundTask
{
    public SequenceTask(string name) : base(name) { }
}

public class SelectorTask : CompoundTask
{
    public SelectorTask(string name) : base(name) { }
}

public class Method {
    // Precondition for choosing this method
    public Func<WorldState, AgentWorldState, bool> Precondition;
    // Ordered subtasks (compound or primitive)
    public List<Task> Subtasks = new List<Task>();

    public Method(Func<WorldState, AgentWorldState, bool> precondition, params Task[] subtasks)
    {
        Precondition = precondition;
        Subtasks.AddRange(subtasks);
    }

}

public class MoveToTask : PrimitiveTask
{
    public Vector3 Target;
    public float StoppingDistance = 1.2f;

    public MoveToTask(string name, Vector3 target, float stoppingDist = 1.2f) : base(name)
    {
        Target = target; 
        StoppingDistance = stoppingDist;
    }

    public override IEnumerator Execute(OgreAgent agent)
    {
        agent.MoveTo(Target);
        float t = 0f;
        while (!agent.Reached(Target, StoppingDistance))
        {
            t += Time.deltaTime;
            if (t > 10f) break;   // avoid infinite loop
            yield return null;
        }
    }
}

public class PickUpBoulderTask : PrimitiveTask
{
    public PickUpBoulderTask() : base("PickUpBoulder") {}
    public override bool Preconditions(WorldState w, AgentWorldState aw) => aw.boulderNearby;

    public override IEnumerator Execute(OgreAgent agent)
    {
        // simple: assume agent moves to nearestBoulderPos before calling this primitive
        agent.PickUpNearestBoulder();
        // instantaneous or play animation then wait
        yield return new WaitForSeconds(0.5f);
        agent.localState.holdingBoulder = true;
        yield break;
    }

    public override void PostEffects(WorldState w, AgentWorldState aw) { aw.holdingBoulder = true; }
}

public class ThrowBoulderTask : PrimitiveTask
{
    public float ThrowForce = 20f;
    public ThrowBoulderTask() : base("ThrowBoulder") {}
    public override bool Preconditions(WorldState w, AgentWorldState aw) => aw.holdingBoulder;

    public override IEnumerator Execute(OgreAgent agent)
    {
        if (!agent.localState.holdingBoulder) { yield break; } // safety
        agent.ThrowHeldBoulder(agent.world.getPlayerPosition(), ThrowForce);
        agent.localState.holdingBoulder = false;
        yield return new WaitForSeconds(0.2f);
        yield break;
    }

    public override void PostEffects(WorldState w, AgentWorldState aw) { aw.holdingBoulder = false; }
}

public class MeleeAttackTask : PrimitiveTask
{
    public MeleeAttackTask() : base("Melee") {}
    public override bool Preconditions(WorldState w, AgentWorldState aw) => aw.playerInAttackRange;
    public override IEnumerator Execute(OgreAgent agent)
    {
        // play attack animation and check collision/hit
        agent.PerformMeleeAttack();
        yield return new WaitForSeconds(0.6f); // attack duration
        yield break;
    }
}

public class EatMushroomTask : PrimitiveTask
{
    public override bool Preconditions(WorldState ws, AgentWorldState aw) {
        return aw.ogreHungerLevel > 50;
    }
    public EatMushroomTask() : base("EatMushroom") {}
    public override IEnumerator Execute(OgreAgent agent)
    {
        agent.ConsumeNearestMushroom();
        agent.localState.ogreHungerLevel = Mathf.Max(0, agent.localState.ogreHungerLevel - 50);
        agent.world.randomMushroomPos = Vector3.zero;
        yield return new WaitForSeconds(1f);
        yield break;
    }
}

public class IdleTask : PrimitiveTask
{
    public float Duration;
    public IdleTask(float seconds, string name = "Idle") : base(name) { Duration = seconds; }
    public override IEnumerator Execute(OgreAgent agent)
    {
        float t = 0f;
        while (t < Duration)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }
}
