using System;
using System.Collections.Generic;
using LogModule;
using GenericUndoRedo;

namespace PathMakerControlViewModel
{
    public class PathDataPool : IEnumerable<string>
    {
        public List<string> datas = new List<string>();

        public string this[int index]
        {
            get { return datas[index]; }
        }
        public void Add(string data)
        {
            datas.Add(data);
        }

        public void Insert(int index, string shape)
        {
            datas.Insert(index, shape);
        }

        public void RemoveAt(int index)
        {
            datas.RemoveAt(index);
        }

        public int IndexOf(string shape)
        {
            return datas.IndexOf(shape);
        }
        public int Count
        {
            get { return datas.Count; }
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return datas.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
    abstract class StringMemento : IMemento<PathDataPool>
    {
        #region IMemento<ShapePool> Members

        public abstract IMemento<PathDataPool> Restore(PathDataPool target);

        #endregion
    }

    class InsertStringMemento : StringMemento
    {
        private int index;
        public InsertStringMemento(int index)
        {
            this.index = index;
        }

        public override IMemento<PathDataPool> Restore(PathDataPool target)
        {
            string removed = target[index];
            IMemento<PathDataPool> inverse = new RemoveStringMemento(index, removed);
            try
            {
                target.RemoveAt(index);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return inverse;
        }
    }

    class RemoveStringMemento : StringMemento
    {
        string removed;
        int index;
        public RemoveStringMemento(int index, string removed)
        {
            try
            {
                this.index = index;
                this.removed = removed;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public override IMemento<PathDataPool> Restore(PathDataPool target)
        {
            IMemento<PathDataPool> inverse = new InsertStringMemento(index);

            try
            {
                target.Insert(index, removed);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return inverse;
        }
    }
    class AddStringMemento : StringMemento
    {
        public AddStringMemento()
        {
        }

        public override IMemento<PathDataPool> Restore(PathDataPool target)
        {
            int index = target.Count - 1;
            IMemento<PathDataPool> inverse = new RemoveStringMemento(index, target[index]);
            try
            {
                target.RemoveAt(target.Count - 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return inverse;
        }
    }
}
