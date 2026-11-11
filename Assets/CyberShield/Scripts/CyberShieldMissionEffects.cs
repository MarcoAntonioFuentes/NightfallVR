using TMPro;
using UnityEngine;

namespace CyberShield
{
    // Small pooled geometry: no post-processing, screen shake or continuous particles.
    public sealed class CyberShieldMissionEffects : MonoBehaviour
    {
        CyberShieldArcade arcade;
        CyberShieldGame game;
        CyberShieldHandsOn hands;
        Transform effectsRoot;
        Transform[] packets=new Transform[16], sparks=new Transform[20];
        Renderer[] packetRenderers=new Renderer[16];
        LineRenderer network, scanRing, restoreRing;
        TMP_Text scannerLabel, nextAction;
        Material material;
        MaterialPropertyBlock block;
        float burstStart=-10, lastCue=-10;
        Vector3 burstPosition;
        int lastStage=-1, lastEvidence, lastMistakes;
        bool[] held=new bool[3];
        bool compactScanner;
        AudioClip checkpoint;
        static readonly Color Cyan=new Color(.02f,.8f,1), Green=new Color(.15f,1,.55f), Red=new Color(1,.17f,.08f);

        void Start()
        {
            arcade=GetComponent<CyberShieldArcade>();game=arcade.game;hands=GetComponent<CyberShieldHandsOn>();
            block=new MaterialPropertyBlock();material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));material.SetColor("_BaseColor",Cyan);
            effectsRoot=new GameObject("Efectos de misión / runtime").transform;effectsRoot.SetParent(transform);
            network=Line("Ruta de propagación",.012f,4,false);
            network.SetPositions(new[]{new Vector3(-2.5f,1.05f,2.73f),new Vector3(-1.3f,1.05f,2.73f),new Vector3(0,1.05f,2.73f),new Vector3(2.5f,1.05f,2.73f)});
            for(int i=0;i<packets.Length;i++){packets[i]=Shape("Paquete de red",.045f);packetRenderers[i]=packets[i].GetComponent<Renderer>();}
            for(int i=0;i<sparks.Length;i++)sparks[i]=Shape("Pulso de logro",.025f);
            scanRing=Line("Progreso de análisis",.018f,48,true);
            restoreRing=Line("Progreso de restauración",.018f,48,true);
            scannerLabel=Text("",hands.game.corporateScenario.scanZone.position+new Vector3(0,.23f,.12f),new Vector2(.7f,.22f),.5f);
            compactScanner=FindFirstObjectByType<CyberShieldPolishDirector>()!=null;if(compactScanner)scannerLabel.fontSize=.28f;
            nextAction=Text("",new Vector3(0,2.3f,2.65f),new Vector2(4,.3f),.7f);
            if(GetComponent<CyberShieldReferenceExperience>()&&GetComponent<CyberShieldReferenceExperience>().volumetricNetwork)nextAction.gameObject.SetActive(false);
            var data=new float[6600];for(int i=0;i<data.Length;i++){float t=i/22050f;data[i]=Mathf.Sin(t*2*Mathf.PI*(i<2200?660:i<4400?880:1100))*.12f*(1-i/(float)data.Length);}
            checkpoint=AudioClip.Create("Objetivo asegurado",data.Length,1,22050,false);checkpoint.SetData(data,0);
            game.MissionReset+=ResetEffects;
        }
        void ResetEffects(){lastStage=-1;lastEvidence=lastMistakes=0;burstStart=-10;for(int i=0;i<held.Length;i++)held[i]=false;}
        Transform Shape(string name,float size)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(effectsRoot);g.transform.localScale=Vector3.one*size;Destroy(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=material;return g.transform;
        }
        LineRenderer Line(string name,float width,int count,bool loop)
        {
            var g=new GameObject(name);g.transform.SetParent(effectsRoot);var l=g.AddComponent<LineRenderer>();l.sharedMaterial=material;l.widthMultiplier=width;l.positionCount=count;l.loop=loop;return l;
        }
        TMP_Text Text(string text,Vector3 position,Vector2 size,float font)
        {
            var g=new GameObject("Indicación de acción",typeof(TextMeshPro));g.transform.SetParent(effectsRoot);g.transform.position=position;var t=g.GetComponent<TextMeshPro>();t.text=text;t.font=TMP_Settings.defaultFontAsset;t.fontSize=font;t.color=Cyan;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=size;return t;
        }
        void Burst(Vector3 position)
        {
            burstPosition=position;burstStart=Time.time;
            if(Time.time-lastCue>.4f && game.speaker){game.speaker.PlayOneShot(checkpoint,.4f);lastCue=Time.time;}
        }
        void Ring(LineRenderer line,Vector3 center,float progress,Color color)
        {
            line.enabled=game.IsStarted;
            for(int i=0;i<48;i++){float angle=i/47f*Mathf.PI*2*Mathf.Max(.025f,progress);line.SetPosition(i,center+new Vector3(Mathf.Cos(angle)*.25f,.08f,Mathf.Sin(angle)*.25f));}
            block.SetColor("_BaseColor",color);line.SetPropertyBlock(block);
        }
        void Update()
        {
            if(!arcade)return;
            int stage=(int)game.Session.Stage;
            int evidence=game.corporateScenario.EvidenceCount;
            if(stage!=lastStage){if(lastStage>=0&&stage>lastStage)Burst(arcade.threat.position);lastStage=stage;}
            if(evidence>lastEvidence)Burst(new Vector3(-2.5f,1.6f,1.8f));lastEvidence=evidence;
            if(game.Session.Mistakes>lastMistakes){burstStart=-10;nextAction.color=Red;}else nextAction.color=Cyan;
            lastMistakes=game.Session.Mistakes;
            nextAction.text=!game.IsStarted?"":stage==0?(evidence<2?"ENCUENTRA DOS PISTAS EN LOS HOLOGRAMAS":"REPORTA EL CORREO DE SOPORTE FALSO"):stage==1?"USA LOS DOS INTERRUPTORES NARANJAS":stage==2?"SUELTA EN ÁMBAR · VERIFICA · LLEVA A CIAN":game.IsRestoring?"VERIFICANDO LA COPIA LIMPIA…":"OPERACIÓN SEGURA · EQUIPO TODAVÍA AISLADO";
            bool dangerous=stage<2;
            block.SetColor("_BaseColor",dangerous?Red:Green);network.SetPropertyBlock(block);
            network.enabled=game.IsStarted&&dangerous;
            for(int i=0;i<packets.Length;i++)
            {
                packets[i].gameObject.SetActive(game.IsStarted);
                float t=Mathf.Repeat(Time.time*(dangerous?.23f:.08f)+i/(float)packets.Length,1);
                packets[i].position=new Vector3(Mathf.Lerp(-2.5f,2.5f,t),1.06f+Mathf.Sin(t*Mathf.PI*4)*.035f,2.7f);
                packetRenderers[i].SetPropertyBlock(block);
            }
            Ring(scanRing,game.corporateScenario.scanZone.position,game.corporateScenario.ScanProgress,new Color(1,.65f,.08f));
            Ring(restoreRing,hands.reader.position,game.IsRestoring?game.RestorationProgress:stage==3?1:.05f,stage==3?Green:Cyan);
            scannerLabel.text=stage<2?"ANÁLISIS OFFLINE":game.corporateScenario.ScanProgress>0?$"ANÁLISIS  {game.corporateScenario.ScanProgress*100:00}%":"SUELTA AQUÍ";
            if(compactScanner)scannerLabel.text=stage==2?(game.corporateScenario.ScanProgress>0?$"{game.corporateScenario.ScanProgress*100:00}%":"ANALIZA"):"";
            for(int i=0;i<hands.cards.Length;i++)
            {
                bool selected=hands.cards[i].isSelected;
                if(selected&&!held[i])Burst(hands.cards[i].transform.position);
                held[i]=selected;
            }
            float age=Time.time-burstStart;
            for(int i=0;i<sparks.Length;i++)
            {
                sparks[i].gameObject.SetActive(age<.8f);
                float angle=i*2.39996f;
                var direction=new Vector3(Mathf.Cos(angle),((i%7)-3)/5f,Mathf.Sin(angle)).normalized;
                sparks[i].position=burstPosition+direction*age*.65f;
                sparks[i].localScale=Vector3.one*.035f*Mathf.Clamp01(1-age/.8f);
            }
        }
        void OnDestroy(){if(game)game.MissionReset-=ResetEffects;if(material)Destroy(material);if(checkpoint)Destroy(checkpoint);if(effectsRoot)Destroy(effectsRoot.gameObject);}
    }
}
