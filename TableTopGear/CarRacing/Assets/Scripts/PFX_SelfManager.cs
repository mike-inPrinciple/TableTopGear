using System.Collections;
using UnityEngine;

public class PFX_SelfManager : MonoBehaviour {

    private bool setup;

    private ParticleSystem ps;
    private TMPro.TextMeshPro tm;

    [SerializeField] private float duration = 3f;
    [SerializeField] private AnimationCurve curve_MoveText;
    [SerializeField] private AnimationCurve curve_ColorText;

    private Color[] colors;
    [SerializeField] private Vector3[] positions;
    [SerializeField] private Vector3[] scales;

    public void Setup(int effect, int pID = -1, string specificCaption = "", int effectID = 0) {
        colors = new Color[2];
        ps = GetComponent<ParticleSystem>();
        if (ps != null) {
            ps.Play();
        }
        tm = transform.Find("Caption").GetComponent<TMPro.TextMeshPro>();
        if (specificCaption == "") {
            tm.SetText(GameManager.script.pfx_Names[effect]);
        } else {
            tm.SetText(specificCaption);
        }
        tm.color = (pID < 0 ? Color.white : GameManager.script.playerColors[pID]);
        colors[0] = tm.color;
        colors[1] = new Color(colors[0].r, colors[0].g, colors[0].b, 0f);
        for (int i = 0; i < 2; i++) {
            positions[i].y += (float)effectID * positions[i].y;
        }
        tm.transform.localPosition = positions[0];
        tm.transform.localScale = scales[0];
        setup = true;
        StartCoroutine(Execute());
    }

    IEnumerator Execute() {
        float passingTime = 0f;
        while (passingTime <= 1f) {
            passingTime += Time.deltaTime / duration;
            tm.transform.localPosition = Vector3.Lerp(positions[0], positions[1], curve_MoveText.Evaluate(passingTime));
            tm.transform.localScale = Vector3.Lerp(scales[0], scales[1], curve_MoveText.Evaluate(passingTime));
            tm.color = Color.Lerp(colors[0], colors[1], curve_ColorText.Evaluate(passingTime));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        if (ps != null) {
            while (ps.isPlaying) {
                yield return null;
            }
        }
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

}
