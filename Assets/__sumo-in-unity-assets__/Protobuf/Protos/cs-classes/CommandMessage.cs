// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: CommandMessage.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from CommandMessage.proto</summary>
public static partial class CommandMessageReflection {

  #region Descriptor
  /// <summary>File descriptor for CommandMessage.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static CommandMessageReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "ChRDb21tYW5kTWVzc2FnZS5wcm90byJeCg5Db21tYW5kTWVzc2FnZRIVCg1j",
          "bWRfdmFyX2J5dGVzGAEgASgMEgoKAmlkGAIgASgJEhMKC2Zsb2F0X3ZhbHVl",
          "GAMgASgCEhQKDGRvdWJsZV92YWx1ZRgEIAEoASIwCgtDb21tYW5kTGlzdBIh",
          "CghtZXNzYWdlcxgBIAMoCzIPLkNvbW1hbmRNZXNzYWdlYgZwcm90bzM="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { },
        new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::CommandMessage), global::CommandMessage.Parser, new[]{ "CmdVarBytes", "Id", "FloatValue", "DoubleValue" }, null, null, null),
          new pbr::GeneratedClrTypeInfo(typeof(global::CommandList), global::CommandList.Parser, new[]{ "Messages" }, null, null, null)
        }));
  }
  #endregion

}
#region Messages
/// <summary>
/// Message to be send by c# client to python client in order to process it. 
/// </summary>
public sealed partial class CommandMessage : pb::IMessage<CommandMessage> {
  private static readonly pb::MessageParser<CommandMessage> _parser = new pb::MessageParser<CommandMessage>(() => new CommandMessage());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<CommandMessage> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::CommandMessageReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CommandMessage() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CommandMessage(CommandMessage other) : this() {
    cmdVarBytes_ = other.cmdVarBytes_;
    id_ = other.id_;
    floatValue_ = other.floatValue_;
    doubleValue_ = other.doubleValue_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CommandMessage Clone() {
    return new CommandMessage(this);
  }

  /// <summary>Field number for the "cmd_var_bytes" field.</summary>
  public const int CmdVarBytesFieldNumber = 1;
  private pb::ByteString cmdVarBytes_ = pb::ByteString.Empty;
  /// <summary>
  /// Contains two bytes. The first one is the byte for the Command and the second is the Variable to set byte. 
  /// </summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public pb::ByteString CmdVarBytes {
    get { return cmdVarBytes_; }
    set {
      cmdVarBytes_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "id" field.</summary>
  public const int IdFieldNumber = 2;
  private string id_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public string Id {
    get { return id_; }
    set {
      id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "float_value" field.</summary>
  public const int FloatValueFieldNumber = 3;
  private float floatValue_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public float FloatValue {
    get { return floatValue_; }
    set {
      floatValue_ = value;
    }
  }

  /// <summary>Field number for the "double_value" field.</summary>
  public const int DoubleValueFieldNumber = 4;
  private double doubleValue_;
  /// <summary>
  /// Maybe extend it with int, sint uint e.t.c https://developers.google.com/protocol-buffers/docs/proto3 
  /// </summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public double DoubleValue {
    get { return doubleValue_; }
    set {
      doubleValue_ = value;
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as CommandMessage);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(CommandMessage other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (CmdVarBytes != other.CmdVarBytes) return false;
    if (Id != other.Id) return false;
    if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(FloatValue, other.FloatValue)) return false;
    if (!pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.Equals(DoubleValue, other.DoubleValue)) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    if (CmdVarBytes.Length != 0) hash ^= CmdVarBytes.GetHashCode();
    if (Id.Length != 0) hash ^= Id.GetHashCode();
    if (FloatValue != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(FloatValue);
    if (DoubleValue != 0D) hash ^= pbc::ProtobufEqualityComparers.BitwiseDoubleEqualityComparer.GetHashCode(DoubleValue);
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
    if (CmdVarBytes.Length != 0) {
      output.WriteRawTag(10);
      output.WriteBytes(CmdVarBytes);
    }
    if (Id.Length != 0) {
      output.WriteRawTag(18);
      output.WriteString(Id);
    }
    if (FloatValue != 0F) {
      output.WriteRawTag(29);
      output.WriteFloat(FloatValue);
    }
    if (DoubleValue != 0D) {
      output.WriteRawTag(33);
      output.WriteDouble(DoubleValue);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    if (CmdVarBytes.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeBytesSize(CmdVarBytes);
    }
    if (Id.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
    }
    if (FloatValue != 0F) {
      size += 1 + 4;
    }
    if (DoubleValue != 0D) {
      size += 1 + 8;
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(CommandMessage other) {
    if (other == null) {
      return;
    }
    if (other.CmdVarBytes.Length != 0) {
      CmdVarBytes = other.CmdVarBytes;
    }
    if (other.Id.Length != 0) {
      Id = other.Id;
    }
    if (other.FloatValue != 0F) {
      FloatValue = other.FloatValue;
    }
    if (other.DoubleValue != 0D) {
      DoubleValue = other.DoubleValue;
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
          CmdVarBytes = input.ReadBytes();
          break;
        }
        case 18: {
          Id = input.ReadString();
          break;
        }
        case 29: {
          FloatValue = input.ReadFloat();
          break;
        }
        case 33: {
          DoubleValue = input.ReadDouble();
          break;
        }
      }
    }
  }

}

/// <summary>
/// We may want to send multiple commands to python. Repeated means 0 or more times. 
/// </summary>
public sealed partial class CommandList : pb::IMessage<CommandList> {
  private static readonly pb::MessageParser<CommandList> _parser = new pb::MessageParser<CommandList>(() => new CommandList());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pb::MessageParser<CommandList> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::CommandMessageReflection.Descriptor.MessageTypes[1]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CommandList() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CommandList(CommandList other) : this() {
    messages_ = other.messages_.Clone();
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public CommandList Clone() {
    return new CommandList(this);
  }

  /// <summary>Field number for the "messages" field.</summary>
  public const int MessagesFieldNumber = 1;
  private static readonly pb::FieldCodec<global::CommandMessage> _repeated_messages_codec
      = pb::FieldCodec.ForMessage(10, global::CommandMessage.Parser);
  private readonly pbc::RepeatedField<global::CommandMessage> messages_ = new pbc::RepeatedField<global::CommandMessage>();
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public pbc::RepeatedField<global::CommandMessage> Messages {
    get { return messages_; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override bool Equals(object other) {
    return Equals(other as CommandList);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public bool Equals(CommandList other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if(!messages_.Equals(other.messages_)) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public override int GetHashCode() {
    int hash = 1;
    hash ^= messages_.GetHashCode();
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
    messages_.WriteTo(output, _repeated_messages_codec);
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public int CalculateSize() {
    int size = 0;
    size += messages_.CalculateSize(_repeated_messages_codec);
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  public void MergeFrom(CommandList other) {
    if (other == null) {
      return;
    }
    messages_.Add(other.messages_);
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
          messages_.AddEntriesFrom(input, _repeated_messages_codec);
          break;
        }
      }
    }
  }

}

#endregion


#endregion Designer generated code