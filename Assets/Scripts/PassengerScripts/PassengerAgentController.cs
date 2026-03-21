using PassengerScripts;
using UnityEngine;
using UnityEngine.AI;

public class PassengerAgentController : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Passenger _passenger;
    private AllPassengerController _agentController;
    
    public Passenger Passenger => _passenger;
    
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _passenger = GetComponent<Passenger>();
        _agentController = FindFirstObjectByType<AllPassengerController>();
    }

    public void MoveToWaypoint(Waypoint destination)
    {
        if(destination.Position == Vector3.zero || destination == null || _agent == null) return;
        
        _agent.SetDestination(destination.Position);
    }

    public void GoToDespawn()
    {
        MoveToWaypoint(_agentController.NPCEndWaypoint);
    }
}