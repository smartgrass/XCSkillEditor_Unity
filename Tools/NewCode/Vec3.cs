//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;
using SimpleJSON;



namespace cfg
{

public sealed partial class Vec3 :  Bright.Config.BeanBase 
{
    public Vec3(JSONNode _json) 
    {
        { if(!_json["x"].IsNumber) { throw new SerializationException(); }  X = _json["x"]; }
        { if(!_json["y"].IsNumber) { throw new SerializationException(); }  Y = _json["y"]; }
        { if(!_json["z"].IsNumber) { throw new SerializationException(); }  Z = _json["z"]; }
        PostInit();
    }

    public Vec3(float x, float y, float z ) 
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        PostInit();
    }

    public static Vec3 DeserializeVec3(JSONNode _json)
    {
        return new Vec3(_json);
    }

    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }

    public const int __ID__ = 2662207;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, object> _tables)
    {
        PostResolve();
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "X:" + X + ","
        + "Y:" + Y + ","
        + "Z:" + Z + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}
}
