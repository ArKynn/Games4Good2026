using UnityEngine;

public class Passenger : MonoBehaviour
{
    [SerializeField] private GameObject _passportHolder;
    private PassengerAgentController _agentController;
    private float _patience = 0;
    public float Patience => _patience;
    
    [SerializeField] private float _maxPatience = 10f;
    [SerializeField] private float _minPatience = 5f;
    
    private bool isLosingPatience = false;
    
    public GameObject Passport { get; private set; }
    public bool IsPassportCorrect {get; private set;}
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agentController = GetComponent<PassengerAgentController>();
        RandomizePatience();
    }

    // Update is called once per frame
    void Update()
    {
        if(isLosingPatience) UpdatePatience();
    }

    private void RandomizePatience()
    {
        _patience = Random.Range(_minPatience, _maxPatience);
    }
    
    public void ToggleLosingPatience()
    {
        isLosingPatience = !isLosingPatience;
    }

    private void UpdatePatience()
    {
        _patience -= Time.deltaTime;
        if (_patience <= 0f)
        {
            _agentController.GoToDespawn();
            // Notify game strike manager with +1 strike
            ToggleLosingPatience();
        }
    }

    public void StartLosingPatience()
    {
        ToggleLosingPatience();
    }

    public void SetPassport(GameObject passport, bool isCorrect)
    {
        if(Passport != null) return;
        Passport = passport;
        Passport.transform.SetParent(_passportHolder.transform);
        Passport.transform.localPosition = Vector3.zero;
        Passport.transform.localRotation = Quaternion.identity;
        IsPassportCorrect = isCorrect;
    }
}
