using UnityEngine;

/// <summary>
/// The BirdBehavior manages the appearance and the movement of a bird.
/// </summary>
public class BirdBehavior : MonoBehaviour
{
    #region Member Fields
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private GameObject _explosionParticles;
    private BirdSize _size;
    private float _sizeS = 0.2f;
    private float _sizeM = 0.4f;
    private float _sizeL = 0.6f;
    private float _lifespan = 15.0f;
    private float _movementShift;
    #endregion

    #region Member Properties
    public BirdSize BirdSize
    {
        get { return _size; }
    }
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initializes the property for the sine-like flying curve behavior of the bird.
    /// </summary>
    private void Start()
    {
        _movementShift = Random.Range(-45.0f, 45.0f);
    }

    /// <summary>
    /// Alter velocity.
    /// </summary>
    private void FixedUpdate()
    {
        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, Mathf.Sin(Time.time * 2.0f + _movementShift));
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Initializes the bird's size and speed.
    /// </summary>
    /// <param name="size">Size of the bird</param>
    /// <param name="speed">Moving speed of the bird</param>
    /// <param name="flipped">Determines the direction, if false direction is positive x</param>
    public void Init(BirdSize size, float speed, bool flipped)
    {
        _size = size;

        // Set scale
        switch (_size)
        {
            case BirdSize.S:
                transform.localScale = new Vector3(_sizeS, _sizeS, 1);
                break;
            case BirdSize.M:
                transform.localScale = new Vector3(_sizeM, _sizeM, 1);
                break;
            case BirdSize.L:
                transform.localScale = new Vector3(_sizeL, _sizeL, 1);
                break;
        }

        // Set rotation
        if(flipped)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }

        // Apply velocity
        _rigidbody.velocity = (Vector2)(transform.right * speed);

        // Kill the bird after a certain amount of time
        Invoke("KillBird", _lifespan);
    }

    /// <summary>
    /// Kills the bird and updates the score.
    /// </summary>
    public void Hit()
    {
        // Determine the score and inform the GameController
        KillBird();
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Destroys the bird's instance and instantiates particles as visual feedback.
    /// </summary>
    private void KillBird()
    {
        CancelInvoke(); // If the bird is killed by the player, make sure that any further invocations are cancelled to avoid NullRefs.
        Destroy(gameObject);
    }
    #endregion
}

/// <summary>
/// Definition of the birds' size classes.
/// </summary>
public enum BirdSize
{
    L, M, S
}