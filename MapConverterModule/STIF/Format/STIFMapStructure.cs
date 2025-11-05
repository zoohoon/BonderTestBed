using LogModule;
using ProberInterfaces.ResultMap.Script;
using System;
using System.Collections.Generic;

namespace MapConverterModule.STIF
{
    public enum STIFCOMPONENTTYPE
    {
        SIGNATURE,
        HEADER,
        MAP,
        FOOTER
    }

    public class STIFMapStructure
    {
        // 데이터의 구분

        // (1). Signature
        // (2). Header
        // (3). Map
        // (4). Fotter

        public List<MapScriptElement> Signatures { get; set; }
        public List<MapScriptElement> Headers { get; set; }
        public List<MapScriptElement> Maps { get; set; }
        public List<MapScriptElement> Footers { get; set; }
        public string Checksum { get; set; }

        public List<MapScriptElement> FullMap { get; set; }

        public STIFMapStructure()
        {
            Signatures = new List<MapScriptElement>();
            Headers = new List<MapScriptElement>();
            Maps = new List<MapScriptElement>();
            Footers = new List<MapScriptElement>();
            //this.Signature = $"WM - V1.1 - STMicroelectronics Wafer Map File";
        }

        public void MakeFullMap(List<STIFCOMPONENTTYPE> orders)
        {
            try
            {
                if (this.FullMap == null)
                {
                    this.FullMap = new List<MapScriptElement>();
                }
                else
                {
                    this.FullMap.Clear();
                }

                foreach (var order in orders)
                {
                    switch (order)
                    {
                        case STIFCOMPONENTTYPE.SIGNATURE:
                            this.FullMap.AddRange(this.Signatures);
                            break;
                        case STIFCOMPONENTTYPE.HEADER:
                            this.FullMap.AddRange(this.Headers);
                            break;
                        case STIFCOMPONENTTYPE.MAP:
                            this.FullMap.AddRange(this.Maps);
                            break;
                        case STIFCOMPONENTTYPE.FOOTER:
                            this.FullMap.AddRange(this.Footers);
                            break;
                        default:
                            break;
                    }
                }

                //foreach (var item in this.FullMap)
                //{
                //    item.Text += "\r\n";
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void AddChecksum(string checksum)
        {
            MapScriptElement tmp = new MapScriptElement();

            try
            {
                tmp.Text = "CHECKSUM" + "\t" + checksum;
                this.FullMap.Add(tmp);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }
    }
}
