using TMPro;
using UnityEngine;

namespace CyberShield
{
    public sealed class CyberShieldCorporateScenario : MonoBehaviour
    {
        public CyberShieldGame game;
        public CyberShieldHandsOn hands;
        public TMP_Text evidenceText, scannerText, serviceText, colleagueText;
        public Transform scanZone;
        public Renderer alertLamp;
        readonly bool[] evidence=new bool[3];
        readonly bool[] scanned=new bool[3];
        readonly float[] scanTimes=new float[3];
        public float ScanProgress { get; private set; }
        public int LastScanned => lastScanned;
        public bool IsScanned(int index) => index>=0 && index<scanned.Length && scanned[index];
        int lastStage=-1, lastSecond=-1, lastScanned=-1;
        int lastAvailability=-1;
        float availability=100;
        MaterialPropertyBlock block;

        void Awake(){block=new MaterialPropertyBlock();ResetIncident();}
        public void InspectEvidence(int index)
        {
            if(!game.IsStarted || game.Session.Stage!=Challenge.Email || index<0 || index>2) return;
            evidence[index]=true;
            string[] details={
                "REMITENTE EXAMINADO\nNombre visible: Soporte Nexora\nDirección real: soporte@empres4.example\nEl nombre visible no prueba la identidad del remitente.",
                "ENLACE EXAMINADO (sin abrirlo)\nDestino: acceso.empres4.example/verificar\nSolicita usuario y contraseña fuera del dominio corporativo.",
                "CONTEXTO EXAMINADO\nAmenaza de bloqueo en 10 minutos.\nEl directorio interno no registra esta campaña. Reporta por el canal de TI."};
            evidenceText.text=details[index]+$"\n\nEVIDENCIAS: {EvidenceCount}/3 · requiere al menos 2";
        }
        public int EvidenceCount => (evidence[0]?1:0)+(evidence[1]?1:0)+(evidence[2]?1:0);
        public bool PermitChoice(int choice)
        {
            if(choice==1 && EvidenceCount<2)
            {game.feedbackText.text="Antes de reportar, inspecciona al menos dos pistas en el panel de evidencias.";return false;}
            if(choice>=6 && choice<=8 && !scanned[choice-6])
            {game.feedbackText.text="Coloca y suelta esa tarjeta en la bandeja de ANÁLISIS antes de usar el lector de restauración.";return false;}
            return true;
        }
        public void ResetIncident()
        {
            for(int i=0;i<3;i++){evidence[i]=false;scanned[i]=false;scanTimes[i]=0;}
            ScanProgress=0;
            availability=100;lastStage=-1;lastSecond=-1;lastScanned=-1;
            if(evidenceText)evidenceText.text="EXPEDIENTE CS-1042\nInspecciona el remitente, el enlace y el contexto.\nNo abras el enlace del correo.\n\nEVIDENCIAS: 0/3";
            if(scannerText)scannerText.text="ANALIZADOR OFFLINE\nColoca y suelta una tarjeta en la bandeja ámbar.\nVerifica fecha, integridad y análisis antes de restaurar.";
        }
        void Update()
        {
            int stage=(int)game.Session.Stage;
            if(game.IsStarted && stage<2)availability=Mathf.Max(35,availability-Time.deltaTime*0.12f);
            if(game.IsStarted && stage==3 && !game.IsRestoring)availability=Mathf.Min(100,availability+Time.deltaTime*12);
            if(game.IsStarted && stage==2)
            {
                ScanProgress=0;
                for(int i=0;i<hands.cards.Length;i++)
                {
                    var card=hands.cards[i];var p=scanZone.InverseTransformPoint(card.transform.position);
                    if(!card.isSelected && Mathf.Abs(p.x)<0.25f && Mathf.Abs(p.z)<0.23f && p.y>0 && p.y<0.28f)
                    {
                        scanTimes[i]=Mathf.Min(1,scanTimes[i]+Time.deltaTime/.9f);
                        ScanProgress=Mathf.Max(ScanProgress,scanTimes[i]);
                        if(scanTimes[i]<1){scannerText.text=$"ANALIZANDO COPIA {(char)('A'+i)}  {scanTimes[i]*100:00}%\nComprobando integridad y origen…";continue;}
                        scanned[i]=true;
                        if(lastScanned!=i)
                        {
                            lastScanned=i;
                            scannerText.text=i==0?"COPIA A / AYER 18:00\nHASH: COINCIDE · ANÁLISIS: LIMPIO\nORIGEN: OFFLINE\nAPTA · llévala al lector cian":i==1?"COPIA B / HOY 10:15\nPOSTERIOR AL INCIDENTE\nANÁLISIS: CONTENIDO SOSPECHOSO\nNO RESTAURAR":"COPIA C / SINCRONIZADA\nORIGEN: RED COMPROMETIDA\nINTEGRIDAD: NO GARANTIZADA\nNO RESTAURAR";
                        }
                    }
                    else if(!scanned[i])scanTimes[i]=0;
                }
            }
            int second=Mathf.FloorToInt(game.Elapsed);
            if(stage!=lastStage || second!=lastSecond || Mathf.RoundToInt(availability)!=lastAvailability)
            {
                lastAvailability=Mathf.RoundToInt(availability);
                lastStage=stage;lastSecond=second;
                serviceText.text=$"NEXORA / ESTADO DE SERVICIOS\nDisponibilidad simulada: {availability:0}%\nERP: {(stage<2?"DEGRADADO":stage==2?"CONTENIDO":"RECUPERANDO")}  |  CORREO: OPERATIVO\nPC-02: {(stage<2?"EN INVESTIGACIÓN":"AISLADO")}\nTicket CS-1042 · prioridad alta";
                colleagueText.text=!game.IsStarted?"MARTA / COORDINACIÓN TI\nBuenos días. Tenemos una alerta en Contabilidad. Inicia el expediente cuando estés listo.":stage==0?"MARTA / COORDINACIÓN TI\nLucía recibió un aviso urgente de cuenta. Revisa las evidencias antes de reportarlo.":stage==1?"LUCÍA / CONTABILIDAD\nMi equipo abrió un adjunto parecido hace unos minutos. Necesitamos contener PC-02 sin apagar toda la oficina.":stage==2?"MARTA / COORDINACIÓN TI\nBuen trabajo: la propagación está contenida. Analiza las copias offline antes de restaurar.":"MARTA / COORDINACIÓN TI\nDocumentamos la recuperación. El equipo permanece aislado hasta la validación técnica. Gracias por proteger la operación.";
                block.SetColor("_BaseColor",stage<2?new Color(1,0.3f,0.08f):new Color(0.1f,0.7f,0.4f));alertLamp.SetPropertyBlock(block);
            }
        }
    }
}
