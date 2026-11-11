using TMPro;
using UnityEngine;

namespace CyberShield
{
    public sealed class CyberShieldPolishDirector : MonoBehaviour
    {
        public CyberShieldGame game;
        public CyberShieldHandsOn hands;
        public CyberShieldReferenceExperience flow;
        public Renderer[] driveStrips;
        public TMP_Text[] driveStatus;
        public LineRenderer[] driveHalos;
        public Transform scannerSweep;
        public Transform[] transferPackets;
        public CyberShieldPanelArt scope;
        public GameObject scopePanel;
        MaterialPropertyBlock block;
        void Awake(){block=new MaterialPropertyBlock();}
        void LateUpdate()
        {
            int stage=(int)game.Session.Stage;bool done=stage==3&&!game.IsRestoring;
            for(int i=0;i<3;i++)
            {
                bool scanned=game.corporateScenario.IsScanned(i),focus=hands.cards[i].isSelected||hands.cards[i].isHovered||(stage==2&&flow.SelectedBackup==i);
                var color=scanned?(i==0?new Color(.08f,1,.5f):new Color(1,.3f,.03f)):new Color(.03f,.7f,1);
                block.SetColor("_BaseColor",color*(focus?1:.65f));driveStrips[i].SetPropertyBlock(block);
                driveStatus[i].text=scanned?(i==0?"LIMPIO":"NO SEGURO"):i==0?"OFFLINE":i==1?"10:15":"RED";
                driveStatus[i].color=color;driveHalos[i].enabled=focus;driveHalos[i].startColor=driveHalos[i].endColor=color;
            }
            float progress=game.corporateScenario.ScanProgress;
            scannerSweep.gameObject.SetActive(game.IsStarted&&stage==2&&progress>0&&progress<1);
            scannerSweep.position=game.corporateScenario.scanZone.position+new Vector3(0,.025f+progress*.15f,0);
            for(int i=0;i<transferPackets.Length;i++)
            {
                transferPackets[i].gameObject.SetActive(game.IsRestoring);
                if(!game.IsRestoring)continue;
                float p=Mathf.Repeat(Time.time*.6f+i/(float)transferPackets.Length,1);
                transferPackets[i].position=Vector3.Lerp(game.corporateScenario.scanZone.position,hands.reader.position,p)+Vector3.up*(.16f+Mathf.Sin(p*Mathf.PI)*.16f);
            }
            scopePanel.SetActive(game.IsStarted&&stage>=1&&!done);
            scope.state=stage;scope.phase=Mathf.Repeat(Time.time*.14f,1);scope.progress=game.IsRestoring?game.RestorationProgress:done?1:stage==2?progress:0;
            scope.color=done?new Color(.1f,1,.5f):stage>=2?new Color(.03f,.75f,1):new Color(1,.24f,.06f);scope.SetVerticesDirty();
        }
    }
}
