using UnityEngine;

/// <summary>
/// This class controls the environment, which is basically the spawn behavior of the birds.
/// </summary>
public class ShootingBirdsEnvironment : MonoBehaviour
{
    #region Member Fields
    // Bird Spawn Members
    [SerializeField]
    private GameObject _birdPrefab;
    [SerializeField]
    private Transform _leftTopSpawnIndicatior;
    [SerializeField]
    private Transform _rightBottomIndicator;
    private int _initialSpawnCount = 10;
    private int _spawnRate = 1;
    private float _spawnInterval = 0.5f;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initially spawns a certain number of birds to begin with. Triggers the invocation of further spawns.
    /// </summary>
    private void Start()
    {
        // Initial spawn
        for (int i = 0; i < _initialSpawnCount; i++)
        {
            Spawn();
        }

        // Continues spawns
        InvokeRepeating("SpawnWave", _spawnInterval, _spawnInterval);
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Spawns a single bird.
    /// </summary>
    private void Spawn()
    {
        // Spawn position (x,y)
        // Decide on spawn site
        var spawnSite = Random.Range(0, 2);
        var size = Random.Range(0, 3);
        var spawnZAdjustment = Random.Range(0.1f, 0.2f); // This modification on the Z-Axis shall prevent flickering issues of overlapping sprites

        Vector3 spawnPos;
        if (spawnSite == 0) // left
        {
            spawnPos = new Vector3(_leftTopSpawnIndicatior.position.x,
                                        Random.Range(_rightBottomIndicator.position.y, _leftTopSpawnIndicatior.position.y), size - spawnZAdjustment);
        }
        else // right
        {
            spawnPos = new Vector3(_rightBottomIndicator.position.x,
                            Random.Range(_rightBottomIndicator.position.y, _leftTopSpawnIndicatior.position.y), size - spawnZAdjustment);
        }
        // Instantiate bird
        var birdBehavior = Instantiate(_birdPrefab, spawnPos, Quaternion.identity).GetComponent<BirdBehavior>();

        // Initialize bird behavior
        var speed = Random.Range(1.5f, 6f);
        birdBehavior.Init((BirdSize)size, speed, spawnSite != 0);
    }

    /// <summary>
    /// Spawns a wave of birds for each interval.
    /// </summary>
    private void SpawnWave()
    {
        for (int i = 0; i < _spawnRate; i++)
        {
            Spawn();
        }
    }
    #endregion
}