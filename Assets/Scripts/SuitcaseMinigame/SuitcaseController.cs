using System;
using PassengerScripts;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;
using DG.Tweening;

public class SuitcaseController : MonoBehaviour
{
    [SerializeField] private AllPassengerController _passengerController;
    [SerializeField] private PassengerIdentities _passengerIdentities;
    [SerializeField] private Material[] _suitcaseMaterials;
    [SerializeField] private GameObject _luggagePrefab;
    private Passenger _waitingPassenger;
    private PassengerAgentController _waitingPassengerController;
    private StrikeManager _strikeManager;
    private SuitcaseMinigame _suitcaseMinigame;
    
    public UnityEvent onNewCase;

    [SerializeField] private float distanceToFly = 20f;

    void Start()
    {
        _strikeManager = FindFirstObjectByType<StrikeManager>();
        _suitcaseMinigame = FindFirstObjectByType<SuitcaseMinigame>();
        _passengerController.SpawnWaypoint.OnWaypointEnter += OnNewPassenger;
        _passengerController.CheckpointWaypoint.OnWaypointEnter += OnPassengerWaiting;
        _passengerController.CheckpointWaypoint.OnWaypointEnter += PassengerLookAtPlayer;
        _passengerController.CheckpointWaypoint.OnWaypointExit += OnPassengerExit;
    }

    private void PassengerLookAtPlayer(object sender, EventArgs e)
    {
        //passenger looks towards the camera on the Y axis
        _waitingPassenger.transform.DOLookAt(new Vector3(Camera.main.transform.position.x, _waitingPassenger.transform.position.y, Camera.main.transform.position.z), 0.5f).SetEase(Ease.InOutSine);
    }

    private void OnNewPassenger(object sender, EventArgs e)
    {
        Passenger passenger = _passengerController.SpawnWaypoint.ConnectedAgentController.Passenger;
        passenger.modelPrefab = _passengerIdentities.GetRandomIdentity();
        Instantiate(passenger.modelPrefab, passenger.transform);
        GetRandomSuitcase(passenger);
    }

    private void OnPassengerWaiting(object sender, EventArgs e)
    {
        _waitingPassengerController = _passengerController.CheckpointWaypoint.ConnectedAgentController;
        _waitingPassenger = _waitingPassengerController.GetComponent<Passenger>();
        _suitcaseMinigame.SpawnSuitCase(_waitingPassenger.LuggageHolder.GetComponentInChildren<SkinnedMeshRenderer>().material);
        onNewCase?.Invoke();

        _waitingPassenger.LuggageHolder.transform.DOMove(_waitingPassenger.LuggageHolder.transform.position + Vector3.up * distanceToFly, 0.5f).SetEase(Ease.InOutSine).onComplete += () =>
        {

            _waitingPassenger.LuggageHolder.gameObject.SetActive(false);
        };
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

    private void GetRandomSuitcase(Passenger passenger)
    {
        passenger.AddLuggage(_luggagePrefab ,_suitcaseMaterials[UnityEngine.Random.Range(0, _suitcaseMaterials.Length)]);
    }

}
