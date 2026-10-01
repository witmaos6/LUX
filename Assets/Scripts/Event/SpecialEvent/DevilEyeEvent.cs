using System.Collections;
using UnityEngine;

public class DevilEyeEvent : MonoBehaviour // To do: 이벤트 저장상태 확인
{
    [SerializeField] private GameObject jumpScareObject;
    [SerializeField] private AudioClip jumpScareSound;
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private float destroyTime = 0.3f;

    private GameObject jumpScareInstance;
    private bool activateOn = false;

    private void Start()
    {
        GameEventManager.Subscribe(gameEvent, Activate);
    }

    public void Activate()
    {
        if (activateOn)
            return;

        activateOn = true;
        if (jumpScareSound != null)
        {
            AudioSource.PlayClipAtPoint(jumpScareSound, gameObject.transform.position);
        }

        if (jumpScareObject != null)
        {
            jumpScareInstance = Instantiate(jumpScareObject, transform);

            StartCoroutine(EndEvent());
        }
    }

    IEnumerator EndEvent()
    {
        yield return new WaitForSeconds(destroyTime);

        if(jumpScareInstance)
        {
            Destroy(jumpScareInstance);
        }
        Destroy(gameObject);
    }
}
