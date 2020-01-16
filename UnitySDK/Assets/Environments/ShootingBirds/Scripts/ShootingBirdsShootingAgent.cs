using UnityEngine;

public class ShootingBirdsShootingAgent : ShootingBirdsAgent
{
    #region Member Fields
    [SerializeField]
    private ShootingBirdsMovementAgent _movementAgent;
    #endregion

    #region Unity ML-Agents
    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    /// <summary>
    /// Sets the agent back to his initial position.
    /// </summary>
    public override void AgentReset()
    {
        base.AgentReset();
    }

    /// <summary>
    /// Observes the state space.
    /// </summary>
    public override void CollectObservations()
    {
        base.CollectObservations();
    }

    /// <summary>
    /// Executes discrete actions: nothing, shooting, reloading.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int action = Mathf.FloorToInt(vectorAction[0]);
        switch (action)
        {
            case 0:
                // Nothing
                break;
            case 1:
                RewardMovementAgent();
                Shoot();
                break;
            case 2:
                Reload();
                break;
        }
    }
    #endregion

    #region Private Functions
    private void RewardMovementAgent()
    {
        Collider2D coll = Physics2D.OverlapPoint((Vector2)transform.position, _layerMaskBirdEnv);
        // Check what's been shot
        if (coll && coll.tag.Equals(_BIRD_TAG))
        {
            BirdBehavior bird = coll.GetComponent<BirdBehavior>();
            // Reward the agent for hitting a bird based on the bird's size
            switch (bird.BirdSize)
            {
                case BirdSize.S:
                    _movementAgent.AddReward(2.0f);
                    break;
                case BirdSize.M:
                    _movementAgent.AddReward(0.75f);
                    break;
                case BirdSize.L:
                    _movementAgent.AddReward(0.25f);
                    break;
            }
        }
        else
        {
            // Punish for hitting nothing
            _movementAgent.AddReward(-0.1f);
        }
    }
    #endregion
}