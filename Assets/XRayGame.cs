using UnityEngine;
using DG.Tweening;

public class XRayGame : MonoBehaviour
{
    [SerializeField] private Transform cameraPosition;
    private Vector3 originalCameraPosition;
    public bool active;
    [SerializeField] private GameObject instructionsUI;
    private StrikeManager _strikeManager;

    private bool buttonClicked = false;
    public void ActivateGame(bool toggle)
    {
        active = toggle;
        

        if(!toggle)
        {
            FirstPersonViewport.Instance.SetMovementActive(true);
        }

        // Make the vision cone increase from 0 to the original size if its to activate the game, and vice versa
        if (toggle)
        {
            

            FirstPersonViewport.Instance.minigameActive = true;
            originalCameraPosition  = Camera.main.transform.position;
            Camera.main.transform.DOMove(cameraPosition.position, 0.5f).SetEase(Ease.OutBack).onComplete += () =>
            {
                if (instructionsUI != null)
                {
                    instructionsUI.SetActive(true);
                    instructionsUI.transform.localScale = Vector3.zero;
                    instructionsUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
                }
            };
            Camera.main.transform.DORotate(cameraPosition.eulerAngles, 0.5f).SetEase(Ease.OutBack);
            visionCone.localScale = Vector3.zero;
            visionCone.DOScale(originalVisionConeScale, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {

            if(instructionsUI != null)
            {
                instructionsUI.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    instructionsUI.SetActive(false);
                });
            }

            
            Camera.main.transform.DOMove(originalCameraPosition, 0.5f).SetEase(Ease.InBack).onComplete += () =>
            {
                FirstPersonViewport.Instance.minigameActive = false;
            };

            visionCone.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
        }

    }

    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    private GameObject currentCase;
    private XRayObject[] currentCaseObjects;

    [SerializeField] private float wrongCaseChance = 0.2f;

    [SerializeField] private Transform caseParent;
    [SerializeField] private Transform[] objectSlots;
    [SerializeField] private XRayObject[] goodObjects;
    [SerializeField] private XRayObject[] wrongObjects;

    [SerializeField] private float caseDuration = 5f;

    [SerializeField] private Transform visionCone;
    private Vector3 originalVisionConeScale;
    [SerializeField] private BoxCollider2D visionCollider;

    [SerializeField] private Camera gameCamera;

    private bool isWrong = false;
    [SerializeField] private SpriteRenderer background;

    [SerializeField] private SpriteRenderer caseForeground;
    [SerializeField] private Sprite[] caseForegrounds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _strikeManager = FindFirstObjectByType<StrikeManager>();
        originalVisionConeScale = visionCone.localScale;
        visionCone.localScale = Vector3.zero; // Start with the vision cone hidden
        StartMovingCase();
    }

    [SerializeField] private float movementSpeed = 5f;

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Escape) && active)
        {
            ActivateGame(false);
        }

        if (!active)
            return;

        // 1. Get Input (WASD or Arrows)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 2. Calculate New Position
        Vector3 movement = new Vector3(moveX, moveY, 0) * movementSpeed * Time.deltaTime;
        Vector3 newPos = visionCone.position + movement;

        // 3. Get Bounds of the Collider
        // This automatically handles the box's position and scale
        Bounds bounds = visionCollider.bounds;

        // 4. Clamp the new position within the collider's world-space edges
        float clampedX = Mathf.Clamp(newPos.x, bounds.min.x, bounds.max.x);
        float clampedY = Mathf.Clamp(newPos.y, bounds.min.y, bounds.max.y);

        // 5. Apply the position while maintaining the Z depth
        visionCone.position = new Vector3(clampedX, clampedY, visionCone.position.z);
    }

    public void CheckCase()
    {
        if (buttonClicked)return;

        buttonClicked = true;
        background.enabled = true;
        if (isWrong)
        {
            caseParent.DOKill(); // Stop any ongoing tweens on the case parent to prevent conflicts
            background.DOColor(Color.yellow, 0.3f).SetLoops(10, LoopType.Yoyo).onComplete += () =>
            {
                background.enabled = false;
                // If wrong the case goes up by 5 units using dotween, then it starts a new case
                
                caseParent.DOMoveY(caseParent.position.y + 15f, 7).SetEase(Ease.Linear).OnComplete(() =>
                {
                    StartMovingCase();
                });
            };
            // Handle failure (e.g., show feedback, reset game, etc.)
        }
        else
        {
            background.DOColor(Color.red, 0.3f).SetLoops(10, LoopType.Yoyo).onComplete += () =>
            {
                background.enabled = false;
            };
            _strikeManager.AddStrike();
            // Handle success (e.g., show feedback, increase score, etc.)
        }
        

    }

    public void StartMovingCase()
    {
        buttonClicked = false;
        bool wrongCase = Random.value < wrongCaseChance;

        isWrong = wrongCase;

        caseForeground.sprite = caseForegrounds[Random.Range(0, caseForegrounds.Length)];

        if (currentCaseObjects != null)
            foreach (XRayObject xRayObject in currentCaseObjects)
            {
                Destroy(xRayObject.gameObject);
            }

        currentCaseObjects = new XRayObject[objectSlots.Length];

        int numberOfBadItems = wrongCase ? Random.Range(1, 4) : 0;

        // Shuffle the object slots to randomize the placement of good and bad items    
        int[] indexes = new int[objectSlots.Length];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = i;
        }
        for (int i = 0; i < indexes.Length; i++)
        {
            int temp = indexes[i];
            int randomIndex = Random.Range(i, indexes.Length);
            indexes[i] = indexes[randomIndex];
            indexes[randomIndex] = temp;
        }


        for (int i = 0; i < objectSlots.Length; i++)
        {
            if (i < numberOfBadItems)
            {
                currentCaseObjects[i] = Instantiate(wrongObjects[Random.Range(0, wrongObjects.Length)], objectSlots[indexes[i]].position, Quaternion.identity, objectSlots[i]);
            }
            else
            {
                currentCaseObjects[i] = Instantiate(goodObjects[Random.Range(0, goodObjects.Length)], objectSlots[indexes[i]].position, Quaternion.identity, objectSlots[i]);
            }

            currentCaseObjects[i].transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
            currentCaseObjects[i].GetComponent<SpriteRenderer>().sortingOrder = 1; // Ensure correct rendering order
        }

        caseParent.position = startPos.position;

        caseParent.DOMove(endPos.position, caseDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            // Handle case reaching the end position (e.g., check for player input, reset case, etc.)
            Debug.Log("Case reached the end position!");
            if(isWrong) _strikeManager.AddStrike();
            StartMovingCase(); // Start the next case immediately after one finishes
        });

    }

}
