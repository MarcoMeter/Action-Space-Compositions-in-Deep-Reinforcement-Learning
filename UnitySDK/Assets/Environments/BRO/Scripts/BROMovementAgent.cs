using UnityEngine;
using MLAgents;
using UnityEngine.UI;
using System;

public class BROMovementAgent : BROAgent
{
    #region Unity ML-Agents
    /// <summary>
    /// Resets the agent and its environment.
    /// </summary>
    public override void AgentReset()
    {
        _allowChanges = false; // The click agent may be allowed to control the velocity of the character
        // UI
        _stepDevisor = 0;
        _sumVelocityX = 0.0f;
        _sumVelocityZ = 0.0f;
        _survivalDuration = 0;
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

        AddReward(0.01f);

        base.AgentAction(vectorAction, textAction);
    }
    #endregion
}