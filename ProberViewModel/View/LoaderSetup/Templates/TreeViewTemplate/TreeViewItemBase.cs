using System;

using System.Collections.ObjectModel;
using System.Collections;
using RelayCommandBase;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoaderSetupPageView.Templates.TreeViewTemplate
{
    public abstract class Node : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public Node(Node parent)
        {
            try
            {
                this.ParentNode = parent;
                this.IsListItemNode = parent is ListNode;
            }
            catch (Exception err)
            {

                throw;
            }
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set { _Text = value; RaisePropertyChanged(); }
        }

        public Node ParentNode { get; private set; }

        public virtual object OwnerInstance { get; set; }

        public string PropertyName { get; set; }

        public bool IsListItemNode { get; set; }

        private ICommand _RemoveItemCommand;
        public ICommand RemoveItemCommand
        {
            get { return _RemoveItemCommand ?? (_RemoveItemCommand = new RelayCommand(RemoveItem)); }
        }

        private void RemoveItem()
        {
            try
            {
                ListNode parentListNode = ParentNode as ListNode;
                int index = parentListNode.Nodes.IndexOf(this);
                parentListNode.Nodes.Remove(this);

                var list = OwnerInstance as IList;
                list.RemoveAt(index);
            }
            catch (Exception err)
            {

                throw;
            }
        }
    }

    public class ClassNode : Node
    {
        public ClassNode(Node parent) : base(parent) { }

        private ObservableCollection<Node> _Nodes = new ObservableCollection<Node>();
        public ObservableCollection<Node> Nodes
        {
            get { return _Nodes; }
            set { _Nodes = value; RaisePropertyChanged(); }
        }
    }

    public class ListNode : Node
    {
        public ListNode(Node parent) : base(parent) { }

        private ObservableCollection<Node> _Nodes = new ObservableCollection<Node>();
        public ObservableCollection<Node> Nodes
        {
            get { return _Nodes; }
            set { _Nodes = value; RaisePropertyChanged(); }
        }

        private ICommand _NewItemCommand;
        public ICommand NewItemCommand
        {
            get { return _NewItemCommand ?? (_NewItemCommand = new RelayCommand(NewItem)); }
        }


        private async void NewItem()
        {
            try
            {
                var currInst = OwnerInstance.GetType().GetProperty(PropertyName).GetValue(OwnerInstance);

                var currType = currInst.GetType();

                Type genericArgType = null;

                while (true)
                {
                    var genericArgs = currType.GetGenericArguments();
                    if (genericArgs != null && genericArgs.Length > 0)
                    {
                        genericArgType = genericArgs[0];
                        break;
                    }

                    currType = currType.BaseType;

                     System.Threading.Thread.Sleep(1);
                }

                //Create instance
                var item = Activator.CreateInstance(genericArgType);
                (currInst as IList).Add(item);

                //Create node
                var childNode = NodeConverter.CreateGroupNode(this, item, currInst, genericArgType.Name, true);

                Nodes.Add(childNode);
            }
            catch (Exception err)
            {

                throw;
            }
        }
    }

    public class UndefinedNode : Node
    {
        public UndefinedNode(Node parent) : base(parent)
        {
            this.Text = "Undefined";
        }
    }

    public abstract class ValueNode : Node
    {
        public ValueNode(Node parent) : base(parent)
        {

        }

        private object _OwnerInstance;
        public override object OwnerInstance
        {
            get { return _OwnerInstance; }
            set
            {
                _OwnerInstance = value;
                RaisePropertyChanged();

                if (_OwnerInstance is INotifyPropertyChanged)
                {
                    (_OwnerInstance as INotifyPropertyChanged).PropertyChanged += Node_PropertyChanged;
                }
            }
        }

        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == this.PropertyName)
            {
                RaisePropertyChanged(nameof(Value));
            }
        }


        public abstract object Value { get; set; }
    }

    public abstract class PrimitiveNode : ValueNode
    {
        public PrimitiveNode(Node parent) : base(parent) { }

        public override object Value
        {
            get
            {
                var val = OwnerInstance.GetType().GetProperty(PropertyName).GetValue(OwnerInstance);
                return val;
            }
            set
            {
                if (IsListItemNode)
                {
                    try
                    {
                        ListNode parentListNode = ParentNode as ListNode;
                        int index = parentListNode.Nodes.IndexOf(this);

                        var list = OwnerInstance as IList;
                        list[index] = value;
                    }
                    catch (Exception err)
                    {

                        throw;
                    }
                }
                else
                {
                    var pi = OwnerInstance.GetType().GetProperty(PropertyName);

                    try
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(pi.PropertyType);
                        object propValue = typeConverter.ConvertFromString(value.ToString());

                        pi.SetValue(OwnerInstance, propValue);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                RaisePropertyChanged();
            }
        }
    }

    public class ReadOnlyNode : ValueNode
    {
        public ReadOnlyNode(Node parent) : base(parent) { }

        public override object Value
        {
            get
            {
                var val = OwnerInstance.GetType().GetProperty(PropertyName).GetValue(OwnerInstance);
                return val;
            }
            set
            {

            }
        }
    }

    public class NumericNode : PrimitiveNode
    {
        public NumericNode(Node parent) : base(parent) { }

    }

    public class BoolNode : PrimitiveNode
    {
        public BoolNode(Node parent) : base(parent) { }
    }

    public class StringNode : PrimitiveNode
    {
        public StringNode(Node parent) : base(parent) { }

    }

    public class EnumNode : PrimitiveNode
    {
        public EnumNode(Node parent) : base(parent) { }

        private Array _Range;
        public Array Range
        {
            get { return _Range; }
            set { _Range = value; RaisePropertyChanged(); }
        }
    }
}
