using UnityEngine;

/// <summary>
/// The BirdBehavior manages the appearance and the movement of a bird.
/// </summary>
public class BirdBehavior : MonoBehaviour
{
    #region Member Fields
    private ShootingBirdsEnvironment _environment;
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private GameObject _explosionParticles;
    private BirdSize _size;
    private Vector3 _sizeS = new Vector3(0.2f, 0.2f, 1.0f);
    private Vector3 _sizeM = new Vector3(0.4f, 0.4f, 1.0f);
    private Vector3 _sizeL = new Vector3(0.6f, 0.6f, 1.0f);
    private float _movementShift;
    private float _aliveRange = 20.0f;
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

        if (Mathf.Abs(transform.localPosition.x) > _aliveRange)
            KillBird();
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Initializes the bird's size and speed.
    /// </summary>
    /// <param name="size">Size of the bird</param>
    /// <param name="speed">Moving speed of the bird</param>
    /// <param name="flipped">Determines the direction, if false direction is positive x</param>
    public void Init(ShootingBirdsEnvironment env, BirdSize size, float speed, bool flipped)
    {
        _environment = env;
        _size = size;

        // Set scale
        switch (_size)
        {
            case BirdSize.S:
                transform.localScale = _sizeS;
                break;
            case BirdSize.M:
                transform.localScale = _sizeM;
                break;
            case BirdSize.L:
                transform.localScale = _sizeL;
                break;
        }

        // Set rotation
        if(flipped)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }

        // Apply velocity
        _rigidbody.velocity = (Vector2)(transform.right * speed);
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
        gameObject.SetActive(false);
        _environment.Spawn();
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