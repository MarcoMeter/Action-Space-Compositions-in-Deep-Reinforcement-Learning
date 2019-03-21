using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MLAgents;

/// <summary>
/// The only use of this academy is to log environment measures to a csv frequently.
/// </summary>
public class ShootingBirdsAcademy : Academy
{
    #region Member Fields
    [SerializeField]
    private bool _log = true;
    [SerializeField]
    private ShootingBirdsAgentType _agentType = ShootingBirdsAgentType.Bucketed;
    protected List<ShootingBirdsAgent> _agents = new List<ShootingBirdsAgent>();
    private string _fileName;
    private int _eps = 1;
    float _summedClickAccuracies = 0;
    float _summedReloadAccuracies = 0;
    int count = 0;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        _fileName = _agentType.ToString() + "-" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + UnityEngine.Random.Range(0, 10000) + ".csv";

    }
    #endregion

    #region Academy Overrides
    public override void InitializeAcademy()
    {
        // Get agent references
        var agentGos = GameObject.FindGameObjectsWithTag("Agent");
        foreach (var go in agentGos)
        {
            switch (_agentType)
            {
                case ShootingBirdsAgentType.Bucketed:
                    if (go.GetComponent<ShootingBirdsBucketedAgent>())
                        _agents.Add(go.GetComponent<ShootingBirdsBucketedAgent>());
                    break;
                case ShootingBirdsAgentType.Threshold:
                    if (go.GetComponent<ShootingBirdsThresholdAgent>())
                        _agents.Add(go.GetComponent<ShootingBirdsThresholdAgent>());
                    break;
                case ShootingBirdsAgentType.Multiagent:
                    if (go.GetComponent<ShootingBirdsShootingAgent>())
                        _agents.Add(go.GetComponent<ShootingBirdsShootingAgent>());
                    break;
            }
        }
    }

    public override void AcademyStep()
    {
        // Log click accuracy to csv
        if(GetStepCount() == 1999 && _log)
        {
            // Get info from agents
            foreach (var agent in _agents)
            {
                if (agent)
                {
                    _summedClickAccuracies += agent.GetClickAccuracy();
                    _summedReloadAccuracies += agent.GetReloadAccuracy();
                }
                count++;
            }
            // create values
            if (count > 0 && _eps % 10 == 0 )
            {
                //string[] headings = new string[] {"click_accuracy", "reload_accuracy"};
                string[] headings = new string[] {"steps", "click_accuracy"};
                //float[] values = new float[] {summedClickAccuracies / count, summedReloadAccuracies / count};
                float[] values = new float[] {_eps * 2000, _summedClickAccuracies / count};
                // write values to csv
                WriteToCSV(",", headings, values);

                _summedClickAccuracies = 0;
                _summedReloadAccuracies = 0;
                count = 0;
            }
            _eps++;
        }
    }
    #endregion

    #region Private Functions
    private void WriteToCSV(string delimiter, string[] headings, float[] values)
    {
        if (headings.Length != values.Length)
        {
            throw new Exception("Headings and values are not the same size.");
        }

        string path = @Application.dataPath + "/"  + _fileName;
        if (!File.Exists(path))
        {
            // Create a file to write to and write the headings to the file.
            using (StreamWriter sw = File.CreateText(path))
            {
                string firstRow = "";
                for (int i = 0; i < headings.Length; i++)
                {
                    firstRow = string.Concat(firstRow, headings[i]);
                    if (i < headings.Length - 1)
                    {
                        firstRow = string.Concat(firstRow, delimiter);
                    }
                }
                sw.WriteLine(firstRow);
            }
        }

        // Append values
        using (StreamWriter sw = new StreamWriter(path, append: true))
        {
            string row = "";
            for (int i = 0; i < values.Length; i++)
            {
                row = string.Concat(row, values[i]);
                if (i < values.Length - 1)
                {
                    row = string.Concat(row, delimiter);
                }
            }
            sw.WriteLine(row);
        }
    }
    #endregion
}

public enum ShootingBirdsAgentType
{
    Bucketed = 0,
    Threshold = 1,
    Multiagent = 2
}