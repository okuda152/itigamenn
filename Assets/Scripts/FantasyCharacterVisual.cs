using UnityEngine;

/// <summary>
/// Blackthornprod 100 Fantasy Characters Pack のキャラを実行時に読み込むラッパー。
/// Resources/Characters/ 以下に配置したプレハブを Init() で指定する。
/// </summary>
public class FantasyCharacterVisual : MonoBehaviour
{
    Animator         anim;
    SpriteRenderer[] renderers;

    // ---- 初期化 ----

    public void Init(string resourcePath, float scale = 1f, bool flipX = false)
    {
        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[FantasyCharacterVisual] Resources/{resourcePath} が見つかりません");
            return;
        }
        var inst = Instantiate(prefab, transform);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localScale    = new Vector3(flipX ? -scale : scale, scale, scale);

        anim      = inst.GetComponentInChildren<Animator>();
        renderers = inst.GetComponentsInChildren<SpriteRenderer>(true);
    }

    // ---- 向き ----

    public bool FacingRight
    {
        set
        {
            float sx = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(value ? sx : -sx,
                                               transform.localScale.y,
                                               transform.localScale.z);
        }
    }

    // ---- アニメーション ----

    public bool IsMoving
    {
        set { if (anim) anim.SetBool("isMoving", value); }
    }

    public void TriggerAttack()
    {
        if (anim) anim.SetTrigger("attack");
    }

    // ---- ヒットフラッシュ ----

    public void SetColor(Color c)
    {
        if (renderers == null) return;
        foreach (var r in renderers) r.color = c;
    }
}
