using UnityEngine;

public class SlowSphereBehavior : MonoBehaviour
{
    #region Member Fields
    [SerializeField]
    private Rigidbody _rigidbody;
    [SerializeField]
    private float _speed = 1.5f;
    [SerializeField]
    private float _slowRatio = 0.75f;
    [SerializeField]
    private SphereCollider _pitchBoundaryCollider;
    [SerializeField]
    private Transform _character;
    [SerializeField]
    private Transform _beast;
    [SerializeField]
    private BROAgent _agent;
    private Vector3 _oldVelocity = Vector3.zero;
    #endregion

    #region Member Properties
    public float SlowRatio
    {
        get { return _slowRatio; }
    }

    public Vector3 Velocity
    {
        get { return _rigidbody.velocity; }
    }

    public float Speed
    {
        get { return _speed; }
    }
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Executes the movement and limits the sphere's position.
    /// </summary>
    public void FixedUpdate ()
    {
        // Limit the sphere's position
        Vector3 limitedPosition = _pitchBoundaryCollider.ClosestPoint(transform.position);
        limitedPosition.y = 0 - 0f;
        transform.position = limitedPosition;

        MoveAround();
    }

    /// <summary>
    /// If the character enters the sphere, slow it.
    /// </summary>
    /// <param name="other"></param>
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            _agent.Slow(true);
        }
    }

    /// <summary>
    /// If the character exits the sphere, stop slowing it.
    /// </summary>
    /// <param name="other"></param>
    public void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            _agent.Slow(false);
        }
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Applies a random position to the sphere.
    /// </summary>
    /// <param name="newPosition"></param>
    public void Reset(Vector3 newPosition)
    {
        newPosition.y = 0.0f;
        transform.position = newPosition;
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Somewhat randomize wandering behavior of the sphere.
    /// </summary>
    private void MoveAround()
    {
        // Direction vector to beast
        Vector3 beastPos = _beast.position;
        beastPos.y = 0.0f;
        Vector3 beastDir = (beastPos - transform.position).normalized;
        // Direction vector to character
        Vector3 charPos = _character.position;
        charPos.y = 0.0f;
        Vector3 charDir = (charPos - transform.position).normalized;
        // Direction vector to character
        Vector3 agentPos = _agent.transform.position;
        charPos.y = 0.0f;
        Vector3 agentDir = (agentPos - transform.position).normalized;
        // Random weights for movement direction
        float[] weights = new float[3];
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Random.Range(0.0f, 1.0f);
        // Sum and weight direction vectors
        Vector3 movementDirection = weights[0] * beastDir + weights[1] * charDir + weights[2] * agentDir;
        // Adjust old velocity (smooths movement) and add more randomness
        Vector3 newDirection = (0.1f * movementDirection.normalized) + (0.7f * _oldVelocity) + (0.2f * new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)).normalized);
        // Apply velocity
        _rigidbody.velocity = newDirection.normalized * _speed;
        _oldVelocity = _rigidbody.velocity;
    }
    #endregion
}