#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace CyberShield
{
    // Only shrinks the decorative controller mesh in desktop simulation.
    // The tracked controller, rays, grab volumes and Quest build are unchanged.
    public sealed class CyberShieldDesktopVisualComfort : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return null;
            if(!FindFirstObjectByType<XRInteractionSimulator>())yield break;
            var rig=FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if(!rig)yield break;
            foreach(var part in rig.GetComponentsInChildren<Transform>(true))
                if(part.name=="UniversalController")part.localScale*=.58f;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var mail=FindFirstObjectByType<CyberShieldMailInvestigation>();
            if(mail&&!mail.GetComponent<CyberShieldDesktopVisualComfort>())mail.gameObject.AddComponent<CyberShieldDesktopVisualComfort>();
        }
    }
}
#endif
