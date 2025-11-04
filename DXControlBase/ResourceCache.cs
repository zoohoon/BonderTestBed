using LogModule;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;

namespace DXControlBase
{
    public class ResourceCache
    {
        // - field -----------------------------------------------------------------------

        private Dictionary<string, Func<RenderTarget, object>> generators = new Dictionary<string, Func<RenderTarget, object>>();
        private Dictionary<string, object> resources = new Dictionary<string, object>();
        private RenderTarget renderTarget = null;

        // - property --------------------------------------------------------------------

        public RenderTarget RenderTarget
        {
            get { return renderTarget; }
            set { renderTarget = value; UpdateResources(); }
        }

        public int Count
        {
            get { return resources.Count; }
        }

        public object this[string key]
        {
            get { return resources[key]; }
        }

        public Dictionary<string, object>.KeyCollection Keys
        {
            get { return resources.Keys; }
        }

        public Dictionary<string, object>.ValueCollection Values
        {
            get { return resources.Values; }
        }

        // - public methods --------------------------------------------------------------

        public void Add(string key, Func<RenderTarget, object> gen)
        {
            try
            {
                object resOld;
                if (resources.TryGetValue(key, out resOld))
                {
                    Disposer.SafeDispose(ref resOld);
                    generators.Remove(key);
                    resources.Remove(key);
                }

                if (renderTarget == null)
                {
                    generators.Add(key, gen);
                    resources.Add(key, null);
                }
                else
                {
                    var res = gen(renderTarget);
                    generators.Add(key, gen);
                    resources.Add(key, res);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Clear()
        {
            try
            {
                foreach (var key in resources.Keys)
                {
                    var res = resources[key];
                    Disposer.SafeDispose(ref res);
                }
                generators.Clear();
                resources.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool ContainsKey(string key)
        {
            try
            {
                return resources.ContainsKey(key);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool ContainsValue(object val)
        {
            try
            {
                return resources.ContainsValue(val);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public Dictionary<string, object>.Enumerator GetEnumerator()
        {
            try
            {
                return resources.GetEnumerator();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool Remove(string key)
        {
            try
            {
                object res;
                if (resources.TryGetValue(key, out res))
                {
                    Disposer.SafeDispose(ref res);
                    generators.Remove(key);
                    resources.Remove(key);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }

        }

        public bool TryGetValue(string key, out object res)
        {
            return resources.TryGetValue(key, out res);
        }

        // - private methods -------------------------------------------------------------

        private void UpdateResources()
        {
            try
            {
                if (renderTarget == null) { return; }

                foreach (var g in generators)
                {
                    var key = g.Key;
                    var gen = g.Value;
                    var res = gen(renderTarget);

                    object resOld;
                    if (resources.TryGetValue(key, out resOld))
                    {
                        Disposer.SafeDispose(ref resOld);
                        resources.Remove(key);
                    }

                    resources.Add(key, res);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }
}
