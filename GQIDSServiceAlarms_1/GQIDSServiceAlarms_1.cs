using System.Collections.Generic;
using System.Threading.Tasks;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Filters;
using Skyline.DataMiner.Net.Messages;

namespace GQIDSServiceAlarms_1
{
	public class ServiceAlarms : IGQIDataSource, IGQIOnInit, IGQIOnPrepareFetch, IGQIInputArguments
	{
		private static readonly GQIStringArgument Service = new GQIStringArgument("Service") { IsRequired = true };
		private static readonly GQIStringColumn IDColumn = new GQIStringColumn("ID");
		private static readonly GQIStringColumn ElementColumn = new GQIStringColumn("Element");
		private static readonly GQIStringColumn ParameterColumn = new GQIStringColumn("Parameter");
		private static readonly GQIStringColumn ValueColumn = new GQIStringColumn("Value");
		private static readonly GQIDateTimeColumn TimeColumn = new GQIDateTimeColumn("Time");
		private static readonly GQIStringColumn SeverityColumn = new GQIStringColumn("Severity");
		private static readonly GQIStringColumn OwnerColumn = new GQIStringColumn("Owner");

		private GQIDMS _dms;
		private Task<ActiveAlarmsResponseMessage> _alarms;
		private string _service;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_dms = args.DMS;
			return new OnInitOutputArgs();
		}

		public GQIArgument[] GetInputArguments()
		{
			return new[]
			{
				Service,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			args.TryGetArgumentValue(Service, out _service);
			return new OnArgumentsProcessedOutputArgs();
		}

		public OnPrepareFetchOutputArgs OnPrepareFetch(OnPrepareFetchInputArgs args)
		{
			if (string.IsNullOrWhiteSpace(_service))
				return new OnPrepareFetchOutputArgs();

			_alarms = Task.Factory.StartNew(() =>
			{
				var item = AlarmFilterItem.Create("service name", "equal to", new string[1] { _service }, null, null);
				var filter = new AlarmFilter(item);
				var msg = new GetActiveAlarmsMessage() { Filter = filter };
				return _dms.SendMessage(msg) as ActiveAlarmsResponseMessage;
			});
			return new OnPrepareFetchOutputArgs();
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				IDColumn,
				ElementColumn,
				ParameterColumn,
				ValueColumn,
				TimeColumn,
				SeverityColumn,
				OwnerColumn,
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			if (_alarms == null)
				return new GQIPage(new GQIRow[0]);

			_alarms.Wait();

			var alarms = _alarms.Result?.ActiveAlarms;
			if (alarms == null)
				throw new GenIfException("No active alarms found.");

			if (alarms.Length == 0)
				return new GQIPage(new GQIRow[0]);

			var rows = new List<GQIRow>(alarms.Length);

			foreach (var alarm in alarms)
			{
				var cells = new GQICell[]
				{
					new GQICell() {Value= $"{alarm.DataMinerID}/{alarm.AlarmID }"}, // IDColumn
					new GQICell() {Value= alarm.ElementName }, // ElementColumn,
					new GQICell() {Value= alarm.ParameterName }, // ParameterColumn,
					new GQICell() {Value= alarm.DisplayValue }, // ValueColumn,
					new GQICell() {Value= alarm.CreationTime.ToUniversalTime() }, // TimeColumn,
					new GQICell() {Value= alarm.Severity }, // SeverityColumn,
					new GQICell() {Value = alarm.Owner}, // OwnerColumn
				};

				rows.Add(new GQIRow(cells));
			}

			return new GQIPage(rows.ToArray()) { HasNextPage = false };
		}
	}
}