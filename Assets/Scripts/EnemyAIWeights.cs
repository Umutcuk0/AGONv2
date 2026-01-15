using UnityEngine;

public class EnemyAIWeights : MonoBehaviour
{
    [Header("Weights (0-10). Total must be <= 10")]
    [Range(0, 10)] public int shoot = 5;
    [Range(0, 10)] public int move = 2;
    [Range(0, 10)] public int cover = 3;      
    [Range(0, 10)] public int overwatch = 0;  

    void OnValidate()
    {
        ClampAndFixTotal();
    }

    void ClampAndFixTotal()
    {
        shoot = Mathf.Clamp(shoot, 0, 10);
        move = Mathf.Clamp(move, 0, 10);
        cover = Mathf.Clamp(cover, 0, 10);
        overwatch = Mathf.Clamp(overwatch, 0, 10);

        int total = shoot + move + cover + overwatch;
        if (total <= 10) return;

        
        while (total > 10)
        {
            int max = Mathf.Max(shoot, Mathf.Max(move, Mathf.Max(cover, overwatch)));
            if (max == 0) break;

            if (shoot == max && shoot > 0) shoot--;
            else if (cover == max && cover > 0) cover--;
            else if (move == max && move > 0) move--;
            else if (overwatch == max && overwatch > 0) overwatch--;

            total = shoot + move + cover + overwatch;
        }
    }

    public int Total => shoot + move + cover + overwatch;
}
