using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyberShield
{
    [DefaultExecutionOrder(-50)]
    public sealed class CyberShieldForensicProbe : MonoBehaviour
    {
        public CyberShieldGame game;
        public CyberShieldReferenceExperience flow;
        public CyberShieldTacticalNetwork tactical;
        public CyberShieldOperatorArms arms;
        public Button analyzeButton;
        public TMP_Text analyzeLabel, bodyLabel, grade, scanSummary;
        public CyberShieldPanelArt scanner;
        public CyberShieldGlyph[] nodeSymbols;
        public Image[] stars;
        public LineRenderer sweep;
        public AudioSource audioSource;
        public AudioClip scanSound;
        readonly bool[] examined=new bool[8];
        int target=-1;
        float progress;
        public int Version {get;private set;}
        public int ExaminedCount {get;private set;}
        public bool IsScanning=>target>=0;
        public float Progress=>progress;
        public bool IsExamined(int index)=>index>=0&&index<examined.Length&&examined[index];
        void Start(){game.MissionReset+=Reset;}
        void Reset(){for(int i=0;i<8;i++)examined[i]=false;target=-1;progress=0;ExaminedCount=0;Version++;}
        public void BeginScan()
        {
            int selected=flow.SelectedComputer;
            if(!game.IsStarted||game.Session.Stage!=Challenge.Isolation||selected<0||selected>=8||IsExamined(selected)||IsScanning)return;
            target=selected;progress=0;if(audioSource&&scanSound)audioSource.PlayOneShot(scanSound,.22f);
        }
        public void ToggleBody(){arms.showArms=!arms.showArms;}
        void Update()
        {
            bool active=game.IsStarted&&game.Session.Stage==Challenge.Isolation;
            int selected=flow.SelectedComputer;
            if(IsScanning&&(!active||selected!=target)){target=-1;progress=0;}
            if(IsScanning)
            {
                progress=Mathf.Min(1,progress+Time.deltaTime/1.6f);
                var pc=tactical.computers[target].transform.position;
                sweep.SetPosition(0,pc+new Vector3(-.17f,.12f+progress*.18f,-.035f));sweep.SetPosition(1,pc+new Vector3(.17f,.12f+progress*.18f,-.035f));
                if(progress>=1){examined[target]=true;ExaminedCount++;Version++;target=-1;if(audioSource&&scanSound)audioSource.PlayOneShot(scanSound,.16f);}
            }
            sweep.enabled=IsScanning;
            analyzeButton.interactable=active&&selected>=0&&!IsScanning&&!IsExamined(selected);
            analyzeLabel.text=IsScanning?$"{progress*100:00}%":IsExamined(selected)?"LISTO":"ANALIZAR";
            scanner.state=IsScanning?0:2;scanner.phase=progress;scanner.progress=IsExamined(selected)?1:progress;scanner.color=IsExamined(selected)&&selected==2?new Color(1,.4f,.05f):new Color(.04f,.8f,1);scanner.SetVerticesDirty();
            for(int i=0;i<nodeSymbols.Length;i++)
            {
                int symbol=!IsExamined(i)?9:i==2?10:8;
                if(nodeSymbols[i]&&nodeSymbols[i].symbol!=symbol){nodeSymbols[i].symbol=symbol;nodeSymbols[i].SetVerticesDirty();}
            }
            bodyLabel.text=arms.showArms?"SOLO GUANTES":"VER BRAZOS";
            bool done=game.Session.Stage==Challenge.Completed&&!game.IsRestoring;
            if(done)
            {
                int score=game.Session.Score;grade.text=score>=90?"S":score>=70?"A":"B";
                for(int i=0;i<stars.Length;i++)stars[i].color=i<(score>=90?3:score>=70?2:1)?new Color(1,.7f,.12f):new Color(.1f,.18f,.2f);
                scanSummary.text=$"EQUIPOS\n{ExaminedCount}/8";
            }
        }
        void OnDestroy(){if(game)game.MissionReset-=Reset;}
    }
}
