using System;
using System.Collections.Generic;

namespace SecsGemServiceInterface
{
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract]
    public class SecsGemDefineReport
    {
        private List<CEID> _CEIDs = new List<CEID>();
        [DataMember]
        public List<CEID> CEIDs
        {
            get { return _CEIDs; }
            set { _CEIDs = value; }
        }

        /// <summary>
        /// 원본 RPTID 들
        /// </summary>
        private List<RPTID> _RPTIDs = new List<RPTID>();
        [DataMember]
        public List<RPTID> RPTIDs
        {
            get { return _RPTIDs; }
            set { _RPTIDs = value; }
        }


        public SecsGemDefineReport()
        {

        }

        public void SetDefault()
        {
            try
            {

                #region Rptid Micron

                //RPTID rPTID100 = new RPTID();
                //rPTID100.Rptid = 100;
                //rPTID100.VIDs = new List<long>() { 1090, 1091, 1520, 1503, 1595, 1596, 1508, 30851, 1105, 30098 };
                //RPTIDs.Add(rPTID100);

                //RPTID rPTID140 = new RPTID();
                //rPTID140.Rptid = 140;
                //rPTID140.VIDs = new List<long>() { 1090, 30851, 1025, 1241 };
                //RPTIDs.Add(rPTID140);

                //RPTID rPTID150 = new RPTID();
                //rPTID150.Rptid = 150;
                //rPTID150.VIDs = new List<long>() { 1090, 1106, 1105 };
                //RPTIDs.Add(rPTID150);

                //RPTID rPTID200 = new RPTID();
                //rPTID200.Rptid = 200;
                //rPTID200.VIDs = new List<long>() { 1035, 1036, 1520, 1140, 1141 };
                //RPTIDs.Add(rPTID200);

                //RPTID rPTID300 = new RPTID();
                //rPTID300.Rptid = 300;
                //rPTID300.VIDs = new List<long>() { 1090, 1035, 1520, 30098, 1037 };
                //RPTIDs.Add(rPTID300);

                //RPTID rPTID400 = new RPTID();
                //rPTID400.Rptid = 400;
                //rPTID400.VIDs = new List<long>() { 1038, 1039, 1040 };
                //RPTIDs.Add(rPTID400);

                //CEID cEID = new CEID() { Ceid = 101 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 102 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 103 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 104 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 105 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 106 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 107 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 108 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 109 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 110 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 112 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 113 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 115 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 121 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 122 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 141 };
                //cEID.RPTIDs.Add(rPTID140.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 142 };
                //cEID.RPTIDs.Add(rPTID140.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 151 };
                //cEID.RPTIDs.Add(rPTID150.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 152 };
                //cEID.RPTIDs.Add(rPTID150.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 153 };
                //cEID.RPTIDs.Add(rPTID150.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 200 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 201 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 202 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 203 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 204 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 205 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 206 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 207 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 208 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 209 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 210 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 301 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 302 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 303 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 304 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 305 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 306 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 307 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 401 };
                //cEID.RPTIDs.Add(rPTID400.Rptid);
                //CEIDs.Add(cEID);

                #endregion

                #region Rptid Micron DY

                //RPTID rPTID100 = new RPTID();
                //rPTID100.Rptid = 100;
                //rPTID100.VIDs = new List<long>() { 1035, 1090, 1091, 1520, 1503, 1595, 1596, 1508, 30851, 1105, 30098 };
                //RPTIDs.Add(rPTID100);

                //RPTID rPTID140 = new RPTID();
                //rPTID140.Rptid = 140;
                //rPTID140.VIDs = new List<long>() { 1090, 30851, 1025, 1241 };
                //RPTIDs.Add(rPTID140);

                //RPTID rPTID150 = new RPTID();
                //rPTID150.Rptid = 150;
                //rPTID150.VIDs = new List<long>() { 1090, 1106, 1105 };
                //RPTIDs.Add(rPTID150);

                //RPTID rPTID200 = new RPTID();
                //rPTID200.Rptid = 200;
                //rPTID200.VIDs = new List<long>() { 1035, 1036, 5002, 1520, 1140, 1141 };
                //RPTIDs.Add(rPTID200);

                //RPTID rPTID300 = new RPTID();
                //rPTID300.Rptid = 300;
                //rPTID300.VIDs = new List<long>() { 1090, 1035, 1520, 30098 };
                //RPTIDs.Add(rPTID300);

                //RPTID rPTID301 = new RPTID();
                //rPTID301.Rptid = 301;
                //rPTID301.VIDs = new List<long>() { 1520 };
                //RPTIDs.Add(rPTID301);

                //RPTID rPTID302 = new RPTID();
                //rPTID302.Rptid = 302;
                //rPTID302.VIDs = new List<long>() { 1035,1520,1503,1508 };
                //RPTIDs.Add(rPTID302);

                //RPTID rPTID400 = new RPTID();
                //rPTID400.Rptid = 400;
                //rPTID400.VIDs = new List<long>() { 1038, 1039, 1040 };
                //RPTIDs.Add(rPTID400);

                //RPTID rPTID870 = new RPTID();
                //rPTID870.Rptid = 870;
                //rPTID870.VIDs = new List<long>() { 1035 };
                //RPTIDs.Add(rPTID870);

                //CEID cEID = new CEID() { Ceid = 101 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 102 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 103 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 104 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 105 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 106 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 107 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 108 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 109 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 110 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 112 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 113 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 115 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 121 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 122 };
                //cEID.RPTIDs.Add(rPTID100.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 141 };
                //cEID.RPTIDs.Add(rPTID140.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 142 };
                //cEID.RPTIDs.Add(rPTID140.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 151 };
                //cEID.RPTIDs.Add(rPTID150.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 152 };
                //cEID.RPTIDs.Add(rPTID150.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 153 };
                //cEID.RPTIDs.Add(rPTID150.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 200 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 201 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 202 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 203 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 204 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 205 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 206 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 207 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 208 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 209 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 210 };
                //cEID.RPTIDs.Add(rPTID200.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 301 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 302 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 303 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 304 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 305 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 306 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 307 };
                //cEID.RPTIDs.Add(rPTID300.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 401 };
                //cEID.RPTIDs.Add(rPTID400.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 1331 };
                //cEID.RPTIDs.Add(rPTID301.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 1351 };
                //cEID.RPTIDs.Add(rPTID302.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 1351 };
                //cEID.RPTIDs.Add(rPTID302.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 9023 };
                //cEID.RPTIDs.Add(rPTID870.Rptid);
                //CEIDs.Add(cEID);

                //cEID = new CEID() { Ceid = 9024 };
                //cEID.RPTIDs.Add(rPTID870.Rptid);
                //CEIDs.Add(cEID);

                #endregion

                #region Rptid Main
                RPTID rPTID100 = new RPTID();
                rPTID100.Rptid = 100;
                rPTID100.VIDs = new List<long>() { 1600 };
                RPTIDs.Add(rPTID100);

                RPTID rPTID300 = new RPTID();
                rPTID300.Rptid = 300;
                rPTID300.VIDs = new List<long>() { 2002, 2003, 1600, 1120, 2005 };
                RPTIDs.Add(rPTID300);

                RPTID rPTID301 = new RPTID();
                rPTID301.Rptid = 301;
                rPTID301.VIDs = new List<long>() { 2002, 1606, 1607, 1605, 1600, 1120, 2004 };
                RPTIDs.Add(rPTID301);

                RPTID rPTID302 = new RPTID();
                rPTID302.Rptid = 302;
                rPTID302.VIDs = new List<long>() { 2002, 1701, 1600 };
                RPTIDs.Add(rPTID302);

                RPTID rPTID303 = new RPTID();
                rPTID303.Rptid = 303;
                rPTID303.VIDs = new List<long>() { 2002, 2004, 2005 };
                RPTIDs.Add(rPTID303);

                RPTID rPTID304 = new RPTID();
                rPTID304.Rptid = 304;
                rPTID304.VIDs = new List<long>() { 5001, 1120, 2006 };
                RPTIDs.Add(rPTID304);

                RPTID rPTID600 = new RPTID();
                rPTID600.Rptid = 600;
                rPTID600.VIDs = new List<long>() { 5001, 5002, 1120, 5004 };
                RPTIDs.Add(rPTID600);

                RPTID rPTID601 = new RPTID();
                rPTID601.Rptid = 601;
                rPTID601.VIDs = new List<long>() { 5001, 5002 };
                RPTIDs.Add(rPTID601);

                RPTID rPTID602 = new RPTID();
                rPTID602.Rptid = 602;
                rPTID602.VIDs = new List<long>() { 10022 };
                RPTIDs.Add(rPTID602);

                RPTID rPTID603 = new RPTID();
                rPTID603.Rptid = 603;
                rPTID603.VIDs = new List<long>() { 10021 };
                RPTIDs.Add(rPTID603);


                CEID cEID = new CEID() { Ceid = 2017 };
                cEID.RPTIDs.Add(rPTID303.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 2018 };
                cEID.RPTIDs.Add(rPTID303.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 2050 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 2051 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3001 };
                cEID.RPTIDs.Add(rPTID304.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3002 };
                cEID.RPTIDs.Add(rPTID304.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3101 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3102 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3103 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3104 };
                cEID.RPTIDs.Add(rPTID301.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3105 };
                cEID.RPTIDs.Add(rPTID301.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3113 };
                cEID.RPTIDs.Add(rPTID301.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3114 };
                cEID.RPTIDs.Add(rPTID301.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3301 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3302 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3303 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3351 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3352 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3353 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3401 };
                cEID.RPTIDs.Add(rPTID302.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3402 };
                cEID.RPTIDs.Add(rPTID302.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3403 };
                cEID.RPTIDs.Add(rPTID302.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3451 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3452 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3453 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3501 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3502 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3503 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3504 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3505 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3601 };
                cEID.RPTIDs.Add(rPTID303.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3602 };
                cEID.RPTIDs.Add(rPTID303.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 3603 };
                cEID.RPTIDs.Add(rPTID303.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 5001 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 5002 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 6001 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 6002 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 6006 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 6007 };
                cEID.RPTIDs.Add(rPTID300.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 6452 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 6453 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 6462 };
                cEID.RPTIDs.Add(rPTID600.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 6463 };
                cEID.RPTIDs.Add(rPTID600.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 7100 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9003 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9004 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9005 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9006 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9014 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9015 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9020 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9021 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9022 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9036 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9037 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9038 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9039 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9046 };
                cEID.RPTIDs.Add(rPTID603.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9047 };
                cEID.RPTIDs.Add(rPTID602.Rptid);
                CEIDs.Add(cEID);

                cEID = new CEID() { Ceid = 9051 };
                cEID.RPTIDs.Add(rPTID601.Rptid);
                CEIDs.Add(cEID);

                #endregion

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }
        }

    }

    [Serializable]
    [DataContract]
    public class CEID 
    {
        private long _Ceid;
        [DataMember]
        public long Ceid
        {
            get { return _Ceid; }
            set { _Ceid = value; }
        }

        private List<long> _RPTIDs = new List<long>();
        [DataMember]
        public List<long> RPTIDs
        {
            get { return _RPTIDs; }
            set { _RPTIDs = value; }
        }

        public CEID()
        {

        }
    }

    [Serializable]
    [DataContract]
    public class RPTID
    {
        private long _Rptid;
        [DataMember]
        public long Rptid
        {
            get { return _Rptid; }
            set { _Rptid = value; }
        }

        private List<long> _VIDs = new List<long>();
        [DataMember]
        public List<long> VIDs
        {
            get { return _VIDs; }
            set { _VIDs = value; }
        }

        public RPTID()
        {

        }
    }
}
