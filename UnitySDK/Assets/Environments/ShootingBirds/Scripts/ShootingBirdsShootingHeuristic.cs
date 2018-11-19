using MLAgents;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A heuristic for the discrete agent of the multiagent, which shoots whenever it hovers a bird and reloads once the gun is emtpy.
/// </summary>
public class ShootingBirdsShootingHeuristic : MonoBehaviour, Decision
{
    #region Heuristic
    public float[] Decide(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        float[] actions = new float[] { 2.0f }; // Default action is to shoot
        // The fourth value of the observation indicates the hovered entity by the agent (1.0f = bird, 0.5f = obstacle, 0.0f = nothing)
        if (vectorObs[3] == 1.0f)
        {
            actions[0] = 0.0f; // Shoot
        }
        
        // The first value states the remaining ammunition
        if(vectorObs[0] == 0.0f)
        {
            actions[0] = 1.0f; // Reload
        }
        return actions;
    }

    public List<float> MakeMemory(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        return new List<float>();
    }
    #endregion
}