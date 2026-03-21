using PassengerScripts;
using UnityEngine;
using UnityEngine.AI;

public class PassengerAgentController : MonoBehaviour
{
    private NavMeshAgent _agent;
    private AllPassengerController _agentController;
    
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agentController = GetComponent<AllPassengerController>();
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