using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Citect.Ampla.Framework;
using Citect.Ampla.General.Common;
using Citect.Ampla.General.Common.Data;
using Citect.Ampla.General.Server;
using Citect.Common;

namespace Code.Records
{
    /// <summary>
    ///     Interface for interacting with records
    /// </summary>
    public interface IRecord : IDisposable
    {
        /// <summary>
        ///     Gets the display value from the field
        /// </summary>
        /// <typeparam name="T">
        ///     DataType to return
        /// </typeparam>
        /// <param name="fieldName">
        ///     name of the field to access
        /// </param>
        /// <returns>
        ///     The value of the field or a default value for type
        /// </returns>
        T GetFieldValue<T>(string fieldName);

        /// <summary>
        ///     Gets the display value from the field
        /// </summary>
        /// <typeparam name="T">
        ///     DataType to return
        /// </typeparam>
        /// <param name="fieldName">
        ///     name of the field to access
        /// </param>
        /// <param name="defaultValue">
        ///     the default value if not set
        /// </param>
        /// <returns>
        ///     The value of the field or a default value for type
        /// </returns>
        T GetFieldValue<T>(string fieldName, T defaultValue);

        /// <summary>
        ///     Gets the Raw Value from the field
        /// </summary>
        /// <typeparam name="T">
        ///     DataType to return
        /// </typeparam>
        /// <param name="fieldName">
        ///     name of the field to access
        /// </param>
        /// <returns>
        ///     The value of the field or a default value for type
        /// </returns>
        T GetFieldRawValue<T>(string fieldName);

        /// <summary>
        ///     Set the specified value in the field
        /// </summary>
        /// <typeparam name="T">
        ///     DataType to return
        /// </typeparam>
        /// <param name="fieldName">
        ///     name of the field to access
        /// </param>
        /// <param name="value">
        ///     The value to set
        /// </param>
        void SetFieldValue<T>(string fieldName, T value);

        /// <summary>
        ///     Outputs the Record details
        /// </summary>
        /// <returns></returns>
        string Details();

        /// <summary>
        /// Saves the changes to the record
        /// </summary>
        void SaveChanges();

        /// <summary>
        ///     Is the record confirmed?
        /// </summary>
        /// <returns></returns>
        bool IsConfirmed();

        /// <summary>
        ///     Is this a new record?
        /// </summary>
        /// <returns></returns>
        bool IsNew { get; }
    }
    
    /// <summary>
    /// Creates a new Record that wraps the Item and Event Args
    /// </summary>
    public abstract class Record : IRecord
    {
        /// <summary>
        ///     The fields that have been changed
        /// </summary>
        protected class FieldChange
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FieldChange"/> class.
            /// </summary>
            /// <param name="fieldName">Name of the field.</param>
            /// <param name="dataType">Type of the data.</param>
            /// <param name="value">The value.</param>
            public FieldChange(string fieldName, Type dataType, object value)
            {
                Field = fieldName;
                DataType = dataType.Name;
                Value = value;
            }

            public string Field { get; private set; }
            public string DataType { get; private set; }
            public object Value { get; private set; }

            public override string ToString()
            {
                return string.Format("{0}:={1}", Field, Value);
            }
        }
        
        /// <summary>
        /// Creates a new Record that wraps the Item and Event Args
        /// </summary>
        private class ChangedRecord : Record
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Record" /> class.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="recordChanged">The <see cref="RecordChangedEventArgs"/> instance containing the event data.</param>
            public ChangedRecord(Item sender, RecordChangedEventArgs recordChanged) : base(sender)
            {
                if (recordChanged != null)
                {
                    AmplaRecord = recordChanged.Record;
                    RecordId = recordChanged.RecordId;
                    State = Convert.ToString(recordChanged.Action);
                    IsNew = (recordChanged.Action & RecordChangeAction.Add) == RecordChangeAction.Add;
                    EventType = "RecordChanged";
                }
            }

            /// <summary>
            /// Saves the changes to the record
            /// </summary>
            protected override void OnSaveChanges()
            {
                //TraceInfo("Saving Changes: {0}", this);
                ReportingPoint reportingPoint = Item as ReportingPoint;
                if (reportingPoint != null)
                {
                    EditedDataDescriptorCollection editedData = AmplaRecord.GetEditedDataDescriptorCollection();
                    if (editedData != null && editedData.Values.Count > 0)
                    {
                        editedData.UserName = EventType;
                        reportingPoint.UpdateSampleData(editedData);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new Record that wraps the Item and Event Args
        /// </summary>
        private class ChangingRecord : Record
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Record" /> class.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="recordChanging">The <see cref="RecordChangingEventArgs"/> instance containing the event data.</param>
            public ChangingRecord(Item sender, RecordChangingEventArgs recordChanging)
                : base(sender)
            {
                if (recordChanging != null)
                {
                    AmplaRecord = recordChanging.Record;
                    RecordId = recordChanging.RecordId;
                    State = Convert.ToString(recordChanging.Action);
                    IsNew = (recordChanging.Action & RecordChangeAction.Add) == RecordChangeAction.Add;
                    EventType = "RecordChanging";
                }
            }

            /// <summary>
            /// Saves the changes to the record
            /// </summary>
            protected override void OnSaveChanges()
            {
            }
        }

        protected string FullName { get; private set; }
        protected int RecordId { get; set; }
        protected string State { get; set; }
        protected string EventType { get; set; }
        protected Item Item { get; private set; }
        public bool IsNew { get; private set; }

        protected List<FieldChange> UpdatedFields = new List<FieldChange>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Record"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        protected Record(Item sender)
        {
            Item = sender;
            FullName = Item.FullName;
        }

        /// <summary>
        /// Gets the ampla record.
        /// </summary>
        /// <value>
        /// The ampla record.
        /// </value>
        protected AmplaRecord AmplaRecord { get; set; }

        /// <summary>
        /// Gets the display value from the field
        /// </summary>
        /// <typeparam name="T">DataType to return</typeparam>
        /// <param name="fieldName">name of the field to access</param>
        /// <returns>
        /// The value of the field or a default value for type
        /// </returns>
        public T GetFieldValue<T>(string fieldName)
        {
            return GetFieldValue(fieldName, default(T));
        }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="defaultValue">The default value if not found</param>
        /// <returns></returns>
        public T GetFieldValue<T>(string fieldName, T defaultValue)
        {
            //TraceInfo("GetFieldValue<{0}>({1})", typeof(T).Name, fieldName);
            var field = AmplaRecord[fieldName];
            if (field != null && field.Actual != null)
            {
                object fieldValue = field.Actual.Value;

                //TraceInfo("GetFieldValue<{0}>({1}) = {2}", typeof(T).Name, fieldName, fieldValue);
                if (typeof(T).IsAssignableFrom(field.Actual.Type))
                {
                    return fieldValue != null ? (T) fieldValue : defaultValue;
                }
                TraceError(
                    "Error: {0} - Incorrect datatype for GetFieldValue('{1}'). Datatype is '{2}' but accessed as '{3}'",
                    FullName, fieldName, field.Actual.Type, typeof (T));

            }
            else
            {
                TraceError("Error: {0} - Field '{1}' does not exist.", FullName, fieldName);
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets the Raw Value from the field
        /// </summary>
        /// <typeparam name="T">DataType to return</typeparam>
        /// <param name="fieldName">name of the field to access</param>
        /// <returns>
        /// The value of the field or a default value for type
        /// </returns>
        public T GetFieldRawValue<T>(string fieldName)
        {
            //TraceInfo("GetFieldRawValue<{0}>({1})", typeof(T).Name, fieldName);
            var field = AmplaRecord[fieldName];
            if (field != null)
            {
                var actual = field.Actual;
                if (actual != null)
                {
                    if (typeof (T).IsAssignableFrom(actual.RawType))
                    {
                        var rawValue = actual.RawValue;
                        return rawValue == null ? default(T) : (T) rawValue;
                    }
                    TraceError(
                        "Error: {0} - Incorrect datatype for GetFieldRawValue('{1}'). Datatype is '{2}' but accessed as '{3}'",
                        FullName, fieldName, actual.RawType, typeof (T));
                }
            }
            else
            {
                TraceError("Error: {0} does not have field '{1}'", FullName, fieldName);
            }
            return default(T);
        }

        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value.</param>
        public void SetFieldValue<T>(string fieldName, T value)
        {
            var field = AmplaRecord[fieldName];
            if (field != null)
            {
                T prevValue = GetFieldValue<T>(fieldName, default(T));
                if (!object.Equals(prevValue, value))
                {
                    FieldChange fieldChange = new FieldChange(fieldName, typeof(T), value);
                    UpdatedFields.Add(fieldChange);

                    field.Actual.Value = value;
                }
            }
            else
            {
                TraceError("Unable to find field: '{0}'", fieldName);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Item = null;
        }

        /// <summary>
        /// Traces the error.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        protected static void TraceError(string format, params object[] args)
        {
            Diagnostics.Write(TraceLevel.Error, format, args);
        }

        /// <summary>
        /// Writes a information message
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        protected static void TraceInfo(string format, params object[] args)
        {
            Diagnostics.Write(TraceLevel.Info, format, args);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Item: {0} - Record: {1} - {2} ({3})", FullName, RecordId, State, EventType);
        }

        /// <summary>
        /// Outputs the Record details
        /// </summary>
        /// <returns></returns>
        public string Details()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Location: " + FullName);
            builder.AppendLine("RecordId: " + RecordId);
            builder.AppendLine("State: " + State);
            builder.AppendLine("EventType: " + EventType);
            AmplaRecord record = AmplaRecord;
            if (record != null)
            {
                var fields = record.GetFields();
                foreach (AmplaField field in fields)
                {
                    string fieldName = field.Name;
                    object rawValue = field.Actual.RawValue;
                    object displayValue = field.Actual.Value;

                    if (displayValue != rawValue)
                    {
                        builder.AppendFormat(" {0} := {1} ({2})", fieldName, displayValue, rawValue);
                    }
                    else
                    {
                        builder.AppendFormat(" {0} := {1}", fieldName, displayValue);
                    }
                    builder.AppendLine();
                }
                builder.AppendFormat(" Total ({0} fields)", fields.Count);
            }
            else
            {
                builder.AppendLine("Fields unavailable (Object disposed)");
            }
            return builder.ToString();
        }

        /// <summary>
        /// Saves the changes to the record
        /// </summary>
        public void SaveChanges()
        {
            int changes = UpdatedFields.Count;
            if (changes > 0)
            {
                List<string> updates = new List<string>();
                foreach (var field in UpdatedFields)
                {
                    updates.Add(field.ToString());
                }
                TraceInfo("{0} - Saving {1} Changes\r\n- {2}", this, UpdatedFields.Count, string.Join("\r\n-", updates));
                OnSaveChanges();
            }
        }

        /// <summary>
        /// Is the record confirmed?
        /// </summary>
        /// <returns></returns>
        public bool IsConfirmed()
        {
            var field = AmplaRecord["IsConfirmed"];
            if (field != null)
            {
                object value = field.Actual.Value;
                if (value != null)
                {
                    return (bool) value;
                }
            }
            return false;
            //return AmplaRecord.IsConfirmed;
        }

        protected abstract void OnSaveChanges();

        public static IRecord CreateRecord(Item eventItem, EventArgs eventArgs)
        {
            if (eventItem == null) throw new ArgumentNullException("eventItem", "No Item specified");
            RecordChangedEventArgs recordChanged = eventArgs as RecordChangedEventArgs;
            RecordChangingEventArgs recordChanging = eventArgs as RecordChangingEventArgs;

            if (recordChanged != null)
            {
                return new ChangedRecord(eventItem, recordChanged);
            }

            if (recordChanging != null)
            {
                return new ChangingRecord(eventItem, recordChanging);
            }

            throw new ArgumentException("Event Args must be RecordChangedEventArgs or RecordChangingEventArgs");
        }
    }

    /// <summary>
    ///     Manages the handling of the Record Changing event
    /// </summary>
    public class RecordChanging
    {
        private readonly IRecordUpdater recordUpdater;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordChanging"/> class.
        /// </summary>
        /// <param name="recordUpdater">The record updater.</param>
        public RecordChanging(IRecordUpdater recordUpdater)
        {
            this.recordUpdater = recordUpdater;
        }

        /// <summary>
        /// Handles the specified record changing event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void Handle(Item sender, EventArgs args)
        {
            using (IRecord record = Record.CreateRecord(sender, args))
            {
                recordUpdater.Update(record);
                record.SaveChanges();
            }
        }
    }

    /// <summary>
    ///     Manages the handling of the Record Changed event
    /// </summary>
    public class RecordChanged
    {
        private readonly IRecordUpdater recordUpdater;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordChanged"/> class.
        /// </summary>
        /// <param name="recordUpdater">The record updater.</param>
        public RecordChanged(IRecordUpdater recordUpdater)
        {
            this.recordUpdater = recordUpdater;
        }

        /// <summary>
        /// Handles the Record Changed Event for the sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void Handle(Item sender, EventArgs args)
        {
            using (IRecord record = Record.CreateRecord(sender, args))
            {
                recordUpdater.Update(record);
                record.SaveChanges();
            }
        }

    }
}