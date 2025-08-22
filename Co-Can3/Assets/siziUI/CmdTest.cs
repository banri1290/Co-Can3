using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // TextMeshPro用

public class CmdTest : MonoBehaviour
{
    [System.Serializable]
    public class TextSet
    {
        public TMP_Text leftText;   // 左のテキスト
        public TMP_Text rightText;  // 右のテキスト
        public Animator animator;   // 1セットに1つのAnimator
    }

    public List<TextSet> sets = new List<TextSet>(); // セットをInspectorに登録
    private int currentSet = 0;

    // EnterボタンのOnClickイベントに登録
    public void SetCmdAct()
    {
        if (currentSet < sets.Count)
        {
            PlayAnimationsForSet(sets[currentSet]);
            currentSet++;
        }
        else
        {
            Debug.Log("すべてのセットのアニメーションが終了しました");
        }
    }

    private void PlayAnimationsForSet(TextSet set)
    {
        // 左と右のテキストを連結して判定（例: "走る+ジャンプ"）
        string combo = set.leftText.text + "+" + set.rightText.text;
        TriggerAnimationByText(combo, set.animator);
    }

    private void TriggerAnimationByText(string combo, Animator animator)
    {
        if (animator == null) return;

        switch (combo)
        {
            case "ninzinn+kiru":
                animator.SetTrigger("B2C");
                break;
            case "攻撃+防御":
                animator.SetTrigger("AttackDefendAnim");
                break;
            case "魔法+回復":
                animator.SetTrigger("MagicHealAnim");
                break;
            default:
                animator.SetTrigger("DefaultAnim");
                break;
        }
    }
}
