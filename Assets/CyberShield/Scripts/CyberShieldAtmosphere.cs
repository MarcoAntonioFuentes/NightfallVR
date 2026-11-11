using TMPro;
using UnityEngine;

namespace CyberShield
{
    public sealed class CyberShieldAtmosphere : MonoBehaviour
    {
        public CyberShieldGame game;
        public GameObject briefing, debrief;
        public TMP_Text telemetry, result, networkStatus, restoreStatus;
        public UnityEngine.UI.Image progressFill;
        public Renderer[] nodes;
        public LineRenderer[] links;
        public Transform hologram;
        public AudioSource ambience;
        public UnityEngine.UI.Button muteButton;
        public TMP_Text muteLabel;
        AudioClip hum;
        MaterialPropertyBlock block;
        static readonly Color Cyan = new Color(0.05f, 0.8f, 1f);
        static readonly Color Red = new Color(1f, 0.18f, 0.08f);
        static readonly Color Green = new Color(0.12f, 1f, 0.55f);
        bool muted;
        int lastSecond = -1;
        int lastState = -1;

        void Start()
        {
            block = new MaterialPropertyBlock();
            var samples = new float[22050 * 2];
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/22050;
                samples[i]=0.06f*(Mathf.Sin(t*2*Mathf.PI*55)+0.35f*Mathf.Sin(t*2*Mathf.PI*110));
            }
            hum=AudioClip.Create("Ventilación del centro de datos",samples.Length,1,22050,false);
            hum.SetData(samples,0);
            ambience.clip=hum;ambience.loop=true;ambience.volume=0.15f;ambience.Play();
            if(muteButton) muteButton.onClick.AddListener(ToggleAudio);
        }

        public void ToggleAudio()
        {
            muted=!muted;
            ambience.mute=muted;
            game.speaker.mute=muted;
            if(muteLabel) muteLabel.text=muted?"AUDIO: SILENCIADO":"AUDIO: ACTIVADO";
        }

        void Update()
        {
            if (block == null) block = new MaterialPropertyBlock();
            int stage=(int)game.Session.Stage;
            bool done=game.IsStarted && stage==3 && !game.IsRestoring;
            briefing.SetActive(!game.IsStarted);
            debrief.SetActive(done);
            if(hologram) hologram.Rotate(0,12f*Time.deltaTime,0,Space.Self);
            int second=Mathf.FloorToInt(game.Elapsed);
            int state=stage+(game.IsRestoring?10:0)+(game.IsStarted?100:0);
            if(second!=lastSecond || state!=lastState)
            {
                lastSecond=second;lastState=state;
                telemetry.text=$"OPERACIÓN BLACKOUT     /     {second/60:00}:{second%60:00}\n{(done?"AMENAZA CONTENIDA":game.IsStarted?"RESPUESTA EN CURSO":"ESPERANDO OPERADOR")}     •     {Mathf.Min(stage,3)}/3 OBJETIVOS";
                networkStatus.text=stage<2?"PC-02  /  COMPROMETIDO\nTRÁFICO SOSPECHOSO DETECTADO":stage==2?"PC-02  /  EN CUARENTENA\nRED DESCONECTADA · EVIDENCIAS A SALVO":"PC-02  /  RECUPERACIÓN\nVALIDACIÓN EN ENTORNO AISLADO";
                if(done)
                {
                    string rank=game.Session.Mistakes==0?"S / CENTINELA":game.Session.Score>=70?"A / DEFENSOR":"B / APRENDIZ";
                    result.text=$"<size=54><color=#69FFD0>{rank}</color></size>\n\n{game.Session.Score}/100 PUNTOS    •    {second/60:00}:{second%60:00}\n{game.Session.Mistakes} errores    •    Récord local: {game.BestScore}\n\n01  Identificaste la suplantación de identidad.\n02  Contuviste el incidente sin borrar evidencias.\n03  Restauraste una copia verificada y anterior.\n\n<color=#81B8CA>Antes de reconectar: valida el equipo y documenta el incidente.</color>";
                }
            }
            if(progressFill) progressFill.fillAmount=game.IsRestoring?game.RestorationProgress:stage==3?1:0;
            if(restoreStatus) restoreStatus.text=game.IsRestoring?$"RESTAURANDO  {game.RestorationProgress*100:00}%":stage==3?"INTEGRIDAD VERIFICADA":stage==2?"RESPALDO LIMPIO REQUERIDO":"REPOSITORIO OFFLINE · EN ESPERA";
            for(int i=0;i<nodes.Length;i++)
            {
                Color c=i==1?(stage<2?Red:stage==2?new Color(1f,0.65f,0.15f):Green):Cyan;
                float pulse=0.8f+0.2f*Mathf.Sin(Time.time*1.8f+i);
                block.SetColor("_BaseColor",c*pulse);
                block.SetColor("_EmissionColor",c*pulse);
                nodes[i].SetPropertyBlock(block);
                if(i<links.Length)
                {
                    links[i].enabled=!(i==1 && stage>=2);
                    links[i].startColor=c;links[i].endColor=c*0.7f;
                    links[i].SetPropertyBlock(block);
                }
            }
        }

        void OnDestroy()
        {
            if(muteButton) muteButton.onClick.RemoveListener(ToggleAudio);
            if(hum) Destroy(hum);
        }
    }
}
