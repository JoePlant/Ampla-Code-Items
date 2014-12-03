using System;
using System.Diagnostics;
using Citect.Ampla.Framework;
using Citect.Ampla.Runtime.Data.Samples;
using Citect.Ampla.Runtime.Data;
using Citect.Common;

namespace Code
{

	public static class Conditions
	{
		private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(5);
		
		public static Sample AllSamples(Sample sample)
		{
			return sample;
		}
			
		public static Sample IgnoreOldSamples(Item item, Sample sample)
		{
			TimeSpan age = DateTime.UtcNow - sample.TimeStamp;
			if (age < MaxAge)
			{
				return sample;
			}
			
			Trace(item, "Ignoring old samples. Age={0:0.0} min", age.TotalMinutes);
		
			return new Sample(sample.SampleTypeCode, sample.TimeStamp, sample.Value, Quality.Bad) ;
		}
		
		private static void Trace(Item item, string format, params object[] args)
        {
			item.WriteTraceMessage(TraceLevel.Warning, format, args);
        }
		
		private static void Trace(string format, params object[] args)
        {
			Diagnostics.Write(TraceLevel.Info, format, args);
       	}

	}
}

