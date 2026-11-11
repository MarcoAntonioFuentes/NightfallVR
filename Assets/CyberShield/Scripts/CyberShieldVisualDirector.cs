using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyberShield
{
    public sealed class CyberShieldVisualDirector : MonoBehaviour
    {
        public CyberShieldGame game;
        public CyberShieldMailInvestigation mail;
        public CyberShieldTacticalNetwork tactical;
        public CyberShieldReferenceExperience flow;
        public Image[] steps, stepChecks, evidenceChecks, telemetryBars, backupBadges;
        public TMP_Text[] telemetryValues, backupStates;
        public TMP_Text score;
        public Image restoreFill;
        public GameObject restoreMeter;
        public Transform[] floorMarkers;
        public Renderer[] markerRenderers;
        MaterialPropertyBlock block;
        int lastSelection=-2,lastStage=-1;
        CyberShieldForensicProbe probe;
        int lastProbeVersion=-1;
        void Start(){probe=FindFirstObjectByType<CyberShieldForensicProbe>();}
        readonly Color cyan=new Color(.02f,.75f,1), green=new Color(.08f,.85f,.48f), orange=new Color(1,.35f,.04f), dark=new Color(.045f,.12f,.18f);
        void Awake(){block=new MaterialPropertyBlock();}
        void LateUpdate()
        {
            int stage=(int)game.Session.Stage;bool done=stage==3&&!game.IsRestoring;
            flow.hands.readerLabel.text=done?"RECUPERADO":game.IsRestoring?"VALIDANDO":stage==2?"RESTAURAR":"OFFLINE";
            for(int i=0;i<steps.Length;i++)
            {
                steps[i].color=stage>i?green:stage==i&&game.IsStarted?cyan:dark;
                stepChecks[i].gameObject.SetActive(stage>i);
            }
            score.text=$"{game.Session.Score}<size=50%>/100</size>";
            for(int i=0;i<evidenceChecks.Length;i++)evidenceChecks[i].gameObject.SetActive(mail.IsInspected(i));
            int selected=flow.SelectedComputer;
            int version=probe?probe.Version:0;bool revealed=!probe||probe.IsExamined(selected);
            if(selected!=lastSelection||version!=lastProbeVersion)
            {
                lastSelection=selected;lastProbeVersion=version;
                telemetryValues[0].text=selected<0?"—":!revealed?"PROCESO / ?":selected==2?"encrypt-agent.exe":"PROCESO HABITUAL";
                telemetryValues[1].text=selected<0?"—":!revealed?"RED / ?":selected==2?"CONEXIÓN EXTERNA":"RED INTERNA";
                telemetryValues[2].text=selected<0?"—":!revealed?"ARCHIVOS / ?":selected==2?"12 MODIFICADOS":"SIN CAMBIOS";
            }
            for(int i=0;i<telemetryBars.Length;i++)
            {
                float target=selected<0||!revealed?0:selected==2?.95f:.16f+i*.05f;
                telemetryBars[i].fillAmount=Mathf.MoveTowards(telemetryBars[i].fillAmount,target,Time.deltaTime*1.5f);
                telemetryBars[i].color=selected==2?orange:cyan;
            }
            for(int i=0;i<backupBadges.Length;i++)
            {
                bool scanned=game.corporateScenario.IsScanned(i);
                backupBadges[i].color=!scanned?dark:i==0?green:orange;
                backupStates[i].text=!scanned?"SIN ANALIZAR":i==0?"LIMPIO":"NO SEGURO";
            }
            restoreMeter.SetActive(game.IsRestoring||done);
            restoreFill.fillAmount=game.IsRestoring?game.RestorationProgress:done?1:0;
            restoreFill.color=done?green:cyan;
            if(stage!=lastStage)
            {
                lastStage=stage;
                Vector3 target=stage==0?new Vector3(-2.3f,.025f,.8f):stage==1?new Vector3(0,.025f,.8f):new Vector3(2.5f,.025f,.8f);
                Vector3 start=new Vector3(0,.025f,-2.25f);
                for(int i=0;i<floorMarkers.Length;i++)
                {
                    var p=Vector3.Lerp(start,target,(i+1f)/(floorMarkers.Length+1));floorMarkers[i].position=p;
                    floorMarkers[i].rotation=Quaternion.LookRotation(target-start);
                }
            }
            for(int i=0;i<floorMarkers.Length;i++)
            {
                floorMarkers[i].gameObject.SetActive(game.IsStarted&&!done);
                float pulse=.35f+.65f*Mathf.Pow(.5f+.5f*Mathf.Sin(Time.time*2-i*.65f),3);
                block.SetColor("_BaseColor",cyan*pulse);markerRenderers[i].SetPropertyBlock(block);
            }
        }
    }
}
