// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace FabricObserver.Observers.MachineInfoModel
{
    public class ApplicationInfo
    {
        public string TargetApp { get; set; }

        public string TargetAppType { get; set; }

        public string ServiceExcludeList { get; set; }

        public string ServiceIncludeList { get; set; }

        public long MemoryWarningLimitMb { get; set; }

        public long MemoryErrorLimitMb { get; set; }

        public int MemoryWarningLimitPercent { get; set; }

        public int MemoryErrorLimitPercent { get; set; }

        public int CpuErrorLimitPercent { get; set; }

        public int CpuWarningLimitPercent { get; set; }

        public int NetworkErrorActivePorts { get; set; }

        public int NetworkWarningActivePorts { get; set; }

        public int NetworkErrorEphemeralPorts { get; set; }

        public int NetworkWarningEphemeralPorts { get; set; }

        public int ReadOpSecWarning { get; set; }

        public int ReadOpSecError { get; set; }

        public int ReadOpBytesWarning { get; set; }

        public int ReadOpBytesError { get; set; }

        public int WriteOpSecWarning { get; set; }

        public int WriteOpSecError { get; set; }

        public int WriteOpBytesWarning { get; set; }

        public int WriteOpBytesError { get; set; }

        public int DataOpSecWarning { get; set; }

        public int DataOpSecError { get; set; }

        public int DataOpBytesWarning { get; set; }

        public int DataOpBytesError { get; set; }

        public int NetworkErrorFirewallRules { get; set; }

        public int NetworkWarningFirewallRules { get; set; }

        public bool DumpProcessOnError { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"ApplicationName: {this.TargetApp ?? string.Empty}\n" +
                                             $"ApplicationTypeName: {this.TargetAppType ?? string.Empty}\n" +
                                             $"ServiceExcludeList: {this.ServiceExcludeList ?? string.Empty}\n" +
                                             $"ServiceIncludeList: {this.ServiceIncludeList ?? string.Empty}\n" +
                                             $"MemoryWarningLimitMB: {this.MemoryWarningLimitMb}\n" +
                                             $"MemoryErrorLimitMB: {this.MemoryErrorLimitMb}\n" +
                                             $"MemoryWarningLimitPercent: {this.MemoryWarningLimitPercent}\n" +
                                             $"MemoryErrorLimitPercent: {this.MemoryErrorLimitPercent}\n" +
                                             $"CpuWarningLimitPercent: {this.CpuWarningLimitPercent}\n" +
                                             $"CpuErrorLimitPercent: {this.CpuErrorLimitPercent}\n" +
                                             $"NetworkErrorActivePorts: {this.NetworkErrorActivePorts}\n" +
                                             $"NetworkWarningActivePorts: {this.NetworkWarningActivePorts}\n" +
                                             $"NetworkErrorEphemeralPorts: {this.NetworkErrorEphemeralPorts}\n" +
                                             $"NetworkWarningEphemeralPorts: {this.NetworkWarningEphemeralPorts}\n" +
                                             $"ReadOpSecWarning: {this.ReadOpSecWarning}\n" +
                                             $"ReadOpSecError: {this.ReadOpSecError}\n" +
                                             $"ReadOpBytesWarning: {this.ReadOpBytesWarning}\n" +
                                             $"ReadOpBytesError: {this.ReadOpBytesError}\n" +
                                             $"WriteOpSecWarning: {this.WriteOpSecWarning}\n" +
                                             $"WriteOpSecError: {this.WriteOpSecError}\n" +
                                             $"WriteOpBytesWarning: {this.WriteOpBytesWarning}\n" +
                                             $"WriteOpBytesError: {this.WriteOpBytesError}\n" +
                                             $"DataOpSecWarning: {this.DataOpSecWarning}\n" +
                                             $"DataOpSecError: {this.DataOpSecError}\n" +
                                             $"DataOpBytesWarning: {this.DataOpBytesWarning}\n" +
                                             $"DataOpBytesError: {this.DataOpBytesError}\n" +
                                             $"NetworkErrorFirewallRules: {this.NetworkErrorFirewallRules}\n" +
                                             $"NetworkWarningFirewallRules: {this.NetworkWarningFirewallRules}\n" +
                                             $"DumpProcessOnError: {this.DumpProcessOnError}\n";
    }
}