using System.Collections.Generic;
using UnityEngine;

public class Planner
{
    public List<PrimitiveTask> BuildPlan(Task root, WorldState world, AgentWorldState agent)
    {
        List<PrimitiveTask> plan = new List<PrimitiveTask>();
        bool ok = DecomposeTask(root, world, agent, plan);
        return ok ? plan : null;
    }

    private bool DecomposeTask(Task task, WorldState world, AgentWorldState agent, List<PrimitiveTask> plan)
    {
        if (task.IsPrimitive)
        {
            var p = (PrimitiveTask)task;
            // new dual-state preconditions
            if (!p.Preconditions(world, agent))
                return false;
            plan.Add(p);
            return true;
        }

        var compound = (CompoundTask)task;
        foreach (var method in compound.Methods)
        {
            if (method.Precondition == null || method.Precondition(world, agent))
            {
                bool ok = true;
                List<PrimitiveTask> tempPlan = new List<PrimitiveTask>();
                foreach (var sub in method.Subtasks)
                {
                    if (!DecomposeTask(sub, world, agent, tempPlan))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    plan.AddRange(tempPlan);
                    return true;
                }
            }
        }
        return false;
    }
}
