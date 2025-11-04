using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace LoaderSetupPageView.Templates.TreeViewTemplate
{
    public class ParamToItemSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<Node> rootNodes = new ObservableCollection<Node>();

            if (value != null)
            {
                string rootName = "";
                if (value is IList)
                    rootName = "Items";
                else
                    rootName = value.GetType().Name;

                rootNodes.Add(NodeConverter.CreateGroupNode(null, value, null, rootName, true));
            }
            else
            {
                rootNodes.Add(new UndefinedNode(null));
            }

            return rootNodes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class NodeConverter
    {
        private static HashSet<Type> _NumericTypeSet = new HashSet<Type>()
        {
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
        };

        public static Node CreateGroupNode(Node parentNode, object instance, object ownerInstance, string nodeName, bool canWrite)
        {
            Node node;
            try
            {
            var instType = instance.GetType();

            if (instance is IList)
            {
                ListNode listNode = new ListNode(parentNode);
                listNode.Text = nodeName;
                listNode.OwnerInstance = ownerInstance;
                listNode.PropertyName = nodeName;

                var list = instance as IList;

                foreach (var item in list)
                {
                    var childNode = CreateGroupNode(listNode, item, list, item.GetType().Name, true);

                    listNode.Nodes.Add(childNode);
                }

                node = listNode;
            }
            else if (instType.IsClass && instType != typeof(string))
            {
                ClassNode classNode = new ClassNode(parentNode);
                classNode.Text = nodeName;
                classNode.OwnerInstance = ownerInstance;
                classNode.PropertyName = nodeName;

                foreach (var pi in instance.GetType().GetProperties())
                {
                    var inst = pi.GetValue(instance);

                    if (inst == null)
                    {
                        if (pi.PropertyType == typeof(string))
                        {
                            pi.SetValue(instance, string.Empty);
                        }
                        else //class type
                        {
                            var defaultVal = Activator.CreateInstance(pi.PropertyType);
                            pi.SetValue(instance, defaultVal);
                        }

                        inst = pi.GetValue(instance);
                    }

                    var childNode = CreateGroupNode(classNode, inst, instance, pi.Name, pi.CanWrite);

                    classNode.Nodes.Add(childNode);
                }

                node = classNode;
            }
            else if (canWrite == false)
            {
                ReadOnlyNode leafNode = new ReadOnlyNode(parentNode);
                leafNode.Text = nodeName;
                leafNode.OwnerInstance = ownerInstance;
                leafNode.PropertyName = nodeName;

                node = leafNode;
            }
            else if (instType == typeof(string))
            {
                StringNode leafNode = new StringNode(parentNode);
                leafNode.Text = nodeName;
                leafNode.OwnerInstance = ownerInstance;
                leafNode.PropertyName = nodeName;

                node = leafNode;
            }
            else if (instType == typeof(bool))
            {
                BoolNode leafNode = new BoolNode(parentNode);
                leafNode.Text = nodeName;
                leafNode.OwnerInstance = ownerInstance;
                leafNode.PropertyName = nodeName;

                node = leafNode;
            }
            else if (_NumericTypeSet.Contains(instType))
            {
                NumericNode leafNode = new NumericNode(parentNode);
                leafNode.Text = nodeName;
                leafNode.OwnerInstance = ownerInstance;
                leafNode.PropertyName = nodeName;

                node = leafNode;
            }
            else if (instType.IsEnum)
            {
                EnumNode leafNode = new EnumNode(parentNode);
                leafNode.Text = nodeName;
                leafNode.OwnerInstance = ownerInstance;
                leafNode.PropertyName = nodeName;
                leafNode.Range = Enum.GetValues(instType);

                node = leafNode;
            }
            else
            {
                node = new UndefinedNode(parentNode);
                node.Text = nodeName;
            }
            }
            catch (Exception err)
            {
                 throw;
            }
            return node;
        }

    }
}
