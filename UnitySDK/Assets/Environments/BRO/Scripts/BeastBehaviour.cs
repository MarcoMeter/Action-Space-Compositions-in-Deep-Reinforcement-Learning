using UnityEngine;

public class BeastBehaviour : MonoBehaviour
{
    #region Member Fields
    private Vector3 _origin;
    [SerializeField]
    private float _minSpeed = 10.0f;
    [SerializeField]
    private float _maxSpeed = 75.0f;
    [SerializeField]
    private float _speedMultiplier = 0.6f;
    [SerializeField]
    private float _speed;
    [SerializeField]
    float _minAccuracyMagnitude = 0.01f;
    [SerializeField]
    float _maxAccuracyMagnitude = 1.0f;
    [SerializeField]
    float _accuracyMagnitude = 1.0f;
    [SerializeField]
    float _accuracyMagnitudeMultiplier = 0.01f;
    [SerializeField]
    private Rigidbody _rigidbody;
    private bool _isSlowed = false;
    [SerializeField]
    private Transform _targetCharacter;
    [SerializeField]
    private Transform _childTransform;
    [SerializeField]
    private BROAgent _agent;
    [SerializeField]
    private BROClickAgent _clickAgent;
    [SerializeField]
    private SlowSphereBehavior _slowSphere;
    #endregion

    #region Member Properties
    public float Speed
    {
        get { return _speed; }
    }

    public float MaxSpeed
    {
        get { return _maxSpeed; }
    }

    public float AccuracyMagnitude
    {
        get { return _accuracyMagnitude; }
    }

    public float MaxAccuracyMagnitude
    {
        get { return _maxAccuracyMagnitude; }
    }

    public Vector3 Velocity
    {
        get { return _rigidbody.velocity; }
    }
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initialize Members.
    /// </summary>
    private void Start()
    {
        _speed = _minSpeed;
        _accuracyMagnitude = _minAccuracyMagnitude;
        _origin = transform.position;
    }

    /// <summary>
    /// Executes movement of the beast.
    /// </summary>
    private void FixedUpdate()
    {
        ChaseTarget();
    }

    /// <summary>
    /// Triggered by the collider on the beast's tip. Determines if a character got hit or the entrance into the slow sphere of the beast.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            _agent.Kill();
            if (_clickAgent)
                _clickAgent.Kill();
        }

        if (other.tag.Equals("Slow"))
        {
            _isSlowed = true;
        }
    }

    /// <summary>
    /// Event, which occurs once the beast exited the slow sphere.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Slow"))
        {
            _isSlowed = false;
        }
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Resets the speed and the position of the beast.
    /// </summary>
    public void Reset()
    {
        transform.position = _origin;
        transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        _speed = _minSpeed;
        _accuracyMagnitude = _minAccuracyMagnitude;
        _isSlowed = false;
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Applies forward velocity and rotates the beast towards its target.
    /// </summary>
    private void ChaseTarget()
    {
        // Visual effect: make the beast look at its target
        Vector3 targetRotation = _targetCharacter.position - transform.position;
        _childTransform.rotation = Quaternion.LookRotation(targetRotation);

        // Apply Seek Steering Behavior
        Vector3 desiredVelocity = (_targetCharacter.position - transform.position).normalized;
        Vector3 steeringForce = (desiredVelocity - _rigidbody.velocity.normalized) * _accuracyMagnitude;
        if (!_isSlowed)
        {
            _rigidbody.velocity = (_rigidbody.velocity + steeringForce).normalized * _speed;
        }
        else
        {
            Vector3 myPos = transform.position;
            myPos.y = 0.0f;
            float distance = (Vector3.Distance(myPos, _slowSphere.transform.position) / (_slowSphere.transform.localScale.y / 2.0f));
            distance += 0.1f; // Make the strength of the slow not as hard as for the character
            float relativeSlowRatio = Mathf.Clamp(distance, _slowSphere.SlowRatio, 1.0f);
            _rigidbody.velocity = (_rigidbody.velocity + steeringForce).normalized * _speed * relativeSlowRatio;
        }

        // Update and clamp speed and accuracy magnitude
        _speed = Mathf.Clamp(_speed + (Time.deltaTime * _speedMultiplier), _minSpeed, _maxSpeed);
        _accuracyMagnitude = Mathf.Clamp(_accuracyMagnitude + (Time.deltaTime * _accuracyMagnitudeMultiplier), _minAccuracyMagnitude, _maxAccuracyMagnitude);
    }
    #endregion
}