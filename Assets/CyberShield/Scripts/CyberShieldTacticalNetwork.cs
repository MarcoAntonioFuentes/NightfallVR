using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CyberShield
{
    // Scene-owned, pooled geometry: no per-frame instantiation or camera movement.
    public sealed class CyberShieldTacticalNetwork : MonoBehaviour
    {
        public CyberShieldReferenceExperience flow;
        public GameObject map, details;
        public TMP_Text heading, activity, response, banner;
        public Renderer[] screens, pedestals, packets;
        public Transform server;
        public LineRenderer[] connections;
        public XRSimpleInteractable[] computers;
        public Image[] selectionFrames;
        readonly string[] names={"PC-01 / VENTAS","PC-03 / DISEÑO","PC-02 / CONTABILIDAD","PC-04 / RR. HH.","PC-05 / FINANZAS","PC-06 / SOPORTE","PC-07 / DIRECCIÓN","PC-08 / ARCHIVO"};
        MaterialPropertyBlock block;
        CyberShieldForensicProbe probe;
        int previousMistakes, lastSelection=-2, lastStage=-1;
        float responseUntil;
        Color cyan=new Color(.03f,.7f,1), green=new Color(.1f,1,.5f), red=new Color(1,.16f,.035f);
        void Start()
        {
            block=new MaterialPropertyBlock();
            probe=FindFirstObjectByType<CyberShieldForensicProbe>();
            for(int i=0;i<computers.Length;i++){int index=i;computers[i].selectEntered.AddListener(_=>Select(index));}
            flow.game.MissionReset+=Reset;
        }
        void Reset(){lastSelection=-2;lastStage=-1;previousMistakes=0;responseUntil=0;}
        public void Select(int index)
        {
            flow.SelectComputer(index);
            if(flow.SelectedComputer==index&&flow.game.Session.Stage==Challenge.Isolation)ShowComputer(index);
        }
        void ShowComputer(int index)
        {
            if(index<0){heading.text="INVESTIGA LA RED";activity.text="Señala un equipo.\n\nCompara su proceso, conexión y archivos.";return;}
            heading.text=names[index];
            activity.text=index==2?"<color=#FF743A>ACTIVIDAD SOSPECHOSA</color>\n\nPROCESO    encrypt-agent.exe\nCONEXIÓN   EXTERNA / NO AUTORIZADA\nARCHIVOS   12 MODIFICADOS\n\nCoincide con el inicio del incidente.":"<color=#40EDC0>ACTIVIDAD HABITUAL</color>\n\nPROCESO    "+new[]{"crm.exe","design.exe","","payroll.exe","ledger.exe","helpdesk.exe","dashboard.exe","archive.exe"}[index]+"\nCONEXIÓN   INTERNA / AUTORIZADA\nARCHIVOS   SIN CAMBIOS ANÓMALOS\n\nNo coincide con el incidente.";
        }
        void Update()
        {
            var game=flow.game;int stage=(int)game.Session.Stage;bool active=game.IsStarted&&stage==1;
            map.SetActive(active);details.SetActive(active);
            int state=stage+(game.IsRestoring?10:0)+(game.IsStarted?100:0);
            if(state!=lastStage){lastStage=state;banner.color=!game.IsStarted?cyan:stage<2?red:stage==3&&!game.IsRestoring?green:cyan;banner.text=!game.IsStarted?"CYBERSHIELD VR  /  CENTRO DE OPERACIONES":stage==0?"01  /  ALERTA · ENCUENTRA LA PISTA":stage==1?"02  /  CONTÉN LA PROPAGACIÓN":stage==2?"03  /  RECUPERA UNA COPIA LIMPIA":game.IsRestoring?"VALIDANDO LA INTEGRIDAD…":"MISIÓN COMPLETADA · RED PROTEGIDA";}
            int selected=flow.SelectedComputer;
            if(lastSelection!=selected){lastSelection=selected;ShowComputer(selected);}
            if(game.Session.Mistakes>previousMistakes){response.text="EQUIPO INCORRECTO · INTÉNTALO OTRA VEZ\nBusca la conexión externa y los archivos modificados.";responseUntil=Time.time+5;}
            previousMistakes=game.Session.Mistakes;
            response.gameObject.SetActive(active&&Time.time<responseUntil);
            if(!active)return;
            for(int i=0;i<screens.Length;i++)
            {
                Color c=i==2&&(!probe||probe.IsExamined(i))?red:cyan;Tint(screens[i],c*(.85f+.15f*Mathf.Sin(Time.time*2+i)));
                Tint(pedestals[i],i==selected?new Color(1,.65f,.06f):c*.4f);
                selectionFrames[i].color=i==selected?new Color(.12f,.36f,.46f):new Color(.025f,.09f,.15f);
                connections[i].startColor=connections[i].endColor=c*.65f;
                packets[i].transform.position=Vector3.Lerp(server.position,computers[i].transform.position+Vector3.up*.1f,Mathf.Repeat(Time.time*.35f+i*.123f,1));Tint(packets[i],c);
            }
        }
        void Tint(Renderer r,Color color){block.SetColor("_BaseColor",color);r.SetPropertyBlock(block);}
        void OnDestroy(){if(flow&&flow.game)flow.game.MissionReset-=Reset;}
    }
}
