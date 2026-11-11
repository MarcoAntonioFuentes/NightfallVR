using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyberShield
{
    public sealed class CyberShieldMailInvestigation : MonoBehaviour
    {
        public CyberShieldGame game;
        public GameObject viewer;
        public TMP_Text subject, message, finding, status;
        public Image[] inspectionFrames, reasonFrames;
        public Button reportButton;
        public int SelectedMail => selected;
        public int SelectedReason => reason;
        public bool IsInspected(int clue)=>selected>=0&&clue>=0&&clue<3&&inspected[selected,clue];
        int selected=-1,reason=-1;
        readonly bool[,] inspected=new bool[3,3];
        readonly string[] subjects={"COMUNICADOS / REUNIÓN","SOPORTE / VERIFICA TU CUENTA","EQUIPO TI / RECURSOS"};
        readonly string[] bodies={"REUNIÓN\nMañana · 10:00\nCalendario interno","BLOQUEO EN 10 MINUTOS\nConfirma usuario y contraseña.\n<color=#FF7744>VERIFICAR CUENTA →</color>","NUEVA GUÍA\nDisponible en la intranet.\nSin otro inicio de sesión."};
        void Start(){game.MissionReset+=Reset;Refresh();}
        void Reset(){selected=reason=-1;for(int i=0;i<3;i++)for(int j=0;j<3;j++)inspected[i,j]=false;finding.text="SELECCIONA UNA PISTA";status.text="";Refresh();}
        bool CanInvestigate => game.IsStarted&&game.Session.Stage==Challenge.Email;
        public void Open(int index)
        {
            if(!CanInvestigate||index<0||index>=3)return;
            selected=index;reason=-1;finding.text="ELIGE UNA PISTA";status.text="";Refresh();
        }
        public void Inspect(int clue)
        {
            if(!CanInvestigate||selected<0||clue<0||clue>=3)return;
            inspected[selected,clue]=true;
            if(selected==1)
            {
                game.corporateScenario.InspectEvidence(clue);
                finding.text=new[]{"<color=#FF7744>empres4.example</color>\nEMPRESA: empresa.example","acceso.empres4.example\n<color=#FF7744>DOMINIO EXTERNO</color>","10 MINUTOS\n<color=#FF7744>CAMPAÑA NO REGISTRADA</color>"}[clue];
            }
            else finding.text=clue==0?(selected==0?"comunicados@empresa.example":"ti@empresa.example")+"\nDIRECTORIO: COINCIDE":clue==1?"intranet.empresa.example\nDOMINIO INTERNO":"AVISO VERIFICADO\nSIN CONTRASEÑAS NI PAGOS";
            Refresh();
        }
        public void SelectReason(int index){if(!CanInvestigate||selected<0||index<0||index>=3)return;reason=index;Refresh();}
        public void Report()
        {
            if(!CanInvestigate||selected<0)return;
            if(Count()<2){status.text="REVISA 2 PISTAS";return;}
            if(reason<0){status.text="ELIGE UN MOTIVO";return;}
            if(selected==1&&reason==1){game.Choose(1);return;}
            game.Choose(selected==1?0:selected);
            status.text=selected==1?"COMPARA LOS DOMINIOS":"AVISO VERIFICADO · SIGUE INVESTIGANDO";
        }
        int Count(){if(selected<0)return 0;int count=0;for(int i=0;i<3;i++)if(inspected[selected,i])count++;return count;}
        void Refresh()
        {
            subject.text=selected<0?"ABRE UN CORREO EN LA BANDEJA":subjects[selected];
            message.text=selected<0?"ABRE UN MENSAJE\nY EXAMINA SUS PISTAS":bodies[selected];
            for(int i=0;i<3;i++)
            {
                inspectionFrames[i].color=selected>=0&&inspected[selected,i]?new Color(.025f,.4f,.3f):new Color(.025f,.15f,.25f);
                reasonFrames[i].color=i==reason?new Color(.02f,.45f,.7f):new Color(.025f,.15f,.25f);
            }
            reportButton.interactable=CanInvestigate&&selected>=0;
        }
        void LateUpdate(){viewer.SetActive(CanInvestigate);}
        void OnDestroy(){if(game)game.MissionReset-=Reset;}
    }
}
