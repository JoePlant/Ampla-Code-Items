using System;
using Citect.Ampla.Framework;
using Citect.Ampla.Runtime.Data;
using Citect.Ampla.Runtime.Data.Samples;
using Citect.Ampla.Runtime.Data.Streams;

namespace Code.TimeCalculations
{

    /// <summary>
    ///     Fluent interface for configuring OperatingTime Variables
    /// </summary>
    public interface IFluentOperatingTime
    {
        IFluentOperatingTime Using(ISampleStream trueSample);
        IFluentOperatingTime UpdateEvery(ISampleStream timerStream);
        IFluentOperatingTime TotalHours();
        IFluentOperatingTime TotalDays();
        Sample GetSample(Project project, DateTime time);
    }

    public class OperatingTimeVariable : IFluentOperatingTime
    {
        private ISampleStream usingStream;
        private bool calculateHours;
        private bool calculateDays;
        public IFluentOperatingTime Using(ISampleStream sampleStream)
        {
            usingStream = sampleStream;
            return this;
        }

        public IFluentOperatingTime UpdateEvery(ISampleStream timerStream)
        {
            // Expression  sampleStream is used to register the dependency
            return this;
        }

        public IFluentOperatingTime TotalHours()
        {
            calculateHours = true;
            calculateDays = false;
            return this;
        }

        public IFluentOperatingTime TotalDays()
        {
            calculateDays = true;
            calculateHours = false;
            return this;
        }

        public Sample GetSample(Project project, DateTime time)
        {
            
            int indexOfSample = usingStream.IndexOnOrBefore(time);
            if (indexOfSample >= 0)
            {
                ISample sample = usingStream[indexOfSample];
                if (sample.IsGood)
                {
                    bool value = false;
                    try
                    {
                        value = Convert.ToBoolean(sample.Value);
                    }
                    catch (Exception)
                    {
                        // swallow exceptions
                    }
                    if (value)
                    {
                        DateTime since = sample.TimeStamp;
                        TimeSpan age = time - since;
                        if (calculateDays)
                        {
                            return new Sample(time, age.TotalDays, Quality.GoodLocalOverride);
                        }
                        else if (calculateHours)
                        {
                            return new Sample(time, age.TotalHours, Quality.GoodNonspecific);
                        }
                        else
                        {
                            return new Sample(time, age.TotalSeconds, Quality.GoodNonspecific);
                        }
                    }
                }
            }
            return new Sample(time, 0.0D, Quality.GoodNonspecific);
        }
    }
}