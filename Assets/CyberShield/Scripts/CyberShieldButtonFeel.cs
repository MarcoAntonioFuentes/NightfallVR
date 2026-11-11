using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyberShield
{
    public sealed class CyberShieldButtonFeel : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
    {
        public CyberShieldPanelArt outline;
        Button button;
        Vector3 restingScale;
        bool hover,pressed;
        void Awake(){button=GetComponent<Button>();restingScale=transform.localScale;}
        public void OnPointerEnter(PointerEventData e){hover=true;}
        public void OnPointerExit(PointerEventData e){hover=pressed=false;}
        public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)pressed=true;}
        public void OnPointerUp(PointerEventData e){pressed=false;}
        void Update()
        {
            bool active=button&&button.IsInteractable();float scale=active&&pressed?.985f:active&&hover?1.025f:1;
            transform.localScale=Vector3.Lerp(transform.localScale,restingScale*scale,1-Mathf.Exp(-Time.unscaledDeltaTime*16));
            outline.color=active&&hover?(pressed?new Color(1,.5f,.1f):new Color(.15f,.9f,1)):new Color(.06f,.35f,.48f,.45f);
        }
        void OnDisable(){hover=pressed=false;if(restingScale!=Vector3.zero)transform.localScale=restingScale;}
    }
}
