using System;
using System.Collections.Generic;
using LogModule;

namespace DutViewer.ViewModel
{
    using ProberInterfaces.PinAlign.ProbeCardData;
    using SharpDXRender.RenderObjectPack;
    using WinSize = System.Windows.Size;
    using WinPoint = System.Windows.Point;

    public class PinGroupDutViewerViewModel : DutViewerViewModel
    {
        private List<GroupProtractor> _GroupProtractorList;
        private GroupProtractor _Group1;
        private GroupProtractor _Group2;
        private GroupProtractor _Group3;
        private GroupProtractor _Group4;
        public List<IPinData> PinDataGroup1 { get; set; }
        public List<IPinData> PinDataGroup2 { get; set; }
        public List<IPinData> PinDataGroup3 { get; set; }
        public List<IPinData> PinDataGroup4 { get; set; }
        List<List<IPinData>> PinDataGroupList { get; set; }
        public PinGroupDutViewerViewModel(WinSize scopeSize)
            : base(scopeSize)
        {
            try
            {
                _GroupProtractorList = new List<GroupProtractor>();
                _Group1 = new GroupProtractor(315, 45);
                _Group2 = new GroupProtractor(45, 135);
                _Group3 = new GroupProtractor(135, 225);
                _Group4 = new GroupProtractor(225, 315);
                _GroupProtractorList.Add(_Group1);
                _GroupProtractorList.Add(_Group2);
                _GroupProtractorList.Add(_Group3);
                _GroupProtractorList.Add(_Group4);

                PinDataGroup1 = new List<IPinData>();
                PinDataGroup2 = new List<IPinData>();
                PinDataGroup3 = new List<IPinData>();
                PinDataGroup4 = new List<IPinData>();
                PinDataGroupList = new List<List<IPinData>>();
                PinDataGroupList.Add(PinDataGroup1);
                PinDataGroupList.Add(PinDataGroup2);
                PinDataGroupList.Add(PinDataGroup3);
                PinDataGroupList.Add(PinDataGroup4);

                UpdatePinColor();
                UpdateDutColor();
                UpdatePinVisible();
                MainRenderLayer.GroupSpliterRenderData.IsVisible = true;
                SideRenderLayer.GroupSpliterRenderData.IsVisible = true;
                ClassifyGroup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void UpdatePinVisible()
        {
            try
            {
                foreach (var pin in MainRenderLayer.PinDataDic)
                {
                    IPinData pinData = pin.Value;
                    RenderObject pinRenderObject = pin.Key;
                    if (pinData.IsRegisteredPin.Value)
                        pinRenderObject.IsVisible = true;
                    else
                        pinRenderObject.IsVisible = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override void ZoomIn()
        {
            try
            {
                base.ZoomIn();
                MainRenderLayer.GroupSpliterRenderData.ZoomIn();
                SideRenderLayer.GroupSpliterRenderData.ZoomIn();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override void ZoomOut()
        {
            try
            {
                base.ZoomOut();
                MainRenderLayer.GroupSpliterRenderData.ZoomOut();
                SideRenderLayer.GroupSpliterRenderData.ZoomOut();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override void LeftRotate()
        {
            try
            {
                MainRenderLayer.GroupSpliterRenderData.LeftRotate(1);
                SideRenderLayer.GroupSpliterRenderData.LeftRotate(1);
                _GroupProtractorList.ForEach(item => item.LeftRotate());
                ClassifyGroup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public override void RightRotate()
        {
            try
            {
                MainRenderLayer.GroupSpliterRenderData.RightRotate(1);
                SideRenderLayer.GroupSpliterRenderData.RightRotate(1);
                _GroupProtractorList.ForEach(item => item.RightRotate());
                ClassifyGroup();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void ClassifyGroup()
        {
            try
            {
                PinDataGroup1.Clear();
                PinDataGroup2.Clear();
                PinDataGroup3.Clear();
                PinDataGroup4.Clear();

                foreach (var item in MainRenderLayer.PinContainerDic)
                {
                    RenderRectangle dutRenderObject = item.Key;
                    List<RenderEllipse> pinRenderObject = item.Value;

                    foreach (RenderEllipse elip in pinRenderObject)
                    {
                        IPinData pinData;
                        if (MainRenderLayer.PinDataDic.TryGetValue(elip, out pinData) == false)
                            continue;

                        if (pinData.IsRegisteredPin.Value == false)
                            continue;

                        WinPoint elipPos = new WinPoint(
                            elip.DrawBasePos.X + elip.CenterX,
                            elip.DrawBasePos.Y + elip.CenterY);

                        WinPoint centerPos = new WinPoint(
                            MainRenderLayer.GroupSpliterRenderData.LayerCenterPos.X,
                            MainRenderLayer.GroupSpliterRenderData.LayerCenterPos.Y);

                        double elipAngle = GetAngleFromPoint(elipPos, centerPos);
                        if (_Group1.CheckInside(elipAngle))
                        {
                            elip.Color = "RoyalBlue";
                            PinDataGroup1.Add(pinData);
                        }
                        else if (_Group2.CheckInside(elipAngle))
                        {
                            elip.Color = "DarkOrange";
                            PinDataGroup2.Add(pinData);
                        }
                        else if (_Group3.CheckInside(elipAngle))
                        {
                            elip.Color = "Violet";
                            PinDataGroup3.Add(pinData);
                        }
                        else
                        {
                            elip.Color = "Yellow";
                            PinDataGroup4.Add(pinData);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private double GetAngleFromPoint(WinPoint point, WinPoint centerPoint)
        {
            double radian = Math.Atan2(point.Y - centerPoint.Y, point.X - centerPoint.X);
            double angle = radian * 180 / Math.PI;
            try
            {

                if (angle < 0)
                    angle += 360;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return angle;
        }
        public override void MainScreen_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                //float left = (float)MainRenderLayer.MouseDownPos.X;
                //float top = (float)MainRenderLayer.MouseDownPos.Y;
                //float width = 50;
                //float height = 50;

                //RenderRectangle renderRect = new RenderRectangle(left, top, width, height, "Red");
                //MainRenderLayer.GroupRectContainer.RenderObjectList.Add(renderRect);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    public class GroupProtractor
    {
        public double BelowDegree { get; set; }
        public double AboveDegree { get; set; }
        public GroupProtractor(double belowDegree, double aboveDegree)
        {
            try
            {
                BelowDegree = belowDegree;
                AboveDegree = aboveDegree;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public bool CheckInside(double degree)
        {

            bool result = false;
            try
            {
                if (BelowDegree < AboveDegree)
                    result = BelowDegree <= degree && degree < AboveDegree;
                else if (BelowDegree > AboveDegree)
                {
                    result =
                        (BelowDegree <= degree && degree < 360) ||
                        (0 <= degree && degree < AboveDegree);
                }
                else
                    result = false;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return result;
        }
        public void LeftRotate()
        {
            try
            {
                BelowDegree = DecreaseDegree(BelowDegree);
                AboveDegree = DecreaseDegree(AboveDegree);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void RightRotate()
        {
            try
            {
                BelowDegree = IncreaseDegree(BelowDegree);
                AboveDegree = IncreaseDegree(AboveDegree);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private double IncreaseDegree(double degree)
        {
            double validDegree = degree;
            try
            {

                validDegree++;

                if (validDegree >= 360)
                    validDegree = 0;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return validDegree;
        }
        private double DecreaseDegree(double degree)
        {
            double validDegree = degree;
            try
            {

                validDegree--;

                if (validDegree < 0)
                    validDegree = 359;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return validDegree;
        }
    }
}
