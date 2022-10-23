using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
	public class AlbumHistory
	{
		public int Album_id { get; set; }
		public decimal Old_price { get; set; }
		public decimal New_price { get; set; }
		public DateTime Modified_at { get; set; }
		public int Employee_id { get; set; }
	}
}
