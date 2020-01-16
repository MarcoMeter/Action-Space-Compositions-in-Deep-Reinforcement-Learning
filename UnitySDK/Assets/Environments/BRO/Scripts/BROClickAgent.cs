using UnityEngine;

public class BROClickAgent : BROAgent
{
    #region Unity ML-Agents
    /// <summary>
    /// Resets the agent and its environment.
    /// </summary>
    public override void AgentReset()
    {
        base.AgentReset();
    }

    /// <summary>
    /// Observation space
    /// </summary>
    public override void CollectObservations()
    {
        base.CollectObservations();
    }

    /// <summary>
    /// Action execution of the agent.
    /// </summary>
    /// <param name="vectorAction"></param>
    /// <param name="textAction"></param>
    public override void AgentAction(float[] vectorAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        switch (action)
        {
            case 0:
                Move();
                break;
            case 1:
                Blink();
                break;
            case 2:
                break;
        }

        AddReward(0.01f); // For each step of survival
    }
    #endregion
}