using System;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    private bool _isEmpty = true;
    public bool IsEmpty => _isEmpty;
    
    public Vector3 Position => transform.position;
    public PassengerAgentController PassengerAgentController { get; private set; }
    
    public event EventHandler OnWaypointEnter;
    public event EventHandler OnWaypointExit;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent(typeof(PassengerAgentController)) != null)
        {
            PassengerAgentController = other.GetComponent(typeof(PassengerAgentController)) as PassengerAgentController;
            
            _isEmpty = false;
            OnWaypointEnter?.Invoke(this ,EventArgs.Empty);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent(typeof(PassengerAgentController)) != null)
        {
            PassengerAgentController = null;
            _isEmpty = true;
            OnWaypointExit?.Invoke(this, EventArgs.Empty);
        }
    }
}
