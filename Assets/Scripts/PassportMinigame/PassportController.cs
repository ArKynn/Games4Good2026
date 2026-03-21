using System;
using PassengerScripts;
using UnityEngine;
using Random = UnityEngine.Random;

public class PassportController : MonoBehaviour
{
    [SerializeField] private AllPassengerController _passengerController;
    [SerializeField] private float _incorrectPassportChance = 0.5f;
    
    private PassportGenerator passportGenerator;
    private Passenger _waitingPassenger;
    private PassengerAgentController _waitingPassengerController;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        passportGenerator = GetComponent<PassportGenerator>();
        _passengerController.SpawnWaypoint.OnWaypointEnter += OnNewPassenger;
        _passengerController.CheckpointWaypoint.OnWaypointEnter += OnPassengerWaiting;
        _passengerController.CheckpointWaypoint.OnWaypointExit += OnPassengerExit;
    }

    private void OnNewPassenger(object sender, EventArgs e)
    {
        Passenger passenger = _passengerController.SpawnWaypoint.ConnectedAgentController.Passenger;
        bool isPassportCorrect = Random.value >= _incorrectPassportChance;
        GameObject generatedPassport = passportGenerator.GenerateNewPassport(isPassportCorrect, passenger);
        passenger.SetPassport(generatedPassport, isPassportCorrect);
    }

    private void OnPassengerWaiting(object sender, EventArgs e)
    {
        _waitingPassengerController = _passengerController.CheckpointWaypoint.ConnectedAgentController;
        _waitingPassenger = _waitingPassengerController.GetComponent<Passenger>();
        ShowPassport();
    }

    private void OnPassengerExit(object sender, EventArgs e)
    {
        ClearWaitingPassenger();
    }

    private void ClearWaitingPassenger()
    {
        _waitingPassengerController = null;
        _waitingPassenger = null;
    }

    public void AcceptButtonPressed()
    {
        if (_waitingPassenger != null)
        {
            _waitingPassengerController.GoToDespawn();
            _waitingPassenger.ToggleLosingPatience();
            CheckDecision(true, _waitingPassenger.IsPassportCorrect);
            ClearWaitingPassenger();
        }
    }

    public void DeclineButtonPressed()
    {
        if (_waitingPassenger != null)
        {
            _waitingPassengerController.GoToDespawn(true);
            _waitingPassenger.ToggleLosingPatience();
            CheckDecision(false, _waitingPassenger.IsPassportCorrect);
            ClearWaitingPassenger();
        }
    }

    private void CheckDecision(bool decision, bool expectedDecision)
    {
        if (decision != expectedDecision)
        {
            //Notify Strike manager with +1 strike
        }
    }

    private void ShowPassport()
    {
        
    }
    
    private void HidePassport()
    {
        
    }
}
