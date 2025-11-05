using System;

namespace LampModule
{
    using ProberInterfaces;
    using ProberInterfaces.Lamp;

    /*
     using (LampBarrier lampBarrier = new LampBarrier(LampStatusEnum.Off, LampStatusEnum.BlinkOn, LampStatusEnum.Off, LampStatusEnum.On, AlarmPriority.Warning, sender: "Cognex Manual"))
     {
         window.ShowDialog();
     }
     */

    public class LampBarrier : IDisposable, IFactoryModule
    {
        private RequestCombination _LampCombo;
        public LampBarrier(
            LampStatusEnum redLamp,
            LampStatusEnum yellowLamp,
            LampStatusEnum greenLamp,
            LampStatusEnum buzzer,
            AlarmPriority priority = AlarmPriority.Warning,
            String sender = "")
        {
            _LampCombo = new RequestCombination(
                redLamp,
                yellowLamp,
                greenLamp,
                buzzer,
                priority,
                sender);

            this.LampManager().SetRequestLampCombo(_LampCombo);

        }
        public void Dispose()
        {
            this.LampManager().ClearRequestLamp();
        }
    }
}
