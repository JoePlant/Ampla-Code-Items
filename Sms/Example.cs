using System;
using Citect.Ampla.Framework;
using Citect.Ampla.Runtime.Data.Streams;
using Code.Services;

namespace Code.Sms
{
    public class Example
    {
        public Item host;
        public ISampleStream checkStream;
        
        public void Main(Project project, DateTime time)
        {
            new Code.Sms.SmsAction(host)
                .Using(new AmplaMessageService())
                .IfTrue(checkStream)
                .Then()
                    .SendMessage("OPC-HDA is broken!")
                    .ToMobile("000-000-000-1")
                .And()
                .Then()
                    .SendMessage("OPC-HDA is broken - notice")
                    .ToMobile("000-000-000-2")
                .And()
                .AfterDelay(TimeSpan.FromMinutes(30))
                    .SendMessage("OPC-HDA has been broken for 30 minutes")
                    .ToMobile("000-000-001-1")
                .And()
                .AfterDelay(TimeSpan.FromMinutes(30))
                    .SendMessage("OPC-HDA has been broken for 60 minutes")
                    .ToMobile("000-000-002-1")
                .Evaluate(project, time);
        }
    }
}