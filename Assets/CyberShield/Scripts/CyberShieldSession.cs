using System;

namespace CyberShield
{
    public enum Challenge { Email, Isolation, Backup, Completed }

    [Serializable]
    public sealed class CyberShieldSession
    {
        public Challenge Stage { get; private set; }
        public int Score { get; private set; }
        public int Mistakes { get; private set; }
        int earnedPoints;
        public string Feedback { get; private set; } = "Revisa los mensajes y reporta el intento de phishing.";

        public bool Choose(Challenge challenge, int option)
        {
            if (Stage == Challenge.Completed || challenge != Stage || option < 0 || option > 2)
                return false;
            // Deliberately vary answer positions across challenges.
            int correct = Stage == Challenge.Email ? 1 : Stage == Challenge.Isolation ? 2 : 0;
            if (option != correct)
            {
                Mistakes++;
                Score = Math.Max(0, earnedPoints - Mistakes * 10);
                Feedback = Stage == Challenge.Email
                    ? "Busca un dominio suplantado, urgencia y una solicitud de credenciales. No abras el enlace."
                    : Stage == Challenge.Isolation
                        ? "PC-02 es el equipo afectado. Aísla su red; no borres evidencias ni desconectes toda la sala."
                        : "Elige un respaldo anterior al incidente, verificado y desconectado de la red comprometida.";
                return false;
            }
            earnedPoints += Stage == Challenge.Backup ? 40 : 30;
            Score = Math.Max(0, earnedPoints - Mistakes * 10);
            Stage++;
            Feedback = Stage == Challenge.Isolation
                ? "Phishing reportado. El dominio y la petición de contraseña eran señales de fraude. Ahora aísla PC-02."
                : Stage == Challenge.Backup
                    ? "PC-02 está aislado de la red. Evidencias conservadas. Ahora selecciona el respaldo limpio."
                    : "Respaldo verificado restaurado en el entorno aislado. Incidente resuelto; valida el equipo antes de reconectarlo.";
            return true;
        }

        public void Reset()
        {
            Stage = Challenge.Email;
            Score = 0;
            earnedPoints = 0;
            Mistakes = 0;
            Feedback = "Revisa los mensajes y reporta el intento de phishing.";
        }
    }
}
