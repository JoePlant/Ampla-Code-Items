using Code.Records;
using Code.Services;

namespace Code.Custom
{
    /// <summary>
    ///     Updates the Material Records for new records and when the Refresh Lookup
    /// </summary>
    public class MaterialRecordUpdater : RecordUpdater
    {
        private readonly IMaterialService materialService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialRecordUpdater"/> class.
        /// </summary>
        public MaterialRecordUpdater() : this(ServiceLocator.GetService<IMaterialService>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialRecordUpdater"/> class.
        /// </summary>
        /// <param name="materialService">The material service.</param>
        public MaterialRecordUpdater(IMaterialService materialService)
        {
            this.materialService = materialService;
        }

        /// <summary>
        /// Called when the record is a new record.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void OnUpdateNewRecord(IRecord record)
        {
            UpdateMaterialFields(record);
        }

        /// <summary>
        /// Called when the record is to be updated
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void OnUpdateRecord(IRecord record)
        {
            bool refresh = record.GetFieldValue<bool>("Refresh Material", false);
            if (refresh)
            {
                UpdateMaterialFields(record);
                record.SetFieldValue("Refresh Material", false);
            }
        }

        /// <summary>
        /// Updates the lookup.
        /// </summary>
        /// <param name="record">The record.</param>
        protected void UpdateMaterialFields(IRecord record)
        {
            string materialCode = record.GetFieldValue<string>("Material Code", null);
            if (!string.IsNullOrEmpty(materialCode))
            {
                string vendor = materialService.GetVendor(materialCode);
                record.SetFieldValue("Material Vendor", vendor);
            }
        }

        public override string ToString()
        {
            return "Material Record Updater";
        }
    }
}