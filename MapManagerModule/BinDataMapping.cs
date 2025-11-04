using LogModule;
using ProberInterfaces;
using ProbingDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResultMapModule
{
    public abstract class BinDataMappingBase
    {
        public abstract void MakeDefinition();

        public int? GetTestBinCode(TestState teststate)
        {
            int? retval = null;

            try
            {
                var definitions = mappingDefinitionBases.FindAll(x => x is MappingDefinitionTestDieType);

                MappingDefinitionBase definition = definitions.FirstOrDefault(x => (x as MappingDefinitionTestDieType).State == teststate);

                if(definition != null)
                {
                    retval = definition.GetValue();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public int? GetNoneTestBinCode(DieTypeEnum dietype)
        {
            int? retval = null;

            try
            {
                var definitions = mappingDefinitionBases.FindAll(x => x is MappingDefinitionNoneTestDieType);

                MappingDefinitionBase definition = definitions.FirstOrDefault(x => (x as MappingDefinitionNoneTestDieType).DieType == dietype);

                if (definition != null)
                {
                    retval = definition.GetValue();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public List<MappingDefinitionBase> mappingDefinitionBases { get; set; }

        public BinDataMappingBase()
        {
            MakeDefinition();
        }
    }

    public class E142BinDataMapping : BinDataMappingBase
    {
        public override void MakeDefinition()
        {
            try
            {
                string hexValue = string.Empty;
                int? decValue = null;

                // SKIP 정의

                this.mappingDefinitionBases = new List<MappingDefinitionBase>();

                MappingDefinitionTestDieType tmpTestDefinition = null;
                MappingDefinitionNoneTestDieType mappingDefinitionNoneTestDieType = null;

                // TEST & PASS
                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_PASS);
                tmpTestDefinition.SetRandomProperties(true, 1, 253);

                mappingDefinitionBases.Add(tmpTestDefinition);

                // TEST & FAIL

                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_FAIL);
                //tmpTestDefinition.SetRandomProperties(true, 5, 89);
                tmpTestDefinition.MappingValue = 0;

                mappingDefinitionBases.Add(tmpTestDefinition);

                // NONETEST & MARK
                // Using edgeorcornerbincode (FE)
                hexValue = "FE";
                decValue = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
                //decValue = Convert.ToInt32(hexValue, 16);

                mappingDefinitionNoneTestDieType = new MappingDefinitionNoneTestDieType(DieTypeEnum.MARK_DIE);
                mappingDefinitionNoneTestDieType.MappingValue = (int)decValue;

                mappingDefinitionBases.Add(mappingDefinitionNoneTestDieType);

                // NONETEST & NOTEXIST
                // Using nullbincode (FF)
                hexValue = "FF";
                decValue = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
                mappingDefinitionNoneTestDieType = new MappingDefinitionNoneTestDieType(DieTypeEnum.NOT_EXIST);
                mappingDefinitionNoneTestDieType.MappingValue = (int)decValue;

                mappingDefinitionBases.Add(mappingDefinitionNoneTestDieType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class STIFBinDataMapping : BinDataMappingBase
    {
        public override void MakeDefinition()
        {
            try
            {
                //  | OLI BIN CODE | STIF ASCII code | Test Result |
                //  |    0 - 89    |    32 - 121     | Bad Dice    |
                //  |    1 - 72    |    161 - 232    | Good Dice   |

                // SKIP 정의

                this.mappingDefinitionBases = new List<MappingDefinitionBase>();

                MappingDefinitionTestDieType tmpTestDefinition = null;
                MappingDefinitionNoneTestDieType mappingDefinitionNoneTestDieType = null;

                // TEST & PASS
                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_PASS);
                tmpTestDefinition.SetRandomProperties(true, 1, 4); // 1 ~ 72 ?

                mappingDefinitionBases.Add(tmpTestDefinition);

                // TEST & FAIL

                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_FAIL);
                tmpTestDefinition.SetRandomProperties(true, 5, 89); //  0 ~ 89 ?

                mappingDefinitionBases.Add(tmpTestDefinition);

                // TEST & MAP_STS_NOT_EXIST

                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_NOT_EXIST);
                tmpTestDefinition.MappingValue = 94;

                mappingDefinitionBases.Add(tmpTestDefinition);

                // TEST & MAP_STS_SKIP

                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_SKIP);
                tmpTestDefinition.MappingValue = 94;

                mappingDefinitionBases.Add(tmpTestDefinition);

                // TEST & MAP_STS_MARK

                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_MARK);
                tmpTestDefinition.MappingValue = 94;

                mappingDefinitionBases.Add(tmpTestDefinition);

                // TEST & MAP_STS_FAIL

                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_FAIL);
                tmpTestDefinition.MappingValue = 94;

                mappingDefinitionBases.Add(tmpTestDefinition);

                // TEST & MAP_STS_CUR_DIE

                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_CUR_DIE);
                tmpTestDefinition.MappingValue = 94;

                mappingDefinitionBases.Add(tmpTestDefinition);

                tmpTestDefinition = new MappingDefinitionTestDieType(TestState.MAP_STS_TEACH);
                tmpTestDefinition.MappingValue = 94;

                mappingDefinitionBases.Add(tmpTestDefinition);

                // NONETEST & MARK
                // Using edgeorcornerbincode (90)
                mappingDefinitionNoneTestDieType = new MappingDefinitionNoneTestDieType(DieTypeEnum.MARK_DIE);
                mappingDefinitionNoneTestDieType.MappingValue = 90;

                mappingDefinitionBases.Add(mappingDefinitionNoneTestDieType);

                // NONETEST & TARGET 
                mappingDefinitionNoneTestDieType = new MappingDefinitionNoneTestDieType(DieTypeEnum.TARGET_DIE);
                mappingDefinitionNoneTestDieType.MappingValue = 93;

                mappingDefinitionBases.Add(mappingDefinitionNoneTestDieType);

                // NONETEST & NOTEXIST
                // Using nullbincode (94)
                mappingDefinitionNoneTestDieType = new MappingDefinitionNoneTestDieType(DieTypeEnum.NOT_EXIST);
                mappingDefinitionNoneTestDieType.MappingValue = 94;

                mappingDefinitionBases.Add(mappingDefinitionNoneTestDieType);

                // NONETEST & NOTEXIST
                // Using nullbincode (94)
                mappingDefinitionNoneTestDieType = new MappingDefinitionNoneTestDieType(DieTypeEnum.SKIP_DIE);
                mappingDefinitionNoneTestDieType.MappingValue = 94;

                mappingDefinitionBases.Add(mappingDefinitionNoneTestDieType);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public abstract class MappingDefinitionBase
    {
        public abstract int? GetValue();
    }

    public class MappingDefinitionTestDieType : MappingDefinitionBase
    {
        public TestState State { get; set; }
        public int MappingValue { get; set; }

        private RandomComponent randomComponent { get; set; }

        public MappingDefinitionTestDieType(TestState state)
        {
            this.State = state;
            randomComponent = new RandomComponent();
        }

        public void SetRandomProperties(bool enable, int min, int max)
        {
            if(randomComponent == null)
            {
                randomComponent = new RandomComponent();
            }

            randomComponent.Enable = enable;
            randomComponent.MinValue = min;
            randomComponent.MaxValue = max;
        }

        public override int? GetValue()
        {
            int? retval = null;

            try
            {
                if(randomComponent.Enable == true)
                {
                    retval = randomComponent.GetValue();
                }
                else
                {
                    retval = MappingValue;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class MappingDefinitionNoneTestDieType : MappingDefinitionBase
    {
        public DieTypeEnum DieType { get; set; }
        public int MappingValue { get; set; }

        public MappingDefinitionNoneTestDieType(DieTypeEnum dietype)
        {
            this.DieType = dietype;
        }

        public override int? GetValue()
        {
            int? retval = null;

            try
            {
                retval = MappingValue;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }

    public class RandomComponent
    {
        // TODO : Value type ?
        /// 

        Random random = new Random(DateTime.Now.Millisecond);
        /// <summary>
        /// Default = false;
        /// </summary>
        public bool Enable { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public RandomComponent()
        {
            this.Enable = false;
        }

        public int? GetValue()
        {
            int? retval = null;

            try
            {
                retval = random.Next(MinValue, MaxValue + 1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
