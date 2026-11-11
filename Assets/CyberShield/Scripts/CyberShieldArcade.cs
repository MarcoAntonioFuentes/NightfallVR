using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CyberShield
{
    public sealed class CyberShieldArcade : MonoBehaviour
    {
        public CyberShieldGame game;
        public XRSimpleInteractable[] clues;
        public Renderer[] clueVisuals;
        public Transform[] markers, celebration;
        public Transform threat;
        public Renderer threatVisual;
        public LineRenderer[] rings;
        public TMP_Text objective, toast, compactStatus;
        public bool screenInvestigationOnly;
        public bool useCinematicFeedback;
        readonly bool[] found = new bool[3];
        MaterialPropertyBlock block;
        int previousStage = -1, previousScore, previousMistakes;
        float toastUntil, victoryStart = -1;
        Vector3[] markerPositions;
        AudioClip clueSound;

        void Start()
        {
            block = new MaterialPropertyBlock();
            markerPositions = new Vector3[markers.Length];
            for (int i=0;i<markers.Length;i++) markerPositions[i]=markers[i].position;
            for (int i=0;i<clues.Length;i++) { int index=i; clues[i].selectEntered.AddListener(_=>Inspect(index)); }
            var samples=new float[4400];
            for(int i=0;i<samples.Length;i++)samples[i]=Mathf.Sin(i/22050f*Mathf.PI*2*(950+i*.1f))*.12f*(1-i/(float)samples.Length);
            clueSound=AudioClip.Create("Pista localizada",samples.Length,1,22050,false);clueSound.SetData(samples,0);
            game.MissionReset+=ResetEffects;
        }

        void Inspect(int index)
        {
            if(!game.IsStarted || game.Session.Stage!=Challenge.Email)return;
            game.corporateScenario.InspectEvidence(index);
            if(found[index])return;
            found[index]=true;
            if(game.speaker)game.speaker.PlayOneShot(clueSound,.65f);
            Show(new[]{"IDENTIDAD FALSA","DOMINIO ALTERADO","URGENCIA ARTIFICIAL"}[index]);
        }

        void Show(string message){toast.text=message;toastUntil=Time.time+2.8f;}
        void ResetEffects()
        {
            for(int i=0;i<found.Length;i++)found[i]=false;
            previousStage=-1;previousScore=previousMistakes=0;victoryStart=-1;
            toast.text="";
        }

        void Update()
        {
            int stage=(int)game.Session.Stage;
            bool done=stage==3&&!game.IsRestoring;
            if(stage!=previousStage)
            {
                if(previousStage>=0 && stage>previousStage)Show(stage==1?"PHISHING DETECTADO  +30":stage==2?"AMENAZA AISLADA  +30":"COPIA LIMPIA  +40");
                previousStage=stage;
            }
            if(game.Session.Mistakes>previousMistakes)Show("DECISIÓN INSEGURA  −10");
            previousScore=game.Session.Score;previousMistakes=game.Session.Mistakes;
            int count=game.corporateScenario.EvidenceCount;
            objective.text=!game.IsStarted?"CYBERSHIELD / INCIDENTE 1042":stage==0?$"01  INVESTIGA  ·  {count}/3 PISTAS":stage==1?"02  AISLA PC-02  ·  CABLE + WI-FI":stage==2?"03  ANALIZA → RESTAURA":game.IsRestoring?$"RECUPERANDO  {game.RestorationProgress*100:00}%":"EMPRESA PROTEGIDA";
            compactStatus.text=$"{game.Session.Score:00}<size=60%>/100</size>   ·   {Mathf.Min(stage,3)}/3 OBJETIVOS";
            toast.gameObject.SetActive(Time.time<toastUntil&&!useCinematicFeedback);
            for(int i=0;i<clues.Length;i++)
            {
                clues[i].gameObject.SetActive(game.IsStarted&&stage==0&&!screenInvestigationOnly);
                bool inspected=found[i];
                // Existing evidence buttons and holograms share the same investigation gate.
                Color color=inspected?new Color(.1f,1,.5f):new Color(.05f,.75f,1);
                block.SetColor("_BaseColor",color);clueVisuals[i].SetPropertyBlock(block);
                clueVisuals[i].transform.localRotation=Quaternion.Euler(0,Time.time*25,45);
            }
            for(int i=0;i<markers.Length;i++)
            {
                markers[i].gameObject.SetActive(game.IsStarted&&stage==i);
                markers[i].position=markerPositions[i]+Vector3.up*(Mathf.Sin(Time.time*2)*.055f);
            }
            float pulse=1+Mathf.Sin(Time.time*(stage<2?5:1.5f))*.08f;
            threat.localScale=Vector3.one*(stage<2?.3f:.2f)*pulse;
            threat.Rotate(13*Time.deltaTime,35*Time.deltaTime,8*Time.deltaTime);
            Color threatColor=stage<2?new Color(1,.12f,.06f):new Color(.05f,1,.6f);
            block.SetColor("_BaseColor",threatColor);threatVisual.SetPropertyBlock(block);
            for(int i=0;i<rings.Length;i++)
            {
                rings[i].transform.Rotate(0,(i%2==0?1:-1)*25*Time.deltaTime,0);
                rings[i].startColor=threatColor;rings[i].endColor=threatColor;
                rings[i].SetPropertyBlock(block);
            }
            if(done&&victoryStart<0){victoryStart=Time.time;Show("MISIÓN COMPLETADA  ·  EXCELENTE TRABAJO");}
            for(int i=0;i<celebration.Length;i++)
            {
                celebration[i].gameObject.SetActive(done);
                if(!done)continue;
                float angle=i*Mathf.PI*2/celebration.Length+(Time.time-victoryStart)*.7f;
                float radius=.65f+.1f*Mathf.Sin(Time.time+i);
                celebration[i].position=new Vector3(Mathf.Cos(angle)*radius,1.4f+(i%5)*.18f,1.5f+Mathf.Sin(angle)*radius);
                celebration[i].Rotate(40*Time.deltaTime,60*Time.deltaTime,0);
            }
        }
        void OnDestroy(){if(game)game.MissionReset-=ResetEffects;if(clueSound)Destroy(clueSound);}
    }
}
