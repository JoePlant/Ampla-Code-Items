using System;
using System.Collections.Generic;
using System.Diagnostics;
using Citect.Ampla.Framework;
using Citect.Ampla.Runtime.Data.Samples;
using Citect.Ampla.Runtime.Data.Streams;
using Citect.Common;


namespace Code.Sms
{
    public interface IFluentSms
    {
        IFluentSms Using(ISmsService service);
        IFluentTime IfTrue(ISampleStream stream);
    }

    public interface IFluentTime
    {
        IFluentAction Then();
        IFluentAction AfterDelay(TimeSpan delay);
    }

    public interface IFluentAction
    {
        IFluentAction SendMessage(string message);
        IFluentAction ToMobile(string mobile);
        
        IFluentTime And();
        void Evaluate(Project project, TimePeriod period);
    }

    public class SmsAction : IFluentSms
    {
        private ISmsService smsService;
        private ISampleStream ifStream;
        private TimeSpan timeline = TimeSpan.Zero;
        private readonly List<FluentAction> actions = new List<FluentAction>();
        private readonly string hostFullName;

        public SmsAction(Item host)
        {
            hostFullName = host.FullName;
        }

        public IFluentSms Using(ISmsService service)
        {
            smsService = service;
            return this;
        }

        public IFluentTime IfTrue(ISampleStream stream)
        {
            ifStream = stream;
            return CreateTime();
        }

        private FluentAction CreateAction(TimeSpan delay)
        {
            timeline = timeline.Add(delay);
            FluentAction action = new FluentAction(this, timeline);
            actions.Add(action);
            action.Priority = actions.Count;
            return action;
        }

        private FluentTime CreateTime()
        {
            return new FluentTime(this);
        }

        private void Evaluate(Project project, TimePeriod period)
        {
            if (ifStream == null)
            {
                Log("SmsAction: {0} is not monitoring a stream.", hostFullName);
            }

            bool result; // whether the result is true.
            DateTime timeStamp; // the time of the last result

            if (TryEvaluateSampleAt(ifStream, period.End, out result, out timeStamp))
            {
                if (result)
                {
                    List<FluentAction> list = new List<FluentAction>();
                    foreach (FluentAction action in actions)
                    {
                        if (action.ShouldSend(timeStamp, period))
                        {
                            list.Add(action);
                        }
                    }

                    foreach (FluentAction action in list)
                    {
                        action.ExecuteSend(smsService);
                    }
                }
            }
        }

        private bool TryEvaluateSampleAt(ISampleStream stream, DateTime time, out bool result, out DateTime timeStamp)
        {
            bool returnValue = false;
            result = false;
            timeStamp = DateTime.MinValue;

            ISample sample = stream[time];
            if (sample != null)
            {
                if (sample.IsGood)
                {
                    try
                    {
                        bool value = Convert.ToBoolean(sample.Value);
                        timeStamp = sample.TimeStamp;

                        result = value;
                        returnValue = true;
                    }
                    catch (Exception)
                    {
                        // swallow exceptions
                    }
                }
            }
            return returnValue;
        }

        private void Log(string format, params object[] args)
        {
            Citect.Common.Diagnostics.Write(TraceLevel.Warning, format, args);
        }

        private class FluentTime : IFluentTime
        {
            private readonly SmsAction parent;
            private FluentAction action;

            internal FluentTime(SmsAction parent)
            {
                this.parent = parent;
            }

            public IFluentAction Then()
            {
                action = parent.CreateAction(TimeSpan.Zero);
                return action;
            }

            public IFluentAction AfterDelay(TimeSpan delta)
            {
                action = parent.CreateAction(delta);
                return action;
            }
        }

        private class FluentAction : IFluentAction
        {
            public FluentAction(SmsAction parent, TimeSpan delay)
            {
                this.parent = parent;
                afterDelay = delay;
            }

            private readonly TimeSpan afterDelay;
            private readonly SmsAction parent;
            private string smsMessage;
            private readonly List<string> mobileList = new List<string>();

            public int Priority
            {
                get; set; 
            }

            public bool ShouldSend(DateTime occuranceTime, TimePeriod period)
            {
                DateTime actionTime = occuranceTime.Add(afterDelay);
                return period.Includes(actionTime);
            }

            public IFluentAction SendMessage(string message)
            {
                smsMessage = message;
                return this;
            }

            public IFluentAction ToMobile(string mobile)
            {
                if (!string.IsNullOrEmpty(mobile))
                {
                    mobileList.Add(mobile);
                }
                return this;
            }

            public IFluentTime And()
            {
                return parent.CreateTime();
            }

            public void Evaluate(Project project, TimePeriod period)
            {
                parent.Evaluate(project, period);
            }

            public void ExecuteSend(ISmsService smsService)
            {
                foreach (string mobile in mobileList)
                {
                    smsService.SendSms(mobile, smsMessage);   
                }
            }
        }
    }
}