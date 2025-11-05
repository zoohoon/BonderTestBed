using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProberDevelopPackWindow.Tab.Run.Gem
{
    public class SecsItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(SecsItem));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObj = JObject.Load(reader);
            SecsItemType itemType = (SecsItemType)Enum.Parse(typeof(SecsItemType), (string)jsonObj["Type"]);
            SecsItem item = null;

            switch (itemType)
            {
                case SecsItemType.Ascii:
                    item = new SecsItemAscii();
                    serializer.Populate(jsonObj.CreateReader(), item);
                    break;
                case SecsItemType.U1:
                    item = new SecsItemU1();
                    serializer.Populate(jsonObj.CreateReader(), item);
                    break;
                case SecsItemType.U2:
                    item = new SecsItemU2();
                    serializer.Populate(jsonObj.CreateReader(), item);
                    break;
                case SecsItemType.U4:
                    item = new SecsItemU4();
                    serializer.Populate(jsonObj.CreateReader(), item);
                    break;
                case SecsItemType.List:
                    item = new SecsItemList();
                    var items = jsonObj["Items"] as JArray;
                    if (items != null)
                    {
                        foreach (var child in items)
                        {
                            var childReader = child.CreateReader();
                            // Recursive call to handle nested lists or other item types
                            SecsItem childItem = (SecsItem)ReadJson(childReader, typeof(SecsItem), null, serializer);
                            ((SecsItemList)item).Items.Add(childItem);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unsupported SecsItemType: {itemType}");
            }

            return item;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public enum SecsItemType
    {
        List,
        Ascii,
        Binary,
        Boolean,
        U1,
        U2,
        U4,
        // Add other necessary types
    }

    public abstract class SecsItem
    {
        public SecsItemType Type { get; set; }

        // Abstract Clone method to be implemented by subclasses
        public abstract SecsItem Clone();

        public static SecsItem Create(SecsItemType type, object value = null)
        {
            switch (type)
            {
                case SecsItemType.List:
                    return new SecsItemList { Type = type };
                case SecsItemType.Ascii:
                    return new SecsItemAscii { Type = type, Value = value as string };
                case SecsItemType.U1:
                    byte u1Value = (byte)value;
                    return new SecsItemU1 { Type = type, Value = new byte[] { u1Value } };
                case SecsItemType.U2:
                    uint u2Value = (uint)value;
                    return new SecsItemU2 { Type = type, Value = new uint[] { u2Value } };
                case SecsItemType.U4:
                    uint u4Value = (uint)value;
                    return new SecsItemU4 { Type = type, Value = new uint[] { u4Value } };
                // Add cases for other types
                default:
                    throw new NotImplementedException($"Type {type} not implemented.");
            }
        }
    }

    public class SecsItemList : SecsItem
    {
        public List<SecsItem> Items { get; set; } = new List<SecsItem>();

        public void AddItem(SecsItem item)
        {
            Items.Add(item);
        }

        // Implementing Clone for SecsItemList
        public override SecsItem Clone()
        {
            var clone = new SecsItemList { Type = this.Type };
            foreach (var item in Items)
            {
                clone.AddItem(item.Clone()); // Ensure deep copy
            }
            return clone;
        }
    }

    public class SecsItemAscii : SecsItem
    {
        public string Value { get; set; }

        // Implementing Clone for SecsItemAscii
        public override SecsItem Clone()
        {
            return new SecsItemAscii { Type = this.Type, Value = this.Value };
        }
    }

    public class SecsItemU1 : SecsItem
    {
        public byte[] Value { get; set; }

        public override SecsItem Clone()
        {
            return new SecsItemU1 { Type = this.Type, Value = this.Value };
        }
    }
    public class SecsItemU2 : SecsItem
    {
        public uint[] Value { get; set; }

        public override SecsItem Clone()
        {
            return new SecsItemU2 { Type = this.Type, Value = this.Value };
        }
    }
    public class SecsItemU4 : SecsItem
    {
        public uint[] Value { get; set; }

        public override SecsItem Clone()
        {
            return new SecsItemU4 { Type = this.Type, Value = this.Value };
        }
    }
}
