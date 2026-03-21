using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PassengerScripts
{
    public class AllPassengerController : MonoBehaviour
    {
        [SerializeField] private GameObject _passengerPrefab;
        [SerializeField] private Waypoint _spawnWaypoint;
        [SerializeField] private List<Waypoint> _lineWaypoints;
        [SerializeField] private Waypoint _checkpointWaypoint; 
        [SerializeField] private Waypoint _NPCEndWaypoint;
        
        public Waypoint CheckpointWaypoint => _checkpointWaypoint;
        public Waypoint NPCEndWaypoint => _NPCEndWaypoint;

        [SerializeField] private float _spawnFrequency;
        private float _spawnTimer = 0;
        private int _checkpointTimerChecks = 0;

        private Queue<PassengerAgentController> _lineAgents;

        void Start()
        {
            _lineAgents = new Queue<PassengerAgentController>(_lineWaypoints.Count);
            
            _checkpointWaypoint.OnWaypointExit += CheckpointGetNextPassenger;
        }

        void Update()
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _spawnFrequency)
            {
                if(_lineAgents.Count < _lineWaypoints.Count) SpawnNewPassenger();
                _checkpointTimerChecks++;
                if(_checkpointTimerChecks > 1)
                {
                    if(_checkpointWaypoint.IsEmpty) CheckpointGetNextPassenger(this, EventArgs.Empty);
                    _checkpointTimerChecks = 0;
                }
                _spawnTimer = 0;
            }
        }

        private void UpdateLine(object sender, EventArgs args)
        {
            PassengerAgentController temp;
            for (int i = 0; i < _lineAgents.Count; i++)
            {
                temp = _lineAgents.ElementAt(i);
                if (temp is null) return; 
                
                temp.MoveToWaypoint(_lineWaypoints[i]);
            }
        }

        private void CheckpointGetNextPassenger(object sender, EventArgs args)
        {
            _lineAgents.TryPeek(out var tempAgent);
            if (tempAgent != null)
            {
                tempAgent.MoveToWaypoint(CheckpointWaypoint);
                _lineAgents.Dequeue();
                UpdateLine(this, EventArgs.Empty);
            }
        }

        private void SpawnNewPassenger()
        {
            _lineAgents.Enqueue(Instantiate(_passengerPrefab, _spawnWaypoint.transform.position, Quaternion.identity).GetComponent<PassengerAgentController>());
            UpdateLine(this, EventArgs.Empty);
        }
    }
}