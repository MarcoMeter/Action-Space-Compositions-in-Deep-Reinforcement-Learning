using UnityEngine;

public class ShootingBirdsShootingAgent : ShootingBirdsAgent
{
    #region Unity ML-Agents
    /// <summary>
    /// Sets the agent back to his initial position.
    /// </summary>
    public override void AgentReset()
    {
        base.AgentReset();
    }

    /// <summary>
    /// Observes the state space.
    /// </summary>
    public override void CollectObservations()
    {
        base.CollectObservations();
    }

    /// <summary>
    /// Executes discrete actions: nothing, shooting, reloading.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        switch (action)
        {
            case 0:
                // Nothing
                break;
            case 1:
                Shoot();
                break;
            case 2:
                Reload();
                break;
        }
    }
    #endregion
}