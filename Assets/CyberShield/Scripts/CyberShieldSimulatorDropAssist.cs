#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CyberShield
{
    // Desktop testing aid only. Quest builds retain normal physical placement.
    public sealed class CyberShieldSimulatorDropAssist : MonoBehaviour
    {
        XRGrabInteractable card;
        CyberShieldHandsOn hands;

        void Start()
        {
            card = GetComponent<XRGrabInteractable>();
            hands = FindFirstObjectByType<CyberShieldHandsOn>();
            card.selectExited.AddListener(OnReleased);
        }

        void OnReleased(SelectExitEventArgs args)
        {
            if (args.isCanceled || !hands || Mouse.current == null || !Camera.main ||
                !FindFirstObjectByType<XRInteractionSimulator>()) return;
            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Transform scan = hands.game.corporateScenario ? hands.game.corporateScenario.scanZone : null;
            if (!PlaceOnTray(ray, scan)) PlaceOnTray(ray, hands.reader);
        }

        bool PlaceOnTray(Ray ray, Transform tray)
        {
            if (!tray) return false;
            var plane = new Plane(Vector3.up, tray.position);
            if (!plane.Raycast(ray, out float distance) || distance > 5f) return false;
            Vector3 local = tray.InverseTransformPoint(ray.GetPoint(distance));
            if (Mathf.Abs(local.x) > .26f || Mathf.Abs(local.z) > .26f) return false;
            StartCoroutine(SettleOnTray(tray.position + Vector3.up * .10f));
            return true;
        }

        System.Collections.IEnumerator SettleOnTray(Vector3 position)
        {
            // XR completes detach at the end of the frame. Place after that step.
            yield return new WaitForFixedUpdate();
            if(card.isSelected)yield break;
            var body = GetComponent<Rigidbody>();
            body.position = position;
            body.rotation = Quaternion.identity;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        void OnDestroy()
        {
            if (card) card.selectExited.RemoveListener(OnReleased);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var hands = FindFirstObjectByType<CyberShieldHandsOn>();
            if (!hands) return;
            foreach (var card in hands.cards)
                if (!card.GetComponent<CyberShieldSimulatorDropAssist>())
                    card.gameObject.AddComponent<CyberShieldSimulatorDropAssist>();
        }
    }
}
#endif
