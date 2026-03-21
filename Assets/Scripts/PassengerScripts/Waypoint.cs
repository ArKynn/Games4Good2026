using System;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private bool _isEmpty = true;
    public bool IsEmpty => _isEmpty;

    public Vector3 Position => transform.position;
    public PassengerAgentController ConnectedAgentController { get; private set; }

    public event EventHandler OnWaypointEnter;
    public event EventHandler OnWaypointExit;

    protected void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent(typeof(Passenger)) != null)
        {

            ConnectedAgentController = other.GetComponent(typeof(PassengerAgentController)) as PassengerAgentController;

            _isEmpty = false;
            OnWaypointEnter?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent(typeof(Passenger)) != null && CheckActiveCollisions())
        {
            ConnectedAgentController = null;
            _isEmpty = true;
            OnWaypointExit?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetEmpty(bool state)
    {
        _isEmpty = state;
    }

    public void SetConnectedAgent(PassengerAgentController agentController)
    {
        ConnectedAgentController = agentController;
    }

    public bool CheckActiveCollisions()
    {
        return Physics.OverlapBox(gameObject.transform.position, Vector3.one, Quaternion.identity).Length == 4;
    }
}
