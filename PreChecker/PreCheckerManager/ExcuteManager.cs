using System;

namespace PreCheckerManager
{
    public class ExcuteManager
    {
        private enum EnumStartType
        {
            ExecDBUpdate = 0
        }

        public void Start(string[] args)
        {
            var eStartType = GetStartType(args);
            if (eStartType == EnumStartType.ExecDBUpdate)
            {
                //install 시,
                // 1. DB Update
                DBUpdate.DiffHashFile();

                // 2. Loader/Cell 폴더에 있는 파일들을 삭제, 복사 등에 처리를 해준다.
                var updateFilesInfoDir = @"C:\ProberSystem\UpdateFiles"; // default 경로
                var bakDir = @"C:\Logs\PreChecker\UpdateFiles_Backup_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                if (args.Length > 1)
                {
                    if (!string.IsNullOrEmpty(args[1]))
                    {
                        // 추후 다른 경로를 사용하고 싶은 경우 argument로 받는다. 없는 경우 default 경로 사용.
                        updateFilesInfoDir = args[1];
                    }
                }

                FileProcess.FuncFileProcess(updateFilesInfoDir, bakDir);
            }
        }

        private EnumStartType GetStartType(string[] args)
        {
            EnumStartType eStartType;
            if (args.Length == 0)
            {
                eStartType = EnumStartType.ExecDBUpdate;
            }
            else
            {
                switch (args[0])
                {
                    case "0":
                        eStartType = EnumStartType.ExecDBUpdate;
                        break;

                    default:
                        eStartType = EnumStartType.ExecDBUpdate;
                        break;
                }
            }

            return eStartType;
        }

    }
}
