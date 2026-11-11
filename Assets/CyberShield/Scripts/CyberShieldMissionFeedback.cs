using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace CyberShield
{
    [DefaultExecutionOrder(-40)]
    public sealed class CyberShieldMissionFeedback : MonoBehaviour
    {
        public CyberShieldGame game;
        public CyberShieldSoundDirector sound;
        public CyberShieldBreachAtmosphere breach;
        public Transform head;
        public AudioSource radio;
        public AudioClip[] lines;
        public GameObject card;
        public CanvasGroup cardFade;
        public CyberShieldGlyph symbol;
        public Image accent;
        public TMP_Text speaker, caption, gain, radioLabel;
        public LineRenderer[] recoveryRings;
        public HapticImpulsePlayer[] haptics;
        public bool voiceEnabled=true;
        public int VoiceCues {get;private set;}
        public int FeedbackCues {get;private set;}
        int lastStage,lastMistakes;
        bool wasStarted,won;
        CyberShieldAtmosphere settings;
        float until,shownAt,lastMistakeVoice=-100;
        static readonly string[] captions={"INTRUSIÓN DETECTADA","FRAUDE CONFIRMADO","ATAQUE CONTENIDO","COPIA VALIDADA","EMPRESA PROTEGIDA","REVISA LAS PISTAS"};
        void Start(){settings=game.GetComponent<CyberShieldAtmosphere>();game.MissionReset+=Reset;card.SetActive(false);}
        void Reset(){lastStage=lastMistakes=0;wasStarted=won=false;until=0;lastMistakeVoice=-100;radio.Stop();card.SetActive(false);sound.musicDucking=1;}
        public void ToggleRadio(){voiceEnabled=!voiceEnabled;if(!voiceEnabled)radio.Stop();}
        void Present(int cue,int points)
        {
            bool caution=cue==0||cue==5;var color=caution?new Color(1,.35f,.05f):new Color(.08f,.9f,.55f);
            caption.text=captions[cue];gain.text=points==0?"":points>0?$"+{points}":points.ToString();speaker.text="MARTA / TI";
            symbol.symbol=cue==0||cue==5?10:cue==1?0:cue==2?8:cue==3?6:12;symbol.color=accent.color=color;symbol.SetVerticesDirty();
            Vector3 forward=Vector3.ProjectOnPlane(head.forward,Vector3.up).normalized;if(forward.sqrMagnitude<.1f)forward=Vector3.forward;Vector3 right=Vector3.Cross(Vector3.up,forward);
            card.transform.position=head.position+forward*2+Vector3.up*.68f-right*.8f;card.transform.rotation=Quaternion.LookRotation(forward,Vector3.up);shownAt=Time.time;until=Time.time+2.2f;FeedbackCues++;
            if(voiceEnabled&&(cue!=5||Time.time-lastMistakeVoice>10))
            {
                radio.Stop();radio.clip=lines[cue];radio.Play();VoiceCues++;if(cue==5)lastMistakeVoice=Time.time;
            }
            // Physical haptics are sent only when the XR runtime reports a device.
            for(int i=0;i<haptics.Length&&i<2;i++)if(haptics[i]&&InputDevices.GetDeviceAtXRNode(i==0?XRNode.LeftHand:XRNode.RightHand).isValid)haptics[i].SendHapticImpulse(cue==5?.08f:.16f,.07f);
        }
        void Update()
        {
            int stage=(int)game.Session.Stage;
            if(game.IsStarted&&!wasStarted)Present(0,0);
            if(stage!=lastStage)
            {
                if(stage==1)Present(1,30);else if(stage==2)Present(2,30);else if(stage==3&&game.IsRestoring)Present(3,40);
            }
            bool complete=game.IsStarted&&stage==3&&!game.IsRestoring;
            if(complete&&!won){Present(4,0);won=true;}
            if(game.Session.Mistakes>lastMistakes)Present(5,-10);
            lastStage=stage;lastMistakes=game.Session.Mistakes;wasStarted=game.IsStarted;
            bool muted=settings.ambience.mute;radio.mute=muted;
            sound.musicDucking=radio.isPlaying?.25f:1;breach.alarm.volume=radio.isPlaying?.045f:.16f;
            radioLabel.text=voiceEnabled?"RADIO: SÍ":"RADIO: NO";
            bool showing=Time.time<until&&game.IsStarted;card.SetActive(showing);
            if(showing){float age=Time.time-shownAt;cardFade.alpha=Mathf.Min(Mathf.Clamp01(age/.16f),Mathf.Clamp01((until-Time.time)/.25f));float ease=1-Mathf.Exp(-age*14);card.transform.localScale=Vector3.one*(.0008f*(.94f+.06f*ease));}
            for(int i=0;i<recoveryRings.Length;i++)
            {
                var ring=recoveryRings[i];ring.enabled=game.IsRestoring;if(!ring.enabled)continue;
                float y=.25f+game.RestorationProgress*2.3f;
                for(int p=0;p<49;p++){float a=p/48f*Mathf.PI*2;ring.SetPosition(p,new Vector3((i==0?-4.08f:4.08f)+Mathf.Cos(a)*.5f,y,2.8f+Mathf.Sin(a)*.53f));}
            }
        }
        void OnDisable(){if(radio)radio.Stop();if(sound)sound.musicDucking=1;if(breach&&breach.alarm)breach.alarm.volume=.16f;}
        void OnDestroy(){if(game)game.MissionReset-=Reset;}
    }
}
