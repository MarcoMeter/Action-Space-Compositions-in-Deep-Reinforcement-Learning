using UnityEngine;
public class BROThresholdAgent : BROAgent
{
    #region Member Fields
    private float _discreteActionThreshold = 0.33f;
    #endregion

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
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // "Mouse Movement"
        // Unit circle locomotion: first action determines the angle (direction), the second one the speed
        // Process the action
        float angle = vectorAction[0] * 90;
        // Retrieve position form unit circle
        Vector3 circumferencePoint = new Vector3((Mathf.Cos(angle * Mathf.Deg2Rad)),
                                                0,
                                                (Mathf.Sin(angle * Mathf.Deg2Rad)));

        // Apply velocity based on direction (coming from the unit circle)
        _rigidbody.velocity = circumferencePoint.normalized * vectorAction[1] * _movementSpeed;



        // Move to mouse position or collect power up by clicking
        if (Mathf.Abs(vectorAction[2]) < _discreteActionThreshold)
        {
            Move();
        }
        // Blink to mouse position
        if (Mathf.Abs(vectorAction[3]) < _discreteActionThreshold)
        {
            Blink();
        }
        AddReward(0.01f);

        // Update UI
        base.AgentAction(vectorAction, textAction);
    }
    #endregion
}