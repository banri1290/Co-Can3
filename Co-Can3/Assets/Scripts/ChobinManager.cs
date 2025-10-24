using UnityEngine;

public class ChobinManager : MonoBehaviour
{
    private int currentCookingChobinNum = 0;

<<<<<<< Updated upstream
    // Start is called before the first frame update
    void Start()
=======
    public int CurrentCookingNum => currentCookingNum;

    public override bool CheckSettings()
>>>>>>> Stashed changes
    {

    }

    // Update is called once per frame
    void Update()
    {

<<<<<<< Updated upstream
=======
    public void IncrementCookingNum()
    {
        currentCookingNum++;
    }

    public void DecrementCookingNum()
    {
        currentCookingNum--;
>>>>>>> Stashed changes
    }
}
