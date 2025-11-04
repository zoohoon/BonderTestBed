namespace ProberInterfaces.Foup
{
    public abstract class FoupCommandBase
    {
    }

    public class FoupInitStateCommand : FoupCommandBase { }

    public class FoupDeviceSetupCommand : FoupCommandBase
    {
        public FoupDeviceParam DeviceParam { get; set; }
    }
    public class FoupLoadCommand : FoupCommandBase { }
    public class FosB_LoadCommand : FoupCommandBase { }
    public class FoupUnloadCommand : FoupCommandBase { }
    public class FosB_UnloadCommand : FoupCommandBase { }
    public class FoupNormalUnloadCommand : FoupCommandBase { }
    public class FoupCoverUpCommand : FoupCommandBase { }
    public class FoupCoverDownCommand : FoupCommandBase { }
    public class FoupDockingPlateLockCommand : FoupCommandBase { }
    public class FoupDockingPlateUnlockCommand : FoupCommandBase { }
    public class FoupDockingPortInCommand : FoupCommandBase { }
    public class FoupDockingPortOutCommand : FoupCommandBase { }
    public class FoupDockingPort40InCommand : FoupCommandBase { }
    public class FoupDockingPort40OutCommand : FoupCommandBase { }
    public class FoupCassetteOpenerLockCommand : FoupCommandBase { }
    public class FoupCassetteOpenerUnlockCommand : FoupCommandBase { }
    public class FoupRecoveryNextCommand : FoupCommandBase { }
    public class FoupRecoveryFastForwardCommand : FoupCommandBase { }
    public class FoupRecoveryPreviousCommand : FoupCommandBase { }
    public class FoupRecoveryReverseCommand : FoupCommandBase { }
    public class FoupRecoveryFastBackwardCommand : FoupCommandBase { }
    public class FoupTiltUpCommand : FoupCommandBase { }
    public class FoupTiltDownCommand : FoupCommandBase { }
}
