using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyberShield
{
    // Entirely scene-local incident theatre: no filesystem, network or OS actions.
    public sealed class CyberShieldCompanyAttack : MonoBehaviour
    {
        public CyberShieldGame game;
        public TMP_Text headline, counter, eventLog;
        public CyberShieldGlyph headlineSymbol;
        public TMP_Text[] officeStatus;
        public CyberShieldGlyph[] officeSymbols;
        public CyberShieldGlyph[] fileSymbols;
        public Image[] officePanels, fileCells, serviceBars;
        public LineRenderer outbound, containmentWave;
        public Transform[] outboundPackets;
        public AudioSource impact, traffic;
        public AudioClip impactClip, recoveryClip;
        public float VisualPressure {get;private set;}
        public int LockedFiles {get;private set;}
        public bool OutboundActive=>outbound.enabled;
        float pressure=.18f,frozen=.18f,waveStart=-10,uiAt;
        int previousStage,lastState=-99;
        bool started;
        CyberShieldAtmosphere settings;
        static readonly Color Red=new Color(1,.16f,.035f),Cyan=new Color(.025f,.75f,1),Green=new Color(.05f,.9f,.45f),Idle=new Color(.04f,.12f,.18f);
        void Start(){settings=game.GetComponent<CyberShieldAtmosphere>();game.MissionReset+=Reset;traffic.Play();}
        void Reset(){pressure=frozen=.18f;VisualPressure=0;LockedFiles=0;previousStage=0;lastState=-99;started=false;waveStart=-10;traffic.volume=0;impact.Stop();}
        void Update()
        {
            int stage=(int)game.Session.Stage;bool active=game.IsStarted,contained=stage>=2,done=stage==3&&!game.IsRestoring;
            bool mute=settings&&settings.ambience.mute;impact.mute=traffic.mute=mute;
            if(active&&!started){started=true;impact.PlayOneShot(impactClip,.65f);}
            if(active&&!contained)pressure=Mathf.Min(.9f,pressure+Time.deltaTime*.0045f);
            if(stage>=2&&previousStage<2){frozen=pressure;waveStart=Time.time;}
            if(done&&previousStage!=3)waveStart=Time.time;
            if(done&&lastState!=103){waveStart=Time.time;impact.PlayOneShot(recoveryClip,.5f);}
            previousStage=stage;
            VisualPressure=!active||done?0:game.IsRestoring?frozen*(1-game.RestorationProgress):contained?frozen:pressure;
            LockedFiles=Mathf.Clamp(Mathf.FloorToInt(VisualPressure*fileCells.Length),0,fileCells.Length);
            bool leaking=active&&!contained;outbound.enabled=leaking;
            for(int i=0;i<outboundPackets.Length;i++)
            {
                var packet=outboundPackets[i];packet.gameObject.SetActive(leaking);if(!leaking)continue;
                float p=Mathf.Repeat(Time.time*.22f+i/(float)outboundPackets.Length,1);
                packet.position=p<.5f?Vector3.Lerp(outbound.GetPosition(0),outbound.GetPosition(1),p*2):Vector3.Lerp(outbound.GetPosition(1),outbound.GetPosition(2),(p-.5f)*2);
            }
            traffic.volume=Mathf.MoveTowards(traffic.volume,leaking?.025f+VisualPressure*.045f:0,Time.deltaTime*.12f);
            float age=Time.time-waveStart;containmentWave.enabled=age>=0&&age<1.8f;
            if(containmentWave.enabled)for(int i=0;i<65;i++){float a=i/64f*Mathf.PI*2;float radius=.3f+age*3;containmentWave.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,.035f,Mathf.Sin(a)*radius));}
            int state=!active?-1:done?103:game.IsRestoring?13:stage;
            if(Time.time<uiAt&&state==lastState)return;uiAt=Time.time+.15f;
            var color=!active?Cyan:done?Green:contained?Cyan:Red;
            if(state!=lastState)
            {
                lastState=state;
                headline.text=!active?"NEXORA / SISTEMAS EN LÍNEA":done?"NEXORA / OPERACIÓN RESTABLECIDA":game.IsRestoring?"NEXORA / RECUPERANDO SISTEMAS":contained?"NEXORA / AMENAZA CONTENIDA":"NEXORA / INTRUSIÓN DETECTADA";
                headline.color=color;
                if(headlineSymbol){headlineSymbol.symbol=!active||done?8:contained?5:10;headlineSymbol.color=color;headlineSymbol.SetVerticesDirty();}
                eventLog.text=!active?"CS-1042 / MONITORIZACIÓN":done?"RED PROTEGIDA\nRESPALDO VALIDADO\nSERVICIOS RECUPERADOS":game.IsRestoring?"COPIA OFFLINE\nVALIDANDO INTEGRIDAD\nRECUPERANDO ARCHIVOS":contained?"PC-02 / AISLADO\nCONEXIÓN EXTERNA / CORTADA\nEVIDENCIAS / CONSERVADAS":"PC-02 / SESIÓN ANÓMALA\nCONEXIÓN EXTERNA / ACTIVA\nARCHIVOS DE RED / BLOQUEADOS";
                for(int i=0;i<officeStatus.Length;i++)
                {
                    officeStatus[i].text=!active||done?"EN LÍNEA":contained?game.IsRestoring?"RECUPERANDO":"CONEXIÓN CONTENIDA":"ARCHIVOS INACCESIBLES";
                    officeSymbols[i].symbol=!active||done?8:contained?5:11;officeSymbols[i].color=color;officeSymbols[i].SetVerticesDirty();officePanels[i].color=leaking?new Color(.16f,.025f,.018f):new Color(.015f,.055f,.08f);
                }
            }
            counter.text=$"ARCHIVOS AFECTADOS  {LockedFiles:00}/{fileCells.Length}";counter.color=color;
            for(int i=0;i<fileCells.Length;i++){bool locked=i<LockedFiles;fileCells[i].color=locked?Red:done?Green:Idle;int symbol=locked?11:6;if(fileSymbols[i].symbol!=symbol){fileSymbols[i].symbol=symbol;fileSymbols[i].SetVerticesDirty();}}
            for(int i=0;i<serviceBars.Length;i++){serviceBars[i].fillAmount=!active||done?1:1-Mathf.Clamp01(VisualPressure*(.8f+i*.25f));serviceBars[i].color=done?Green:leaking?Red:Cyan;}
        }
        void OnDestroy(){if(game)game.MissionReset-=Reset;}
    }
}
