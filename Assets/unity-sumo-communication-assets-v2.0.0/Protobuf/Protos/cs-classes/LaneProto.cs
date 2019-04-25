// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: LaneProto.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from LaneProto.proto</summary>
public static partial class LaneProtoReflection {

  #region Descriptor
  /// <summary>File descriptor for LaneProto.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static LaneProtoReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "Cg9MYW5lUHJvdG8ucHJvdG8iVgoJTGFuZVByb3RvEgoKAmlkGAEgASgJEg0K",
          "BWluZGV4GAIgASgNEg0KBXNwZWVkGAMgASgCEg4KBmxlbmd0aBgEIAEoAhIP",
          "CgdlZGdlX2lkGAUgASgJYgZwcm90bzM="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { },
        new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::LaneProto), global::LaneProto.Parser, new[]{ "Id", "Index", "Speed", "Length", "EdgeId" }, null, null, null)
        }));
  }
  #endregion

}
#region Messages
public sealed partial class LaneProto : pb::IMessage<LaneProto> {
  private static readonly pb::MessageParser<LaneProto> _parser = new pb::MessageParser<LaneProto>(() => new LaneProto());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<LaneProto> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::LaneProtoReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public LaneProto() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public LaneProto(LaneProto other) : this() {
    id_ = other.id_;
    index_ = other.index_;
    speed_ = other.speed_;
    length_ = other.length_;
    edgeId_ = other.edgeId_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public LaneProto Clone() {
    return new LaneProto(this);
  }

  /// <summary>Field number for the "id" field.</summary>
  public const int IdFieldNumber = 1;
  private string id_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public string Id {
    get { return id_; }
    set {
      id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "index" field.</summary>
  public const int IndexFieldNumber = 2;
  private uint index_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public uint Index {
    get { return index_; }
    set {
      index_ = value;
    }
  }

  /// <summary>Field number for the "speed" field.</summary>
  public const int SpeedFieldNumber = 3;
  private float speed_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public float Speed {
    get { return speed_; }
    set {
      speed_ = value;
    }
  }

  /// <summary>Field number for the "length" field.</summary>
  public const int LengthFieldNumber = 4;
  private float length_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public float Length {
    get { return length_; }
    set {
      length_ = value;
    }
  }

  /// <summary>Field number for the "edge_id" field.</summary>
  public const int EdgeIdFieldNumber = 5;
  private string edgeId_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public string EdgeId {
    get { return edgeId_; }
    set {
      edgeId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as LaneProto);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(LaneProto other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Id != other.Id) return false;
    if (Index != other.Index) return false;
    if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Speed, other.Speed)) return false;
    if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Length, other.Length)) return false;
    if (EdgeId != other.EdgeId) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (Id.Length != 0) hash ^= Id.GetHashCode();
    if (Index != 0) hash ^= Index.GetHashCode();
    if (Speed != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Speed);
    if (Length != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Length);
    if (EdgeId.Length != 0) hash ^= EdgeId.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void WriteTo(pb::CodedOutputStream output) {
    if (Id.Length != 0) {
      output.WriteRawTag(10);
      output.WriteString(Id);
    }
    if (Index != 0) {
      output.WriteRawTag(16);
      output.WriteUInt32(Index);
    }
    if (Speed != 0F) {
      output.WriteRawTag(29);
      output.WriteFloat(Speed);
    }
    if (Length != 0F) {
      output.WriteRawTag(37);
      output.WriteFloat(Length);
    }
    if (EdgeId.Length != 0) {
      output.WriteRawTag(42);
      output.WriteString(EdgeId);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (Id.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
    }
    if (Index != 0) {
      size += 1 + pb::CodedOutputStream.ComputeUInt32Size(Index);
    }
    if (Speed != 0F) {
      size += 1 + 4;
    }
    if (Length != 0F) {
      size += 1 + 4;
    }
    if (EdgeId.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(EdgeId);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(LaneProto other) {
    if (other == null) {
      return;
    }
    if (other.Id.Length != 0) {
      Id = other.Id;
    }
    if (other.Index != 0) {
      Index = other.Index;
    }
    if (other.Speed != 0F) {
      Speed = other.Speed;
    }
    if (other.Length != 0F) {
      Length = other.Length;
    }
    if (other.EdgeId.Length != 0) {
      EdgeId = other.EdgeId;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(pb::CodedInputStream input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          Id = input.ReadString();
          break;
        }
        case 16: {
          Index = input.ReadUInt32();
          break;
        }
        case 29: {
          Speed = input.ReadFloat();
          break;
        }
        case 37: {
          Length = input.ReadFloat();
          break;
        }
        case 42: {
          EdgeId = input.ReadString();
          break;
        }
      }
    }
  }

}

#endregion


#endregion Designer generated code
