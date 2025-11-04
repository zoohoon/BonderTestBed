using Autofac;
using CameraModule;
using FileSystem.Tests;
using Matrox.MatroxImagingLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Param;
using ProberInterfaces.WaferAlignEX.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vision.ProcessingModule;
using WAEdgeStadnardModule;

namespace VisionModuleUnitTests
{
    [TestClass()]
    public class EdgeProcessing : TestBaseProberSystem
    {
        [TestMethod()]
        public void EdgeFind_IndexAlignTest()
        {
            Container.Resolve<IFileManager>(new NamedParameter("ParamPath", "C:\\ProberSystem\\C01"));

            this.StageSupervisor();
            this.WaferAligner();
            this.WaferAligner().WaferAlignControItems.IsDebugEdgeProcessing = true;
            string folderPath = @"C:\Logs\TEST\WaferEdge\HBM_WAFER_1";
            string outputFilePath = @"C:\Logs\TEST\WaferEdge\results.txt";

            var directories = Directory.GetDirectories(folderPath);
            EdgeStandard edgeStandard = new EdgeStandard();
            edgeStandard.LoadDevParameter();

            double edgepos = ((300000 / 2) / Math.Sqrt(2));

            edgeStandard.EdgePos.Clear();
            edgeStandard.EdgePos.Add(new WaferCoordinate(edgepos, edgepos));
            edgeStandard.EdgePos.Add(new WaferCoordinate(-edgepos, edgepos));
            edgeStandard.EdgePos.Add(new WaferCoordinate(-edgepos, -edgepos));
            edgeStandard.EdgePos.Add(new WaferCoordinate(edgepos, -edgepos));

            var curcam = new Camera();
            curcam.Param.RatioX.Value = 5.47;
            curcam.Param.RatioY.Value = 5.47;
            curcam.Param.GrabSizeX.Value = 960;
            curcam.Param.GrabSizeY.Value = 960;

            edgeStandard.CurCam = curcam;


            using (StreamWriter writer = new StreamWriter(outputFilePath)) 
            {
                // directories 배열을 숫자로 변환하여 정렬
                var sortedDirectories = directories
                    .Select(dir => new
                    {
                        DirectoryName = dir,
                        DirectoryNumber = int.Parse(Path.GetFileName(dir))
                    })
                    .OrderBy(x => x.DirectoryNumber)
                    .Select(x => x.DirectoryName)
                    .ToArray();

                for (int dirIndex = 0; dirIndex < sortedDirectories.Length; dirIndex += 10)
                {
                    var directory = sortedDirectories[dirIndex];
                    string[] fileEntries = Directory.GetFiles(directory, "*.bmp");

                    var sortedFiles = fileEntries
                        .Select(filepath => new
                        {
                            FilePath = filepath,
                            SortOrder = int.Parse(Path.GetFileNameWithoutExtension(filepath).Split('_').Last())
                        })
                        .OrderBy(x => x.SortOrder)
                        .ToArray();

                    ImageBuffer[] EdgeBuffer = new ImageBuffer[sortedFiles.Length];
                    ImageBuffer[] EdgeLineBuffer = new ImageBuffer[sortedFiles.Length];
                    ImageBuffer[] EdgeBinarizeBuffer = new ImageBuffer[sortedFiles.Length];

                    for (int i = 0; i < sortedFiles.Length; i++)
                    {
                        EdgeBuffer[i] = this.VisionManager().LoadImageFile(sortedFiles[i].FilePath);
                    }
                    for (int i = 0; i < sortedFiles.Length; i++)
                    {
                        EdgeLineBuffer[i] = this.VisionManager().Line_Equalization(EdgeBuffer[i], i);
                    }

                    EventCodeEnum Ret = edgeStandard.Edgedetection(EdgeBuffer, EdgeLineBuffer, null, null);
                    if (Ret == EventCodeEnum.NONE)
                    {
                        writer.WriteLine($"{directory}: O");
                    }
                    else
                    {
                        writer.WriteLine($"{directory}: X");
                    }
                }
            }
        }
    }
}
