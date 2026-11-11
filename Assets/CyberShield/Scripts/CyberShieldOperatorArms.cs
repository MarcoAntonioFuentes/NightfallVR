using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace CyberShield
{
    // Cosmetic controller-driven body. No colliders and no changes to tracked poses.
    [DefaultExecutionOrder(100)]
    public sealed class CyberShieldOperatorArms : MonoBehaviour
    {
        public Transform head;
        public Transform[] controllers, wrists, upperArms, forearms, elbows, armRoots;
        public XRBaseInputInteractor[] inputs;
        public Transform[] fingerJoints;
        public int[] jointHands;
        public bool[] indexJoints;
        public CyberShieldGame game;
        public Renderer[] missionLeds;
        public bool showArms=true;
        MaterialPropertyBlock ledBlock;
        Quaternion[] rest;
        Vector3 bodyForward=Vector3.forward;
        readonly float[] grip=new float[2],trigger=new float[2];
        public float Grip(int hand)=>grip[hand];
        void Awake(){rest=new Quaternion[fingerJoints.Length];for(int i=0;i<rest.Length;i++)rest[i]=fingerJoints[i].localRotation;ledBlock=new MaterialPropertyBlock();}
        void LateUpdate()
        {
            var forward=Vector3.ProjectOnPlane(head.forward,Vector3.up);
            if(forward.sqrMagnitude>.05f)bodyForward=Vector3.Slerp(bodyForward,forward.normalized,1-Mathf.Exp(-Time.deltaTime*7));
            var right=Vector3.Cross(Vector3.up,bodyForward).normalized;
            for(int i=0;i<2;i++)
            {
                bool active=controllers[i].gameObject.activeInHierarchy;
                armRoots[i].gameObject.SetActive(active&&showArms);
                if(!active)continue;
                float g=inputs[i]?inputs[i].selectInput.ReadValue():0;
                float t=inputs[i]?inputs[i].activateInput.ReadValue():0;
                grip[i]=Mathf.MoveTowards(grip[i],Mathf.Clamp01(g),Time.deltaTime*8);
                trigger[i]=Mathf.MoveTowards(trigger[i],Mathf.Clamp01(t),Time.deltaTime*12);
            }
            for(int j=0;j<fingerJoints.Length;j++)
            {
                int hand=jointHands[j];float curl=indexJoints[j]?trigger[hand]:grip[hand];
                fingerJoints[j].localRotation=rest[j]*Quaternion.Euler(Mathf.Lerp(8,65,curl),0,0);
            }
            for(int i=0;i<2;i++)
            {
                if(!armRoots[i].gameObject.activeSelf)continue;
                var shoulder=head.position-Vector3.up*.24f-bodyForward*.1f+right*(i==0?-.19f:.19f);
                var wrist=wrists[i].position;var offset=wrist-shoulder;
                if(offset.magnitude>1.3f){armRoots[i].gameObject.SetActive(false);continue;}
                float d=Mathf.Clamp(offset.magnitude,.065f,.575f);var direction=offset.sqrMagnitude>.0001f?offset.normalized:bodyForward;
                var bend=Vector3.ProjectOnPlane(right*(i==0?-.8f:.8f)-Vector3.up*.85f-bodyForward*.12f,direction).normalized;
                const float upper=.31f,lower=.28f;
                float along=(upper*upper-lower*lower+d*d)/(2*d),height=Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
                var elbow=shoulder+direction*along+bend*height;
                Segment(upperArms[i],shoulder,elbow,.095f);Segment(forearms[i],elbow,wrist,.075f);elbows[i].position=elbow;
            }
            if(game&&missionLeds!=null)for(int i=0;i<missionLeds.Length;i++)
            {
                int stage=(int)game.Session.Stage;var color=stage>i?new Color(.05f,1,.45f):game.IsStarted&&stage==i?new Color(1,.35f,.03f):new Color(.03f,.15f,.2f);
                ledBlock.SetColor("_BaseColor",color);missionLeds[i].SetPropertyBlock(ledBlock);
            }
        }
        static void Segment(Transform part,Vector3 a,Vector3 b,float thickness)
        {
            part.position=(a+b)*.5f;part.rotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
            part.localScale=new Vector3(thickness,Vector3.Distance(a,b)*.5f,thickness);
        }
    }
}
