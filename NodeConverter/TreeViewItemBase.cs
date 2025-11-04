using LogModule;
using RelayCommandBase;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NodeConverter
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
                LoggerManager.Exception(err);
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
                LoggerManager.Exception(err);
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
            get { return _NewItemCommand ?? (_NewItemCommand = new AsyncCommand(NewItem)); }
        }

        private Task NewItem()
        {
            try
            {
                if (OwnerInstance == null)
                    return Task.CompletedTask;

                var currInst = OwnerInstance.GetType().GetProperty(PropertyName).GetValue(OwnerInstance);

                var currType = currInst.GetType();
                if (currType.IsArray)
                    return Task.CompletedTask;

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

                        //_delays.DelayFor(1);
                         System.Threading.Thread.Sleep(1);
                }

                if (genericArgType.IsAbstract)
                {
                    //Nothing 
                }
                else
                {
                    //Create instance
                    var item = Activator.CreateInstance(genericArgType);
                    (currInst as IList).Add(item);

                    //Create node
                    var childNode = NodeConverter.CreateGroupNode(this, item, currInst, genericArgType.Name, true);

                    Nodes.Add(childNode);
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.CompletedTask;
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
                if (IsListItemNode)
                {
                    ListNode parentListNode = ParentNode as ListNode;
                    int index = parentListNode.Nodes.IndexOf(this);

                    return (OwnerInstance as IList)[index];
                }
                else
                {
                    var val = OwnerInstance.GetType().GetProperty(PropertyName).GetValue(OwnerInstance);
                    return val;
                }
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
                        LoggerManager.Exception(err);
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
    public class GUIDNode : PrimitiveNode
    {
        public GUIDNode(Node parent) : base(parent) { }

        private ObservableCollection<Node> _Nodes = new ObservableCollection<Node>();
        public ObservableCollection<Node> Nodes
        {
            get { return _Nodes; }
            set { _Nodes = value; RaisePropertyChanged(); }
        }

        private ICommand _NewItemCommand;
        public ICommand NewItemCommand
        {
            get { return _NewItemCommand ?? (_NewItemCommand = new AsyncCommand(NewItem)); }
        }

        private Task NewItem()
        {
            try
            {

                if (OwnerInstance == null)
                    return Task.CompletedTask;

                //var currInst = OwnerInstance.GetType().GetProperty(PropertyName).GetValue(OwnerInstance);
                var inst1 = OwnerInstance;
                //var currType = currInst.GetType();

                var inst1type = inst1.GetType();
                if (inst1type.IsArray)
                    return Task.CompletedTask;

                Type genericArgType = null;

                while (true)
                {
                    var genericArgs = inst1type.GetGenericArguments();
                    if (genericArgs != null && genericArgs.Length > 0)
                    {
                        genericArgType = genericArgs[0];
                        break;
                    }

                    inst1type = inst1type.BaseType;

                    //_delays.DelayFor(1);
                    System.Threading.Thread.Sleep(1);
                }
                //while (true)
                //{
                //    var genericArgs = currType.GetGenericArguments();
                //    if (genericArgs != null && genericArgs.Length > 0)
                //    {
                //        genericArgType = genericArgs[0];
                //        break;
                //    }

                //    currType = currType.BaseType;
                //}

                //Create instance
                var item = Activator.CreateInstance(genericArgType);

                //(currInst as IList).Add(item);
                (inst1 as IList).Add(item);

                var childNode = NodeConverter.CreateGroupNode(this, item, inst1, genericArgType.Name, true);

                //Create node
                //var childNode = NodeConverter.CreateGroupNode(this, item, currInst, genericArgType.Name, true);

                //(OwnerInstance as IList).Add(childNode);
                Nodes.Add(childNode);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                //throw;
            }
            return Task.CompletedTask;
        }
    }
}
