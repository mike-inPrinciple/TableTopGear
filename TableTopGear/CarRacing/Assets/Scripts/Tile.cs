using UnityEngine;

public class Tile : MonoBehaviour {

    public int tileID;
    public TILETYPE tileType = TILETYPE.standard;
    public Vector3 forwardControlPoint;
    public Vector3 backwardControlPoint;
    private MeshRenderer mr;
    private Material[] materials;
    [HideInInspector] public TileObject tileObject;

    public void Setup(int ID, Vector3 f, Vector3 b) {
        tileID = ID;
        forwardControlPoint = f;
        backwardControlPoint = b;
        mr = transform.Find("Model").GetComponent<MeshRenderer>();
        materials = new Material[2];
        AssignMaterials();
    }

    void AssignMaterials() {
        materials[0] = GameManager.script.tileBorderMaterials[(int)tileType];
        materials[1] = GameManager.script.tileCoreMaterials[(int)tileType];
        mr.sharedMaterials = materials;
    }

    public void AddTileObject(TILEOBJECT t, int pID, int value = -1, bool launch = false) {
        tileObject = Instantiate(GameManager.script.tileObjectTemplate, transform.position, GameManager.script.tileObjectTemplate.transform.rotation).GetComponent<TileObject>();
        tileObject.Setup(this, t, pID, value, launch);
        tileObject.transform.parent = transform;
        tileObject.transform.localScale = Vector3.one;
    }

    public void DestroyTileObject() {
        if (tileObject != null) {
            Destroy(tileObject.gameObject);
            tileObject = null;
        }
    }

}
