using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
	public class Purchase
	{
		public int Purchase_id { get; set; }
		public char Status { get; set;}
		public DateTime? Date_completed { get; set; }
		public int Employee_id { get; set; }
		public int Customer_id { get; set; }

		public override string ToString()
		{
			string state = "";
			if (Status == 'S') { state = "selected"; }
			if (Status == 'P') { state = "purchased"; }
			if (Status == 'C') { state = "completed"; }

			return $"Order {Purchase_id} - state: {state}";
		}
	}
}
