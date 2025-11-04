namespace ProberInterfaces
{
    public interface IForceMeasure: IModule
    {
        double ForceValue { get; set; }
        double FSensorOrg0 { get; set; }
        double FSensorOrg1 { get; set; }
        double FSensorOrg2 { get; set; }
        new bool Initialized { get; set; }
        //ProbeAxisObject Z0Axis { get; set; }
        double Z0ForceValue { get; set; }
        //ProbeAxisObject Z1Axis { get; set; }
        double Z1ForceValue { get; set; }
        //ProbeAxisObject Z2Axis { get; set; }
        double Z2ForceValue { get; set; }
        //ProbeAxisObject ZAxis { get; set; }
        double ZFOrgRawPos { get; set; }

        void ResetMeasurement();
        void MeasureProbingForce();
    }
}
