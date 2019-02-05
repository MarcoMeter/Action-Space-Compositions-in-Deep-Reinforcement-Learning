using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;
using UnityEngine.UI;

public class ShootingBirdsMovementAgent : Agent
{
    #region Member Fields
    private ShootingBirdsEnvironment _environment;
    private const string _BIRD_TAG = "Bird";
    private const string _ENV_TAG = "Environment";
    private Vector3 _origin;
    [Header("UI")]
    [SerializeField]
    private Text _averageVelocityText;
    private float _sumVelocityX = 0.0f;
    private float _sumVelocityY = 0.0f;
    private int _stepDevisor = 0;
    [Header("Action Space")]
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private float _movementSpeed = 50.0f;
    [Header("State Space")]
    [SerializeField]
    private int _numVisionRays = 24;
    [SerializeField]
    private float _visionRayLength = 14.0f;
    private float _angleStep;
    private List<Ray2D> _rays;
    [SerializeField]
    private LayerMask _layerMaskBird;
    [SerializeField]
    private LayerMask _layerMaskBirdEnv;
    #endregion

    #region Member Properties
    public Vector3 Origin
    {
        get { return _origin; }
    }
    #endregion

    #region Unity ML-Agents
    /// <summary>
    /// Saves the position of the agent.
    /// </summary>
    public override void InitializeAgent()
    {
        _origin = transform.position;
        // Initialize rays for the agent's input
        _angleStep = 360.0f / _numVisionRays;
    }

    /// <summary>
    /// Sets the agent back to his initial position.
    /// </summary>
    public override void AgentReset()
    {
        transform.position = _origin;
        _sumVelocityX = 0.0f;
        _sumVelocityY = 0.0f;
        _stepDevisor = 0;
    }

    /// <summary>
    /// Observes the state space.
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
        AddVectorObs(SenseSurroundings());                              // 24 (numVisionRays)
        // Check what's being hovered
        AddVectorObs(SenseHoveredEntity());                             // 1
    }

    /// <summary>
    /// Executes actions for movement, shooting and reloading.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
    {        
        if (brain.brainParameters.vectorActionSpaceType.Equals(SpaceType.continuous))
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
        }

        // Update speed UI
        _stepDevisor++;
        _sumVelocityX += Math.Abs(_rigidbody.velocity.x);
        _sumVelocityY += Math.Abs(_rigidbody.velocity.y);
        _averageVelocityText.text = "|Average speed|: " + (_sumVelocityX / _stepDevisor).ToString("0.00") + " | " + (_sumVelocityY / _stepDevisor).ToString("0.00");
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Fire raycasts to observe the agent's surroundings.
    /// </summary>
    /// <returns></returns>
    public List<float> SenseSurroundings()
    {
        List<float> observation = new List<float>();

        // Update agent's vision
        _rays = new List<Ray2D>();
        for (int i = 0; i < _numVisionRays; i++)
        {
            Vector2 circumferencePoint = new Vector2(transform.position.x + (_visionRayLength * Mathf.Cos(((_angleStep * i)) * Mathf.Deg2Rad)),
                                            transform.position.y + (_visionRayLength * Mathf.Sin((transform.rotation.eulerAngles.z + (_angleStep * i)) * Mathf.Deg2Rad)));
            _rays.Add(new Ray2D((Vector2)transform.position, (circumferencePoint - (Vector2)transform.position).normalized));
        }

        // Execute raycasts to query the agent's vision (1 inputs per raycast)
        foreach (var ray in _rays)
        {
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, _visionRayLength, _layerMaskBird);
            if (hit)
            {
                observation.Add(hit.distance / _visionRayLength);
            }
            else
            {
                // if no bird is spotted
                observation.Add(-1.0f);
            }

            Debug.DrawLine(ray.origin, ray.origin + ray.direction * _visionRayLength, Color.red, 0.0f); // Check correct behavior of raycasts
        }
        return observation;
    }

    /// <summary>
    /// Uses 2D Physics to check what entity is hovered by the agent.
    /// </summary>
    /// <returns>Returns 1.0f for a bird being sensed, 0.5f for the environment and 0.0f for nothing.</returns>
    private float SenseHoveredEntity()
    {
        Collider2D coll = Physics2D.OverlapPoint((Vector2)transform.position, _layerMaskBirdEnv);

        if (coll)
        {
            switch (coll.tag)
            {
                case _BIRD_TAG:
                    return 1.0f;
                case _ENV_TAG:
                    return 0.5f;
            }
        }
        return 0.0f;
    }
    #endregion
}