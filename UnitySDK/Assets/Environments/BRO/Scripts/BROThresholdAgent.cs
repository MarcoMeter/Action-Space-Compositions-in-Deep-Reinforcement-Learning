using UnityEngine;
using MLAgents;
using UnityEngine.UI;
using System;

public class BROThresholdAgent : BROAgent
{
    #region Member Fields
    [SerializeField]
    private float _discreteActionThreshold = 0.33f;
    [Header("UI")]
    [SerializeField]
    private Text _averageSpeedText;
    private float _sumVelocityX = 0.0f;
    private float _sumVelocityZ = 0.0f;
    private int _stepDevisor = 0;
    [SerializeField]
    private Text _survivalDurationText;
    private int _survivalDuration = 0;
    #endregion

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

        // UI
        _stepDevisor = 0;
        _sumVelocityX = 0.0f;
        _sumVelocityZ = 0.0f;
        _survivalDuration = 0;
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
        if (brain.brainParameters.vectorActionSpaceType.Equals(SpaceType.continuous))
        {
            // "Mouse Movement"
            _rigidbody.velocity = new Vector3(vectorAction[0], 0, vectorAction[1]) * _movementSpeed;

            // Move to mouse position
            if (Mathf.Abs(vectorAction[2]) < _discreteActionThreshold)
            {
                Move();
            }
            // Blink to mouse position
            if (Mathf.Abs(vectorAction[3]) < _discreteActionThreshold)
            {
                Blink();
            }
        }
        AddReward(0.01f);

        // Update UI
        _stepDevisor++;
        _sumVelocityX += Math.Abs(_rigidbody.velocity.x);
        _sumVelocityZ += Math.Abs(_rigidbody.velocity.z);
        _averageSpeedText.text = (_sumVelocityX / _stepDevisor).ToString("0.00") + " | " + (_sumVelocityZ / _stepDevisor).ToString("0.00");
        if (_stepDevisor % 6 == 0) // 6 = decision frequency
        {
            _survivalDuration++;
        }
        _survivalDurationText.text = _survivalDuration.ToString();
    }
    #endregion
}