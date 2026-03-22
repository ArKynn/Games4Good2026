using System;
using PassengerScripts;
using UnityEngine;

public class SuitcaseController : MonoBehaviour
{
    [SerializeField] private AllPassengerController _passengerController;
    [SerializeField] private PassengerIdentities _passengerIdentities;
    private Passenger _waitingPassenger;
    private PassengerAgentController _waitingPassengerController;
    private StrikeManager _strikeManager;
    private SuitcaseMinigame _suitcaseMinigame;
    
    void Start()
    {
        _strikeManager = FindFirstObjectByType<StrikeManager>();
        _suitcaseMinigame = FindFirstObjectByType<SuitcaseMinigame>();
        _passengerController.SpawnWaypoint.OnWaypointEnter += OnNewPassenger;
        _passengerController.CheckpointWaypoint.OnWaypointEnter += OnPassengerWaiting;
        _passengerController.CheckpointWaypoint.OnWaypointExit += OnPassengerExit;
    }

    private void OnNewPassenger(object sender, EventArgs e)
    {
        Passenger passenger = _passengerController.SpawnWaypoint.ConnectedAgentController.Passenger;
        passenger.modelPrefab = _passengerIdentities.GetRandomIdentity();
        Instantiate(passenger.modelPrefab, passenger.transform);
    }

    private void OnPassengerWaiting(object sender, EventArgs e)
    {
        _waitingPassengerController = _passengerController.CheckpointWaypoint.ConnectedAgentController;
        _waitingPassenger = _waitingPassengerController.GetComponent<Passenger>();
        _suitcaseMinigame.SpawnSuitCase();
    }

    private void OnPassengerExit(object sender, EventArgs e)
    {
        if(_suitcaseMinigame.CurrentCase != null) _suitcaseMinigame.StartDespawnCase();
        ClearWaitingPassenger();
    }

    public void ErrorHandlingCase()
    {
        _strikeManager.AddStrike();
    }
    
    public void DoneInspectingCase()
    {
        if (_waitingPassenger != null)
        {
            _waitingPassengerController.GoToDespawn();
            _waitingPassenger.ToggleLosingPatience();
            ClearWaitingPassenger();
        }
    }
    
    private void ClearWaitingPassenger()
    {
        _waitingPassengerController = null;
        _waitingPassenger = null;
    }

}
