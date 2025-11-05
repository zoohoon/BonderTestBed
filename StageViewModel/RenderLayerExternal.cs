using System;
using System.Collections.Generic;

namespace PnpViewModelModule
{
    using Autofac;
    using LogModule;
    using System.Windows;
    using SharpDXRender.RenderObjectPack;
    using DXControlBase;
    using SharpDX.Direct2D1;
    using SharpDXRender;
    using ProberInterfaces;
    using LoaderBase.Communication;
    using System.Threading;
    using System.Timers;

    public class RenderLayerExternal : RenderLayer, IFactoryModule
    {
        #region //..Property

        protected Autofac.IContainer _Container { get; set; }
        public ILoaderCommunicationManager LoaderCommunicationManager
        {
            get
            {
                return _Container?.Resolve<ILoaderCommunicationManager>();
            }
        }
        #region // Update thread
        bool bStopUpdateThread;
        Thread UpdateThread;
        #endregion
        private List<RenderContainer> _RenderContainers
            = new List<RenderContainer>();

        public List<RenderContainer> RenderContainers
        {
            get { return _RenderContainers; }
            set { _RenderContainers = value; }
        }


        #endregion

        #region //..Creator
        public RenderLayerExternal(Size layerSize) : base(layerSize)
        {
            InitUpdateProc();
        }

        public RenderLayerExternal(Size layerSize, float r, float g, float b, float a) : base(layerSize, r, g, b, a)
        {
            InitUpdateProc();
        }
        ~RenderLayerExternal()
        {
            try
            {
                LoggerManager.Debug($"Destructor in {this.GetType().Name}");

                bStopUpdateThread = true;
                UpdateThread?.Join();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RenderLayerExternal.Destructor() Function error: " + err.Message);
            }
        }

        public void DeInit()
        {
            try
            {
                LoggerManager.Debug($"Destructor in {this.GetType().Name}");

                bStopUpdateThread = true;
                UpdateThread?.Join();
            }
            catch (Exception err)
            {
                LoggerManager.Error($"RenderLayerExternal.Destructor() Function error: " + err.Message);
            }
        }
        private void InitUpdateProc()
        {
            bStopUpdateThread = false;
            UpdateThread = new Thread(new ThreadStart(UpdateRenderData));
            UpdateThread.Name = this.GetType().Name;
            UpdateThread.Start();
        }
        object contLockObj = new object();
        private void UpdateRenderData()
        {
            bool errorHandled = false;
            while (bStopUpdateThread == false)
            {
                try
                {
                    var renderContainers = LoaderCommunicationManager.GetProxy<IRemoteMediumProxy>()?.GetRenderContainers();

                    if (renderContainers != null)
                    {
                        lock (contLockObj)
                        {
                            RenderContainers = renderContainers;
                        }
                    }

                    if (errorHandled == true)
                    {
                        LoggerManager.Debug($"RenderLayerExternal.UpdateRenderData(): Error recovered.");
                        errorHandled = false;
                    }

                }
                catch (Exception err)
                {
                    if (errorHandled == false)
                    {
                        LoggerManager.Error($"RenderLayerExternal.UpdateRenderData: Error occurred. Err = {err.Message}");
                        errorHandled = true;
                    }
                }

                System.Threading.Thread.Sleep(30);
            }
        }
        #endregion

        #region //.. Methods

        public void SetContainer(Autofac.IContainer container)
        {
            _Container = container;
        }

        public override void Init()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override void InitRender(RenderTarget target, ResourceCache resCache)
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected override void RenderCore(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                //RenderContainers = LoaderCommunicationManager.GetRemoteMediumClient().GetRenderContainers();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected override void RenderHudCore(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                if (RenderContainers == null)
                    return;

                foreach (var container in RenderContainers)
                {
                    if (container.WindowEdge != null)
                    {
                        container.Edge = new Rect(container.WindowEdge.X, container.WindowEdge.Y,
                                        container.WindowEdge.Width, container.WindowEdge.Height);
                    }

                    foreach (var obj in container.RenderObjectList)
                    {
                        if (obj.WLT != null)
                            obj.LT = new Point(obj.WLT.X, obj.WLT.Y);
                        if (obj.WRT != null)
                            obj.RT = new Point(obj.WRT.X, obj.WRT.Y);
                        if (obj.WLB != null)
                            obj.LB = new Point(obj.WLB.X, obj.WLB.Y);
                        if (obj.WRB != null)
                            obj.RB = new Point(obj.WRB.X, obj.WRB.Y);
                        if (obj.WDrawBasePos != null)
                            obj.DrawBasePos = new Point(obj.WDrawBasePos.X, obj.WDrawBasePos.Y);
                    }

                    foreach (var obj in container.DisplayedRenderList)
                    {
                        if (container.WindowEdge != null)
                        {
                            container.Edge = new Rect(container.WindowEdge.X, container.WindowEdge.Y,
                                            container.WindowEdge.Width, container.WindowEdge.Height);
                        }

                        if (obj.WLT != null)
                            obj.LT = new Point(obj.WLT.X, obj.WLT.Y);
                        if (obj.WRT != null)
                            obj.RT = new Point(obj.WRT.X, obj.WRT.Y);
                        if (obj.WLB != null)
                            obj.LB = new Point(obj.WLB.X, obj.WLB.Y);
                        if (obj.WRB != null)
                            obj.RB = new Point(obj.WRB.X, obj.WRB.Y);
                        if (obj.WDrawBasePos != null)
                            obj.DrawBasePos = new Point(obj.WDrawBasePos.X, obj.WDrawBasePos.Y);
                    }
                    container.Draw(target, resCache);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public override List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> containers = new List<RenderContainer>();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return containers;
        }
        #endregion

    }
}
