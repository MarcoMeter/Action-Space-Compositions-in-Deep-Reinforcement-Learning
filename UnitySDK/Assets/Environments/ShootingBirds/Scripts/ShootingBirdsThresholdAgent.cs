using UnityEngine;

public class ShootingBirdsThresholdAgent : ShootingBirdsAgent
{
    #region Member Fields
    private float _discreteActionThreshold = 0.5f;
    #endregion

    #region Unity ML-Agents
    /// <summary>
    /// Saves the position of the agent.
    /// </summary>
    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

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
    /// Executes actions for movement, shooting and reloading.
    /// </summary>
    public override void AgentAction(float[] vectorAction)
    {
        // Unit circle locomotion: first action determines the angle (direction), the second one the speed
        // Process the action
        float angle = vectorAction[0] * 90;
        // Retrieve position form unit circle
        Vector3 circumferencePoint = new Vector3((Mathf.Cos(angle * Mathf.Deg2Rad)),
                                                (Mathf.Sin(angle * Mathf.Deg2Rad)),
                                                0);
        // Apply velocity based on direction (coming from the unit circle)
        _rigidbody.velocity = circumferencePoint.normalized * vectorAction[1] * _movementSpeed;

        // Shoot
        if (vectorAction[2] > 0)
        {
            Shoot();
        }
        // Reload
        if (vectorAction[3] > 0)
        {
            Reload();
        }

        base.AgentAction(vectorAction);
    }
    #endregion
}