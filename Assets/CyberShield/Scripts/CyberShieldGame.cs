using TMPro;
using UnityEngine;

namespace CyberShield
{
    public sealed class CyberShieldGame : MonoBehaviour
    {
        public TMP_Text scoreText;
        public TMP_Text feedbackText;
        public TMP_Text[] stationStatus;
        public UnityEngine.UI.Button[] choices;
        public Renderer compromisedIndicator;
        public AudioSource speaker;
        public bool requireBriefing;
        public bool physicalMode;
        public CyberShieldCorporateScenario corporateScenario;
        public int HintsUsed { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsRestoring { get; private set; }
        public float RestorationProgress { get; private set; }
        public float Elapsed { get; private set; }
        public int BestScore => PlayerPrefs.GetInt("CyberShield.BestScore", 0);
        public CyberShieldSession Session { get; private set; } = new CyberShieldSession();
        public event System.Action MissionReset;
        AudioClip successSound, errorSound, completeSound;

        void Awake()
        {
            successSound = Tone("Respuesta correcta", 660, 0.18f);
            errorSound = Tone("Revisar respuesta", 180, 0.22f);
            completeSound = Tone("Misión completada", 880, 0.6f);
            IsStarted = !requireBriefing;
            Refresh();
        }

        void Update()
        {
            if (IsStarted && (Session.Stage != Challenge.Completed || IsRestoring)) Elapsed += Time.deltaTime;
        }

        public void BeginMission()
        {
            if (IsStarted) return;
            IsStarted = true;
            if (speaker) speaker.PlayOneShot(successSound, 0.5f);
            Refresh();
        }

        public void Choose(int encodedChoice)
        {
            if (!IsStarted || IsRestoring || encodedChoice < 0 || encodedChoice >= 9) return;
            var stage = (Challenge)(encodedChoice / 3);
            if (stage != Session.Stage || Session.Stage == Challenge.Completed) return;
            if (corporateScenario && !corporateScenario.PermitChoice(encodedChoice)) return;
            bool correct = Session.Choose(stage, encodedChoice % 3);
            if (speaker) speaker.PlayOneShot(correct
                ? (Session.Stage == Challenge.Completed && !requireBriefing ? completeSound : successSound) : errorSound, 0.5f);
            if (correct && Session.Stage == Challenge.Completed && requireBriefing)
                StartCoroutine(RestoreSequence());
            Refresh();
        }

        System.Collections.IEnumerator RestoreSequence()
        {
            IsRestoring = true;
            RestorationProgress = 0;
            while (RestorationProgress < 1f)
            {
                RestorationProgress = Mathf.Min(1f, RestorationProgress + Time.deltaTime / 4f);
                yield return null;
            }
            IsRestoring = false;
            PlayerPrefs.SetInt("CyberShield.BestScore", Mathf.Max(BestScore, Session.Score));
            PlayerPrefs.Save();
            if (speaker) speaker.PlayOneShot(completeSound, 0.5f);
            Refresh();
        }

        public void Restart()
        {
            StopAllCoroutines();
            IsStarted = !requireBriefing;
            IsRestoring = false;
            RestorationProgress = 0;
            Elapsed = 0;
            HintsUsed = 0;
            Session.Reset();
            if (corporateScenario) corporateScenario.ResetIncident();
            MissionReset?.Invoke();
            Refresh();
        }

        void Refresh()
        {
            scoreText.text = $"CYBERSHIELD VR   |   {Session.Score:00}/100 puntos   |   Errores: {Session.Mistakes}";
            feedbackText.text = IsRestoring ? "Restaurando la copia limpia en un entorno aislado. Verificando integridad…" : Session.Feedback;
            for (int i = 0; i < stationStatus.Length; i++)
                stationStatus[i].text = i == 2 && IsRestoring ? "RESTAURACIÓN EN CURSO" : i < (int)Session.Stage ? "COMPLETADO"
                    : i == (int)Session.Stage ? "RETO ACTIVO" : "BLOQUEADO · completa el reto anterior";
            for (int i = 0; i < choices.Length; i++)
                choices[i].interactable = IsStarted && !IsRestoring && i / 3 == (int)Session.Stage && !(physicalMode && (i == 5 || i >= 6));
            if (compromisedIndicator)
            {
                var properties = new MaterialPropertyBlock();
                properties.SetColor("_BaseColor", Session.Stage >= Challenge.Backup ? new Color(0.1f, 0.9f, 0.55f) : new Color(1f, 0.22f, 0.12f));
                compromisedIndicator.SetPropertyBlock(properties);
            }
        }

        public void ShowHint()
        {
            if (!IsStarted || IsRestoring || Session.Stage == Challenge.Completed) return;
            HintsUsed++;
            feedbackText.text = Session.Stage == Challenge.Email
                ? "PISTA: compara empresa.example con empres4.example. La urgencia y pedir tu contraseña son señales de phishing."
                : Session.Stage == Challenge.Isolation
                    ? "PISTA: desconectar solo el cable no basta si Wi-Fi sigue activo. Aísla las dos conexiones de PC-02."
                    : "PISTA: el incidente fue hoy a las 10:00. Busca la tarjeta OFFLINE de AYER 18:00, verificada y limpia.";
        }

        static AudioClip Tone(string label, float frequency, float duration)
        {
            const int rate = 22050;
            var samples = new float[Mathf.CeilToInt(rate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Min(t / 0.015f, 1f) * Mathf.Clamp01((duration - t) / 0.06f);
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * 0.25f;
            }
            var clip = AudioClip.Create(label, samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        void OnDestroy()
        {
            if (successSound) Destroy(successSound);
            if (errorSound) Destroy(errorSound);
            if (completeSound) Destroy(completeSound);
        }
    }
}
