using ProberErrorCode;
using System;
using System.ServiceModel;

namespace ProberInterfaces.ResultMap
{
    public enum FileTrasnferType
    {
        UPLOAD,
        DOWNLOAD
    }
    public enum FileManagerType
    {
        UNDEFINED = 0,
        HDD,
        FTP,
        FDD
    }

    public enum FileReaderType
    {
        UNDEFINED,
        BINARY,
        STREAM,
        XML,
        EXTENDEDXML
    }

    public enum ResultMapConvertType
    {
        OFF = 0x0000,
        EG = 0x0001,
        STIF,
        E142
    }

    public interface IResultMapManager : IFactoryModule, IModule,
                                        IHasSysParameterizable, IHasDevParameterizable
    {
        /// <summary>
        /// Chuck에 올릴 Result Map의 Header Buffer 입니다.
        /// </summary>
        MapHeaderObject ResultMapHeader { get; set; }
        /// <summary>
        /// Chuck에 올릴 Result Map의 Data Buffer 입니다.
        /// </summary>
        ResultMapData ResultMapData { get; set; }

        //object ConvertedResultMap { get;}

        EventCodeEnum Upload();
        EventCodeEnum Download(bool IsLotNameChangedTriggered);
        bool NeedDownload();
        EventCodeEnum CheckAndDownload(bool IsLotNameChangedTriggered);
        EventCodeEnum ManualUpload(object param, FileManagerType managertype, string filepath, FileReaderType readertype, Type serializeObjtype = null);

        /// <summary>
        /// Result Map을 Prober에서 사용하는 Data로 Convert합니다.
        /// </summary>
        /// <param name="mapHeader"> Result Header로부터 가져온 Data를 Convert하는 Out Data. </param>
        /// <param name="resultMap"> Result Map로부터 가져온 Data를 Convert하는 Out Data. </param>
        /// <param name="lotID"></param>
        /// <param name="waferID"></param>
        /// <param name="slotNum"></param>
        /// <returns></returns>
        ///
        EventCodeEnum ConvertResultMapToProberDataAfterReadyToLoadWafer();
        EventCodeEnum ManualConvertResultMapToProberData(string filepath, FileManagerType managerType, FileReaderType readertype, Type deserializeObjtype = null);
        /// <summary>
        /// BaseMap을 Prober Property/Parameter에 적용(대입)합니다.
        /// </summary>
        /// <param name="mapHeader"></param>
        /// <param name="resultMap"></param>
        /// <returns></returns>
        EventCodeEnum ApplyBaseMap(MapHeaderObject mapHeader, ResultMapData resultMap);

        //bool IsUsingResultMap();

        bool NeedUpload();
        bool CanDownload();

        /// <summary>
        /// LOT에 관한 Result Map들이 성공적으로 다운 받았는지 확인하는 Function입니다.
        /// </summary>
        /// <returns></returns>
        bool IsMapDownloadDone();

        /// <summary>
        /// Real Time으로 Probing Data를 저장할 때 사용하는 Function입니다.
        /// </summary>
        /// <returns></returns>
        EventCodeEnum SaveRealTimeProbingData();

        EventCodeEnum LoadRealTimeProbingData();

        //EventCodeEnum MakeResultMap(bool IsDummy = false);
        EventCodeEnum MakeResultMap(ref object resultmap, bool IsDummy = false);

        char[,] GetASCIIMap();

        IParam ResultMapManagerSysIParam { get;}

        [OperationContract]
        IParam GetResultMapConvIParam();
        [OperationContract]
        byte[] GetResultMapConvParam();
        [OperationContract]
        void SetResultMapConvIParam(byte[] param);
        bool SetResultMapByFileName(byte[] device, string resultmapname);
        ResultMapConvertType GetUploadConverterType();
        ResultMapConvertType GetDownloadConverterType();

        string[] GetNamerAliaslist();
        object GetOrgResultMap();

        string LocalUploadPath { get; }
        string LocalDownloadPath { get; }
    }
}
