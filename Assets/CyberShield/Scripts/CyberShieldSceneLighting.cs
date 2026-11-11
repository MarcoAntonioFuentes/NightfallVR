using UnityEngine;
using UnityEngine.Rendering;

namespace CyberShield
{
    public sealed class CyberShieldSceneLighting : MonoBehaviour
    {
        public RenderPipelineAsset sceneProfile;
        RenderPipelineAsset previous;
        bool applied;
        void OnEnable()
        {
            if(!sceneProfile)return;
            previous=QualitySettings.renderPipeline;
            QualitySettings.renderPipeline=sceneProfile;
            applied=true;
        }
        void OnDisable()
        {
            if(applied&&QualitySettings.renderPipeline==sceneProfile)QualitySettings.renderPipeline=previous;
            applied=false;
        }
    }
}
