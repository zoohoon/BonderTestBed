using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using ProberInterfaces.Param;
using ProberInterfaces.PinAlign;

namespace InternalCommands
{
    public class DoPinAlign : ProbeCommand, IDOPINALIGN
    {
        public override bool Execute()
        {
            bool retVal = false;

            IPinAligner PinAligner = this.PinAligner();
            retVal = SetCommandTo(PinAligner);
            return retVal;
        }
    }
    public class DOPinAlignAfterSoaking : ProbeCommand, IDOPinAlignAfterSoaking
    {
        public override bool Execute()
        {
            IPinAligner PinAligner = this.PinAligner();
            PinAligner.UseSoakingSamplePinAlign = false;
                //PinAligner.SoakingPinTolerance.X.Value = tolparam.SoakingPinTolerance.X.Value;
                //PinAligner.SoakingPinTolerance.Y.Value = tolparam.SoakingPinTolerance.Y.Value;
                //PinAligner.SoakingPinTolerance.Z.Value = tolparam.SoakingPinTolerance.Z.Value;
            PinAligner.PinAlignSource = PINALIGNSOURCE.SOAKING;
            return SetCommandTo(PinAligner);
        }
    }

    public class DOSamplePinAlignForSoaking : ProbeCommand, IDOSamplePinAlignForSoaking
    {
        public override bool Execute()
        {
            IPinAligner PinAligner = this.PinAligner();
            PinAligner.UseSoakingSamplePinAlign = true;            
            PinAligner.PinAlignSource = PINALIGNSOURCE.SOAKING;
            return SetCommandTo(PinAligner);
        }
    }

    public class DoManualPinAlign : ProbeCommand, IDoManualPinAlign
    {
        public override bool Execute()
        {
            bool retVal;

            retVal = SetCommandTo(this.PinAligner());

            return retVal;
        }
    }

    public class SoakingCommandParam : IProbeCommandParameter
    {
        public string Command { get; set; }

        private PinCoordinate _SoakingPinTolerance = new PinCoordinate();

        public PinCoordinate SoakingPinTolerance
        {
            get { return _SoakingPinTolerance; }
            set { _SoakingPinTolerance = value; }
        }

    }
}
