using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CyberShield
{
    public sealed class CyberShieldHandsOn : MonoBehaviour
    {
        public CyberShieldGame game;
        public XRSimpleInteractable ethernetSwitch, wifiSwitch;
        public Renderer ethernetLamp, wifiLamp;
        public TMP_Text isolationLabel, readerLabel;
        public Transform reader;
        public XRGrabInteractable[] cards;
        Vector3[] positions;
        Quaternion[] rotations;
        bool ethernetOff, wifiOff;
        float nextRead;
        MaterialPropertyBlock block;

        void Awake()
        {
            positions=new Vector3[cards.Length]; rotations=new Quaternion[cards.Length];
            for(int i=0;i<cards.Length;i++){positions[i]=cards[i].transform.position;rotations[i]=cards[i].transform.rotation;}
            block=new MaterialPropertyBlock();
            ethernetSwitch.selectEntered.AddListener(OnEthernet);
            wifiSwitch.selectEntered.AddListener(OnWifi);
            game.MissionReset += ResetInteraction;
            UpdateLamps();
        }
        void OnEthernet(SelectEnterEventArgs args) => DisconnectEthernet();
        void OnWifi(SelectEnterEventArgs args) => DisconnectWifi();
        public void DisconnectEthernet(){if(CanIsolate()){ethernetOff=true;CheckIsolation();}}
        public void DisconnectWifi(){if(CanIsolate()){wifiOff=true;CheckIsolation();}}
        bool CanIsolate() => game.IsStarted && game.Session.Stage==Challenge.Isolation;
        void CheckIsolation()
        {
            UpdateLamps();
            if(ethernetOff && wifiOff) game.Choose(5);
        }
        void UpdateLamps()
        {
            isolationLabel.text=$"PC-02 / AISLAMIENTO\nEthernet: {(ethernetOff?"DESCONECTADO":"CONECTADO")}\nWi-Fi: {(wifiOff?"DESACTIVADO":"ACTIVO")}";
            block.SetColor("_BaseColor",ethernetOff?Color.green:new Color(1,0.35f,0.1f));ethernetLamp.SetPropertyBlock(block);
            block.SetColor("_BaseColor",wifiOff?Color.green:new Color(1,0.35f,0.1f));wifiLamp.SetPropertyBlock(block);
        }
        void ResetInteraction()
        {
            ethernetOff=wifiOff=false;
            nextRead=0;
            UpdateLamps();
            ReturnCards();
            readerLabel.text="LECTOR OFFLINE\nColoca y suelta una tarjeta aquí";
        }
        void Update()
        {
            if(game.IsStarted && game.Session.Stage==Challenge.Completed)
                readerLabel.text=game.IsRestoring?"RESPALDO ACEPTADO\nValidando integridad…":"RESTAURACIÓN COMPLETA\nEquipo aún aislado";
            for(int i=0;i<cards.Length;i++)
            {
                if(!cards[i].isSelected && cards[i].transform.position.y<0.25f) ReturnCard(i);
                if(game.Session.Stage==Challenge.Backup && game.IsStarted && !game.IsRestoring && Time.time>=nextRead && !cards[i].isSelected)
                {
                    var p=reader.InverseTransformPoint(cards[i].transform.position);
                    if(Mathf.Abs(p.x)<0.25f && Mathf.Abs(p.z)<0.25f && p.y>0 && p.y<0.3f) ReadCard(i);
                }
            }
        }
        public void ReadCard(int index)
        {
            if(index<0 || index>=cards.Length || !game.IsStarted || game.Session.Stage!=Challenge.Backup || game.IsRestoring) return;
            int mistakes=game.Session.Mistakes;
            game.Choose(6+index);
            readerLabel.text=game.IsRestoring?"RESPALDO ACEPTADO\nValidando integridad…":game.Session.Mistakes>mistakes?"COPIA RECHAZADA\nRevisa fecha, integridad y aislamiento":"ANÁLISIS REQUERIDO\nUsa primero el escáner de respaldo";
            nextRead=Time.time+1f;
            ReturnCard(index);
        }
        public void ReturnCards(){for(int i=0;i<cards.Length;i++) if(!cards[i].isSelected) ReturnCard(i);}
        void ReturnCard(int i)
        {
            var rb=cards[i].GetComponent<Rigidbody>();
            rb.position=positions[i];rb.rotation=rotations[i];
            rb.linearVelocity=Vector3.zero;rb.angularVelocity=Vector3.zero;
        }
        void OnDestroy()
        {
            if(game) game.MissionReset -= ResetInteraction;
            ethernetSwitch.selectEntered.RemoveListener(OnEthernet);
            wifiSwitch.selectEntered.RemoveListener(OnWifi);
        }
    }
}
