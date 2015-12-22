Sms
================

Provides some classes for sending Sms messages to an Sms service when a condition it true.



##Code##

The following code items are required

* [Code.Sms.SmsAction.cs](../SmsAction.cs)
* [Code.Sms.SmsService.cs](../SmsService.cs)
* [Code.Sms.ConnectionStrings.cs](../ConnectionStrings.cs)
* [Code.Database.SqlHelper.cs](../Database/SqlHelper.cs)

###Requirements###
The purpose of the Sms code is to trigger an Sms message to a mobile when a condition is true.  
If the condition stays true for multiple amounts of time, then it will other messages to be send to different numbers.

###Configuration###
The configuration uses an ```Action``` item to trigger when a dependency changes.

###Example usage###
Requirement 1: 

* Send a message "It has happend" when a condition is true to the mobiles 123-456-789 and 123-456-780. 
* If the condition is still true after 60 seconds then send a "different" message to 987-654-321

###Action Function###
 
```
CSharp
new Code.Sms.SmsAction(host)
	.Using(new SqlSmsService())
//	.Using(new AmplaMessageService())
    .IfTrue(Project.Enterprise.Site.Area.Sms.Monitoring)
    .Then()
    	.SendMessage("It has happened")
        .ToMobile("123-456-789")
		.ToMobile("123-456-780")
	.And()
    .AfterDelay(TimeSpan.FromSeconds(60))
		.SendMessage("different")
        .ToMobile("987-654-321")
    .Evaluate(project, period);

```

Requirement 2: 

* Send a message "It has happened" when a condition is true to the mobile 123-456-789. 
* Send a message "It has happened again" when a condition is true to the mobile 987-654-321

###Action Function###
 
```
CSharp
new Code.Sms.SmsAction(host)
	.Using(new SqlSmsService())
//	.Using(new AmplaMessageService())
    .IfTrue(Project.Enterprise.Site.Area.Sms.Monitoring)
    .Then()
    	.SendMessage("It has happened")
        .ToMobile("123-456-789")
	.And()
	.Then()
		.SendMessage("It has happened again")
        .ToMobile("987-654-321")
    .Evaluate(project, period);

```

#Configuring the Action#

The action will require the following properties to be set:

* ```EnableExecutionsOnDependencies``` = ```True```
* ```EnableExecutionsOnStart``` = ```False```
* ```EnableExecutionsOnSubscriptions``` = ```False```
* ```EnableManualExecution``` = ```False```

* ```Dependencies``` = (collection of dependencies)
* ```DependencyExecutionMode``` = ```ExecuteForEachPeriod```
* ```Function``` = (sms function as shown above)
 
##How it works##

The Action will watch for changes to the dependency, either a change in value or an update in the period.  
When a change occurs, the code will check if the stream has a true value. If so, it will check what actions will need to occur in the period.
When it detects a change needs to occur, it will call the ```ISmsService``` interface and call a message.

##```ISmsInterface```##

The ISmsInterface has a simple interface to send messages to mobile numbers.
Example implementations are shown in [Code.Sms.SmsService.cs](../SmsService.cs).

```
CSharp

    public interface ISmsService
    {
        void SendSms(string mobile, string message);
    }

```

Examples include:
	
###Code.Sms.AmplaMessageService###

This code writes Sms messages to the Event Log and can be used for testing.
```
CSharp

    public class AmplaMessageService : ISmsService
    {
        public void SendSms(string mobile, string message)
        {
            string result = string.Format("Mobile: {0} - Message: {1}", mobile, message);

            Citect.Common.Diagnostics.Write(TraceLevel.Warning, result);
        }
    }
```

###Code.Sms.SqlSmsService###

This code inserts values into a sql table called ```SMS.dbo.OUTBOX```

```
CSharp

    public class SqlSmsService : ISmsService
    {
        private readonly ISqlHelper sqlHelper;

        public SqlSmsService() : this(ConnectionStrings.SmsDatabase)
        {
        }

        public SqlSmsService(string connectionString)
        {
            sqlHelper = new SqlHelper(connectionString);
        }

        public void SendSms(string mobile, string message)
        {
            SqlParameter mobileP = new SqlParameter("mobile", mobile);
            SqlParameter messageP = new SqlParameter("message", message);
            const string sql = "INSERT INTO [SMS].[dbo].[OUTBOX] ([Mobile_no] ,[Message]) VALUES (@mobile, @message)";
            sqlHelper.ExecuteNonQuery(CommandType.Text, sql, mobileP, messageP);
        }
    }
```

Each project will need to implement an SmsService as appropriate to their SMS component.