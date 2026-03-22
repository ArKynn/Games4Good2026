using System;
using PassengerScripts;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class PassportController : MonoBehaviour
{
    [SerializeField] private AllPassengerController _passengerController;
    [SerializeField] private float _incorrectPassportChance = 0.5f;
    
    private PassportGenerator _passportGenerator;
    private PassengerIdentities _passengerIdentities;
    private Passenger _waitingPassenger;
    private PassengerAgentController _waitingPassengerController;
    private StrikeManager _strikeManager;
    
    public UnityEvent onNewPassport;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _passportGenerator = GetComponent<PassportGenerator>();
        _passengerIdentities = GetComponent<PassengerIdentities>();
        _strikeManager = FindFirstObjectByType<StrikeManager>();
        _passengerController.SpawnWaypoint.OnWaypointEnter += OnNewPassenger;
        _passengerController.CheckpointWaypoint.OnWaypointEnter += OnPassengerWaiting;
        _passengerController.CheckpointWaypoint.OnWaypointExit += OnPassengerExit;
    }

    private void OnNewPassenger(object sender, EventArgs e)
    {
        Passenger passenger = _passengerController.SpawnWaypoint.ConnectedAgentController.Passenger;
        passenger.modelPrefab = _passengerIdentities.GetRandomIdentity();
        Instantiate(passenger.modelPrefab, passenger.transform);
        bool isPassportCorrect = Random.value >= _incorrectPassportChance;
        GameObject generatedPassport = _passportGenerator.GenerateNewPassport(isPassportCorrect, passenger);
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
        HidePassport();
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
            _strikeManager.AddStrike();
        }
    }

    private void ShowPassport()
    {
        onNewPassport?.Invoke();
        _waitingPassenger.PassportHolder.SetActive(true);
    }
    
    private void HidePassport()
    {
        if(_waitingPassenger == null) return;
        var temp = _waitingPassenger.PassportHolder;
        if(temp.activeSelf) temp.SetActive(false);
    }
}
