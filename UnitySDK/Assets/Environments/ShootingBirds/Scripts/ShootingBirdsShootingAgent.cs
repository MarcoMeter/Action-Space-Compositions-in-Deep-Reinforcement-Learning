using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class ShootingBirdsShootingAgent : Agent
{
    #region Member Fields
    private ShootingBirdsEnvironment _environment;
    private const string _BIRD_TAG = "Bird";
    private const string _ENV_TAG = "Environment";
    private int _leftAmmo = 8;
    [SerializeField]
    ShootingBirdsMovementAgent _movementAgent;
    [Header("UI")]
    [SerializeField]
    private Text _accuracyText;
    [SerializeField]
    private Text _ammoText;
    private int _shotCount = 0;
    private int _hitCount = 0;
    private int _reloads = 0;
    private int _reloadsOnEmpty = 0;
    [Header("Action Space")]
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private int _maxAmmo = 8;
    [SerializeField]
    private bool _infinteAmmo = true;
    [SerializeField]
    private float _movementSpeed = 50.0f;
    [Header("State Space")]
    [SerializeField]
    private LayerMask _layerMaskBird;
    [SerializeField]
    private LayerMask _layerMaskBirdEnv;
    #endregion

    #region Unity ML-Agents
    /// <summary>
    /// Sets the agent back to his initial position.
    /// </summary>
    public override void AgentReset()
    {
        _hitCount = _shotCount = _reloads = _reloadsOnEmpty = 0;
        _leftAmmo = UnityEngine.Random.Range(1, _maxAmmo);
    }

    /// <summary>
    /// Observes the state space.
    /// </summary>
    public override void CollectObservations()
    {
        // Is the gun loaded?
        AddVectorObs((_leftAmmo > 0));                                              // 1
        // Remaining ammunation
        AddVectorObs(_leftAmmo / _maxAmmo);                                         // 1
        // Relative position to the origin
        AddVectorObs((transform.position.x - _movementAgent.Origin.x) / 17.715f);   // 1
        AddVectorObs((transform.position.y - _movementAgent.Origin.y) / 10.215f);   // 1
        // Velocity of the agent (i.e. direction)
        AddVectorObs(_rigidbody.velocity.normalized);                               // 2
        // Speed of the agent
        AddVectorObs(_rigidbody.velocity.magnitude / _movementSpeed);               // 1
        // Distances to spotted birds (-1.0 if nothing is spotted)
        AddVectorObs(_movementAgent.SenseSurroundings());                           // 24 (numVisionRays)
        // Check what's being hovered
        AddVectorObs(SenseHoveredEntity());                                         // 1
    }

    /// <summary>
    /// Executes actions for movement, shooting and reloading.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (brain.brainParameters.vectorActionSpaceType.Equals(SpaceType.discrete))
        {
            int action = Mathf.FloorToInt(vectorAction[0]);
            switch (action)
            {
                // Move horizontal
                case 0:
                    Shoot();
                    break;
                case 1:
                    Reload();
                    break;
                // Move Vertical
                case 2:
                    // Nothing
                    break;
            }
        }
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Shoots at the position of the agent, if enough ammo is left.
    /// </summary>
    private void Shoot()
    {
        if (_leftAmmo > 0 || _infinteAmmo)
        {
            _shotCount++;

            Collider2D coll = Physics2D.OverlapPoint((Vector2)transform.position, _layerMaskBird);
            // Check what's been shot
            if (coll && coll.tag.Equals(_BIRD_TAG))
            {
                BirdBehavior bird = coll.GetComponent<BirdBehavior>();
                bird.Hit();
                // Reward the agent for hitting a bird based on the bird's size
                switch (bird.BirdSize)
                {
                    case BirdSize.S:
                        AddReward(1.0f);
                        _movementAgent.AddReward(1.0f);
                        break;
                    case BirdSize.M:
                        AddReward(0.5f);
                        _movementAgent.AddReward(0.5f);
                        break;
                    case BirdSize.L:
                        AddReward(0.25f);
                        _movementAgent.AddReward(0.25f);
                        break;
                }
                _hitCount++;
            }
            else
            {
                // Punish for hitting nothing
                AddReward(-0.2f);
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
    private void Reload()
    {
        _reloads++;
        // Punish the agent for reloading if it has ammo left
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