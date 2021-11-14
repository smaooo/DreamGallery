using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public class PostProcessingManager : MonoBehaviour
{
    public class ProfileParameters
    {
        public Bloom ProfileBloom;
        public ChromaticAberration ProfileChromaticAberration;
        public DepthOfField ProfileDoF;
        public MotionBlur ProfileMotionBlur;
        public ColorAdjustments ProfileColorAdjustment;
        public FilmGrain ProfileFilmGrain;
        public Vignette ProfileVignette;
        public LensDistortion ProfileLensDistortion;
        public ProfileParameters(VolumeProfile profile)
        {
            profile.TryGet<Bloom>(out ProfileBloom);
            profile.TryGet<ChromaticAberration>(out ProfileChromaticAberration);
            profile.TryGet<DepthOfField>(out ProfileDoF);
            profile.TryGet<MotionBlur>(out ProfileMotionBlur);
            profile.TryGet<ColorAdjustments>(out ProfileColorAdjustment);
            profile.TryGet<FilmGrain>(out ProfileFilmGrain);
            profile.TryGet<Vignette>(out ProfileVignette);
            profile.TryGet<LensDistortion>(out ProfileLensDistortion);
        }
        
    }

    [Header("Main Post-Processing Profiles")]
    public Volume firstLayerVolume;
    public Volume secondLayerVolume;
    private VolumeProfile firstLayerProfile;
    private VolumeProfile secondLayerProfile;

    [Header("Paintings Post-Processing Profiles")]
    public VolumeProfile paintingOnFocusProfile;
    public VolumeProfile paintingOutFocusProfile;
    public VolumeProfile generalBWProfile;
    public VolumeProfile paintingBeginningProfile;

    [Header("Puzzle Area Post-Processing Profiles")]
    public VolumeProfile puzzleFirstLayerProfile;
    public VolumeProfile puzzleSecondLayerProfile;


    private ProfileParameters firstLayerParameters;
    private ProfileParameters secondLayerParameters;
    public ProfileParameters paintingOnFocusParameters;
    public ProfileParameters paintingOutFocusParameters;
    public ProfileParameters generalBWParameters;
    public ProfileParameters puzzleFirstLayerParameters;
    public ProfileParameters puzzleSecondLayerParameters;
    public ProfileParameters paintingBeginningParameters;

    public enum TargetProfile
    {
        FirstLayer,
        SecondLayer
    }

    private void Awake()
    {
        firstLayerProfile = firstLayerVolume.GetComponent<Volume>().profile;
        secondLayerProfile = secondLayerVolume.GetComponent<Volume>().profile;
        firstLayerParameters = new ProfileParameters(firstLayerProfile);
        secondLayerParameters = new ProfileParameters(secondLayerProfile);
        paintingOnFocusParameters = new ProfileParameters(paintingOnFocusProfile);
        paintingOutFocusParameters = new ProfileParameters(paintingOutFocusProfile);
        generalBWParameters = new ProfileParameters(generalBWProfile);
        puzzleFirstLayerParameters = new ProfileParameters(puzzleFirstLayerProfile);
        puzzleSecondLayerParameters = new ProfileParameters(puzzleSecondLayerProfile);
        paintingBeginningParameters = new ProfileParameters(paintingBeginningProfile);

        InitiateLerping(TargetProfile.FirstLayer, generalBWParameters);
        InitiateLerping(TargetProfile.SecondLayer, paintingBeginningParameters);
    }

    
    public void InitiateLerping(TargetProfile target, ProfileParameters profile)
    {
        switch (target)
        {
            case TargetProfile.FirstLayer:
                StartCoroutine(LerpToFirstLayerProfile(firstLayerParameters, profile));
                break;
            case TargetProfile.SecondLayer:
                StartCoroutine(LerpToSecondLayerProfile(secondLayerParameters, profile));
                break;
        }
    }
    private IEnumerator LerpToFirstLayerProfile(ProfileParameters target, ProfileParameters values)
    {
        float timer = 0f;

        if (values.ProfileBloom != null)
        {
            if (values.ProfileBloom.active)
            {
                target.ProfileBloom.active = values.ProfileBloom.active;
                for (int i = 0; i < values.ProfileBloom.parameters.Count; i++)
                {
                    target.ProfileBloom.parameters[i].overrideState = values.ProfileBloom.parameters[i].overrideState;
                }
            }
        }
        if (values.ProfileChromaticAberration != null)
        {
            if (values.ProfileChromaticAberration.active)
            {
                target.ProfileChromaticAberration.active = values.ProfileChromaticAberration.active;
                for (int i = 0; i < values.ProfileChromaticAberration.parameters.Count; i++)
                {
                    target.ProfileChromaticAberration.parameters[i].overrideState = values.ProfileChromaticAberration.parameters[i].overrideState;
                }
            }
        }
        if (values.ProfileDoF != null)
        {target.ProfileDoF.active = values.ProfileDoF.active;
            for (int i = 0; i < values.ProfileDoF.parameters.Count; i++)
            {
                target.ProfileDoF.parameters[i].overrideState = values.ProfileDoF.parameters[i].overrideState;
            }
        }
        if (values.ProfileMotionBlur != null)
        {
            target.ProfileMotionBlur.active = values.ProfileMotionBlur.active;
            for (int i = 0; i < values.ProfileMotionBlur.parameters.Count; i++)
            {
                target.ProfileMotionBlur.parameters[i].overrideState = values.ProfileMotionBlur.parameters[i].overrideState;
            }
        } 
        if (values.ProfileColorAdjustment != null)
        {
            target.ProfileColorAdjustment.active = values.ProfileColorAdjustment.active;
            for (int i = 0; i < values.ProfileColorAdjustment.parameters.Count; i++)
            {
                target.ProfileColorAdjustment.parameters[i].overrideState = values.ProfileColorAdjustment.parameters[i].overrideState;
            }
        }
        if (values.ProfileFilmGrain != null)
        {
            target.ProfileFilmGrain.active = values.ProfileFilmGrain.active;
            for (int i = 0; i < values.ProfileFilmGrain.parameters.Count; i++)
            {
                target.ProfileFilmGrain.parameters[i].overrideState = values.ProfileFilmGrain.parameters[i].overrideState;
            }
        }
        if (values.ProfileVignette != null)
        {
            target.ProfileVignette.active = values.ProfileVignette.active;
            for (int i = 0; i < values.ProfileVignette.parameters.Count; i++)
            {
                target.ProfileVignette.parameters[i].overrideState = values.ProfileVignette.parameters[i].overrideState;
            }
        }
        if (values.ProfileLensDistortion != null)
        {
            target.ProfileLensDistortion.active = values.ProfileLensDistortion.active;
            for (int i = 0; i < values.ProfileLensDistortion.parameters.Count; i++)
            {
                target.ProfileLensDistortion.parameters[i].overrideState = values.ProfileLensDistortion.parameters[i].overrideState;
            }
        }

        while (timer < 2f)
        {
            yield return new WaitForEndOfFrame();

            timer += Time.deltaTime;

            if (values.ProfileBloom != null && values.ProfileBloom.active)
            {

                target.ProfileBloom.threshold.value = values.ProfileBloom.threshold.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.threshold.value, values.ProfileBloom.threshold.value, Mathf.Clamp01(timer)) : 0.9f;

                target.ProfileBloom.intensity.value = values.ProfileBloom.intensity.overrideState 
                    ? Mathf.SmoothStep(target.ProfileBloom.intensity.value, values.ProfileBloom.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileBloom.scatter.value = values.ProfileBloom.scatter.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.scatter.value, values.ProfileBloom.scatter.value, Mathf.Clamp01(timer)) : 0.7f;

                target.ProfileBloom.tint.value = values.ProfileBloom.tint.overrideState
                    ? Color.Lerp(target.ProfileBloom.tint.value, values.ProfileBloom.tint.value, Mathf.Clamp01(timer)) : Color.white;

                target.ProfileBloom.clamp.value = values.ProfileBloom.clamp.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.clamp.value, values.ProfileBloom.clamp.value, Mathf.Clamp01(timer)) : 65472f;

                target.ProfileBloom.dirtTexture.value = values.ProfileBloom.dirtTexture.overrideState
                    ? values.ProfileBloom.dirtTexture.value : null;

                target.ProfileBloom.dirtIntensity.value = values.ProfileBloom.dirtIntensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.dirtIntensity.value, values.ProfileBloom.dirtIntensity.value, Mathf.Clamp01(timer)) : 0;
            }

            if (values.ProfileChromaticAberration != null && values.ProfileChromaticAberration.active)
            {
                target.ProfileChromaticAberration.intensity.value = values.ProfileChromaticAberration.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileChromaticAberration.intensity.value, values.ProfileChromaticAberration.intensity.value, Mathf.Clamp01(timer)) : 0;
            }

            if (values.ProfileDoF != null && values.ProfileDoF.active)
            {
                target.ProfileDoF.mode.value = values.ProfileDoF.mode.overrideState ? values.ProfileDoF.mode.value : DepthOfFieldMode.Off;

                target.ProfileDoF.focusDistance.value = values.ProfileDoF.focusDistance.overrideState
                    ? Mathf.SmoothStep(target.ProfileDoF.focusDistance.value, values.ProfileDoF.focusDistance.value, Mathf.Clamp01(timer)) : 10;

                target.ProfileDoF.focalLength.value = values.ProfileDoF.focalLength.overrideState
                    ? Mathf.SmoothStep(target.ProfileDoF.focalLength.value, values.ProfileDoF.focalLength.value, Mathf.Clamp01(timer)) : 50;

                target.ProfileDoF.aperture.value = values.ProfileDoF.aperture.overrideState
                    ? Mathf.SmoothStep(target.ProfileDoF.aperture.value, values.ProfileDoF.aperture.value, Mathf.Clamp01(timer)) : 5.6f;
            }

            if (values.ProfileMotionBlur != null && values.ProfileMotionBlur.active)
            {
                target.ProfileMotionBlur.quality.value = values.ProfileMotionBlur.quality.overrideState
                    ? values.ProfileMotionBlur.quality.value : MotionBlurQuality.Low;

                target.ProfileMotionBlur.intensity.value = values.ProfileMotionBlur.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileMotionBlur.intensity.value, values.ProfileMotionBlur.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileMotionBlur.clamp.value = values.ProfileMotionBlur.clamp.overrideState
                    ? Mathf.SmoothStep(target.ProfileMotionBlur.clamp.value, values.ProfileMotionBlur.clamp.value, Mathf.Clamp01(timer)) : 0.05f;
            }

            if (values.ProfileColorAdjustment != null && values.ProfileColorAdjustment.active)
            {
                target.ProfileColorAdjustment.postExposure.value = values.ProfileColorAdjustment.postExposure.overrideState
                    ? Mathf.SmoothStep(target.ProfileColorAdjustment.postExposure.value, values.ProfileColorAdjustment.postExposure.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileColorAdjustment.contrast.value = values.ProfileColorAdjustment.contrast.overrideState 
                    ? Mathf.SmoothStep(target.ProfileColorAdjustment.contrast.value, values.ProfileColorAdjustment.contrast.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileColorAdjustment.colorFilter.value = values.ProfileColorAdjustment.colorFilter.overrideState
                    ? Color.Lerp(target.ProfileColorAdjustment.colorFilter.value, values.ProfileColorAdjustment.colorFilter.value, Mathf.Clamp01(timer)) : Color.white;

                target.ProfileColorAdjustment.hueShift.value = values.ProfileColorAdjustment.hueShift.overrideState
                    ? Mathf.SmoothStep(target.ProfileColorAdjustment.hueShift.value, values.ProfileColorAdjustment.hueShift.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileColorAdjustment.saturation.value = values.ProfileColorAdjustment.saturation.overrideState
                    ? Mathf.SmoothStep(target.ProfileColorAdjustment.saturation.value, values.ProfileColorAdjustment.saturation .value, Mathf.Clamp01(timer)) : 0;
            }

            if (values.ProfileFilmGrain != null && values.ProfileFilmGrain.active)
            {
                target.ProfileFilmGrain.type.value = values.ProfileFilmGrain.type.overrideState
                    ? values.ProfileFilmGrain.type.value : FilmGrainLookup.Thin1;

                target.ProfileFilmGrain.intensity.value = values.ProfileFilmGrain.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileFilmGrain.intensity.value, values.ProfileFilmGrain.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileFilmGrain.response.value = values.ProfileFilmGrain.response.overrideState
                    ? Mathf.SmoothStep(target.ProfileFilmGrain.response.value, values.ProfileFilmGrain.response.value, Mathf.Clamp01(timer)) : 0.8f ;
            }

            if (values.ProfileVignette != null && values.ProfileVignette.active)
            {
                target.ProfileVignette.color.value = values.ProfileVignette.color.overrideState
                    ? Color.Lerp(target.ProfileVignette.color.value, values.ProfileVignette.color.value, Mathf.Clamp01(timer)) : Color.black;

                target.ProfileVignette.center.value = values.ProfileVignette.center.overrideState
                    ? Vector2.Lerp(target.ProfileVignette.center.value, values.ProfileVignette.center.value, Mathf.Clamp01(timer)) : new Vector2(0.5f, 0.5f);

                target.ProfileVignette.intensity.value = values.ProfileVignette.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileVignette.intensity.value, values.ProfileVignette.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileVignette.smoothness.value = values.ProfileVignette.smoothness.overrideState
                    ? Mathf.SmoothStep(target.ProfileVignette.smoothness.value, values.ProfileVignette.smoothness.value, Mathf.Clamp01(timer)) : 0.2f;

                target.ProfileVignette.rounded.value = values.ProfileVignette.rounded.overrideState
                    ? values.ProfileVignette.rounded.value : false;
            }
            
            if (values.ProfileLensDistortion != null && values.ProfileLensDistortion.active)
            {
                target.ProfileLensDistortion.intensity.value = values.ProfileLensDistortion.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileLensDistortion.intensity.value, values.ProfileLensDistortion.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileLensDistortion.xMultiplier.value = values.ProfileLensDistortion.xMultiplier.overrideState
                    ? Mathf.SmoothStep(target.ProfileLensDistortion.xMultiplier.value, values.ProfileLensDistortion.xMultiplier.value, Mathf.Clamp01(timer)) : 1;

                target.ProfileLensDistortion.yMultiplier.value = values.ProfileLensDistortion.yMultiplier.overrideState
                    ? Mathf.SmoothStep(target.ProfileLensDistortion.yMultiplier.value, values.ProfileLensDistortion.yMultiplier.value, Mathf.Clamp01(timer)) : 1;
                    
                target.ProfileLensDistortion.center.value = values.ProfileLensDistortion.center.overrideState
                    ? Vector2.Lerp(target.ProfileLensDistortion.center.value, values.ProfileLensDistortion.center.value, Mathf.Clamp01(timer)) : new Vector2(0.5f, 0.5f);

                target.ProfileLensDistortion.scale.value = values.ProfileLensDistortion.center.overrideState
                    ? Mathf.SmoothStep(target.ProfileLensDistortion.scale.value, values.ProfileLensDistortion.scale.value, Mathf.Clamp01(timer)) : 1;
            }
            if (timer > 2f)
            {
                break;
            }
        }
    }
    private IEnumerator LerpToSecondLayerProfile(ProfileParameters target, ProfileParameters values)
    {
        float timer = 0f;

        if (values.ProfileBloom != null)
        {
            if (values.ProfileBloom.active)
            {
                target.ProfileBloom.active = values.ProfileBloom.active;
                for (int i = 0; i < values.ProfileBloom.parameters.Count; i++)
                {
                    target.ProfileBloom.parameters[i].overrideState = values.ProfileBloom.parameters[i].overrideState;
                }
            }
        }
        if (values.ProfileChromaticAberration != null)
        {
            if (values.ProfileChromaticAberration.active)
            {
                target.ProfileChromaticAberration.active = values.ProfileChromaticAberration.active;
                for (int i = 0; i < values.ProfileChromaticAberration.parameters.Count; i++)
                {
                    target.ProfileChromaticAberration.parameters[i].overrideState = values.ProfileChromaticAberration.parameters[i].overrideState;
                }
            }
        }
        if (values.ProfileDoF != null)
        {
            target.ProfileDoF.active = values.ProfileDoF.active;
            for (int i = 0; i < values.ProfileDoF.parameters.Count; i++)
            {
                target.ProfileDoF.parameters[i].overrideState = values.ProfileDoF.parameters[i].overrideState;
            }
        }
        if (values.ProfileMotionBlur != null)
        {
            target.ProfileMotionBlur.active = values.ProfileMotionBlur.active;
            for (int i = 0; i < values.ProfileMotionBlur.parameters.Count; i++)
            {
                target.ProfileMotionBlur.parameters[i].overrideState = values.ProfileMotionBlur.parameters[i].overrideState;
            }
        }
        if (values.ProfileColorAdjustment != null)
        {
            target.ProfileColorAdjustment.active = values.ProfileColorAdjustment.active;
            for (int i = 0; i < values.ProfileColorAdjustment.parameters.Count; i++)
            {
                target.ProfileColorAdjustment.parameters[i].overrideState = values.ProfileColorAdjustment.parameters[i].overrideState;
            }
        }
        if (values.ProfileFilmGrain != null)
        {
            target.ProfileFilmGrain.active = values.ProfileFilmGrain.active;
            for (int i = 0; i < values.ProfileFilmGrain.parameters.Count; i++)
            {
                target.ProfileFilmGrain.parameters[i].overrideState = values.ProfileFilmGrain.parameters[i].overrideState;
            }
        }
        if (values.ProfileVignette != null)
        {
            target.ProfileVignette.active = values.ProfileVignette.active;
            for (int i = 0; i < values.ProfileVignette.parameters.Count; i++)
            {
                target.ProfileVignette.parameters[i].overrideState = values.ProfileVignette.parameters[i].overrideState;
            }
        }
        if (values.ProfileLensDistortion != null)
        {
            //target.ProfileLensDistortion.active = values.ProfileLensDistortion.active;
            for (int i = 0; i < values.ProfileLensDistortion.parameters.Count; i++)
            {
                target.ProfileLensDistortion.parameters[i].overrideState = values.ProfileLensDistortion.parameters[i].overrideState;
            }
        }

        while (timer < 2f)
        {
            yield return new WaitForEndOfFrame();

            timer += Time.deltaTime;

            if (values.ProfileBloom != null && values.ProfileBloom.active)
            {

                target.ProfileBloom.threshold.value = values.ProfileBloom.threshold.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.threshold.value, values.ProfileBloom.threshold.value, Mathf.Clamp01(timer)) : 0.9f;

                target.ProfileBloom.intensity.value = values.ProfileBloom.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.intensity.value, values.ProfileBloom.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileBloom.scatter.value = values.ProfileBloom.scatter.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.scatter.value, values.ProfileBloom.scatter.value, Mathf.Clamp01(timer)) : 0.7f;

                target.ProfileBloom.tint.value = values.ProfileBloom.tint.overrideState
                    ? Color.Lerp(target.ProfileBloom.tint.value, values.ProfileBloom.tint.value, Mathf.Clamp01(timer)) : Color.white;

                target.ProfileBloom.clamp.value = values.ProfileBloom.clamp.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.clamp.value, values.ProfileBloom.clamp.value, Mathf.Clamp01(timer)) : 65472f;

                target.ProfileBloom.dirtTexture.value = values.ProfileBloom.dirtTexture.overrideState
                    ? values.ProfileBloom.dirtTexture.value : null;

                target.ProfileBloom.dirtIntensity.value = values.ProfileBloom.dirtIntensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileBloom.dirtIntensity.value, values.ProfileBloom.dirtIntensity.value, Mathf.Clamp01(timer)) : 0;
            }

            if (values.ProfileChromaticAberration != null && values.ProfileChromaticAberration.active)
            {
                target.ProfileChromaticAberration.intensity.value = values.ProfileChromaticAberration.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileChromaticAberration.intensity.value, values.ProfileChromaticAberration.intensity.value, Mathf.Clamp01(timer)) : 0;
            }

            if (values.ProfileDoF != null && values.ProfileDoF.active)
            {
                target.ProfileDoF.mode.value = values.ProfileDoF.mode.overrideState ? values.ProfileDoF.mode.value : DepthOfFieldMode.Off;

                target.ProfileDoF.focusDistance.value = values.ProfileDoF.focusDistance.overrideState
                    ? Mathf.SmoothStep(target.ProfileDoF.focusDistance.value, values.ProfileDoF.focusDistance.value, Mathf.Clamp01(timer)) : 10;

                target.ProfileDoF.focalLength.value = values.ProfileDoF.focalLength.overrideState
                    ? Mathf.SmoothStep(target.ProfileDoF.focalLength.value, values.ProfileDoF.focalLength.value, Mathf.Clamp01(timer)) : 50;

                target.ProfileDoF.aperture.value = values.ProfileDoF.aperture.overrideState
                    ? Mathf.SmoothStep(target.ProfileDoF.aperture.value, values.ProfileDoF.aperture.value, Mathf.Clamp01(timer)) : 5.6f;
            }

            if (values.ProfileMotionBlur != null && values.ProfileMotionBlur.active)
            {
                target.ProfileMotionBlur.quality.value = values.ProfileMotionBlur.quality.overrideState
                    ? values.ProfileMotionBlur.quality.value : MotionBlurQuality.Low;

                target.ProfileMotionBlur.intensity.value = values.ProfileMotionBlur.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileMotionBlur.intensity.value, values.ProfileMotionBlur.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileMotionBlur.clamp.value = values.ProfileMotionBlur.clamp.overrideState
                    ? Mathf.SmoothStep(target.ProfileMotionBlur.clamp.value, values.ProfileMotionBlur.clamp.value, Mathf.Clamp01(timer)) : 0.05f;
            }

            if (values.ProfileColorAdjustment != null && values.ProfileColorAdjustment.active)
            {
                target.ProfileColorAdjustment.postExposure.value = values.ProfileColorAdjustment.postExposure.overrideState
                    ? Mathf.SmoothStep(target.ProfileColorAdjustment.postExposure.value, values.ProfileColorAdjustment.postExposure.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileColorAdjustment.contrast.value = values.ProfileColorAdjustment.contrast.overrideState
                    ? Mathf.SmoothStep(target.ProfileColorAdjustment.contrast.value, values.ProfileColorAdjustment.contrast.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileColorAdjustment.colorFilter.value = values.ProfileColorAdjustment.colorFilter.overrideState
                    ? Color.Lerp(target.ProfileColorAdjustment.colorFilter.value, values.ProfileColorAdjustment.colorFilter.value, Mathf.Clamp01(timer)) : Color.white;

                target.ProfileColorAdjustment.hueShift.value = values.ProfileColorAdjustment.hueShift.overrideState
                    ? Mathf.SmoothStep(target.ProfileColorAdjustment.hueShift.value, values.ProfileColorAdjustment.hueShift.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileColorAdjustment.saturation.value = values.ProfileColorAdjustment.saturation.overrideState
                    ? Mathf.SmoothStep(target.ProfileColorAdjustment.saturation.value, values.ProfileColorAdjustment.saturation.value, Mathf.Clamp01(timer)) : 0;
            }

            if (values.ProfileFilmGrain != null && values.ProfileFilmGrain.active)
            {
                target.ProfileFilmGrain.type.value = values.ProfileFilmGrain.type.overrideState
                    ? values.ProfileFilmGrain.type.value : FilmGrainLookup.Thin1;

                target.ProfileFilmGrain.intensity.value = values.ProfileFilmGrain.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileFilmGrain.intensity.value, values.ProfileFilmGrain.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileFilmGrain.response.value = values.ProfileFilmGrain.response.overrideState
                    ? Mathf.SmoothStep(target.ProfileFilmGrain.response.value, values.ProfileFilmGrain.response.value, Mathf.Clamp01(timer)) : 0.8f;
            }

            if (values.ProfileVignette != null && values.ProfileVignette.active)
            {
                target.ProfileVignette.color.value = values.ProfileVignette.color.overrideState
                    ? Color.Lerp(target.ProfileVignette.color.value, values.ProfileVignette.color.value, Mathf.Clamp01(timer)) : Color.black;

                target.ProfileVignette.center.value = values.ProfileVignette.center.overrideState
                    ? Vector2.Lerp(target.ProfileVignette.center.value, values.ProfileVignette.center.value, Mathf.Clamp01(timer)) : new Vector2(0.5f, 0.5f);

                target.ProfileVignette.intensity.value = values.ProfileVignette.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileVignette.intensity.value, values.ProfileVignette.intensity.value, Mathf.Clamp01(timer)) : 0;

                target.ProfileVignette.smoothness.value = values.ProfileVignette.smoothness.overrideState
                    ? Mathf.SmoothStep(target.ProfileVignette.smoothness.value, values.ProfileVignette.smoothness.value, Mathf.Clamp01(timer)) : 0.2f;

                target.ProfileVignette.rounded.value = values.ProfileVignette.rounded.overrideState
                    ? values.ProfileVignette.rounded.value : false;
            }

            if (values.ProfileLensDistortion != null && values.ProfileLensDistortion.active)
            {
                target.ProfileLensDistortion.intensity.value = values.ProfileLensDistortion.intensity.overrideState
                    ? Mathf.SmoothStep(target.ProfileLensDistortion.intensity.value, values.ProfileLensDistortion.intensity.value, Mathf.Clamp01(timer/10)) : 0;
                

                target.ProfileLensDistortion.xMultiplier.value = values.ProfileLensDistortion.xMultiplier.overrideState
                    ? Mathf.SmoothStep(target.ProfileLensDistortion.xMultiplier.value, values.ProfileLensDistortion.xMultiplier.value, Mathf.Clamp01(timer)) : 1;

                target.ProfileLensDistortion.yMultiplier.value = values.ProfileLensDistortion.yMultiplier.overrideState
                    ? Mathf.SmoothStep(target.ProfileLensDistortion.yMultiplier.value, values.ProfileLensDistortion.yMultiplier.value, Mathf.Clamp01(timer)) : 1;

                target.ProfileLensDistortion.center.value = values.ProfileLensDistortion.center.overrideState
                    ? Vector2.Lerp(target.ProfileLensDistortion.center.value, values.ProfileLensDistortion.center.value, Mathf.Clamp01(timer)) : new Vector2(0.5f, 0.5f);

                target.ProfileLensDistortion.scale.value = values.ProfileLensDistortion.center.overrideState
                    ? Mathf.SmoothStep(target.ProfileLensDistortion.scale.value, values.ProfileLensDistortion.scale.value, Mathf.Clamp01(timer)) : 1;
            }
            if (timer > 2f)
            {
                break;
            }
        }
    }
}
