using System.Collections.Generic;
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
    private int _initialSpawnCount = 15;
    // Bird pool
    private List<GameObject> _birdPool = new List<GameObject>();
    private int _birdPoolSize = 20;
    [SerializeField]
    private GameObject _birdPoolParent;
    [SerializeField]
    private bool _allowPoolGrowth = true;
    #endregion

    #region Unity Lifecycle
    /// <summary>
    /// Initially spawns a certain number of birds to begin with.
    /// </summary>
    private void Start()
    {
        InitBirdPool();

        // Spawn initial birds
        for (int i = 0; i < _initialSpawnCount; i++)
        {
            Spawn();
        }
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Spawns a single bird.
    /// </summary>
    public void Spawn()
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

        // Instantiate bird (i.e. reuse pooledbird)
        GameObject bird = GetBird();
        if (bird != null)
        {
            bird.transform.position = spawnPos;
            bird.transform.rotation = Quaternion.identity;
            bird.SetActive(true);
            BirdBehavior birdBehavior = bird.GetComponent<BirdBehavior>();

            // Initialize bird behavior
            var speed = Random.Range(1.5f, 6f);
            birdBehavior.Init(this, (BirdSize)size, speed, spawnSite != 0);
        }
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Initializes the bird pool by instantiating birds and setting them as inactive.
    /// </summary>
    private void InitBirdPool()
    {
        for (int i = 0; i < _birdPoolSize; i++)
        {
            GameObject bird = Instantiate(_birdPrefab, _birdPoolParent.transform);
            bird.transform.rotation = Quaternion.identity;
            bird.SetActive(false);
            _birdPool.Add(bird);
        }
    }

    /// <summary>
    /// Selects an inactive bird from the pool.
    /// </summary>
    /// <returns>An inactive bird GameObject</returns>
    private GameObject GetBird()
    {
        foreach(var bird in _birdPool)
        {
            if(!bird.activeInHierarchy)
            {
                return bird;
            }
        }

        // Add a new bird if not enough birds are available
        if (_allowPoolGrowth)
        {
            GameObject newBird = Instantiate(_birdPrefab, _birdPoolParent.transform);
            newBird.transform.rotation = Quaternion.identity;
            newBird.SetActive(false);
            _birdPool.Add(newBird);
            return newBird;
        }
        else
        {
            return null;
        }
    }
    #endregion
}