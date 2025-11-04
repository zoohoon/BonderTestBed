using DXControlBase;
using ProberInterfaces.PMI;
using SharpDX.Direct2D1;
using SharpDXRender;
using SharpDXRender.RenderObjectPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
//using ProberInterfaces.ThreadSync;
using LogModule;
using ProberInterfaces;

namespace PMIModuleSubRutineStandard
{
    using WinSize = System.Windows.Size;

    public class PMIRenderLayer : RenderLayer, INotifyPropertyChanged, IPMIRenderLayer
    {
        #region ==> PropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        protected new void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        //private LockKey _TemplateLock = new LockKey("PMI - Template Render");
        //private LockKey _JudgingLock = new LockKey("PMI - Judging Window Render");
        //private LockKey _MarkMinSize = new LockKey("PMI - Mark Min Size Render");
        //private LockKey _MarkMaxSizeLock = new LockKey("PMI - Mark Max Size Render");
        //private LockKey _RegisteredPadRenderLock = new LockKey("PMI - Registered Pad Render");
        //private LockKey _RegisteredPadRenderIndexLock = new LockKey("PMI - Registered Pad Render Index");
        //private LockKey _DetectedMarksLock = new LockKey("PMI - Detected Marks");

        private static object _TemplateLock = new object();
        private static object _JudgingLock = new object();
        private static object _MarkMinSize = new object();
        private static object _MarkMaxSizeLock = new object();
        private static object _RegisteredPadRenderLock = new object();
        private static object _RegisteredPadRenderIndexLock = new object();
        private static object _DetectedMarksLock = new object();
        private static object _ProximityLineLock = new object();

        public object Module { get; set; }
        public RenderContainer TemplateRenderContainer { get; set; }
        public RenderContainer JudgingWindowContainer { get; set; }
        public RenderContainer MarkMinSizeContainer { get; set; }
        public RenderContainer MarkMaxSizeContainer { get; set; }
        public RenderContainer RegisteredPadRenderContainer { get; set; }
        public RenderContainer RegisteredPadRenderIndexContainer { get; set; }
        public RenderContainer DetectedMarksContainer { get; set; }
        public RenderContainer ProximityLineContainer { get; set; }
        //public PMIRenderLayer(Size layerSize) : base(layerSize)
        //{
        //    TemplateRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
        //    JudgingWindowContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
        //    MarkMinSizeContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
        //    MarkMaxSizeContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
        //    RegisteredPadRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
        //    RegisteredPadRenderIndexContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
        //}

        //public PMIRenderLayer(object module)
        //{
        //    Module = module;
        //}

        public PMIRenderLayer(WinSize layerSize, float r, float g, float b, float a) : base(layerSize, r, g, b, a)
        {
        }

        public override void Init()
        {
            try
            {
                TemplateRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
                JudgingWindowContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
                MarkMinSizeContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
                MarkMaxSizeContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
                RegisteredPadRenderContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
                RegisteredPadRenderIndexContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
                DetectedMarksContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));
                ProximityLineContainer = new RenderContainer(new Rect(0, 0, LayerSize.Width, LayerSize.Height));

                //if (Module != null)
                //{
                //    if (Module is IPMIModule)
                //    {
                //        (Module as IPMIModule).UpdateRenderLayer();
                //    }
                //}
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
                TemplateRenderContainer.Draw(target, resCache);
                JudgingWindowContainer.Draw(target, resCache);
                MarkMinSizeContainer.Draw(target, resCache);
                MarkMaxSizeContainer.Draw(target, resCache);
                RegisteredPadRenderContainer.Draw(target, resCache);
                RegisteredPadRenderIndexContainer.Draw(target, resCache);
                DetectedMarksContainer.Draw(target, resCache);
                ProximityLineContainer.Draw(target, resCache);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected override void RenderCore(RenderTarget target, ResourceCache resCache)
        {

        }

        protected override void RenderHudCore(RenderTarget target, ResourceCache resCache)
        {
            try
            {
                //this.BackgroundColor = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0);

                ContainerDraw(target, resCache, TemplateRenderContainer, _TemplateLock);
                ContainerDraw(target, resCache, JudgingWindowContainer, _JudgingLock);
                ContainerDraw(target, resCache, MarkMinSizeContainer, _MarkMinSize);
                ContainerDraw(target, resCache, MarkMaxSizeContainer, _MarkMaxSizeLock);
                ContainerDraw(target, resCache, RegisteredPadRenderContainer, _RegisteredPadRenderLock);
                ContainerDraw(target, resCache, RegisteredPadRenderIndexContainer, _RegisteredPadRenderIndexLock);
                ContainerDraw(target, resCache, DetectedMarksContainer, _DetectedMarksLock);
                ContainerDraw(target, resCache, ProximityLineContainer, _ProximityLineLock);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void ClearAllRenderContainer()
        {
            try
            {
                if (TemplateRenderContainer != null)
                    TemplateRenderContainer.RenderObjectList.Clear();

                if (JudgingWindowContainer != null)
                    JudgingWindowContainer.RenderObjectList.Clear();

                if (MarkMinSizeContainer != null)
                    MarkMinSizeContainer.RenderObjectList.Clear();

                if (MarkMaxSizeContainer != null)
                    MarkMaxSizeContainer.RenderObjectList.Clear();

                if (RegisteredPadRenderContainer != null)
                    RegisteredPadRenderContainer.RenderObjectList.Clear();

                if (RegisteredPadRenderIndexContainer != null)
                    RegisteredPadRenderIndexContainer.RenderObjectList.Clear();

                if (DetectedMarksContainer != null)
                    DetectedMarksContainer.RenderObjectList.Clear();

                if (ProximityLineContainer != null)
                    ProximityLineContainer.RenderObjectList.Clear();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void ContainerDraw(RenderTarget target, ResourceCache resCache, RenderContainer container, object lockKey)
        {
            try
            {
                lock(lockKey)
                { 
                    container.Draw(target, resCache);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        private void ContainerUpdate(RenderContainer container, AsyncObservableCollection<RenderObject> objects, object lockKey)
        {
            try
            {
                //using (Locker locker = new Locker(lockKey))
                //{
                //    if (locker.AcquiredLock == false)
                //    {
                //        System.Diagnostics.Debugger.Break();
                //        return;
                //    }
                lock(lockKey)
                { 
                    if (container != null)
                    {
                        if (objects.Count <= 0)
                        {
                            container.RenderObjectList.Clear();
                        }
                        else
                        {
                            container.RenderObjectList = objects.ToList();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void UpdateTemplate(AsyncObservableCollection<RenderObject> objects)
        {
            ContainerUpdate(TemplateRenderContainer, objects, _TemplateLock);
        }

        public void UpdateJudgingWindow(AsyncObservableCollection<RenderObject> objects)
        {
            ContainerUpdate(JudgingWindowContainer, objects, _JudgingLock);
        }
        public void UpdateMarkMinSize(AsyncObservableCollection<RenderObject> objects)
        {
            ContainerUpdate(MarkMinSizeContainer, objects, _MarkMinSize);
        }

        public void UpdateMarkMaxSize(AsyncObservableCollection<RenderObject> objects)
        {
            ContainerUpdate(MarkMaxSizeContainer, objects, _MarkMaxSizeLock);
        }

        public void UpdateRegisterdPad(AsyncObservableCollection<RenderObject> objects)
        {
            ContainerUpdate(RegisteredPadRenderContainer, objects, _RegisteredPadRenderLock);
        }

        public void UpdateRegisterdPadIndex(AsyncObservableCollection<RenderObject> objects)
        {
            ContainerUpdate(RegisteredPadRenderIndexContainer, objects, _RegisteredPadRenderIndexLock);
        }

        public void UpdateDetectedMarks(AsyncObservableCollection<RenderObject> objects)
        {
            ContainerUpdate(DetectedMarksContainer, objects, _DetectedMarksLock);
        }

        public void UpdateProximityLine(AsyncObservableCollection<RenderObject> objects)
        {
            ContainerUpdate(ProximityLineContainer, objects, _ProximityLineLock);
        }

        public override List<RenderContainer> GetRenderContainers()
        {
            List<RenderContainer> containers = new List<RenderContainer>();
            try
            {
                containers.Add(TemplateRenderContainer);
                containers.Add(JudgingWindowContainer);
                containers.Add(MarkMinSizeContainer);
                containers.Add(MarkMaxSizeContainer);
                containers.Add(RegisteredPadRenderContainer);
                containers.Add(RegisteredPadRenderIndexContainer);
                containers.Add(DetectedMarksContainer);
                containers.Add(ProximityLineContainer);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return containers;
        }
    }
}
