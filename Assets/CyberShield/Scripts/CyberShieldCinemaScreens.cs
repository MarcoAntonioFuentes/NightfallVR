using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyberShield
{
    public sealed class CyberShieldCinemaScreens : MonoBehaviour
    {
        public CyberShieldGame game;
        public Image[] nodes, routes, telemetry, files, evidenceFlags, gauge;
        public TMP_Text mailStatus, networkStatus, recoveryStatus, gaugeLabel, proofLabel;
        public RectTransform cursor;
        readonly Color cyan=new Color(.04f,.78f,1), green=new Color(.18f,1,.58f), red=new Color(1,.2f,.16f), idle=new Color(.07f,.17f,.24f);
        int previousState=-1;
        void Update()
        {
            int stage=(int)game.Session.Stage;
            bool isolated=stage>=2;
            float progress=game.IsRestoring?game.RestorationProgress:stage==3?1:0;
            for(int i=0;i<nodes.Length;i++)
            {
                Color color=i==2?(isolated?green:red):cyan;
                nodes[i].color=color*(.8f+.2f*Mathf.Sin(Time.time*1.8f+i));
            }
            for(int i=0;i<routes.Length;i++)routes[i].color=isolated?new Color(.07f,.13f,.19f):new Color(.7f,.17f,.16f);
            for(int i=0;i<telemetry.Length;i++)
            {
                var size=telemetry[i].rectTransform.sizeDelta;
                size.y=12+Mathf.Abs(Mathf.Sin(Time.time*(isolated?.55f:1.5f)+i*1.7f))*48;
                telemetry[i].rectTransform.sizeDelta=size;telemetry[i].color=isolated?green:cyan;
            }
            for(int i=0;i<files.Length;i++)
            {
                bool rebuilt=progress>(i+.5f)/files.Length;
                files[i].color=rebuilt?green:stage<2?idle:new Color(.12f,.3f,.4f);
                if(stage==2&&i==Mathf.FloorToInt(Time.time*4)%files.Length)files[i].color=cyan;
            }
            int evidence=game.corporateScenario.EvidenceCount;
            for(int i=0;i<evidenceFlags.Length;i++)evidenceFlags[i].color=i<evidence?green:idle;
            float displayed=stage==2?game.corporateScenario.ScanProgress:progress;
            for(int i=0;i<gauge.Length;i++)gauge[i].color=(i+.5f)/gauge.Length<displayed?(stage==2?cyan:green):idle;
            gaugeLabel.text=$"{displayed*100:00}<size=50%>%</size>";
            if(cursor)cursor.anchoredPosition=new Vector2(Mathf.Repeat(Time.time*65,700)-350,0);
            int scanned=game.corporateScenario.LastScanned;
            int state=stage*10+evidence+(game.IsRestoring?100:0)+(scanned+1)*1000;
            if(state==previousState)return;previousState=state;
            mailStatus.text=stage==0?$"{evidence}/3 PISTAS":"PHISHING REPORTADO";
            networkStatus.text=isolated?"PC-02 AISLADO":"PC-02 / ALERTA ACTIVA";
            recoveryStatus.text=stage<2?"REPOSITORIO OFFLINE":stage==2?(scanned<0?"ANALIZA LA TARJETA EN ÁMBAR":scanned==0?"COPIA A · LIMPIA / OFFLINE":"COPIA NO SEGURA · NO RESTAURAR"):game.IsRestoring?"RECONSTRUYENDO ARCHIVOS":"INTEGRIDAD VERIFICADA";
            if(proofLabel)proofLabel.text=evidence==0?"ENTRADA\n\nDESTACADOS\n\nENVIADOS":"REMITENTE\nempres4.example\n\nDOMINIO REAL\nempresa.example";
        }
    }
}
