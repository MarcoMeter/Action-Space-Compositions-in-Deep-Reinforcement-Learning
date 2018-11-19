using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BROBucketedAgent : BROAgent
{
    #region Member Fields
    // 41 actions
    //private float[] _movementBuckets = new float[] {-1.0f, -0.95f, -0.9f, -0.85f, -0.8f, -0.75f, -0.7f, -0.65f, -0.6f, -0.55f, -0.5f, -0.45f, -0.4f, -0.35f, -0.3f, -0.25f,
    //                                              -0.2f, -0.15f, -0.1f, -0.05f, 0.0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f,
    //                                                0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1.0f};

    // 21 actions
    //private float[] _movementBuckets = new float[] {-1.0f, -0.9f, -0.8f, -0.7f, -0.6f, -0.5f, -0.4f, -0.3f, -0.2f, -0.1f, 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f,
    //                                               0.8f, 0.9f, 1.0f};

    // 11 Actions
    //private float[] _movementBuckets = new float[] {-1.0f, -0.8f, -0.6f, -0.4f, -0.2f, 0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f};

    // 7 actions
    private float[] _movementBuckets = new float[] {-1.0f, -0.5f, -0.1f, 0.0f, 0.1f, 0.5f, 1.0f};

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

        // Bucket action setup
        // Override for 201 actions
        //var numbers = Enumerable.Range(-100, 201).ToArray();
        //_movementBuckets = numbers.Select(x => (float)x / 100.0f).ToArray();
    }

    /// <summary>
    /// Observation space
    /// </summary>
    public override void CollectObservations()
    {
        // Remaining blink cooldown as ration
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
        // "Mouse Movement"
        Vector3 newVelocity = Vector3.zero;
        // Horizontal
        newVelocity.x = _movementBuckets[(int)vectorAction[0]] * _movementSpeed;
        // Vertical
        newVelocity.z = _movementBuckets[(int)vectorAction[1]] * _movementSpeed;
        _rigidbody.velocity = newVelocity;

        // Move to mouse position
        if ((int)vectorAction[2] == 0)
        {
            Move();
        }
        // Blink to mouse position
        else if ((int)vectorAction[2] == 1)
        {
            Blink();
        }
        else
        {
            // Nothing
        }
        AddReward(0.01f); // For each step of survival

        // Update UI
        _stepDevisor++;
        _sumVelocityX += Math.Abs(newVelocity.x);
        _sumVelocityZ += Math.Abs(newVelocity.z);
        _averageSpeedText.text = (_sumVelocityX / _stepDevisor).ToString("0.00") + " | " + (_sumVelocityZ / _stepDevisor).ToString("0.00");
        if(_stepDevisor % 6 == 0) // 6 = decision frequency
        {
            _survivalDuration++;
        }
        _survivalDurationText.text = _survivalDuration.ToString();
    }
    #endregion
}