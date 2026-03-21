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
        [SerializeField] private Waypoint _RejectedNPCEndWaypoint;
        
        public Waypoint CheckpointWaypoint => _checkpointWaypoint;
        public Waypoint NPCEndWaypoint => _NPCEndWaypoint;
        public Waypoint RejectedNPCEndWaypoint => _RejectedNPCEndWaypoint;
        public Waypoint SpawnWaypoint => _spawnWaypoint;

        [SerializeField] private float _spawnFrequency;
        private float _spawnTimer = 0;
        private int _checkpointTimerChecks = 0;
        
        private bool _checkpointOccupied = false;

        private Queue<PassengerAgentController> _lineAgents;

        void Start()
        {
            _lineAgents = new Queue<PassengerAgentController>(_lineWaypoints.Count);

            _checkpointWaypoint.OnWaypointEnter += StartNpcPatience;
            _checkpointWaypoint.OnWaypointExit += TryClearCheckpointOccupied;
            _checkpointWaypoint.OnWaypointExit += CheckpointGetNextPassenger;
            
            _NPCEndWaypoint.OnWaypointEnter += DeleteOOBNPC;
            _RejectedNPCEndWaypoint.OnWaypointEnter += DeleteAltOOBNPC;
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
                    if (_checkpointWaypoint.IsEmpty)
                    {
                        CheckpointGetNextPassenger(this, EventArgs.Empty);
                        
                    }
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
            if (tempAgent != null && !_checkpointOccupied)
            {
                tempAgent.MoveToWaypoint(CheckpointWaypoint);
                _lineAgents.Dequeue();
                UpdateLine(this, EventArgs.Empty);
                _checkpointTimerChecks = 0;
                _checkpointOccupied = true;
            }
        }

        private void SpawnNewPassenger()
        {
            var temp = Instantiate(_passengerPrefab, _spawnWaypoint.transform.position, Quaternion.identity)
                .GetComponent<PassengerAgentController>();
            temp.SetAllPassengerController(this);
            _lineAgents.Enqueue(temp);
            UpdateLine(this, EventArgs.Empty);
        }

        private void StartNpcPatience(object sender, EventArgs args)
        {
            CheckpointWaypoint.ConnectedAgentController.Passenger.StartLosingPatience();
        }

        private void DeleteOOBNPC(object sender, EventArgs args)
        {
            Destroy(_NPCEndWaypoint.ConnectedAgentController.gameObject);
            _NPCEndWaypoint.SetEmpty(true);
            _NPCEndWaypoint.SetConnectedAgent(null);
            
        }
        private void DeleteAltOOBNPC(object sender, EventArgs args)
        {
            Destroy(_RejectedNPCEndWaypoint.ConnectedAgentController.gameObject);
            _NPCEndWaypoint.SetEmpty(true);
            _NPCEndWaypoint.SetConnectedAgent(null);
            
        }

        private void TryClearCheckpointOccupied(object sender, EventArgs args)
        {
            if(!_checkpointWaypoint.CheckActiveCollisions())
                _checkpointOccupied = false;
        }
    }
}