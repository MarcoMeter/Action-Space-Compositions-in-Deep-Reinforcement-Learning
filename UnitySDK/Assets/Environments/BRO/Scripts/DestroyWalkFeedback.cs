using UnityEngine;

/// <summary>
/// This component destroy the WalkFeedback animation after a give time.
/// </summary>
public class DestroyWalkFeedback : MonoBehaviour 
{
    #region Member Fields
    [SerializeField]
    private float m_destroyAfterSeconds = 0.5f;
    private float m_countTime = 0;
    #endregion

    #region Unity Lifecycle
    private void Update () 
    {
        m_countTime += Time.deltaTime;
        if (m_countTime > m_destroyAfterSeconds)
            Destroy(gameObject); // Destroy the walk feedback after a short time, so that the instantiated game object doesn't remain in the scene after it's animation
	}
    #endregion
}