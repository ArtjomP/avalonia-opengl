using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.IsfScene;
using Silk.NET.OpenGL;

namespace Avalonia.PixelColor.Utils.OpenGl.ShaderToy;

public class ShaderToyUniforms
{
    public Int32 iResolution = -1;
    public Int32 iTime = -1;
    public Int32 iTimeDelta = -1;
    public Int32 iFrameRate = -1;
    public Int32 iFrame = -1;
    public Int32 iChannelTime = -1;
    public Int32 iChannelResolution = -1;
    public Int32 iMouse = -1;
    public Int32 iDate = -1;
    public Int32 iChannel0 = -1;
    public Int32 iChannel1 = -1;
    public Int32 iChannel2 = -1;
    public Int32 iChannel3 = -1;
    public Int32 iAudioFFT = -1;
    public Int32 iAudioSamples = -1;
    public Int32 iAudioLow = -1;
    public Int32 iAudioMid = -1;
    public Int32 iAudioHi = -1;
    public Int32 iAudioRMS = -1;
    public Int32 iForce = -1;
    public Int32 iForce2 = -1;
    public Int32 iForce3 = -1;
    public Int32 iComplexity = -1;
    public Int32 iNbItems = -1;
    public Int32 iNbItems2 = -1;
    public Int32 mColorMode = -1;

    public Single prevTime = 0f;
    public Int64 startMs = 0;
    public Int64 lastMs = 0;
    public Int32 frame = 0;

    public Single audioLow = 1f;
    public Single audioMid = 1f;
    public Single audioHi = 1f;
    public Single audioRMS = 1f;
    public Single force = 1f;
    public Single force2 = 1f;
    public Single force3 = 1f;
    public Int32 complexity = 1;
    public Int32 nbItems = 2;
    public Int32 nbItems2 = 1;
    public Int32 colorMode = 1;


    public IEnumerable<IsfSceneParameterOfSingle> FindParameters(
        UInt32 shaderProgram, 
        GL gl)
    {
        List<IsfSceneParameterOfSingle> parameters = [];
        foreach (var fieldInfo in GetType().GetFields())
        {
            if ((fieldInfo.Name[0] == 'i' || fieldInfo.Name[0] == 'm') &&
                !Char.IsLower(fieldInfo.Name[1]) &&
                fieldInfo.FieldType == typeof(Int32))
            {
                Int32 loc = gl.GetUniformLocation(shaderProgram, fieldInfo.Name);
                fieldInfo.SetValue(obj: this, value: loc);
                if (loc >= 0)
                {
                    String reflectName = fieldInfo.Name[1..].ToLower();
                    var foundField = GetType()
                        .GetFields()
                        .ToArray()
                        .SingleOrDefault(e => e.Name.Equals(reflectName, StringComparison.CurrentCultureIgnoreCase));
                    if (foundField is not null)
                    {
                        var isInt32 = foundField.FieldType == typeof(Int32);
                        OpenGlSceneParameter openGlSceneParameter = new(name: fieldInfo.Name)
                        {
                            Value = 128
                        };
                        IsfSceneParameterOfSingle parameter = new(
                            sceneParameter: openGlSceneParameter,
                            min: 0,
                            max: isInt32 ? 20 : 10);
                        parameters.Add(parameter);
                    }
                }
            }
        }

        return [..parameters];
    }

    public void SetUniforms(
        IEnumerable<IsfSceneParameterOfSingle> parameters)
    {
        FieldInfo[] fields = GetType().GetFields();
        foreach (IsfSceneParameterOfSingle e in parameters)
        {
            String reflectName = e.OpenGlSceneParameter.Name[1..].ToLower();
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.Name.Equals(
                        value: reflectName, 
                        comparisonType: StringComparison.CurrentCultureIgnoreCase))
                {
                    if (fieldInfo.FieldType == typeof(Int32))
                    {
                        fieldInfo.SetValue(this, (Int32)e.CalculateValue());
                    }
                    else if (fieldInfo.FieldType == typeof(Single))
                    {
                        fieldInfo.SetValue(this, e.CalculateValue());
                    }
                }
            }
        }
    }
}