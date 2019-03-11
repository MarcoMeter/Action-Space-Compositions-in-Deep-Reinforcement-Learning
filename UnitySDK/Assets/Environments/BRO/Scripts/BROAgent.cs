using UnityEngine;
using MLAgents;
using UnityEngine.UI;
using System;

/// <summary>
/// Base class for building agents playing BRO. It contains most of the agent's logic, except for the ML-Agent functionalities.
/// </summary>
public abstract class BROAgent : Agent
{
    #region Member Fields
    [SerializeField]
    protected Academy _academy;
    protected float _movementSpeed = 20.0f;
    [SerializeField]
    protected GameObject _walkFeedback;
    protected int _maxBlinkCooldown = 200;
    protected int _currentBlinkCooldown = 0;
    [SerializeField]
    protected Rigidbody _rigidbody;
    [SerializeField]
    protected Transform _pitchTransform;
    [SerializeField]
    protected BeastBehaviour _beast;
    [SerializeField]
    protected SlowSphereBehavior _slowSphere;
    [SerializeField]
    protected SphereCollider _pitchBoundaryCollider;
    [Header("Controlled Character")]
    [SerializeField]
    protected Transform _characterTransform;
    [SerializeField]
    protected Rigidbody _characterRigidbody;
    protected float _characterMovementSpeed = 4.0f;
    protected float _characterRotationSpeed = 10.0f;
    protected float _currentArrivalSpeed = 1.0f;
    protected Vector3 _destination = Vector3.zero;
    protected Vector3 _lookDirection = Vector3.zero;
    protected float _agentHeight = 0.0f;
    protected float _characterHeight = 0.0f;
    protected bool _destinationSet = false;
    [SerializeField]
    protected Material _defaultMaterial;
    [SerializeField]
    protected Material _blinkReadyMaterial;
    [SerializeField]
    protected Renderer _renderer;
    protected bool _isSlowed = false;
    protected bool _allowChanges = true;

    [Header("UI")]
    [SerializeField]
    private Text _averageSpeedText;
    protected float _sumVelocityX = 0.0f;
    protected float _sumVelocityZ = 0.0f;
    protected int _stepDevisor = 0;
    [SerializeField]
    private Text _survivalDurationText;
    protected int _survivalDuration = 0;
    #endregion

    #region ML-Agents
    /// <summary>
    /// Initially randomizes the positions of the environment's entities
    /// </summary>
    public override void AgentReset()
    {
        Vector3 characterPosition = SamplePitchLocation();
        _characterTransform.position = new Vector3(characterPosition.x, _characterHeight, characterPosition.z);
        _characterTransform.rotation = Quaternion.LookRotation(_pitchTransform.position - _characterTransform.position);
        Vector3 mousePosition = SamplePitchLocation();
        transform.position = new Vector3(mousePosition.x, _agentHeight, mousePosition.z);
        _lookDirection = _pitchTransform.position - _characterTransform.position;
        _currentBlinkCooldown = 0;
        _isSlowed = false;
        _beast.Reset();
        _slowSphere.Reset(SamplePitchLocation());
        _destinationSet = false;
        _renderer.material = _blinkReadyMaterial;
        
        // UI
        _stepDevisor = 0;
        _sumVelocityX = 0.0f;
        _sumVelocityZ = 0.0f;
        _survivalDuration = 0;
    }

    /// <summary>
    /// Collects 22 values for the observation vector.
    /// </summary>
    public override void CollectObservations()
    {
        // Remaining blink cooldown as ratio
        AddVectorObs((float)_currentBlinkCooldown / (float)_maxBlinkCooldown);
        // Remaining blink cooldown as binary (excluded due to using old results)
        AddVectorObs(_currentBlinkCooldown == 0.0f);
        // Mouse position
        AddVectorObs(transform.localPosition.x / 7);
        AddVectorObs(transform.localPosition.z / 7);
        // Mouse velocity
        AddVectorObs(_rigidbody.velocity.x / _movementSpeed);
        AddVectorObs(_rigidbody.velocity.z / _movementSpeed);
        // Character position
        AddVectorObs(_characterTransform.localPosition.x / 7);
        AddVectorObs(_characterTransform.localPosition.z / 7);
        // Character velocity
        AddVectorObs(_characterRigidbody.velocity.x / (_characterMovementSpeed));
        AddVectorObs(_characterRigidbody.velocity.z / (_characterMovementSpeed));
        // Relative position character to beast
        AddVectorObs((_characterTransform.localPosition - _beast.transform.localPosition).x / 10);
        AddVectorObs((_characterTransform.localPosition - _beast.transform.localPosition).z / 10);
        // Beast position (beast can drift out of bounds)
        AddVectorObs(_beast.transform.localPosition.x / 10);
        AddVectorObs(_beast.transform.localPosition.z / 10);
        // Beast velocity
        AddVectorObs(_beast.Velocity.x / _beast.MaxSpeed);
        AddVectorObs(_beast.Velocity.z / _beast.MaxSpeed);
        // Beast speed
        AddVectorObs(_beast.Speed / _beast.MaxSpeed);
        // Beast rotation speed
        AddVectorObs(_beast.AccuracyMagnitude / _beast.MaxAccuracyMagnitude);
        // Relative position to slow sphere
        AddVectorObs((_characterTransform.localPosition - _slowSphere.transform.localPosition).x / 7);
        AddVectorObs((_characterTransform.localPosition - _slowSphere.transform.localPosition).z / 7);
        // Slow sphere velocity
        AddVectorObs(_slowSphere.Velocity.x / _slowSphere.Speed);
        AddVectorObs(_slowSphere.Velocity.z / _slowSphere.Speed);
    }

    /// <summary>
    /// Updates UI.
    /// </summary>
    /// <param name="vectorAction"></param>
    /// <param name="textAction"></param>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Update UI
        _stepDevisor++;
        _sumVelocityX += Math.Abs(_rigidbody.velocity.x);
        _sumVelocityZ += Math.Abs(_rigidbody.velocity.z);
        _averageSpeedText.text = (_sumVelocityX / _stepDevisor).ToString("0.00") + " | " + (_sumVelocityZ / _stepDevisor).ToString("0.00");
        if (_stepDevisor % 6 == 0) // 6 = decision frequency
        {
            _survivalDuration++;
        }
        _survivalDurationText.text = _survivalDuration.ToString();
    }
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Set intial member fields on start.
    /// </summary>
    protected void Start()
    {
        _agentHeight = transform.position.y;
        _characterHeight = _characterTransform.position.y;
    }

    /// <summary>
    /// Constrains the position of the mouse to the pitch's boundaries. Also, it processes the blink's cooldown.
    /// </summary>
    protected void FixedUpdate()
    {
        // Limit the agent's position
        Vector3 limitedPosition = _pitchBoundaryCollider.ClosestPoint(transform.position);
        limitedPosition.y = _agentHeight;
        transform.position = limitedPosition;

        if (_allowChanges)
        {
            // Process blink cooldown
            if (_currentBlinkCooldown > 0)
            {
                _currentBlinkCooldown--;
            }
            else if (_currentBlinkCooldown == 0)
            {
                _renderer.material = _blinkReadyMaterial;
            }

            // Process character movement
            if (_destinationSet)
            {
                if (Vector3.Distance(transform.position, _destination) <= 0.1f)
                {
                    // Decrease velocity once the target is being approached
                    _currentArrivalSpeed = 0.1f;
                }
                else
                {
                    // Normal movement speed
                    _currentArrivalSpeed = 1.0f;
                }

                if (Vector3.Distance(_characterTransform.position, _destination) > 0.01f)
                {
                    // Move
                    if (!_isSlowed)
                    {
                        _characterRigidbody.velocity = (_destination - _characterTransform.position).normalized * _characterMovementSpeed * _currentArrivalSpeed;
                    }
                    // If the character is inside the slow sphere, decrease the character's movement speed relatively to the distance of the center of the sphere.
                    else
                    {
                        Vector3 charPos = _characterTransform.position;
                        charPos.y = 0.0f;
                        float distance = (Vector3.Distance(charPos, _slowSphere.transform.position) / (_slowSphere.transform.localScale.y / 2.0f));
                        float relativeSlowRatio = Mathf.Clamp(distance, _slowSphere.SlowRatio, 1.0f);
                        _characterRigidbody.velocity = ((_destination - _characterTransform.position).normalized * _characterMovementSpeed * _currentArrivalSpeed) * relativeSlowRatio;
                    }
                }
                else
                {
                    // Stop
                    _characterTransform.position = _destination;
                    _destinationSet = false;
                    _characterRigidbody.velocity = Vector3.zero;
                }
            }
            else
            {
                _characterRigidbody.velocity = Vector3.zero;
            }
        }

        // Rotate towards movement direction
        if (_lookDirection != Vector3.zero)
        {
            _characterTransform.rotation = Quaternion.RotateTowards(_characterTransform.rotation, Quaternion.LookRotation(_lookDirection), _characterRotationSpeed);
        }
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Sets the character's destination based on the "mouse position", which is ultimately indicated by this agent.
    /// </summary>
    protected void Move()
    {
        _destinationSet = true;
        _currentArrivalSpeed = 1.0f;
        _destination = new Vector3(transform.position.x, 0, transform.position.z);
        _lookDirection = (_destination - _characterTransform.position).normalized;
        if (_academy.GetIsInference())
            Instantiate(_walkFeedback, new Vector3(transform.position.x, 0.1f, transform.position.z), Quaternion.Euler(new Vector3(90, 0, 0)));
    }

    /// <summary>
    /// Teleports the agent's controlled character to its "mouse position".
    /// </summary>
    protected void Blink()
    {
        if (_currentBlinkCooldown == 0)
        {
            Vector3 lookDirection = transform.position - _characterTransform.position;
            //lookDirection.y = _characterTransform.position.y;
            _characterTransform.position = new Vector3(transform.position.x, _characterTransform.position.y, transform.position.z);
            _characterTransform.rotation = Quaternion.LookRotation(lookDirection);
            _currentBlinkCooldown = _maxBlinkCooldown;
            _destinationSet = false;
            _renderer.material = _defaultMaterial;
        }
        else
        {
            AddReward(-0.025f); // Punish for trying to blink while blinking is on cooldown
        }
    }

    /// <summary>
    /// Samples a random position which is on the pitch.
    /// </summary>
    /// <returns>Random position on the pitch.</returns>
    public Vector3 SamplePitchLocation()
    {
        float a = UnityEngine.Random.Range(0.0f, 1.0f) * 2 * Mathf.PI;
        float r = (_pitchTransform.localScale.x / 2) * Mathf.Sqrt(UnityEngine.Random.Range(0.0f, 1.0f)); // Radius is assumed to be the scale
        Vector3 location = new Vector3(_pitchTransform.position.x + (r * Mathf.Cos(a)), 0,
                                       _pitchTransform.position.z + (r * Mathf.Sin(a)));

        return location;
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Sert the agent to be done once the beast killed it.
    /// </summary>
    public void Kill()
    {
        AddReward(-1.0f);
        Done();
    }

    /// <summary>
    /// Slow or unslow the character.
    /// </summary>
    /// <param name="slow"></param>
    public void Slow(bool slow)
    {
        _isSlowed = slow;
    }
    #endregion
}