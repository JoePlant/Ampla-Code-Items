using System;
using System.Diagnostics;
using Citect.Ampla.Connectors.OpcHdaConnector;
using Citect.Ampla.Framework;
using Citect.Ampla.Runtime.Data;
using Citect.Ampla.Runtime.Data.Samples;
using Citect.Ampla.Runtime.Data.Streams;
using Citect.Common;
using Code.Services;

namespace Code.Connectors
{

    public interface IFluentOpcHdaMonitor
    {
        IFluentOpcHdaMonitor Monitor(Item opcHdaDataSource);
        IFluentOpcHdaMonitor UpdateEvery(ISampleStream timerStream);
        IFluentOpcHdaMonitor AlertOnBrokenConnection();
        Sample GetSample(Project project, DateTime time);
    }

    public class OpcHdaMonitor : IFluentOpcHdaMonitor
    {
        public OpcHdaMonitor(Item host)
        {
            hostName = host.FullName;
        }

        private string dataSourceFullName;
        private readonly string hostName;
        private bool alertBrokenConnection;
        private Type smsServiceType;
        private string mobileNumber;

        public IFluentOpcHdaMonitor Monitor(Item opcHdaDataSource)
        {
            if (opcHdaDataSource.GetType().FullName == "Citect.Ampla.Connectors.OpcHdaConnector.OpcHdaConnection")
            {
                dataSourceFullName = opcHdaDataSource.FullName;
            }
            else
            {
                string message = string.Format("OpcHdaMonitor - Invalid connection - Monitor({0})", opcHdaDataSource.FullName);
                Diagnostics.Write(TraceLevel.Warning, message);
            }
            return this;
        }

        public IFluentOpcHdaMonitor UpdateEvery(ISampleStream timerStream)
        {
            // Expression  sampleStream is used to register the dependency
            return this;
        }

        public IFluentOpcHdaMonitor AlertOnBrokenConnection()
        {
            alertBrokenConnection = true;
            return this;
        }

        public Sample GetSample(Project project, DateTime time)
        {
            if (string.IsNullOrEmpty(dataSourceFullName))
            {
                Diagnostics.Write(TraceLevel.Warning, "OpcHdaMonitor object on {0} is not configured.", hostName);
                return new Sample(time, Quality.BadConfigurationError);
            }

            OpcHdaConnection connection = project[dataSourceFullName] as OpcHdaConnection;
            if (connection == null)
            {
                Diagnostics.Write(TraceLevel.Warning, "OpcHdaMonitor object on {0} is not configured correctly.  Expected Monitor({1}) to reference an OPC-HDA Connection item.", hostName, dataSourceFullName);
                return new Sample(time, Quality.BadConfigurationError);
            }

            object cs = connection.ConnectionState;
            string connectionState = Convert.ToString(cs);

            if (alertBrokenConnection)
            {
                if (connectionState == "Broken")
                {
                    Diagnostics.Write(TraceLevel.Warning, "OpcHdaMonitor detected a broken Connection on {0}.", dataSourceFullName);
                }
            }
            return new Sample(time, connectionState, Quality.Good);
        }
    }
}