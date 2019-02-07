using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class ShootingBirdsAgent : Agent
{
    #region Member Fields
    private ShootingBirdsEnvironment _environment;
    private const string _BIRD_TAG = "Bird";
    private const string _ENV_TAG = "Environment";
    private int _leftAmmo = 8;
    protected Vector3 _origin;
    [Header("UI")]
    [SerializeField]
    private Text _accuracyText;
    [SerializeField]
    private Text _ammoText;
    private int _shotCount = 0;
    private int _hitCount = 0;
    private int _reloads = 0;
    private int _reloadsOnEmpty = 0;
    [SerializeField]
    private Text _averageVelocityText;
    private float _sumVelocityX = 0.0f;
    private float _sumVelocityY = 0.0f;
    private int _stepDevisor = 0;
    [Header("Action Space")]
    [SerializeField]
    protected Rigidbody2D _rigidbody;
    [SerializeField]
    protected float _movementSpeed = 50.0f;
    [SerializeField]
    private int _maxAmmo = 8;
    [SerializeField]
    private bool _infinteAmmo = false;
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
    private LayerMask _layerMaskEnv;
    [SerializeField]
    private LayerMask _layerMaskBirdEnv;
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
        // Randomize the agent's position and ammunition
        transform.position = new Vector3(
            UnityEngine.Random.Range(_origin.x - 10, _origin.x + 10),
            UnityEngine.Random.Range(_origin.y - 10, _origin.y + 10),
            _origin.z);
        _leftAmmo = UnityEngine.Random.Range(1, _maxAmmo);
        // Randomize obstacles (TODO)

        // Reset data holders
        _hitCount = _shotCount = _reloads = _reloadsOnEmpty = 0;
        _sumVelocityX = 0.0f;
        _sumVelocityY = 0.0f;
        _stepDevisor = 0;
    }

    /// <summary>
    /// Observes the complete state space.
    /// </summary>
    public override void CollectObservations()
    {
        // Is the gun loaded?
        AddVectorObs((_leftAmmo > 0));                                  // 1
        // Remaining ammunation
        AddVectorObs(_leftAmmo / _maxAmmo);                             // 1
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
    /// Call this base to update the speed UI.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Update speed UI
        _stepDevisor++;
        _sumVelocityX += Math.Abs(_rigidbody.velocity.x);
        _sumVelocityY += Math.Abs(_rigidbody.velocity.y);
        _averageVelocityText.text = "|Average speed|: " + (_sumVelocityX / _stepDevisor).ToString("0.00") + " | " + (_sumVelocityY / _stepDevisor).ToString("0.00");
    }
    #endregion

    #region Protected Functions
    /// <summary>
    /// Shoots at the position of the agent, if enough ammo is left.
    /// </summary>
    protected void Shoot()
    {
        if (_leftAmmo > 0 || _infinteAmmo)
        {
            _shotCount++;

            Collider2D coll = Physics2D.OverlapPoint((Vector2)transform.position, _layerMaskBirdEnv);
            // Check what's been shot
            if (coll && coll.tag.Equals(_BIRD_TAG))
            {
                BirdBehavior bird = coll.GetComponent<BirdBehavior>();
                bird.Hit();
                // Reward the agent for hitting a bird based on the bird's size
                switch (bird.BirdSize)
                {
                    case BirdSize.S:
                        AddReward(2.0f);
                        break;
                    case BirdSize.M:
                        AddReward(0.75f);
                        break;
                    case BirdSize.L:
                        AddReward(0.25f);
                        break;
                }
                _hitCount++;
            }
            else
            {
                // Punish for hitting nothing
                AddReward(-0.1f);
            }

            if (!_infinteAmmo)
            {
                // Decrease ammo
                _leftAmmo--;
            }
        }
        else
        {
            // Punish the agent for trying to shoot without ammo
            AddReward(-1f);
        }
        // Update UI
        _accuracyText.text = "Shot Accuracy: " + ((float)_hitCount / (float)_shotCount).ToString("0.00");
        _ammoText.text = "Ammo: " + _leftAmmo + " / " + _maxAmmo;
    }

    /// <summary>
    /// Refills the ammo at some cost.
    /// </summary>
    protected void Reload()
    {
        _reloads++;
        // Proportionally punish the agent for reloading if it has ammo left
        if (_leftAmmo > 0)
        {
            AddReward((_leftAmmo / _maxAmmo) * -1.0f);
        }
        else
        {
            _reloadsOnEmpty++;
        }
        _leftAmmo = _maxAmmo;
        // Update UI
        _ammoText.text = "Ammo: " + _leftAmmo + " / " + _maxAmmo;
    }

    /// <summary>
    /// Fire raycasts to observe the agent's surroundings.
    /// </summary>
    /// <returns></returns>
    protected List<float> SenseSurroundings()
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

        // Execute raycasts to query the agent's vision (4 inputs per raycast)
        foreach (var ray in _rays)
        {
            // Perceive birds
            RaycastHit2D birdHit = Physics2D.Raycast(ray.origin, ray.direction, _visionRayLength, _layerMaskBird);
            if (birdHit)
            {
                // Add distance
                observation.Add(birdHit.distance / _visionRayLength);

                // Add one hot encoded bird type
                switch (birdHit.transform.GetComponent<BirdBehavior>().BirdSize)
                {
                    case BirdSize.S:
                        observation.AddRange(new float[] { 1.0f, 0.0f, 0.0f }); // Alternative encoding: -1, 0, +1
                        break;
                    case BirdSize.M:
                        observation.AddRange(new float[] { 0.0f, 1.0f, 0.0f });
                        break;
                    case BirdSize.L:
                        observation.AddRange(new float[] { 0.0f, 0.0f, 1.0f });
                        break;
                }
            }
            else
            {
                // if no bird is spotted
                observation.Add(-1.0f);
                observation.AddRange(new float[] { 0.0f, 0.0f, 0.0f });
            }

            // Perceive obstacles
            RaycastHit2D envHit = Physics2D.Raycast(ray.origin, ray.direction, _visionRayLength, _layerMaskEnv);
            if (envHit)
            {
                observation.Add(envHit.distance / _visionRayLength);
            }
            else
            {
                observation.Add(-1.0f);
            }

            // Visualize raycasts in scene view
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * _visionRayLength, Color.red, 0.0f); // Check correct behavior of raycasts
        }
        return observation;
    }

    /// <summary>
    /// Uses 2D Physics to check what entity is hovered by the agent.
    /// </summary>
    /// <returns>Returns 1.0f for a bird being sensed, -1.0f for the environment and 0.0f for nothing.</returns>
    protected float SenseHoveredEntity()
    {
        Collider2D coll = Physics2D.OverlapPoint((Vector2)transform.position, _layerMaskBirdEnv);

        if (coll)
        {
            switch (coll.tag)
            {
                case _BIRD_TAG:
                    return 1.0f;
                case _ENV_TAG:
                    return -1.0f;
            }
        }
        return 0.0f;
    }
    #endregion

    #region Public Functions
    public float GetClickAccuracy()
    {
        if (_shotCount <= 0)
            return 0;

        return ((float)_hitCount / (float)_shotCount);
    }

    public float GetReloadAccuracy()
    {
        if (_reloads <= 0)
            return 0;

        return ((float)_reloadsOnEmpty / (float)_reloads);
    }
    #endregion
}