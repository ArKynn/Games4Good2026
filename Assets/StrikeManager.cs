using UnityEngine;

public class StrikeManager : MonoBehaviour
{
    [SerializeField] private int maxStrikes = 3;
    private int currentStrikes = 0;

    [SerializeField] private Material strikeOnMaterial;
    [SerializeField] private Material strikeOffMaterial;
    [SerializeField] private Renderer[] strike1;
    [SerializeField] private Renderer[] strike2;
    [SerializeField] private Renderer[] strike3;

    private Renderer[][] strikeRenderers;

    private void Start()
    {
        strikeRenderers = new Renderer[][] { strike1, strike2, strike3 };

    }

    public void AddStrike()
    {

        if (currentStrikes < maxStrikes)
            foreach (Renderer r in strikeRenderers[currentStrikes])
            {
                r.material = strikeOnMaterial;
            }

        currentStrikes++;
        if (currentStrikes >= maxStrikes) LoseGame();
        else print($"Error made! {currentStrikes} out of  {maxStrikes} strikes!");
    }

    private void LoseGame()
    {
        print("Game Over! 3 or more errors made!");
    }
}
