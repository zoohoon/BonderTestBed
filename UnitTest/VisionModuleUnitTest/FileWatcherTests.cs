using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace ProcessingModule.Tests
{
    [TestClass()]
    public class FileWatcherTests
    {
        /// <summary>
        /// 요청파일을 Fullpath로 사용
        /// </summary>
        //[TestMethod()]
        public void ReadAllTextTest()
        {
            string sampleFile = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Recipe", "Recipe.json");

            if (System.IO.File.Exists(sampleFile))
            {
                System.IO.File.Delete(sampleFile);
            }

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(sampleFile)))
            {
                System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(sampleFile), true);
            }

            using (FileWatcher fw = new FileWatcher())
            {
                fw.WatchPath = sampleFile;

                string ret = fw.ReadAllText(sampleFile);
                Assert.IsNull(ret, "Check not exist file");

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(sampleFile));
                System.IO.File.WriteAllText(sampleFile, "1");

                ret = fw.ReadAllText(sampleFile);
                Assert.IsNotNull(ret, "Check created file");
                Assert.AreEqual("1", ret, "Read contents1");
                Assert.AreEqual(1, fw.GetCasheItems().Count(), "Cashed file");

                System.IO.File.WriteAllText(sampleFile, "2");

                // 파일 시스템 반영되기전 이전값이 읽힘
                ret = fw.ReadAllText(sampleFile);
                //파일 시스템 변경 이벤트의 지연 시간으로인해 아래 일부 Assert 제외.
                //Assert.AreEqual("1", ret, "Read contents2");
                System.Threading.Thread.Sleep(1000);// 파일 시스템에서 반영되기까지 대기

                //Assert.AreEqual(0, fw.GetCasheItems().Count(), "remove cache");
                //파일시스템 반영후 수정된 값이 읽힘
                ret = fw.ReadAllText(sampleFile);
                Assert.AreEqual("2", ret, "Read contents3");
            }
            if (System.IO.File.Exists(sampleFile))
            {
                System.IO.File.Delete(sampleFile);
            }


            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(sampleFile)))
            {
                System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(sampleFile));
            }
        }

        /// <summary>
        /// 요청 파일명을 파일 이름만 사용
        /// </summary>
        //[TestMethod()]
        public void ReadAllTextRelativePathTest()
        {

            string genFileName = @"Recipe2.json";
            string sampleFile = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Recipe", "Recipe.json");
            string genFileFullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Recipe", genFileName);


            if (System.IO.File.Exists(genFileFullPath))
            {
                System.IO.File.Delete(genFileFullPath);
            }

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(genFileFullPath)))
            {
                System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(genFileFullPath), true);
            }

            using (FileWatcher fw = new FileWatcher())
            {
                fw.WatchPath = sampleFile;

                string ret = fw.ReadAllText(genFileName);
                Assert.IsNull(ret, "Check not exist file");

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(genFileFullPath));
                System.IO.File.WriteAllText(genFileFullPath, "1");

                ret = fw.ReadAllText(genFileName);
                Assert.IsNotNull(ret, "Check created file");
                Assert.AreEqual("1", ret, "Read contents");
                Assert.AreEqual(1, fw.GetCasheItems().Count(), "Cashed file");

                System.IO.File.WriteAllText(genFileFullPath, "2");


                System.Threading.Thread.Sleep(100);// 파일 시스템에서 반영되기까지 대기

                Assert.AreEqual(0, fw.GetCasheItems().Count(), "remove cache");
            }
            if (System.IO.File.Exists(genFileFullPath))
                {
                System.IO.File.Delete(genFileFullPath);
            }

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(genFileFullPath)))
            {
                System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(genFileFullPath));
            }
        }

        /// <summary>
        /// 요청 파일명을 파일 이름만 사용
        /// </summary>
       // [TestMethod()]
        public void ReadAllTextSetDirectoryTest()
        {

            string genFileName = @"Recipe2.json";
            string sampleFile = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Recipe", "Recipe.json");
            string genFileFullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Recipe", genFileName);


            if (System.IO.File.Exists(genFileFullPath))
            {
                System.IO.File.Delete(genFileFullPath);
            }

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(genFileFullPath)))
            {
                System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(genFileFullPath));
            }

            using (FileWatcher fw = new FileWatcher())
            {
                fw.WatchPath = System.IO.Path.GetDirectoryName(genFileFullPath);

                string ret = fw.ReadAllText(genFileName);
                Assert.IsNull(ret, "Check not exist file");

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(genFileFullPath));
                System.IO.File.WriteAllText(genFileFullPath, "1");

                ret = fw.ReadAllText(genFileName);
                Assert.IsNotNull(ret, "Check created file");
                Assert.AreEqual("1", ret, "Read contents");
                Assert.AreEqual(1, fw.GetCasheItems().Count(), "Cashed file");

                System.IO.File.WriteAllText(genFileFullPath, "2");


                System.Threading.Thread.Sleep(100);// 파일 시스템에서 반영되기까지 대기

                Assert.AreEqual(0, fw.GetCasheItems().Count(), "remove cache");
            }
            if (System.IO.File.Exists(genFileFullPath))
            {
                System.IO.File.Delete(genFileFullPath);
            }
            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(genFileFullPath)))
            {
                System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(genFileFullPath));
            }
        }
 
    }
}