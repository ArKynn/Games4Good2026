using UnityEngine;

public class StrikeManager : MonoBehaviour
{
    [SerializeField] private int maxStrikes = 3;
    private int currentStrikes = 0;

    public void AddStrike()
    {
        currentStrikes++;
        if (currentStrikes >= maxStrikes) LoseGame();
        else print($"Error made! {currentStrikes} out of  {maxStrikes} strikes!");
    }

    private void LoseGame()
    {
        print("Game Over! 3 or more errors made!");
    }
}
