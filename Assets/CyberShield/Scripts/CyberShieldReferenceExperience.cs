using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyberShield
{
    public sealed class CyberShieldReferenceExperience : MonoBehaviour
    {
        public CyberShieldGame game;
        public CyberShieldHandsOn hands;
        public GameObject networkPanel, recoveryPanel, victoryPanel, oldDebrief;
        public GameObject monitorBackdrop;
        public Button isolateButton, restoreButton;
        public TMP_Text isolationAction, restoreAction, resultScore;
        public Image[] computerFrames, backupFrames;
        public bool volumetricNetwork;
        public int SelectedComputer => selectedComputer;
        public int SelectedBackup => selectedBackup;
        int selectedComputer=-1,selectedBackup=-1;
        GameObject[] tutorialHints;
        CyberShieldForensicProbe probe;
        readonly Color idle=new Color(.035f,.13f,.22f), amber=new Color(1,.36f,.04f), cyan=new Color(.02f,.7f,1), green=new Color(.08f,.8f,.46f);
        void Start(){game.MissionReset+=ResetSelection;probe=FindFirstObjectByType<CyberShieldForensicProbe>();}
        void ResetSelection(){selectedComputer=selectedBackup=-1;}
        public void SelectComputer(int index)
        {
            if(!game.IsStarted||game.Session.Stage!=Challenge.Isolation||index<0||index>=computerFrames.Length)return;
            selectedComputer=index;
        }
        public void Isolate()
        {
            if(!game.IsStarted||game.Session.Stage!=Challenge.Isolation)return;
            if(selectedComputer<0)return;
            if(probe&&!probe.IsExamined(selectedComputer))return;
            if(selectedComputer!=2){game.Choose(3);selectedComputer=-1;return;}
            hands.DisconnectEthernet();hands.DisconnectWifi();
        }
        public void SelectBackup(int index)
        {
            if(!game.IsStarted||game.Session.Stage!=Challenge.Backup||index<0||index>=backupFrames.Length)return;
            selectedBackup=index;
        }
        public void Restore()
        {
            if(selectedBackup<0||!game.corporateScenario.IsScanned(selectedBackup))return;
            hands.ReadCard(selectedBackup);
        }
        void LateUpdate()
        {
            int stage=(int)game.Session.Stage;
            networkPanel.SetActive(game.IsStarted&&stage==1&&!volumetricNetwork);
            recoveryPanel.SetActive(game.IsStarted&&stage>=2&&!(stage==3&&!game.IsRestoring));
            bool won=game.IsStarted&&stage==3&&!game.IsRestoring;
            if(monitorBackdrop)monitorBackdrop.SetActive(!game.IsStarted);
            hands.isolationLabel.gameObject.SetActive(!game.IsStarted);
            if(game.IsStarted&&tutorialHints==null)
            {
                var hints=new System.Collections.Generic.List<GameObject>();
                foreach(var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                    if(text.transform.root.name=="Lazy Tooltip"&&!hints.Contains(text.transform.root.gameObject))hints.Add(text.transform.root.gameObject);
                tutorialHints=hints.ToArray();
            }
            if(tutorialHints!=null)foreach(var hint in tutorialHints)if(hint)hint.SetActive(!game.IsStarted);
            victoryPanel.SetActive(won);if(oldDebrief)oldDebrief.SetActive(false);
            isolateButton.interactable=stage==1&&selectedComputer>=0&&(!probe||probe.IsExamined(selectedComputer));
            isolationAction.text=selectedComputer<0?"ELIGE EQUIPO":probe&&!probe.IsExamined(selectedComputer)?"ANALIZA":"AISLAR";
            for(int i=0;i<computerFrames.Length;i++)computerFrames[i].color=i==selectedComputer?amber:idle;
            for(int i=0;i<backupFrames.Length;i++)backupFrames[i].color=i==selectedBackup?(game.corporateScenario.IsScanned(i)?i==0?green:amber:cyan):idle;
            bool scanned=selectedBackup>=0&&game.corporateScenario.IsScanned(selectedBackup);
            restoreButton.interactable=stage==2&&scanned;
            restoreAction.text=game.IsRestoring?$"{game.RestorationProgress*100:00}%":selectedBackup<0?"ELIGE COPIA":!scanned?"ANALIZA":"RESTAURAR";
            int seconds=Mathf.FloorToInt(game.Elapsed);
            resultScore.text=$"{game.Session.Score}/100   ·   {seconds/60:00}:{seconds%60:00}";
        }
        void OnDestroy(){if(game)game.MissionReset-=ResetSelection;}
    }
}
