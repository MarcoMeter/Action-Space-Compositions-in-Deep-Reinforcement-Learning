using UnityEngine;

public class ShootingBirdsMovementAgent : ShootingBirdsAgent
{
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
    /// Observes the complete state space, excluding the ammunition's state.
    /// </summary>
    public override void CollectObservations()
    {
        // Relative position to the origin
        AddVectorObs((transform.position.x - _origin.x) / 17.715f);     // 1
        AddVectorObs((transform.position.y - _origin.y) / 10.215f);     // 1
        // Velocity of the agent (i.e. direction)
        AddVectorObs(_rigidbody.velocity.normalized);                   // 2
        // Speed of the agent
        AddVectorObs(_rigidbody.velocity.magnitude / _movementSpeed);   // 1
        // Distances to spotted birds (-1.0 if nothing is spotted)
        AddVectorObs(SenseSurroundings());                              // 5 * 24 (numVisionRays)
        // Check what's being hovered
        AddVectorObs(SenseHoveredEntity());                             // 1
    }

    /// <summary>
    /// Executes continuous movement.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
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

        base.AgentAction(vectorAction, textAction);
    }
    #endregion
}