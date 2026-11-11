using TMPro;
using UnityEngine;

namespace CyberShield
{
    public sealed class CyberShieldBreachAtmosphere : MonoBehaviour
    {
        public CyberShieldGame game;
        public Renderer[] warningStrips, serverLeds;
        public Light[] accentLights;
        public TMP_Text incidentDisplay;
        public GameObject[] compromisedScreens;
        public AudioSource alarm;
        public float lightStrength=2f;
        Color current=new Color(1,.15f,.06f);
        MaterialPropertyBlock block;
        AudioClip alarmClip;
        CyberShieldAtmosphere audioSettings;
        int lastSecond=-1,lastState=-1;
        void Start()
        {
            block=new MaterialPropertyBlock();audioSettings=game.GetComponent<CyberShieldAtmosphere>();
            var data=new float[22050*3];
            for(int i=0;i<data.Length;i++)
            {
                float t=i/22050f;
                float envelope=t<.24f?Mathf.Sin(t/.24f*Mathf.PI):t>.45f&&t<.69f?Mathf.Sin((t-.45f)/.24f*Mathf.PI):0;
                data[i]=Mathf.Sin(2*Mathf.PI*480*t)*envelope*.12f;
            }
            alarmClip=AudioClip.Create("Double signal / incident",data.Length,1,22050,false);alarmClip.SetData(data,0);
            alarm.clip=alarmClip;alarm.loop=true;alarm.volume=.16f;alarm.Play();
        }
        void Update()
        {
            int stage=(int)game.Session.Stage;
            bool contained=stage>=2, complete=stage==3&&!game.IsRestoring;
            Color target=!game.IsStarted?new Color(.04f,.65f,1):complete?new Color(.1f,1,.55f):contained?new Color(.04f,.65f,1):new Color(1,.16f,.05f);
            current=Color.Lerp(current,target,Time.deltaTime*1.4f);
            // Slow light breathing; no flashing or camera motion.
            float breathing=.8f+.2f*Mathf.Sin(Time.time*1.5f);
            block.SetColor("_BaseColor",current*breathing);block.SetColor("_EmissionColor",current*breathing);
            foreach(var r in warningStrips)if(r)r.SetPropertyBlock(block);
            for(int i=0;i<serverLeds.Length;i++)
            {
                Color c=complete?new Color(.08f,.95f,.45f):(i%4==0&&!contained)?current:(Mathf.Sin(Time.time*(2+i%3)+i)>-.3f?new Color(.05f,.8f,1):new Color(.03f,.16f,.21f));
                block.SetColor("_BaseColor",c);serverLeds[i].SetPropertyBlock(block);
            }
            foreach(var light in accentLights){light.color=current;light.intensity=lightStrength*(contained?.5f:breathing*.7f);}
            alarm.mute=!game.IsStarted||contained||(audioSettings&&audioSettings.ambience.mute);
            int second=Mathf.FloorToInt(game.Elapsed);
            if(second!=lastSecond||stage!=lastState)
            {
                lastSecond=second;lastState=stage;
                incidentDisplay.text=$"<color=#{ColorUtility.ToHtmlStringRGB(target)}>{(complete?"SISTEMA RECUPERADO":contained?"AMENAZA CONTENIDA":"INTRUSIÓN DETECTADA")}</color>\n<size=55%>CS-1042    /    {second/60:00}:{second%60:00}    /    {Mathf.Min(stage,3)}/3 OBJETIVOS</size>";
            }
            foreach(var screen in compromisedScreens)screen.SetActive(game.IsStarted&&!contained);
        }
        void OnDestroy(){if(alarmClip)Destroy(alarmClip);}
    }
}
