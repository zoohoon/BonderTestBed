using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;

namespace ProberInterfaces.Proxies.Behaviors
{
    internal class ReferencePreservingBehavior : DataContractSerializerOperationBehavior
    {

        public ReferencePreservingBehavior(OperationDescription operationDescription)
            : base(operationDescription)
        {
            MaxItemsInObjectGraph = 2147483647;
        }

        public override XmlObjectSerializer CreateSerializer(
            Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes,
                                              int.MaxValue,
                                              IgnoreExtensionDataObject,
                                              true, // preserveObjectReferences
                                              DataContractSurrogate);
        }

        public override XmlObjectSerializer CreateSerializer(
            Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes,
                                              int.MaxValue,
                                              IgnoreExtensionDataObject,
                                              true, // preserveObjectReferences
                                              DataContractSurrogate);
        }

    }
}
