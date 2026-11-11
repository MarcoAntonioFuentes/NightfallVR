#if UNITY_EDITOR && CYBERSHIELD_EXPERIMENTAL_CLICK_TIMING
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace CyberShield
{
    // Let simulated tracking aim at the new cursor position before pressing the trigger.
    // This only feeds the simulator's existing input reader; XR still performs selection.
    [DefaultExecutionOrder(10000)]
    public sealed class CyberShieldSimulatorClickTiming : MonoBehaviour
    {
        XRInteractionSimulator simulator;
        XRInputButtonReader.InputSourceMode originalMode;
        int downAt=-1, upAt=-1;
        bool pressed, configured;
        void LateUpdate()
        {
            if(!configured)
            {
                simulator=FindFirstObjectByType<XRInteractionSimulator>();
                if(!simulator)return;
                originalMode=simulator.mouseClickInput.inputSourceMode;
                simulator.mouseClickInput.inputSourceMode=XRInputButtonReader.InputSourceMode.ManualValue;
                configured=true;
            }
            var mouse=Mouse.current;if(mouse==null)return;
            int frame=Time.frameCount;
            if(mouse.leftButton.wasPressedThisFrame)
            {
                // Grabbable physics objects need the press immediately, before a drag
                // moves away from them. Only UI needs the extra tracking delay.
                bool ui=false;
                var system=UnityEngine.EventSystems.EventSystem.current;
                if(system)
                {
                    var pointer=new UnityEngine.EventSystems.PointerEventData(system){position=mouse.position.ReadValue()};
                    var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                    system.RaycastAll(pointer,hits);
                    ui=hits.Exists(hit=>hit.module is UnityEngine.UI.GraphicRaycaster);
                }
                downAt=frame+(ui?2:0);upAt=-1;
            }
            if(mouse.leftButton.wasReleasedThisFrame)upAt=Mathf.Max(frame+2,downAt+2);
            if(downAt>=0&&frame>=downAt){pressed=true;downAt=-1;}
            if(upAt>=0&&frame>=upAt){pressed=false;upAt=-1;}
            if(!Application.isFocused){pressed=false;downAt=upAt=-1;}
            simulator.mouseClickInput.QueueManualState(pressed,pressed?1:0);
        }
        void OnDestroy()
        {
            if(configured&&simulator)simulator.mouseClickInput.inputSourceMode=originalMode;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var game=FindFirstObjectByType<CyberShieldGame>();
            if(game&&!game.GetComponent<CyberShieldSimulatorClickTiming>())game.gameObject.AddComponent<CyberShieldSimulatorClickTiming>();
        }
    }
}
#endif
