using UnityEngine;

public class BROClickAgent : BROAgent
{
    #region Unity ML-Agents
    /// <summary>
    /// Resets the agent and its environment.
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
    }

    /// <summary>
    /// Observation space
    /// </summary>
    public override void CollectObservations()
    {
        // Remaining blink cooldown
        AddVectorObs((float)_currentBlinkCooldown / (float)_maxBlinkCooldown);
        // Remaining blink cooldown as binary (excluded due to using old results)
        //if (_currentBlinkCooldown == 0.0f)
        //{
        //    AddVectorObs(0.0f);
        //}
        //else
        //{
        //    AddVectorObs(1.0f);
        //}
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
        AddVectorObs(_characterRigidbody.velocity.x / _characterMovementSpeed);
        AddVectorObs(_characterRigidbody.velocity.z / _characterMovementSpeed);
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
    /// Action execution of the agent.
    /// </summary>
    /// <param name="vectorAction"></param>
    /// <param name="textAction"></param>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Clicks
        // Move to mouse position
        if ((int)vectorAction[0] == 0)
        {
            Move();
        }
        // Blink to mouse position
        else if ((int)vectorAction[0] == 1)
        {
            Blink();
        }
        else
        {
            // Nothing
        }
        AddReward(0.01f); // For each step of survival
    }
    #endregion
}