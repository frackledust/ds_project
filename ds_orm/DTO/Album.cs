using DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
	public class Album
	{
		public int Album_id { get; set; } = -1;
		public string Name { get; set; } = "";
		public string? Description { get; set; }
		public DateTime Date_released { get; set; } = DateTime.Now;
		public decimal Current_price { get; set;} = Decimal.Zero;
		public int Available_quantity { get; set; } = 0;
		public int Interpret_id { get; set; } = - 1;

		private Interpret interpret = null;
		public Interpret Interpret {
			get
			{
				if(interpret is null)
                {
					interpret = InterpretTable.SelectID(Interpret_id);
                }
				return interpret;
			}
		}
	}
}
