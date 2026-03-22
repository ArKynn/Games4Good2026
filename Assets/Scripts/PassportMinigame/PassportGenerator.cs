using System;
using UnityEngine;

public class PassportGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _passportPrefab;
    private PassengerIdentities _passengerIdentities;

    private void Start()
    {
        _passengerIdentities = GetComponent<PassengerIdentities>();
    }

    public GameObject GenerateNewPassport(bool isCorrect, Passenger passenger)
    {
        var tempPassport = Instantiate(_passportPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        if (isCorrect) GenerateCorrectPassport(tempPassport, passenger);
        else GenerateIncorrectPassport(tempPassport, passenger);
        return tempPassport;
    }

    private void GenerateCorrectPassport(GameObject passport, Passenger passenger)
    {
        passport.transform.GetChild(0).transform.gameObject.GetComponent<SpriteRenderer>().sprite = _passengerIdentities.GetPhoto(true, prefab: passenger.modelPrefab);
    }

    private void GenerateIncorrectPassport(GameObject passport, Passenger passenger)
    {
        passport.transform.GetChild(0).transform.gameObject.GetComponent<SpriteRenderer>().sprite = _passengerIdentities.GetPhoto(false, prefab: passenger.modelPrefab);
    }
}
