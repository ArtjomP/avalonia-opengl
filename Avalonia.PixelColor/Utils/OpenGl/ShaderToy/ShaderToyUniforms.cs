using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;
using Silk.NET.OpenGL;

namespace Avalonia.PixelColor.Utils.OpenGl.ShaderToy;

public class ShaderToyUniforms {
    public int iResolution = -1;
    public int iTime = -1;
    public int iTimeDelta = -1;
    public int iFrameRate = -1;
    public int iFrame = -1;
    public int iChannelTime = -1;
    public int iChannelResolution = -1;
    public int iMouse = -1;
    public int iDate = -1;
    public int iChannel0 = -1;
    public int iChannel1 = -1;
    public int iChannel2 = -1;
    public int iChannel3 = -1;
    public int iAudioFFT = -1;
    public int iAudioSamples = -1;
    public int iAudioLow = -1;
    public int iAudioMid = -1;
    public int iAudioHi = -1;
    public int iAudioRMS = -1;
    public int iForce = -1;
    public int iForce2 = -1;
    public int iForce3 = -1;
    public int iComplexity = -1;
    public int iNbItems = -1;
    public int iNbItems2 = -1;
    public int mColorMode = -1;

    public float prevTime = 0f;
    public long startMs = 0;
    public long lastMs = 0;
    public int frame = 0;

    public float audioLow = 1f;
    public float audioMid = 1f;
    public float audioHi = 1f;
    public float audioRMS = 1f;
    public float force = 1f;
    public float force2 = 1f;
    public float force3 = 1f;
    public int complexity = 1;
    public int nbItems = 2;
    public int nbItems2 = 1;
    public int colorMode = 1;


    public List<IsfSceneParameterOfSingle> FindParameters(uint shaderProgram, GL gl)
    {
        List<IsfSceneParameterOfSingle> parameters = [];
        foreach (var fieldInfo in GetType().GetFields())
        {
            if (fieldInfo.Name[0] != 'i' && fieldInfo.Name[0] != 'm' || char.IsLower(fieldInfo.Name[1]) ||
                fieldInfo.FieldType != typeof(int))
                continue;
            var loc = gl.GetUniformLocation(shaderProgram, fieldInfo.Name);
            fieldInfo.SetValue(this, loc);
            if (loc < 0)
                continue;

            var reflectName = fieldInfo.Name[1..].ToLower();
            var foundField = GetType().GetFields().ToList()
                .Find(e => e.Name.Equals(reflectName, StringComparison.CurrentCultureIgnoreCase));
            if (foundField is null)
                continue;

            var isInt = foundField.FieldType == typeof(int);
            parameters.Add(new IsfSceneParameterOfSingle(new OpenGlSceneParameter(fieldInfo.Name), 0,
                isInt ? 20 : 10));
        }

        return parameters;
    }

    public void SetUniforms(IEnumerable<IsfSceneParameterOfSingle> parameters)
    {
        var fields = GetType().GetFields();
        foreach (var e in parameters)
        {
            var reflectName = e.OpenGlSceneParameter.Name[1..].ToLower();

            foreach (var fieldInfo in fields)
            {
                if (!fieldInfo.Name.Equals(reflectName, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (fieldInfo.FieldType == typeof(int))
                    fieldInfo.SetValue(this, (int)e.CalculateValue());
                else if (fieldInfo.FieldType == typeof(float))
                    fieldInfo.SetValue(this, e.CalculateValue());
            }
        }
    }
}