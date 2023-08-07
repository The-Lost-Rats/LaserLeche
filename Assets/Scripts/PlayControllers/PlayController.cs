using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : MonoBehaviour
{
    public static PlayController instance = null;
    private List<ParallaxObject> parallaxObjects;
    [SerializeField] private Transform sceneObjectsParent;
    [SerializeField] private PlayerController playerController;

    [SerializeField]
    [Range(0.0f, 2.0f)]
    private float distanceScalar = 1.0f;

    [SerializeField] private GameObject ufoPrefab;

    public void Awake() {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }

    protected void Start()
    {
        parallaxObjects = new List<ParallaxObject>(GameObject.FindObjectsOfType<ParallaxObject>());
    }

	protected void FixedUpdate()
    {
        for (int i = 0; i < parallaxObjects.Count; i++)
        {
            ParallaxObject parallaxObject = parallaxObjects[i];
            if (parallaxObject == null)
            {
                parallaxObjects.RemoveAt(i);
                i--;
                continue;
            }
            Vector3 playObjPos = parallaxObject.transform.position;
            playObjPos.x -= playerController.PlayerVelX / (1 + (distanceScalar * parallaxObject.parallaxDistanceFromForeground));
            if (parallaxObject.looping)
            {
                if (playObjPos.x >= 16)
                    playObjPos.x -= 16;
                else if (playObjPos.x <= -16)
                    playObjPos.x += 16;
            }
            parallaxObject.transform.position = playObjPos;
        }

        // TODO Debug code
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameObject ufo = Instantiate(ufoPrefab, new Vector3(0, 5), Quaternion.identity, sceneObjectsParent);
            parallaxObjects.Add(ufo.GetComponent<ParallaxObject>());
        }
	}
}
