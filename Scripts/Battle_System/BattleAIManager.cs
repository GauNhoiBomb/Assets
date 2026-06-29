using UnityEngine;
using System.Collections;

public class BattleAIManager : MonoBehaviour
{
    // Hàm được BattleManager gọi sang khi đến lượt Enemy hoặc NPC
    public void ExecuteAITurn(BattleUnit activeUnit, BattleManager battleManager)
    {
        // Chạy một luồng đếm thời gian suy nghĩ giả lập để game không bị quá nhanh
        StartCoroutine(AIRoutine(activeUnit, battleManager));
    }

    IEnumerator AIRoutine(BattleUnit activeUnit, BattleManager battleManager)
    {
        // Trì hoãn 1.5 giây để người chơi nhìn thấy quái/NPC đang trong lượt
        yield return new WaitForSeconds(1.5f);

        if (activeUnit.role == CharacterRole.Enemy)
        {
            Debug.Log($"🤖 AI Quái Vật [{activeUnit.unitName}] gầm gừ một cái rồi qua lượt!");
            // Tương lai: Viết code trừ máu người chơi ở đây...
        }
        else if (activeUnit.role == CharacterRole.NPC_Army)
        {
            Debug.Log($"⚡ AI NPC [{activeUnit.unitName}] niệm chú hỗ trợ rồi qua lượt!");
            // Tương lai: Viết code hồi máu hoặc cộng hiệu ứng ở đây...
        }

        // Sau khi "suy nghĩ" xong, tự động ra lệnh kết thúc lượt giống như bấm nút END
        if (battleManager != null)
        {
            battleManager.PlayerEndTurn();
        }
    }
}