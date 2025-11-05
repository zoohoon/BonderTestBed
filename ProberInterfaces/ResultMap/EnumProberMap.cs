using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ProberInterfaces.ResultMap
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ProberMapPropertyAttribute : System.Attribute
    {
        public EnumProberMapProperty ProberProperty { get; set; }
        public string PropertyName { get; set; }

        public ProberMapPropertyAttribute(EnumProberMapProperty property,
                                         [CallerMemberName] string propertyanme = null)
        {
            ProberProperty = property;
            PropertyName = propertyanme;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ProberMapSectionAttribute : System.Attribute
    {
        public EnumProberMapSection Section { get; set; }

        public ProberMapSectionAttribute(EnumProberMapSection section)
        {
            this.Section = section;
        }
    }

    public enum EnumSTIFMapProperty
    {

    }

    public enum EnumE142MapProperty
    {

    }

    public enum EnumProberMapSection
    {
        UNDEFINED,

        BASICDATA,
        CASSETTEDATA,
        LOTDATA,
        WAFERDATA,
        WAFERALIGNDATA,
        MAPCONFIGDATA,
        TESTRESULTDATA,
        TIMEDATA,
        EXTENSION,
    }

    [DataContract]
    public enum EnumProberMapProperty
    {
        [EnumMember]
        UNDEFINED,

        // [BASIC]
        /// <summary>
        ///  Unknown
        /// </summary>
        [Description("Unknown")]
        [EnumMember]
        MAPFILEVERSION,
        /// <summary>
        ///  디바이스 이름
        /// </summary>
        [Description("디바이스 이름")]
        [EnumMember]
        DEVICENAME,
        /// <summary>
        ///  프로버 ID
        /// </summary>
        [Description("프로버 ID")]
        [EnumMember]
        PROBERID,
        /// <summary>
        ///  오퍼레이터 이름
        /// </summary>
        [Description("오퍼레이터 이름")]
        [EnumMember]
        OPERATORNAME,
        /// <summary>
        ///  카드 이름
        /// </summary>
        [Description("카드 이름")]
        [EnumMember]
        CARDNAME,
        /// <summary>
        ///  카드의 더트 정보
        /// </summary>
        [Description("카드의 더트 정보")]
        [EnumMember]
        CARDSITE,

        // [CASSETTE]
        /// <summary>
        ///  카세트 번호
        /// </summary>
        [Description("카세트 번호")]
        [EnumMember]
        CASSETTENO,
        /// <summary>
        ///  카세트 ID
        /// </summary>
        [Description("카세트 ID")]
        [EnumMember]
        CASSETTEID,

        // [LOT]
        /// <summary>
        ///  LOT ID
        /// </summary>
        [Description("LOT ID")]
        [EnumMember]
        LOTID,
        /// <summary>
        ///  현재 온도
        /// </summary>
        [Description("현재 온도")]
        [EnumMember]
        CHUCKTEMP,
        /// <summary>
        ///  Unknown
        /// </summary>
        [Description("Unknown")]
        [EnumMember]
        OCRTYPE,

        // [MAPCONFIG]
        /// <summary>
        ///  웨이퍼맵의 X 개수
        /// </summary>
        [Description("웨이퍼맵의 X 개수")]
        [EnumMember]
        MAPSIZEX,
        /// <summary>
        ///  웨이퍼맵의 Y 개수
        /// </summary>
        [Description("웨이퍼맵의 Y 개수")]
        [EnumMember]
        MAPSIZEY,
        /// <summary>
        ///  웨이퍼맵의 축 방향 정보
        /// </summary>
        [Description("웨이퍼맵의 축 방향 정보")]
        [EnumMember]
        AXISDIR,
        /// <summary>
        ///  첫 번째 다이 X 인덱스
        /// </summary>
        [Description("첫 번째 다이 X 인덱스")]
        [EnumMember]
        FIRSTDIEX,
        /// <summary>
        ///  첫 번째 다이 Y 인덱스
        /// </summary>
        [Description("첫 번째 다이 Y 인덱스")]
        [EnumMember]
        FIRSTDIEY,
        /// <summary>
        ///  레퍼런스 다이 X 인덱스
        /// </summary>
        [Description("레퍼런스 다이 X 인덱스")]
        [EnumMember]
        REFDIEX,
        /// <summary>
        ///  레퍼런스 다이 Y 인덱스
        /// </summary>
        [Description("레퍼런스 다이 Y 인덱스")]
        [EnumMember]
        REFDIEY,
        /// <summary>
        ///  센터 다이 X 인덱스
        /// </summary>
        [Description("센터 다이 X 인덱스")]
        [EnumMember]
        CENTERDIEX,
        /// <summary>
        ///  센터 다이 Y 인덱스
        /// </summary>
        [Description("센터 다이 Y 인덱스")]
        [EnumMember]
        CENTERDIEY,
        /// <summary>
        ///  웨이퍼 센터에서 REF 다이 센터까지의 X 거리
        /// </summary>
        [Description("웨이퍼 센터에서 REF 다이 센터까지의 X 거리")]
        [EnumMember]
        DISTANCEWAFERCENTERTOREFDIECENTERX,
        /// <summary>
        ///  웨이퍼 센터에서 REF 다이 센터까지의 Y 거리
        /// </summary>
        [Description("웨이퍼 센터에서 REF 다이 센터까지의 Y 거리")]
        [EnumMember]
        DISTANCEWAFERCENTERTOREFDIECENTERY,

        // [TEST RESULT]
        /// <summary>
        ///  프로빙 시퀀스 전체 개수
        /// </summary>
        [Description("프로빙 시퀀스 전체 개수")]
        [EnumMember]
        PROBINGSEQCOUNT,
        /// <summary>
        ///  CP 개수
        /// </summary>
        [Description("CP 개수")]
        [EnumMember]
        CPCOUNT,
        /// <summary>
        ///  RETEST 개수
        /// </summary>
        [Description("RETEST 개수")]
        [EnumMember]
        RETESTCOUNT,
        /// <summary>
        ///  프로빙 종료 원인
        /// </summary>
        [Description("프로빙 종료 원인")]
        [EnumMember]
        PROBINGENDREASON,
        /// <summary>
        ///  LOT의 테스트 다이 개수
        /// </summary>
        [Description("LOT의 테스트 다이 개수")]
        [EnumMember]
        TESTDIECOUNTINLOT,
        /// <summary>
        ///  LOT의 PASS 다이 개수
        /// </summary>
        [Description("LOT의 PASS 다이 개수")]
        [EnumMember]
        GOODDEVICESINLOT,
        /// <summary>
        ///  LOT의 FAIL 다이 개수
        /// </summary>
        [Description("LOT의 FAIL 다이 개수")]
        [EnumMember]
        BADDEVICESINLOT,
        /// <summary>
        ///  WAFER의 테스트 다이 개수
        /// </summary>
        [Description("WAFER의 테스트 다이 개수")]
        [EnumMember]
        TESTDIECOUNTINWAFER,
        /// <summary>
        ///  WAFER의 PASS 다이 개수
        /// </summary>
        [Description("WAFER의 PASS 다이 개수")]
        [EnumMember]
        GOODDEVICESINWAFER,
        /// <summary>
        ///  WAFER의 FAIL 다이 개수
        /// </summary>
        [Description("WAFER의 FAIL 다이 개수")]
        [EnumMember]
        BADDEVICESINWAFER,
        /// <summary>
        ///  수율
        /// </summary>
        [Description("수율")]
        [EnumMember]
        YIELD,
        /// <summary>
        ///  RETEST 수율
        /// </summary>
        [Description("RETEST 수율")]
        [EnumMember]
        RETESTYIELD,
        /// <summary>
        ///  TOUCH DOWN 개수
        /// </summary>
        [Description("TOUCH DOWN 개수")]
        [EnumMember]
        TOUCHDOWNCOUNT,

        // [TIME]
        /// <summary>
        ///  LOT 시작 시간
        /// </summary>
        [Description("LOT 시작 시간")]
        [EnumMember]
        LOTSTARTTIME,
        /// <summary>
        ///  LOT 종료 시간
        /// </summary>
        [Description("LOT 종료 시간")]
        [EnumMember]
        LOTENDTIME,
        /// <summary>
        ///  WAFER 로딩 시간
        /// </summary>
        [Description("WAFER 로딩 시간")]
        [EnumMember]
        WAFERLOADINGTIME,
        /// <summary>
        ///  WAFER 언로딩 시간
        /// </summary>
        [Description("WAFER 언로딩 시간")]
        [EnumMember]
        WAFERUNLOADINGTIME,
        /// <summary>
        ///  프로빙 시작 시간
        /// </summary>
        [Description("프로빙 시작 시간")]
        [EnumMember]
        PROBINGSTARTTIME,
        /// <summary>
        ///  프로빙 종료 시간
        /// </summary>
        [Description("프로빙 종료 시간")]
        [EnumMember]
        PROBINGENDTIME,

        // [WAFER ALIGN]
        // Step Size - Die Size = Street Size.
        /// <summary>
        ///  ALIGN 후, 측정 된 DIE 크기 X
        /// </summary>
        [Description("ALIGN 후, 측정 된 DIE 크기 X")]
        [EnumMember]
        ALIGNEDINDEXSIZEX,
        /// <summary>
        ///  ALIGN 후, 측정 된 DIE 크기 Y
        /// </summary>
        [Description("ALIGN 후, 측정 된 DIE 크기 Y")]
        [EnumMember]
        ALIGNEDINDEXSIZEY,
        /// <summary>
        ///  인접한 다이의 사이 간격 X
        /// </summary>
        [Description("인접한 다이의 사이 간격 X")]
        [EnumMember]
        STREETSIZEX,
        /// <summary>
        ///  인접한 다이의 사이 간격 Y
        /// </summary>
        [Description("인접한 다이의 사이 간격 Y")]
        [EnumMember]
        STREETSIZEY,

        // [WAFER]
        /// <summary>
        ///  WAFER ID(=OCR)
        /// </summary>
        [Description("WAFER ID(=OCR)")]
        [EnumMember]
        WAFERID,
        /// <summary>
        ///  WAFER SLOT 번호
        /// </summary>
        [Description("WAFER SLOT 번호")]
        [EnumMember]
        SLOTNO,
        /// <summary>
        ///  WAFER 크기(단위 : um)
        /// </summary>
        [Description("WAFER 크기(단위 : um)")]
        [EnumMember]
        WAFERSIZE,
        /// <summary>
        ///  NOTCH 종류
        /// </summary>
        [Description("NOTCH 종류")]
        [EnumMember]
        NOTCHTYPE,
        /// <summary>
        ///  NOTCH 방향
        /// </summary>
        [Description("NOTCH 방향")]
        [EnumMember]
        NOTCHDIR,
        /// <summary>
        ///  FRAMED NOTCH 방향
        /// </summary>
        [Description("FRAMED NOTCH 방향")]
        [EnumMember]
        FRAMEDNOTCHDIR,

        // [EXTENSION : STIF]
        /// <summary>
        /// Unknown
        /// </summary>
        [Description("Unknown")]
        [EnumMember]
        REFDIECENTERTOFDITARGETCENTERDISTANCEX,
        /// <summary>
        /// Unknown
        /// </summary>
        [Description("Unknown")]
        [EnumMember]
        REFDIECENTERTOFDITARGETCENTERDISTANCEY,

        // [EXTENSION : E142]
        /// <summary>
        /// Die size X + street size X
        /// </summary>
        [Description("Die size X + street size X")]
        [EnumMember]
        STEPSIZEX,
        /// <summary>
        /// Die size Y + street size Y
        /// </summary>
        [Description("Die size Y + street size Y")]
        [EnumMember]
        STEPSIZEY
    }
}
