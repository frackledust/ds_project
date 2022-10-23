using DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
	public class Item
	{
		public int Purchase_id { get; set; }
		public int Album_id { get; set; }
		public int Quantity { get; set; }
		public decimal Price_per_item { get; set; }
		public DateTime Date_added { get; set; }

		private Album album = null;
		public Album Album
		{
			get
			{
				if(album == null)
                {
					album = AlbumTable.SelectID(Album_id);
                }
				return album;
			}
		}
	}
}
