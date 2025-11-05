using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SorterSystem
{
    using ProberInterfaces;
    using Motion;
    using System.Collections.ObjectModel;
    using ProbeMotion;
    using LogModule;
    using SciChart.Charting.Visuals;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SciChartSurface.SetRuntimeLicenseKey(@"<LicenseContract>
                <Customer>Semics</Customer>
                <OrderId>ABT190823-9438-44125</OrderId>
                <LicenseCount>1</LicenseCount>
                <IsTrialLicense>false</IsTrialLicense>
                <SupportExpires>08/22/2020 00:00:00</SupportExpires>
                <ProductCode>SC-WPF-2D-ENTERPRISE-SRC</ProductCode>
                <KeyCode>lwAAAQEAAAA4sZ8VwFnVAXEAQ3VzdG9tZXI9U2VtaWNzO09yZGVySWQ9QUJUMTkwODIzLTk0MzgtNDQxMjU7U3Vic2NyaXB0aW9uVmFsaWRUbz0yMi1BdWctMjAyMDtQcm9kdWN0Q29kZT1TQy1XUEYtMkQtRU5URVJQUklTRS1TUkMestJKxT7JacnNzG+RIIi8Gbc78LhqY0Jf/G1UbPjQ+2O9sxKq6fxli7uHDjo2J0E=</KeyCode>
                </LicenseContract>");
        }
    }
}
