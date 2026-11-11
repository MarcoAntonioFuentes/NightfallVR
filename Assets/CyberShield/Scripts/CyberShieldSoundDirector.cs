using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace CyberShield
{
    public sealed class CyberShieldSoundDirector : MonoBehaviour
    {
        public CyberShieldGame game;
        public CyberShieldHandsOn hands;
        public AudioClip click, grab, drop, scan, contain, victory;
        public AudioSource ui, scoreBed;
        public AudioSource[] cardSources, fans;
        public Button[] buttons;
        public HapticImpulsePlayer[] haptics;
        bool[] scanned=new bool[3];
        CyberShieldAtmosphere settings;
        int previousStage;
        bool completed;
        public int CueCount {get;private set;}
        public float musicDucking=1;
        void Start()
        {
            settings=game.GetComponent<CyberShieldAtmosphere>();previousStage=(int)game.Session.Stage;
            foreach(var button in buttons)button.onClick.AddListener(Click);
            for(int i=0;i<hands.cards.Length;i++){hands.cards[i].selectEntered.AddListener(OnGrab);hands.cards[i].selectExited.AddListener(OnDrop);}
            game.MissionReset+=Reset;scoreBed.Play();foreach(var fan in fans)fan.Play();
        }
        void Reset(){for(int i=0;i<3;i++)scanned[i]=false;completed=false;previousStage=0;}
        void Click(){if(ui&&click){ui.PlayOneShot(click,.22f);CueCount++;}}
        void OnGrab(SelectEnterEventArgs args)
        {
            if(args.interactableObject==null)return;
            var t=args.interactableObject.transform;for(int i=0;i<hands.cards.Length;i++)if(hands.cards[i].transform==t){cardSources[i].PlayOneShot(grab,.42f);CueCount++;break;}
            if(args.interactorObject!=null)foreach(var h in haptics)if(h&&args.interactorObject.transform.IsChildOf(h.transform)){h.SendHapticImpulse(.15f,.05f);break;}
        }
        void OnDrop(SelectExitEventArgs args)
        {
            if(args.isCanceled||args.interactableObject==null)return;
            for(int i=0;i<hands.cards.Length;i++)if(hands.cards[i].transform==args.interactableObject.transform){cardSources[i].PlayOneShot(drop,.3f);CueCount++;break;}
        }
        void Update()
        {
            bool mute=settings&&settings.ambience.mute;ui.mute=scoreBed.mute=mute;
            foreach(var s in cardSources)s.mute=mute;foreach(var fan in fans)fan.mute=mute;
            int stage=(int)game.Session.Stage;
            float target=game.IsStarted&&stage<3?(stage<2?.055f:.025f):0;
            scoreBed.volume=Mathf.MoveTowards(scoreBed.volume,target*Mathf.Clamp01(musicDucking),Time.deltaTime*.15f);
            scoreBed.pitch=stage<2?1:.9f;
            for(int i=0;i<3;i++)if(game.corporateScenario.IsScanned(i)&&!scanned[i]){scanned[i]=true;cardSources[i].PlayOneShot(scan,.38f);CueCount++;}
            if(stage>=2&&previousStage<2){ui.PlayOneShot(contain,.35f);CueCount++;}
            previousStage=stage;
            if(game.IsStarted&&stage==3&&!game.IsRestoring&&!completed){completed=true;ui.PlayOneShot(victory,.32f);CueCount++;}
        }
        void OnDestroy()
        {
            if(game)game.MissionReset-=Reset;
            foreach(var button in buttons)if(button)button.onClick.RemoveListener(Click);
            foreach(var card in hands.cards)if(card){card.selectEntered.RemoveListener(OnGrab);card.selectExited.RemoveListener(OnDrop);}
        }
    }
}
