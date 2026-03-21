using UnityEngine;

public class PassportGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _passportPrefab;

    public GameObject GenerateNewPassport(bool isCorrect, Passenger passenger)
    {
        var tempPassport = Instantiate(_passportPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        if (isCorrect) GenerateCorrectPassport(tempPassport, passenger);
        else GenerateIncorrectPassport(tempPassport, passenger);
        return tempPassport;
    }

    private void GenerateCorrectPassport(GameObject passport, Passenger passenger)
    {
        passport.GetComponent<MeshRenderer>().material.color = Color.green;
    }

    private void GenerateIncorrectPassport(GameObject passport, Passenger passenger)
    {
        passport.GetComponent<MeshRenderer>().material.color = Color.red;
    }
}
