using UnityEngine;

public class WallUpgrader : MonoBehaviour
{
    public GameObject wall1;
    public GameObject wall2;

    public void UpgradeWall()
    {
        if (wall1 != null && wall2 != null)
        {
            wall1.SetActive(false);
            wall2.SetActive(true);
            Debug.Log("Wall upgraded from Wall1 to Wall2");
        }
        else
        {
            Debug.LogError("WallUpgrader: Wall1 or Wall2 is not assigned!");
        }
    }

    public void EnableWall1()
    {
        if (wall1 == null)
        {
            Debug.LogError("WallUpgrader: Wall1 is not assigned!");
            return;
        }

        if (wall2 == null)
        {
            Debug.LogError("WallUpgrader: Wall2 is not assigned!");
            return;
        }

        // Only enable wall1 if it is not active and wall2 is not active
        if (!wall1.activeSelf && !wall2.activeSelf)
        {
            wall1.SetActive(true);
            Debug.Log("Wall1 enabled");
        }
        else
        {
            Debug.Log("Wall1 not enabled: Either Wall1 is already active or Wall2 is active");
        }
    }
}