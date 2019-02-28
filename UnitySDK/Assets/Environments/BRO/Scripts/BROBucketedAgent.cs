using System.Linq;
using UnityEngine;

public class BROBucketedAgent : BROAgent
{
    #region Member Fields
    //201 actions setup in the InitializeAgent function
    private float[] _movementBuckets201;

    // 41 Actions
    private float[] _movementBuckets41 = new float[] {-1.0f, -0.95f, -0.9f, -0.85f, -0.8f, -0.75f, -0.7f, -0.65f, -0.6f, -0.55f, -0.5f, -0.45f, -0.4f, -0.35f, -0.3f, -0.25f,
                                                  -0.2f, -0.15f, -0.1f, -0.05f, 0.0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f,
                                                    0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1.0f};

    // 21 Actions
    private float[] _movementBuckets21 = new float[] {-1.0f, -0.9f, -0.8f, -0.7f, -0.6f, -0.5f, -0.4f, -0.3f, -0.2f, -0.1f, 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f,
                                                   0.8f, 0.9f, 1.0f};

    // 11 Actions
    private float[] _movementBuckets11 = new float[] { -1.0f, -0.8f, -0.6f, -0.4f, -0.2f, 0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

    // 7 Actions
    private float[] _movementBuckets7 = new float[] { -1.0f, -0.5f, -0.1f, 0.0f, 0.1f, 0.5f, 1.0f };
    #endregion

    #region Unity ML-Agents
    /// <summary>
    /// Resets the agent and its environment.
    /// </summary>
    public override void AgentReset()
    {
        base.AgentReset();

        // Bucket201 action setup
        var numbers = Enumerable.Range(-100, 201).ToArray();
        _movementBuckets201 = numbers.Select(x => (float)x / 100.0f).ToArray();
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
        // Unit circle locomotion: first action determines the angle(direction), the second one the speed
        // Process the action
        float angle = _movementBuckets11[(int)vectorAction[0]] * 90;
        // Retrieve position form unit circle
        Vector3 circumferencePoint = new Vector3((Mathf.Cos(angle * Mathf.Deg2Rad)),
                                                0,
                                                (Mathf.Sin(angle * Mathf.Deg2Rad)));

        // Apply velocity based on direction (coming from the unit circle)
        _rigidbody.velocity = circumferencePoint.normalized * _movementBuckets11[(int)vectorAction[1]] * _movementSpeed;

        // Move to mouse position
        if ((int)vectorAction[2] == 0)
        {
            Move();
        }
        // Blink to mouse position
        else if ((int)vectorAction[2] == 1)
        {
            Blink();
        }
        else
        {
            // Nothing
        }
        AddReward(0.01f); // For each step of survival

        // Updates the UI
        base.AgentAction(vectorAction, textAction);
    }
    #endregion
}